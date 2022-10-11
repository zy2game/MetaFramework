using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using System.IO;
using GameFramework.Runtime.Game;
using DG.Tweening;
using UnityEngine.EventSystems;

namespace GameFramework
{

    public sealed class TaskCompletionSource : TaskCompletionSource<int>
    {
        public static readonly TaskCompletionSource Void = CreateVoidTcs();

        public TaskCompletionSource(object state) : base(state)
        {
        }

        public TaskCompletionSource()
        {
        }

        public bool TryComplete() => this.TrySetResult(0);

        public void Complete() => this.SetResult(0);

        // todo: support cancellation token where used
        public bool SetUncancellable() => true;

        public override string ToString() => "TaskCompletionSource[status: " + this.Task.Status.ToString() + "]";

        static TaskCompletionSource CreateVoidTcs()
        {
            var tcs = new TaskCompletionSource();
            tcs.TryComplete();
            return tcs;
        }
    }

    [XLua.LuaCallCSharp]
    public static class Utility
    {
        public static Transform EmptyTransform = null;
        public static GameObject EmptyGameObject = null;
        public static string GetMd5Hash(this string input)
        {
            using (MD5 md5Hash = MD5.Create())
            {
                byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));
                StringBuilder sBuilder = new StringBuilder();
                for (int i = 0; i < data.Length; i++)
                {
                    sBuilder.Append(data[i].ToString("x2"));
                }
                return sBuilder.ToString();
            }
        }

        public static string GetMd5Hash(this byte[] input)
        {
            using (MD5 md5Hash = MD5.Create())
            {
                byte[] data = md5Hash.ComputeHash(input);
                StringBuilder sBuilder = new StringBuilder();
                for (int i = 0; i < data.Length; i++)
                {
                    sBuilder.Append(data[i].ToString("x2"));
                }
                return sBuilder.ToString();
            }
        }

        /// <summary>
        /// 读取文件数据
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static byte[] ReadFileData(string filePath)
        {
            string dir = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            if (!File.Exists(filePath))
            {
                return null;
            }
            return File.ReadAllBytes(filePath);
        }

        /// <summary>
        /// 读取文件数据
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static Task<byte[]> ReadFileDataAsync(string filePath)
        {
            string dir = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            if (!File.Exists(filePath))
            {
                return null;
            }
            return File.ReadAllBytesAsync(filePath);
        }

        public static string ReadFileAllText(string filePath)
        {

            return File.ReadAllText(filePath);
        }

        /// <summary>
        /// 写入文件数据
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="bytes"></param>
        public static void WriteFileData(string filePath, byte[] bytes)
        {
            string dir = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            File.WriteAllBytes(filePath, bytes);
        }

        /// <summary>
        /// 写入文件数据
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static Task WriteFileDataAsync(string filePath, byte[] bytes)
        {
            string dir = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            return File.WriteAllBytesAsync(filePath, bytes);
        }

        public static byte[] SerializeMessage(short opcode, byte[] bytes)
        {
            byte[] op = BitConverter.GetBytes(opcode);
            return op.Concat(bytes).ToArray();
        }


        public static void SetParent(this GameObject gameObject, Transform parent)
        {
            gameObject.SetParent(parent, Vector3.zero);
        }

        public static void SetParent(this GameObject gameObject, Transform parent, Vector3 position)
        {
            gameObject.SetParent(parent, position, Vector3.zero);
        }

        public static void SetParent(this GameObject gameObject, Transform parent, Vector3 position, Vector3 rotation)
        {
            gameObject.SetParent(parent, position, rotation, Vector3.one);
        }

        public static void SetParent(this GameObject gameObject, Transform parent, Vector3 position, Vector3 rotation, Vector3 scale)
        {
            if (parent != null)
            {
                gameObject.transform.SetParent(parent);
            }
            gameObject.transform.localPosition = position;
            gameObject.transform.localRotation = Quaternion.Euler(rotation);
            gameObject.transform.localScale = scale;
            RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
            if (rectTransform == null)
            {
                return;
            }
            if (rectTransform.anchorMax == Vector2.one)
            {
                rectTransform.sizeDelta = Vector2.zero;
                rectTransform.anchoredPosition = Vector2.zero;
            }
        }

        public static void SetParent(this GameObject gameObject, GameObject parent)
        {
            gameObject.SetParent(parent, Vector3.zero);
        }

        public static void SetParent(this GameObject gameObject, GameObject parent, Vector3 position)
        {
            gameObject.SetParent(parent, position, Vector3.zero);
        }

        public static void SetParent(this GameObject gameObject, GameObject parent, Vector3 position, Vector3 rotation)
        {
            gameObject.SetParent(parent, position, rotation, Vector3.one);
        }

        public static void SetParent(this GameObject gameObject, GameObject parent, Vector3 position, Vector3 rotation, Vector3 scale)
        {
            Transform transform = parent == null ? null : parent.transform;
            gameObject.SetParent(transform, position, rotation, scale);
        }


        public static Vector3 ToScreenPosition(this GameObject gameObject, Camera camera, Canvas canvas)
        {
            //世界转屏幕 Camera_Main世界的摄像机
            Vector3 pos = Camera.main.WorldToScreenPoint(gameObject.transform.position);
            Vector3 worldPoint;
            //屏幕转UI  ui(当前的canvas)  _camera_UiCamera(UI的摄像机)
            if (RectTransformUtility.ScreenPointToWorldPointInRectangle(canvas.GetComponent<RectTransform>(), pos, camera, out worldPoint))
            {
                return worldPoint;
            }
            return Vector2.zero;
        }

        public static Vector3 ToWorldPosition(this GameObject gameObject, Camera camera)
        {
            return camera.ScreenToWorldPoint(gameObject.transform.localPosition);
        }

        public static Vector3 ToWorldPosition(this Vector3 point, Camera camera)
        {
            return camera.ScreenToWorldPoint(point);
        }

        public static RaycastHit Raycast(Vector3 position)
        {

            //声明变量，用于保存信息
            RaycastHit hitInfo;
            //发射射线，起点是当前物体位置，方向是世界前方
            if (Physics.Raycast(position, Vector3.forward, out hitInfo))
            {
                return hitInfo;
            }
            return default;
        }

        public static RaycastHit Raycast(Vector3 position, string layerName)
        {
            int finger = -1;
#if !UNITY_EDITOR
     finger = 0; 
#endif
            if (EventSystem.current.currentSelectedGameObject != null)
            {
                Debug.Log(EventSystem.current.currentSelectedGameObject.name);
                return default;
            }
            if (EventSystem.current.IsPointerOverGameObject(finger))
            {
                return default;
            }
            Ray ray = Camera.main.ScreenPointToRay(position);
            if (Physics.Raycast(ray, out RaycastHit hitInfo, 1000, LayerMask.GetMask(layerName)))
            {
                Debug.Log(hitInfo.transform.name);
                return hitInfo;
            }
            return default;
        }

        public static void MoveTo(this GameObject gameObject, Vector3 position, float time)
        {
            gameObject.transform.MoveTo(position, time);
        }

        public static void MoveTo(this Component component, Vector3 position, float time)
        {
            component.transform.DOMove(position, time);
        }

        public static void MovePath(this GameObject gameObject, Vector3[] paths, float time)
        {
            if (gameObject == null)
            {
                return;
            }
            gameObject.transform.MovePath(paths, time);
        }


        public static void MovePath(this Component component, Vector3[] paths, float time)
        {
            if (component == null)
            {
                return;
            }
            if (paths == null || paths.Length <= 0)
            {
                return;
            }
            component.DOKill();
            component.transform.DOPath(paths, time).SetEase(Ease.Linear);
        }
        public static void MovePathAndLookPath(this GameObject gameObject, Vector3[] paths, float time)
        {
            if (gameObject == null)
            {
                return;
            }
            gameObject.transform.MovePathAndLookPath(paths, time);
        }
        public static void MovePathAndLookPath(this Component component, Vector3[] paths, float time)
        {
            if (component == null)
            {
                return;
            }
            if (paths == null || paths.Length <= 0)
            {
                return;
            }
            void OnChange(int index)
            {
                component.transform.DOLookAt(paths[index], 0.2f, AxisConstraint.Y, null);
            }
            component.DOKill();
            component.transform.DOPath(paths, time).SetEase(Ease.Linear).OnWaypointChange(OnChange);
        }

        public static void LookRotation(this GameObject gameObject, Vector3 dir)
        {
            Quaternion quaternion = Quaternion.LookRotation(dir);
            quaternion.x = 0;
            quaternion.z = 0;
            gameObject.transform.rotation = quaternion;
        }

        public static void LookRotation(this Component component, Vector3 dir)
        {
            Quaternion quaternion = Quaternion.LookRotation(dir);
            quaternion.x = 0;
            quaternion.z = 0;
            component.transform.rotation = quaternion;
        }

        public static int Round(this float value)
        {
            return (int)MathF.Round(value);
        }

        public static GameObject GetChild(this GameObject basic, string name)
        {
            if (basic == null)
            {
                return default;
            }
            Transform transform = basic.transform.Find(name);
            if (transform == null)
            {
                return default;
            }
            return transform.gameObject;
        }

        public static GameObject GetChild(this Transform basic, string name)
        {
            if (basic == null)
            {
                return default;
            }
            Transform transform = basic.Find(name);
            if (transform == null)
            {
                return default;
            }
            return transform.gameObject;
        }

        public static GameObject GetChild(this Component basic, string name)
        {
            if (basic == null)
            {
                return default;
            }
            Transform transform = basic.transform.Find(name);
            if (transform == null)
            {
                return default;
            }
            return transform.gameObject;
        }
    }
}
