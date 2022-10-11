using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using XLua;
using GameFramework.Runtime.Assets;
using UnityEngine.Rendering.Universal;

namespace GameFramework.Runtime.Game
{
    /// <summary>
    /// UI管理器
    /// </summary>
    /// <remarks>每一个UILayout表示一个Canvas,使用UGUI的顺序渲染特点管理UI层级</remarks>
    public sealed class UIManager : IUIManager
    {
        private GameWorld gameWorld;
        private Dictionary<int, Canvas> layers;
        private Dictionary<string, IUIHandler> caches;
        private Dictionary<string, IUIHandler> handlers;
        private const int MESSAGE_BOX_UI_LAYER = 999;
        private const int LOADING_UI_LAYER = 998;

        public Camera UICamera { get; private set; }

        public IUIHandler current
        {
            get;
            private set;
        }

        public UIManager(GameWorld world)
        {
            this.gameWorld = world;
            UICamera = GameObject.Instantiate<Camera>(Resources.Load<Camera>("Camera/UICamera"));
            UICamera.name = world.name + "_UICamera";
            UICamera.gameObject.SetParent(Utility.EmptyTransform);
            UniversalAdditionalCameraData universalAdditionalCameraData = world.WorldCamera.GetComponent<UniversalAdditionalCameraData>();
            universalAdditionalCameraData.cameraStack.Add(UICamera);
            layers = new Dictionary<int, Canvas>();
            caches = new Dictionary<string, IUIHandler>();
            handlers = new Dictionary<string, IUIHandler>();
        }

        /// <summary>
        /// 设置是否显示当前layout
        /// </summary>
        /// <param name="active"></param>
        public void SetActive(bool active)
        {
            foreach (var item in layers.Values)
            {
                item.gameObject.SetActive(active);
            }
        }

        /// <summary>
        /// 轮询所有UI
        /// </summary>
        public void FixedUpdate()
        {
            foreach (var item in handlers.Values)
            {
                item.FixedUpdate();
            }
        }

        /// <summary>
        /// 打开UI
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public IUIHandler OpenUI(string name)
        {
            if (caches.TryGetValue(name, out IUIHandler uiHandler))
            {
                uiHandler.OnEnable();
                caches.Remove(name);
                handlers.Add(name, uiHandler);
                return uiHandler;
            }
            AssetHandle handle = ResourcesManager.Instance.Load(name);
            if (handle == null)
            {
                return default;
            }
            GameObject gameObject = handle.CreateGameObject();
            if (gameObject == null)
            {
                return default;
            }
            LuaTable table = LuaManager.Instance.GetTable(gameObject.name);
            uiHandler = new CommonUIFormHandler(this, gameObject.name, gameObject, table);
            ToLayer(uiHandler, uiHandler.layer);
            handlers.Add(gameObject.name, uiHandler);
            uiHandler.Start();
            uiHandler.OnEnable();
            return uiHandler;
        }

        /// <summary>
        /// 关闭UI
        /// </summary>
        /// <param name="name"></param>
        /// <param name="isCache"></param>
        public void CloseUI(string name, bool isCache = false)
        {
            IUIHandler handler = GetUIHandler(name);
            if (handler == null)
            {
                return;
            }
            handlers.Remove(name);
            if (isCache)
            {
                handler.OnDisable();
                caches.Add(name, handler);
                return;
            }
            handler.Dispose();
        }

        /// <summary>
        /// 获取UI对象
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public IUIHandler GetUIHandler(string name)
        {
            if (handlers.TryGetValue(name, out IUIHandler handler))
            {
                return handler;
            }
            return default;
        }

        /// <summary>
        /// 清理所有UI
        /// </summary>
        public void Clear()
        {
            foreach (var item in handlers.Values)
            {
                item.Dispose();
            }
            handlers.Clear();
        }

        /// <summary>
        /// 释放当前UI管理器
        /// </summary>
        public void Dispose()
        {
            Clear();
            GameObject.DestroyImmediate(UICamera);
            UICamera = null;
        }

        /// <summary>
        /// 将UI设置到指定的层级
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="layer"></param>
        public Canvas ToLayer(IUIHandler handler, int layer)
        {
            if (!layers.TryGetValue(layer, out Canvas canvas))
            {
                canvas = CreateCanvas(layer);
                layers.Add(layer, canvas);
            }
            handler.gameObject.SetParent(canvas.transform);
            handler.canvas = canvas;
            if (current == null || handler.layer >= current.layer)
            {
                current = handler;
            }
            return canvas;
        }
        public Canvas ToLayer(GameObject handle, int layer)
        {
            if (!layers.TryGetValue(layer, out Canvas canvas))
            {
                canvas = CreateCanvas(layer);
                layers.Add(layer, canvas);
            }
            handle.SetParent(canvas.transform);
            return canvas;
        }

        public ILoading OnLoading()
        {
            CommonLoading loading = new CommonLoading();
            if (!layers.TryGetValue(LOADING_UI_LAYER, out Canvas canvas))
            {
                canvas = CreateCanvas(LOADING_UI_LAYER);
                layers.Add(LOADING_UI_LAYER, canvas);
            }
            loading.gameObject.SetParent(canvas.transform);
            return loading;
        }

        public IMessageBox OnMsgBox(string text, GameFrameworkAction ok = null, GameFrameworkAction cancel = null)
        {
            CommonMessageBox messageBox = new CommonMessageBox(text);
            if (ok != null)
            {
                messageBox.entry += ok;
            }
            if (cancel != null)
            {
                messageBox.cancel += cancel;
            }
            messageBox.tilet = "Tips";
            if (!layers.TryGetValue(MESSAGE_BOX_UI_LAYER, out Canvas canvas))
            {
                canvas = CreateCanvas(MESSAGE_BOX_UI_LAYER);
                layers.Add(MESSAGE_BOX_UI_LAYER, canvas);
            }
            messageBox.message = text;
            messageBox.gameObject.SetParent(canvas.transform);
            return messageBox;
        }

        private Canvas CreateCanvas(int layer)
        {
            Canvas canvas = GameObject.Instantiate<Canvas>(Resources.Load<Canvas>("Camera/Canvas"));
            canvas.name = "Canvas";
            canvas.gameObject.SetParent(UICamera.transform);
            canvas.worldCamera = UICamera;
            canvas.sortingOrder = layer;
            return canvas;
        }

        public void ClearMessageBox()
        {
            if (!layers.TryGetValue(MESSAGE_BOX_UI_LAYER, out Canvas canvas))
            {
                return;
            }
            GameObject.DestroyImmediate(canvas.gameObject);
        }

        public void ClearLoading()
        {
            if (!layers.TryGetValue(LOADING_UI_LAYER, out Canvas canvas))
            {
                return;
            }
            GameObject.DestroyImmediate(canvas.gameObject);
        }


    }
}