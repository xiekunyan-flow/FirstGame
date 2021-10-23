using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyScript : MonoBehaviour
{
    public GameObject myCube;
    public int transSpeed = 100;
    public float rotaSpeed = 10.5f;
    public float scale = 3;
    void OnGUI()
    {
        if (GUILayout.Button("移动立方体"))
        {
            myCube.transform.Translate(Vector3.forward * transSpeed * Time.deltaTime, Space.World);
        }
        if (GUILayout.Button("旋转立方体"))
        {
            myCube.transform.Rotate(Vector3.up * rotaSpeed, Space.World);
        }
        if (GUILayout.Button("缩放立方体"))
        {
            myCube.transform.localScale = new Vector3(scale, scale, scale);
        }
    }
}
