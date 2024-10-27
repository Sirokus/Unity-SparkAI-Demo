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

        //��ȡ�ļ�����ȡ�����Json�ļ������л�ΪData
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
    //��ʷ�Ի�
    public LinkedList<Content> dialogues = new LinkedList<Content>();

    //����
    //��ģ������
    public int LLMSparkType = 0;
    //��������
    public float uiVolumeMultiper = 1;
    public float bgmVolumeMultiper = 1;

    //��Դ����
    //��Դ�Ƕ�
    public float lightHorizontalAngle = 324;
    public float lightVerticalAngle = 51;

    //����������
    public bool isMusicPlay = true;
}
