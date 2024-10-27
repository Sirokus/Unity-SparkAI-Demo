using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SaveManager : UnitySingleton<SaveManager>
{
    public string dataName = "data.json";

    public Data data;

    private string dataPath;

    protected override void Awake()
    {
        base.Awake();

        //获取文件，读取所需的Json文件并序列化为Data
        dataPath = Application.persistentDataPath + "/" + dataName;
        if(File.Exists(dataPath))
        {
            using(StreamReader sr = new StreamReader(dataPath))
            {
                string json = sr.ReadToEnd();
                data = JsonConvert.DeserializeObject<Data>(json);
            }
        }
        else
        {
            data = new Data();
        }
    }

    public void Save()
    {
        if (File.Exists(dataPath))
            File.Delete(dataPath);

        string json = JsonConvert.SerializeObject(data);
        File.WriteAllText(dataPath, json);
    }
}

public class Data
{
    //历史对话
    public LinkedList<Content> dialogues = new LinkedList<Content>();

    //设置
    //大模型设置
    public int LLMSparkType = 0;
    //音量设置
    public float uiVolumeMultiper = 1;
    public float bgmVolumeMultiper = 1;

    //光源设置
    //光源角度
    public float lightHorizontalAngle = 324;
    public float lightVerticalAngle = 51;

    //播放器设置
    public bool isMusicPlay = true;
}
