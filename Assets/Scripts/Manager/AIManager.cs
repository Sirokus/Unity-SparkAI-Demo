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
    //用户信息
    public string appId, apiSecret, apiKey;

    //选择使用的星火模型
    public enum LLMType
    {
        SparkLite,
        SparkMax
    }
    public LLMType AIType = LLMType.SparkLite;
    private string llmVersion = "v1.1/chat";

    //最大回复Token
    public int MaxReplyTokes = 512;

    //WebSocket
    private ClientWebSocket webSocket;
    private CancellationToken cancellation;

    //请求参数相关
    JsonRequest request;

    //委托相关
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
                domain = "general",//模型领域，默认为星火通用大模型
                temperature = 0.5,//温度采样阈值，用于控制生成内容的随机性和多样性，值越大多样性越高；范围（0，1）
                max_tokens = MaxReplyTokes,//生成内容的最大长度，范围（0，4096）
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
        //获取鉴权URL
        string authUrl = GetAuthURL();
        string url = authUrl.Replace("http://", "ws://").Replace("https://", "wss://");

        using(webSocket = new ClientWebSocket())
        {
            try
            {
                //向服务器发起连接请求
                await webSocket.ConnectAsync(new Uri(url), cancellation);

                Debug.Log("成功连接服务器");

                //改变参数的上下文对话（这一块外包给了dialogueManager进行控制，因为要控制上下文长度）
                request.payload.message.text = dialogueContext;

                //将Json序列化为byte流
                string jsonString = JsonConvert.SerializeObject(request);
                byte[] binaryJsonData = Encoding.UTF8.GetBytes(jsonString.ToString());

                //向服务器发送数据
                _=webSocket.SendAsync(new ArraySegment<byte>(binaryJsonData), WebSocketMessageType.Text, true, cancellation);

                Debug.Log("已向服务器发送数据，正在等待消息返回...");

                //循环等待服务器返回内容
                byte[] receiveBuffer = new byte[1024];
                WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(receiveBuffer), cancellation);
                String resp = "";
                while (!result.CloseStatus.HasValue)
                {
                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        string receivedMessage = Encoding.UTF8.GetString(receiveBuffer, 0, result.Count);

                        //将结果解释为Json（没有指定具体类型）
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
                                Debug.Log($"最后一帧： {receivedMessage}");
                                int totalTokens = (int)jsonObj["payload"]["usage"]["text"]["total_tokens"];
                                Debug.Log($"整体返回结果： {resp}");
                                Debug.Log($"本次消耗token数： {totalTokens}");

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
                            Debug.Log($"请求报错： {receivedMessage}");
                            if (code == 10013)
                                OnRequestBan?.Invoke();
                            break;
                        }
                    }
                    else if (result.MessageType == WebSocketMessageType.Close)
                    {
                        Debug.Log("已关闭WebSocket连接");
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

    #region 鉴权URL
    string GetAuthURL()
    {
        //date参数生成
        string date = DateTime.UtcNow.ToString("r");

        //authorization参数生成
        StringBuilder stringBuilder = new StringBuilder("host: spark-api.xf-yun.com\n");
        stringBuilder.Append("date: ").Append(date).Append("\n");
        stringBuilder.Append("GET /").Append(llmVersion).Append(" HTTP/1.1");

        //利用hmac-sha256算法结合APISecret对上一步的tmp签名，获得签名后的摘要tmp_sha。
        //将上方的tmp_sha进行base64编码生成signature
        string signature = HMACsha256(apiSecret, stringBuilder.ToString());

        //利用上面生成的签名，拼接下方的字符串生成authorization_origin
        string authorization_origin = string.Format("api_key=\"{0}\", algorithm=\"{1}\", headers=\"{2}\", signature=\"{3}\"", apiKey, "hmac-sha256", "host date request-line", signature);

        //最后再将上方的authorization_origin进行base64编码，生成最终的authorization
        string authorization = Convert.ToBase64String(Encoding.UTF8.GetBytes(authorization_origin));

        //将鉴权参数组合成最终的键值对，并urlencode生成最终的握手URL。
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

        //将上方的tmp_sha进行base64编码生成signature
        return Convert.ToBase64String(date);
    }
    #endregion
}

//需要的类
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