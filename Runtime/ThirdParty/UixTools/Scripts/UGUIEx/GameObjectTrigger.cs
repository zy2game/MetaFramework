using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameObjectTrigger : MonoBehaviour
{
    [Serializable]
    public class Trigger
    {
        public GameObject obj;
        public bool active;       

    }

    [Serializable]
    public class Item
    {
        public int state;
        public Trigger[] triggers;

        public void Trigger()
        {
            foreach (var v in triggers)
            {
                if (v.obj.activeSelf != v.active)
                    v.obj.SetActive(v.active);
            }
        }

    }

    public Item[] items;

    public void SetState(int state)
    {
        foreach (var v in items)
        {
            if (v.state == state)
            {
                v.Trigger();
                break;
            }
        }
    }
}
