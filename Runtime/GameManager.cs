using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameFramework.Runtime.Assets;
using GameFramework.Runtime.Game;
using UnityEngine.Networking;

public class GameManager : MonoBehaviour
{
    IEnumerator Start()
    {
        Application.targetFrameRate = 60;
        DontDestroyOnLoad(gameObject);
        yield return AppConst.LoadLocalConfig();
        yield return new VersionManager().Init();
        ResourcesManager.Instance.Init();
        LuaManager.Instance.Init();
        LuaManager.Instance.DoString("require 'Main/Game'");
        GlobalEvent.Notify(EventName.EnterGame);

    }

    private void Update()
    {
        if (GameWorld.current == null)
        {
            return;
        }
        GameWorld.current.Update();
    }

    private void LateUpdate()
    {
        if (GameWorld.current == null)
        {
            return;
        }
        GameWorld.current.LateUpdate();
    }

    private void FixedUpdate()
    {
        if (GameWorld.current == null)
        {
            return;
        }
        GameWorld.current.FixedUpdate();
    }
}
