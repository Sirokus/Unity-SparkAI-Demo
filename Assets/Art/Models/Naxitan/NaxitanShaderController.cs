using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NaxitanShaderController : MonoBehaviour
{
    private Transform headTransform;
    private Transform headForward;
    private Transform headRight;

    private Material[] faceMaterials = new Material[2];

    // Start is called before the first frame update
    void Start()
    {
        //̫���ˣ�����·����ʼ���ˣ�ֱ���ڱ༭�����ϰ�
        headTransform = transform.Find("�����/�����_arm/ȫ�Ƥ��H/���󥿩`/���󥿩`2/����`��/����`��2/��/�ϰ���/�ϰ���3/�ϰ���2/��/�^").GetComponent<Transform>();

        headForward = headTransform.Find("HeadForward");
        if(!headForward)
        {
            GameObject emptyObj = new GameObject("HeadForward");
            headForward = emptyObj.transform;
            headForward.parent = headTransform;
            headForward.position = headTransform.position + headTransform.forward;
        }

        headRight = headTransform.Find("HeadRight");
        if (!headRight)
        {
            GameObject emptyObj = new GameObject("HeadRight");
            headRight = emptyObj.transform;
            headRight.parent = headTransform;
            headRight.position = headTransform.position + headTransform.right;
        }


        SkinnedMeshRenderer smr = transform.Find("�����_mesh").GetComponent<SkinnedMeshRenderer>();
        
        if(smr != null)
        {
            faceMaterials[0] = smr.materials[0];
            faceMaterials[1] = smr.materials[1];
        }
        else
        {
            MeshRenderer mr = transform.Find("�����_mesh").GetComponent<MeshRenderer>();

            faceMaterials[0] = mr.materials[0];
            faceMaterials[1] = mr.materials[1];
        }

        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 forwardVector = headForward.position - headTransform.position;
        Vector3 rightVector  = headRight.position - headTransform.position;

        forwardVector = forwardVector.normalized;
        rightVector = rightVector.normalized;

        Vector4 forwardVector4 = new Vector4(forwardVector.x, forwardVector.y, forwardVector.z);
        Vector4 rightVector4 = new Vector4(rightVector.x, rightVector.y, rightVector.z);

        for(int i = 0; i < faceMaterials.Length; i++)
        {
            faceMaterials[i].SetVector("_ForwardVector", forwardVector4);
            faceMaterials[i].SetVector("_RightVector", rightVector4);
        }
    }
}
