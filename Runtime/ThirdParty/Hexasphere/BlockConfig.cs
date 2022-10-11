using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


[Serializable]
public class BlockConfigData
{
    public Transform trans;
    public int blockIndex;
}

public class BlockConfig : MonoBehaviour
{

    public BlockConfigData[] data;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
