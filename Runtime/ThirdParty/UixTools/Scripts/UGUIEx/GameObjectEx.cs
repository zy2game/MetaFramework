using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameObjectEx : MonoBehaviour
{
    public GameObject[] reverseActive;

    public void SetActive(bool active)
    {
        if (gameObject.activeSelf != active)
            gameObject.SetActive(active);
        foreach (var v in reverseActive)
        {
            if (v.activeSelf != !active)
                v.SetActive(!active);
        }
    }
}
