using System.Collections;
using System.Collections.Generic;
using TouchScript.Gestures.TransformGestures;
using UnityEngine;

namespace GameFramework.Runtime.Game
{
    public class CameraContorller : MonoBehaviour
    {
        private float xSpeed = 200;//x轴的旋转速度
        private float ySpeed = 150;//x轴的旋转速度
        public float yMin = -20;//y最小角度
        public float yMax = 20;//y最大角度
        private float damping = 5;//阻尼 
        private float x = 0;
        private float y = 0;
        private bool isMoveScreen;
        private Camera _camera;
        private Vector3 change;
        private bool isDragObject;


        public static CameraContorller contorller { get; private set; }

        private void Start()
        {
            contorller = this;
            _camera = GetComponent<Camera>();
        }

        private void Update()
        {
            if (isDragObject)
            {
                return;
            }
            CameraScale();
            MoveScreen();
        }

        private void CameraScale()
        {
            //视野放大缩小
            if (Input.GetAxis("Mouse ScrollWheel") > 0)
            {
                if (_camera.fieldOfView >= 30)
                {
                    _camera.fieldOfView--;
                }
            }
            if (Input.GetAxis("Mouse ScrollWheel") < 0)
            {
                if (_camera.fieldOfView <= 80)
                {
                    _camera.fieldOfView++;
                }
            }
        }


        private void MoveScreen()
        {
            if (Input.GetMouseButtonDown(0) && !isMoveScreen)
            {
                isMoveScreen = true;
            }

            if (Input.GetMouseButtonUp(0) && isMoveScreen)
            {
                isMoveScreen = false;
            }
            if (!isMoveScreen)
            {
                return;
            }
            change.x = Input.GetAxis("Mouse X") * xSpeed * 0.02f;
            change.z = Input.GetAxis("Mouse Y") * ySpeed * 0.02f;
            if (change != Vector3.zero)
            {
                change = transform.position - change;
                change.y = 5;
                transform.position = Vector3.Lerp(transform.position, change, Time.deltaTime * damping);
            }

        }

        public static void OnDragObject()
        {
            contorller.isDragObject = true;
        }

        public static void OnDragObjectCompleted()
        {
            contorller.isDragObject = false;
        }
    }
}
