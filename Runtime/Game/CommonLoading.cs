using GameFramework.Runtime.Assets;
using UnityEngine;
using UnityEngine.UI;

namespace GameFramework.Runtime.Game
{
    public sealed class CommonLoading : ILoading
    {
        private GameObject _object;

        public static bool UsingInternalLoadingPrefab = true;
        public CommonLoading()
        {
            if (UsingInternalLoadingPrefab)
            {
                _object = GameObject.Instantiate(Resources.Load<GameObject>("Loading/Loading"));
            }
            else
            {
                AssetHandle handle = ResourcesManager.Instance.Load("common/prefab/MessageBox");
                _object = handle.CreateGameObject();
            }
        }
        public GameObject gameObject
        {
            get
            {
                return _object;
            }
        }
        private string _text;
        private string _version;
        public string text
        {
            get
            {
                return _text;
            }
            set
            {
                _text = value;
                if (_object != null)
                {
                    Text com = _object.transform.Find("info")?.GetComponent<Text>();
                    if (com != null)
                    {
                        com.text = _text;
                    }
                }
            }
        }

        public string version
        {
            get
            {
                return _version;
            }
            set
            {
                _version = value;
                if (_object != null)
                {
                    Text com = _object.transform.Find("version")?.GetComponent<Text>();
                    if (com != null)
                    {
                        com.text = _version;
                    }
                }
            }
        }

        public void Dispose()
        {
            throw new System.NotImplementedException();
        }
    }
}