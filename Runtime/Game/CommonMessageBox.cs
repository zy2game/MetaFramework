using UnityEngine;
using GameFramework.Runtime.Assets;
using UnityEngine.UI;

namespace GameFramework.Runtime.Game
{
    public sealed class CommonMessageBox : IMessageBox
    {
        private GameObject _object;
        public static bool UsingInternalMessageBoxPrefab = true;

        public string tilet
        {
            get
            {
                return GetTilet();
            }
            set
            {
                SetTilet(value);
            }
        }

        public string message
        {
            get
            {
                return GetMessage();
            }
            set
            {
                SetMessage(value);
            }
        }

        public GameObject gameObject
        {
            get
            {
                return _object;
            }
        }

        private GameFrameworkAction _cancel;

        public event GameFrameworkAction cancel
        {
            add
            {
                _cancel += value;
            }
            remove
            {
                _cancel -= value;
            }
        }
        private GameFrameworkAction _entry;
        public event GameFrameworkAction entry
        {
            add
            {
                entry += value;
            }
            remove
            {
                _entry -= value;
            }
        }

        public CommonMessageBox(string text)
        {
            if (UsingInternalMessageBoxPrefab)
            {
                _object = GameObject.Instantiate(Resources.Load<GameObject>("Loading/MessageBox"));
            }
            else
            {
                AssetHandle handle = ResourcesManager.Instance.Load("common/prefab/MessageBox");
                _object = handle.CreateGameObject();
            }

            if (_object == null)
            {
                return;
            }
            Button cancelBtn = _object.transform.Find("Cancel")?.GetComponent<Button>();
            Button entryBtn = _object.transform.Find("Entry")?.GetComponent<Button>();

            cancelBtn.onClick.AddListener(() =>
            {
                if (_cancel != null)
                {
                    _cancel();
                }
                Dispose();
            });

            entryBtn.onClick.AddListener(() =>
            {
                if (_entry != null)
                {
                    _entry();
                }
                Dispose();
            });
        }
        public string GetTilet()
        {
            if (_object == null)
            {
                return string.Empty;
            }
            Text com = _object.transform.Find("Tilet")?.GetComponent<Text>();
            if (com != null)
            {
                return com.text;
            }
            return string.Empty;
        }
        public void SetTilet(string tilet)
        {
            if (_object != null)
            {
                Text com = _object.transform.Find("Tilet")?.GetComponent<Text>();
                if (com != null)
                {
                    com.text = tilet;
                }
            }
        }

        public string GetMessage()
        {
            if (_object == null)
            {
                return string.Empty;
            }
            Text com = _object.transform.Find("Text")?.GetComponent<Text>();
            if (com != null)
            {
                return com.text;
            }
            return string.Empty;
        }

        public void SetMessage(string message)
        {
            if (_object != null)
            {
                Text com = _object.transform.Find("Text")?.GetComponent<Text>();
                if (com != null)
                {
                    com.text = message;
                }
            }
        }


        public void Dispose()
        {
            GameObject.DestroyImmediate(_object);
            _entry = null;
            _cancel = null;
        }
    }
}