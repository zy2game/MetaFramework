using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XLua;
using UnityEngine;


namespace GameFramework
{

    /// <summary>
    /// 游戏世界
    /// </summary>
    /// <remarks>此接口是游戏核心接口，在游戏中以World区分场景，每个场景都应该继承World</remarks>
    public interface IWorld : GObject
    {
        /// <summary>
        /// 世界名称
        /// </summary>
        string name { get; }

        /// <summary>
        /// 当前世界的天空盒
        /// </summary>
        /// <value></value>
        ISkybox skybox { get; }

        /// <summary>
        /// 是否激活
        /// </summary>
        /// <value></value>
        bool activeSelf { get; }

        /// <summary>
        /// 世界主相机
        /// </summary>
        /// <value></value>
        Camera WorldCamera { get; }

        /// <summary>
        /// UI管理器
        /// </summary>
        /// <remarks>每个World单独使用一个，防止UI管理混乱</remarks>
        IUIManager UIManager { get; }

        /// <summary>
        /// 音效播放器
        /// </summary>
        /// <remarks>每个World分开使用，防止管理混乱</remarks>
        IAudioManager AudioManager { get; }

        /// <summary>
        /// 寻路器
        /// </summary>
        /// <value></value>
        MapGrid Map { get; }

        /// <summary>
        /// 输入管理
        /// </summary>
        /// <value></value>
        InputManager input { get; }


        void CreateMapPath(float gridSize, int gridCount, List<bool> walls);

        /// <summary>
        /// 是否拥有指定的实体对象
        /// </summary>
        /// <param name="guid"></param>
        bool HasEntity(string guid);

        /// <summary>
        /// 创建实体对象
        /// </summary>
        IEntity CreateEntity();

        /// <summary>
        /// 创建实体对象
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        IEntity CreateEntity(string name);

        /// <summary>
        /// 创建实体对象
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        IEntity CreateEntity(string name, string guid);

        /// <summary>
        /// 移除实体对象
        /// </summary>
        /// <param name="guid"></param>
        void RemoveEntity(string guid);

        /// <summary>
        /// 获取指定的实体
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        IEntity GetEntity(string guid);

        /// <summary>
        /// 通过实体名称获取实体对象
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        IEntity GetEntityWithName(string name);

        /// <summary>
        /// 获取所有实体对象
        /// </summary>
        /// <returns></returns>
        List<IEntity> GetEntitys();

        /// <summary>
        /// 获取所有实体对象
        /// </summary>
        /// <returns></returns>
        Context[] GetEntitys(int tags);

        /// <summary>
        /// 加载逻辑单元
        /// </summary>
        /// <param name="scriptble"></param>
        void AddScriptble(IScriptble scriptble);

        /// <summary>
        /// 加载逻辑单元
        /// </summary>
        /// <param name="table"></param>
        void AddLuaScriptble(LuaTable table);

        /// <summary>
        /// 设置是否激活当前世界
        /// </summary>
        /// <param name="active"></param>
        /// <remarks>如果世界失活，那么所有的逻辑单元将都不会被执行</remarks>
        void SetActive(bool active);

        /// <summary>
        /// 刷新当前世界
        /// </summary>
        void FixedUpdate();

        void Update();

        void LateUpdate();
    }
}
