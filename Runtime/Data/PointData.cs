using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameFramework.Runtime.Data
{
    public class PointData : MonoBehaviour
    {
        public int index;
        public string Key
        {
            get; private set;
        }
        public Vector3 position;
        public Quaternion rotate;
        public int vertexCount;
        public Vector3[] vertices;
        public PointData[] neighbours;
        public MeshRenderer meshRenderer;
        public GameObject land;

        public void Awake()
        {
            Key = index.ToString();
        }
    }
}
