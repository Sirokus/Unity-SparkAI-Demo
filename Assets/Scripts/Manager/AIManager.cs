using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class AIManager : UnitySingleton<AIManager>
{
    //�û���Ϣ
    public string appId, apiSecret, apiKey;

    //ѡ��ʹ�õ��ǻ�ģ��
    public enum LLMType
    {
        SparkLite,
        SparkMax
    }
    public LLMType AIType = LLMType.SparkLite;
    private string llmVersion = "v1.1/chat";

    //���ظ�Token
    public int MaxReplyTokes = 512;

    //WebSocket
    private ClientWebSocket webSocket;
    private CancellationToken cancellation;

    //����������
    JsonRequest request;

    //ί�����
    public event Action OnRequestBan;


    protected override void Awake()
    {
        base.Awake();

        request = new JsonRequest();
        request.header = new Header()
        {
            app_id = appId,
            uid = "12345"
        };
        request.parameter = new Parameter()
        {
            chat = new Chat()
            {
                domain = "general",//ģ������Ĭ��Ϊ�ǻ�ͨ�ô�ģ��
                temperature = 0.5,//�¶Ȳ�����ֵ�����ڿ����������ݵ�����ԺͶ����ԣ�ֵԽ�������Խ�ߣ���Χ��0��1��
                max_tokens = MaxReplyTokes,//�������ݵ���󳤶ȣ���Χ��0��4096��
            }
        };
        request.payload = new Payload()
        {
            message = new Message()
        };
    }

    private void Start()
    {
        SelectLLMType((LLMType)SaveManager.Instance.data.LLMSparkType);
    }

    public void SelectLLMType(LLMType type)
    {
        AIType = type;

        switch(type)
        {
        case LLMType.SparkLite:
            llmVersion = "v1.1/chat";
            break;
        case LLMType.SparkMax:
            llmVersion = "v3.5/chat";
            break;
        default:
            break;
        }
    }

    public async void RequestAnswer(List<Content> dialogueContext, Action<string> completeCallback, Action<string> streamingCallback = null)
    {
        //��ȡ��ȨURL
        string authUrl = GetAuthURL();
        string url = authUrl.Replace("http://", "ws://").Replace("https://", "wss://");

        using(webSocket = new ClientWebSocket())
        {
            try
            {
                //�������������������
                await webSocket.ConnectAsync(new Uri(url), cancellation);

                Debug.Log("�ɹ����ӷ�����");

                //�ı�����������ĶԻ�����һ���������dialogueManager���п��ƣ���ΪҪ���������ĳ��ȣ�
                request.payload.message.text = dialogueContext;

                //��Json���л�Ϊbyte��
                string jsonString = JsonConvert.SerializeObject(request);
                byte[] binaryJsonData = Encoding.UTF8.GetBytes(jsonString.ToString());

                //���������������
                _=webSocket.SendAsync(new ArraySegment<byte>(binaryJsonData), WebSocketMessageType.Text, true, cancellation);

                Debug.Log("����������������ݣ����ڵȴ���Ϣ����...");

                //ѭ���ȴ���������������
                byte[] receiveBuffer = new byte[1024];
                WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(receiveBuffer), cancellation);
                String resp = "";
                while (!result.CloseStatus.HasValue)
                {
                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        string receivedMessage = Encoding.UTF8.GetString(receiveBuffer, 0, result.Count);

                        //���������ΪJson��û��ָ���������ͣ�
                        JObject jsonObj = JObject.Parse(receivedMessage);
                        int code = (int)jsonObj["header"]["code"];

                        if (0 == code)
                        {
                            int status = (int)jsonObj["payload"]["choices"]["status"];

                            JArray textArray = (JArray)jsonObj["payload"]["choices"]["text"];
                            string content = (string)textArray[0]["content"];
                            resp += content;

                            if (status == 2)
                            {
                                Debug.Log($"���һ֡�� {receivedMessage}");
                                int totalTokens = (int)jsonObj["payload"]["usage"]["text"]["total_tokens"];
                                Debug.Log($"���巵�ؽ���� {resp}");
                                Debug.Log($"��������token���� {totalTokens}");

                                completeCallback(resp.TrimStart('\n'));
                                return;
                            }
                            else
                            {
                                streamingCallback?.Invoke(resp.TrimStart('\n'));
                            }
                        }
                        else
                        {
                            Debug.Log($"���󱨴� {receivedMessage}");
                            if (code == 10013)
                                OnRequestBan?.Invoke();
                            break;
                        }
                    }
                    else if (result.MessageType == WebSocketMessageType.Close)
                    {
                        Debug.Log("�ѹر�WebSocket����");
                        break;
                    }

                    result = await webSocket.ReceiveAsync(new ArraySegment<byte>(receiveBuffer), cancellation);
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
            }

            completeCallback(null);
        }
    }

    #region ��ȨURL
    string GetAuthURL()
    {
        //date��������
        string date = DateTime.UtcNow.ToString("r");

        //authorization��������
        StringBuilder stringBuilder = new StringBuilder("host: spark-api.xf-yun.com\n");
        stringBuilder.Append("date: ").Append(date).Append("\n");
        stringBuilder.Append("GET /").Append(llmVersion).Append(" HTTP/1.1");

        //����hmac-sha256�㷨���APISecret����һ����tmpǩ�������ǩ�����ժҪtmp_sha��
        //���Ϸ���tmp_sha����base64��������signature
        string signature = HMACsha256(apiSecret, stringBuilder.ToString());

        //�����������ɵ�ǩ����ƴ���·����ַ�������authorization_origin
        string authorization_origin = string.Format("api_key=\"{0}\", algorithm=\"{1}\", headers=\"{2}\", signature=\"{3}\"", apiKey, "hmac-sha256", "host date request-line", signature);

        //����ٽ��Ϸ���authorization_origin����base64���룬�������յ�authorization
        string authorization = Convert.ToBase64String(Encoding.UTF8.GetBytes(authorization_origin));

        //����Ȩ������ϳ����յļ�ֵ�ԣ���urlencode�������յ�����URL��
        string path1 = "authorization=" + authorization;
        string path2 = "date=" + WebUtility.UrlEncode(date);
        string path3 = "host=" + "spark-api.xf-yun.com";

        return "wss://spark-api.xf-yun.com/" + llmVersion + "?" + path1 + "&" + path2 + "&" + path3;
    }
    public string HMACsha256(string apiSecretIsKey, string buider)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(apiSecretIsKey);
        System.Security.Cryptography.HMACSHA256 hMACSHA256 = new System.Security.Cryptography.HMACSHA256(bytes);
        byte[] date = Encoding.UTF8.GetBytes(buider);
        date = hMACSHA256.ComputeHash(date);
        hMACSHA256.Clear();

        //���Ϸ���tmp_sha����base64��������signature
        return Convert.ToBase64String(date);
    }
    #endregion
}

//��Ҫ����
public class JsonRequest
{
    public Header header { get; set; }
    public Parameter parameter { get; set; }
    public Payload payload { get; set; }
}

public class Header
{
    public string app_id { get; set; }
    public string uid { get; set; }
}

public class Parameter
{
    public Chat chat { get; set; }
}

public class Chat
{
    public string domain { get; set; }
    public double temperature { get; set; }
    public int max_tokens { get; set; }
}

public class Payload
{
    public Message message { get; set; }
}

public class Message
{
    public List<Content> text { get; set; }
}

public class Content
{
    public string role { get; set; }
    public string content { get; set; }
}

public enum Role
{
    System,
    User,
    AI
}