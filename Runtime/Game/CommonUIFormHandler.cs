using System;
using UnityEngine;
using XLua;
using System.Collections.Generic;
using DG.Tweening;
using GameFramework.Runtime.Assets;
namespace GameFramework.Runtime.Game
{
    public enum TriggerType
    {
        Start,
        Enable,
        Disable,
        Time,
        Click,
    }
    public sealed class CommonUIFormHandler : IUIHandler
    {
        public int layer
        {
            get;
            private set;
        }

        public string name
        {
            get;
            private set;
        }

        public GameObject gameObject
        {
            get;
        }

        public Camera UICamera { get; private set; }
        public Canvas canvas { get; set; }

        private IUIManager manager;
        private LuaTable table;
        private LuaFunction _start;
        private LuaFunction _dispose;
        private LuaFunction _disable;
        private LuaFunction _enable;
        private LuaFunction _eventHandle;
        private LuaFunction _fixedUpdate;

        private Dictionary<string, RectTransform> childs;

        /// <summary>
        /// UI管道
        /// </summary>
        /// <param name="uIManager"></param>
        /// <param name="table"></param>
        public CommonUIFormHandler(IUIManager uIManager, string name, GameObject gameObject, LuaTable table)
        {
            this.manager = uIManager;
            this.table = table;
            this.gameObject = gameObject;
            this.name = name;
            this.UICamera = uIManager.UICamera;
            if (table == null)
            {
                return;
            }

            this.layer = table.Get<int>("layer");
            _start = table.Get<LuaFunction>("start");
            _dispose = table.Get<LuaFunction>("dispose");
            _disable = table.Get<LuaFunction>("disable");
            _enable = table.Get<LuaFunction>("enable");
            _fixedUpdate = table.Get<LuaFunction>("update");
            _eventHandle = table.Get<LuaFunction>("event");
        }

        /// <summary>
        /// 释放UI管理器
        /// </summary>
        public void Dispose()
        {
            if (_dispose != null)
            {
                _dispose.Call(table, this);
            }
            GameObject.DestroyImmediate(this.gameObject);
        }

        /// <summary>
        /// 隐藏UI
        /// </summary>
        public void OnDisable()
        {
            this.gameObject.SetActive(false);
            if (_disable != null)
            {
                _disable.Call(table, this);
            }
        }

        /// <summary>
        /// 显示UI
        /// </summary>
        public void OnEnable()
        {
            this.gameObject.SetActive(true);
            if (_enable != null)
            {
                _enable.Call(table, this);
            }
        }

        /// <summary>
        /// 初始化
        /// </summary>
        public void Start()
        {
            childs = new Dictionary<string, RectTransform>();
            RectTransform[] transforms = this.gameObject.transform.GetComponentsInChildren<RectTransform>(true);
            for (int i = 0; i < transforms.Length; i++)
            {
                childs.Add(transforms[i].name, transforms[i]);
            }

            UnityEngine.UI.Button[] buttons = this.gameObject.transform.GetComponentsInChildren<UnityEngine.UI.Button>(true);
            foreach (var item in buttons)
            {
                GameObject eventObject = item.gameObject;
                item.onClick.AddListener(() =>
                {
                    OnNotify(item.gameObject.name, eventObject, null);
                });
            }

            UnityEngine.UI.InputField[] inputs = this.gameObject.transform.GetComponentsInChildren<UnityEngine.UI.InputField>(true);
            foreach (var item in inputs)
            {
                GameObject eventObject = item.gameObject;
                item.onEndEdit.AddListener((args) =>
                {
                    OnNotify(item.gameObject.name, eventObject, args);
                });
            }
            if (_start == null)
            {
                return;
            }
            _start.Call(table, this);
        }

        /// <summary>
        /// 时间通知
        /// </summary>
        /// <param name="eventId"></param>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public void OnNotify(string eventId, GameObject sender, object args)
        {
            if (_eventHandle == null)
            {
                return;
            }
            _eventHandle.Call(table, this, eventId, sender, args);
        }


        /// <summary>
        /// 移动动画
        /// </summary>
        /// <param name="name"></param>
        /// <param name="paths"></param>
        /// <param name="time"></param>
        /// <param name="callback"></param>
        public void TweenMovement(string name, Vector3[] paths, float time, Action callback = null)
        {
            if (!childs.TryGetValue(name, out RectTransform child))
            {
                return;
            }
            child.DOLocalPath(paths, time, PathType.Linear).OnComplete(() =>
            {
                if (callback != null)
                {
                    callback();
                }
            });
        }

        /// <summary>
        /// 旋转动画
        /// </summary>
        /// <param name="name"></param>
        /// <param name="endValue"></param>
        /// <param name="time"></param>
        /// <param name="callback"></param>
        public void TweenRotation(string name, Vector3 endValue, float time, Action callback = null)
        {
            if (!childs.TryGetValue(name, out RectTransform child))
            {
                return;
            }
            child.DOLocalRotate(endValue, time, RotateMode.FastBeyond360).OnComplete(() =>
            {
                if (callback != null)
                {
                    callback();
                }
            });
        }

        /// <summary>
        /// 缩放动画
        /// </summary>
        /// <param name="name"></param>
        /// <param name="endValue"></param>
        /// <param name="time"></param>
        /// <param name="callback"></param>
        public void TweenScale(string name, Vector3 endValue, float time, Action callback = null)
        {
            if (!childs.TryGetValue(name, out RectTransform child))
            {
                return;
            }
            child.DOScale(endValue, time).OnComplete(() =>
            {
                if (callback != null)
                {
                    callback();
                }
            });
        }

        /// <summary>
        /// 颜色动画
        /// </summary>
        /// <param name="name"></param>
        /// <param name="color"></param>
        /// <param name="time"></param>
        /// <param name="callback"></param>
        public void TweenColor(string name, Color color, float time, Action callback = null)
        {
            if (!childs.TryGetValue(name, out RectTransform child))
            {
                return;
            }
            UnityEngine.UI.Image image = child.GetComponent<UnityEngine.UI.Image>();
            if (image != null)
            {
                image.DOColor(color, time).OnComplete(() =>
                {
                    if (callback != null)
                    {
                        callback();
                    }
                });
            }
            UnityEngine.UI.Text text = child.GetComponent<UnityEngine.UI.Text>();
            if (image != null)
            {
                image.DOColor(color, time).OnComplete(() =>
                {
                    if (callback != null)
                    {
                        callback();
                    }
                });
            }
        }

        /// <summary>
        /// 获取子节点
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public GameObject GetChild(string name)
        {
            if (!childs.TryGetValue(name, out RectTransform transform))
            {
                return default;
            }
            return transform.gameObject;
        }

        /// <summary>
        /// 获取精灵图片
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Sprite GetSprite(string name)
        {
            if (!childs.TryGetValue(name, out RectTransform transform))
            {
                return default;
            }
            UnityEngine.UI.Image image = transform.GetComponent<UnityEngine.UI.Image>();
            if (image == null)
            {
                return default;
            }
            return image.sprite;
        }

        /// <summary>
        /// 设置精灵图片
        /// </summary>
        /// <param name="name"></param>
        /// <param name="sprite"></param>
        public void SetSprite(string name, Sprite sprite)
        {
            if (!childs.TryGetValue(name, out RectTransform transform))
            {
                return;
            }
            UnityEngine.UI.Image image = transform.GetComponent<UnityEngine.UI.Image>();
            if (image == null)
            {
                return;
            }
            image.sprite = sprite;
        }

        /// <summary>
        /// 轮询
        /// </summary>
        public void FixedUpdate()
        {
            if (_fixedUpdate == null)
            {
                return;
            }
            _fixedUpdate.Call(table);
        }

        /// <summary>
        /// 设置到指定的层级
        /// </summary>
        /// <param name="layer"></param>
        public void ToLayer(int layer)
        {
            manager.ToLayer(this, layer);
        }

        /// <summary>
        /// 设置精灵图片
        /// </summary>
        /// <param name="name"></param>
        /// <param name="spriteName"></param>
        public void SetSprite(string name, string spriteName)
        {
            AssetHandle handle = ResourcesManager.Instance.Load(spriteName);
            if (handle == null)
            {
                return;
            }

            Sprite sprite = (Sprite)handle.LoadAsset(typeof(Sprite));
            SetSprite(name, sprite);
        }

        /// <summary>
        /// 获取文本
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string GetText(string name)
        {
            if (!childs.TryGetValue(name, out RectTransform transform))
            {
                return default;
            }
            UnityEngine.UI.Text text = transform.GetComponent<UnityEngine.UI.Text>();
            if (text == null)
            {
                return default;
            }
            return text.text;
        }

        /// <summary>
        /// 设置文本
        /// </summary>
        /// <param name="name"></param>
        /// <param name="info"></param>
        public void SetText(string name, string info)
        {
            if (!childs.TryGetValue(name, out RectTransform transform))
            {
                return;
            }
            UnityEngine.UI.Text text = transform.GetComponent<UnityEngine.UI.Text>();
            if (text == null)
            {
                return;
            }
            text.text = info;
        }
    }
}