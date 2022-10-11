using System;
using UnityEngine;

namespace GameFramework.Runtime.Game
{
    /// <summary>
    /// ��պ�
    /// </summary>
    public sealed class Skybox : ISkybox
    {
        private GameWorld gameWorld;
        private UnityEngine.Skybox _skybox;
        private float RotationInternalTime;
        private float RatationInternalAngle;
        private float lastRotaionTime;
        private float current_Rotation = 0;

        public Skybox(GameWorld gameWorld)
        {
            RotationInternalTime = 0.1f;
            this.gameWorld = gameWorld;
            _skybox = gameWorld.WorldCamera.gameObject.AddComponent<UnityEngine.Skybox>();
        }

        /// <summary>
        /// release the skybox
        /// </summary>
        public void Dispose()
        {
            GameObject.DestroyImmediate(_skybox);
            _skybox = null;
        }

        /// <summary>
        /// update the skybox
        /// </summary>
        public void FixedUpdate()
        {
            if (_skybox == null || _skybox.material == null)
            {
                return;
            }
            if (Time.realtimeSinceStartup - lastRotaionTime > RotationInternalTime)
            {
                lastRotaionTime = Time.realtimeSinceStartup;
                _skybox.material.SetFloat("_Rotation", current_Rotation);
                current_Rotation += RatationInternalAngle;
            }
        }

        /// <summary>
        /// init the skybox
        /// </summary>
        /// <param name="skyName"></param>
        public void Initialize(float rotationTime, float rotationAngle, string skyName)
        {
            RotationInternalTime = rotationTime;
            RatationInternalAngle = rotationAngle;
            Assets.AssetHandle handle = Assets.ResourcesManager.Instance.Load(skyName);
            _skybox.material = (Material)handle.LoadAsset(typeof(Material));
        }

        /// <summary>
        /// set the skybox active
        /// </summary>
        /// <param name="active"></param>
        public void SetActive(bool active)
        {
            _skybox.enabled = active;
        }
    }
}