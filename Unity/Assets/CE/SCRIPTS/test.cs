using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class test : MonoBehaviour
{
    public GameObject canvas;

    private bool pre = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        if(pre!= canvas.activeSelf){
            Debug.Log("TTTTTTTT");
            Debug.Log(canvas.activeSelf);
            pre = canvas.activeSelf;        }
    }
}
