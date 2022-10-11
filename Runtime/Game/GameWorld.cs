using System.Linq;
using System.Collections.Generic;
using System;
using XLua;
using UnityEngine;

namespace GameFramework.Runtime.Game
{
    /// <summary>
    /// 游戏世界
    /// </summary>
    public sealed class GameWorld : IWorld
    {

        private bool active;
        private LuaFunction dispose;
        private LuaFunction setActive;
        private List<IEntity> entitys;
        private List<Context> contexts;
        private LuaFunction fixedUpdate;
        private LuaFunction update;
        private LuaFunction lateUpdate;
        private List<IScriptble> runnables;
        private static Dictionary<string, IWorld> worlds = new Dictionary<string, IWorld>();

        /// <summary>
        /// lua表
        /// </summary>
        /// <value></value>
        public LuaTable table { get; }

        /// <summary>
        /// 天空盒
        /// </summary>
        /// <value></value>
        public ISkybox skybox
        {
            get;
        }

        /// <summary>
        /// 是否激活
        /// </summary>
        /// <value></value>
        public bool activeSelf
        {
            get => active;
            set => SetActive(value);
        }
        /// <summary>
        /// 名称
        /// </summary>
        /// <value></value>
        public string name
        {
            get;
        }

        /// <summary>
        /// UI管理器
        /// </summary>
        /// <value></value>
        public IUIManager UIManager
        {
            get;
        }

        /// <summary>
        /// 音效管理器
        /// </summary>
        /// <value></value>
        public IAudioManager AudioManager
        {
            get;
        }

        /// <summary>
        /// 当前激活的
        /// </summary>
        /// <value></value>
        public static IWorld current
        {
            get;
            private set;
        }

        public Camera WorldCamera { get; }

        public MapGrid Map { get; private set; }

        public InputManager input { get; }


        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="name"></param>
        public GameWorld(string name) : this(name, null)
        {

        }

        public GameWorld(string name, LuaTable table)
        {
            this.name = name;
            input = new InputManager();
            WorldCamera = GameObject.Instantiate<Camera>(Resources.Load<Camera>("Camera/Main Camera"));
            WorldCamera.name = name + "_Camera";
            WorldCamera.gameObject.SetParent(Utility.EmptyTransform, new Vector3(0, 5, 0), new Vector3(45, 0, 0));
            this.entitys = new List<IEntity>();
            this.contexts = new List<Context>();
            this.runnables = new List<IScriptble>();
            this.skybox = new CommonWorldSkybox(this);
            this.UIManager = new UIManager(this);
            this.AudioManager = new AudioManager(this);

            this.activeSelf = true;
            this.table = table;

            if (table == null)
            {
                return;
            }
            dispose = table.Get<LuaFunction>("dispose");
            setActive = table.Get<LuaFunction>("active");
            fixedUpdate = table.Get<LuaFunction>("fixed");
            update = table.Get<LuaFunction>("update");
            lateUpdate = table.Get<LuaFunction>("late");
            LuaFunction start = table.Get<LuaFunction>("start");
            start.Call(table, this);
        }
        /// <summary>
        /// 创建世界
        /// </summary>
        /// <param name="name">世界名称</param>
        /// <returns></returns>
        public static IWorld CreateWorld(string name)
        {
            if (worlds.TryGetValue(name, out IWorld world))
            {
                return world;
            }
            world = new GameWorld(name);
            worlds.Add(name, world);
            return world;
        }

        /// <summary>
        /// 创建世界
        /// </summary>
        /// <param name="table">世界名称</param>
        /// <returns></returns>
        public static IWorld CreateWorld(LuaTable table)
        {
            string name = table.Get<string>("name");
            if (worlds.TryGetValue(name, out IWorld world))
            {
                return world;
            }
            world = new GameWorld(name, table);
            worlds.Add(name, world);
            return world;
        }

        /// <summary>
        /// 获取指定的世界
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static IWorld GetWorld(string name)
        {
            if (worlds.TryGetValue(name, out IWorld world))
            {
                return world;
            }
            return default;
        }

        /// <summary>
        /// 移除指定的世界
        /// </summary>
        /// <param name="name"></param>
        public static void RemoveWorld(string name)
        {
            if (worlds.TryGetValue(name, out IWorld world))
            {
                world.Dispose();
                worlds.Remove(name);
            }
        }

        /// <summary>
        /// 轮询
        /// </summary>
        public void FixedUpdate()
        {
            if (!active)
            {
                return;
            }
            if (fixedUpdate != null)
            {
                fixedUpdate.Call(table, this);
            }
            skybox.FixedUpdate();
            UIManager.FixedUpdate();
            AudioManager.FixedUpdate();
        }

        public void Update()
        {
            if (!active)
            {
                return;
            }
            input.Update();
            if (update != null)
            {
                update.Call(table, this);
            }
            UpdateScript();
        }

        public void LateUpdate()
        {
            if (!active)
            {
                return;
            }
            if (lateUpdate != null)
            {
                lateUpdate.Call(table, this);
            }
        }

        /// <summary>
        /// 创建寻路器
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="walls"></param>
        public void CreateMapPath(float gridSize, int gridCount, List<bool> walls)
        {
            Map = new MapGrid();
            Map.Initialize(gridSize, gridCount, walls);
        }

        /// <summary>
        /// 创建一个游戏实体对象
        /// </summary>
        /// <returns></returns>
        public IEntity CreateEntity()
        {
            return CreateEntity(string.Empty, Guid.NewGuid().ToString());
        }

        /// <summary>
        /// 创建一个游戏实体对象
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public IEntity CreateEntity(string name)
        {
            return CreateEntity(name, Guid.NewGuid().ToString().ToLower());
        }

        /// <summary>
        /// 创建一个游戏实体对象
        /// </summary>
        /// <param name="name">实体名称</param>
        /// <param name="guid">实体唯一ID</param>
        /// <param name="contextName">连接的实体资源</param>
        /// <returns></returns>
        public IEntity CreateEntity(string name, string guid)
        {
            guid = guid.Replace("-", "");
            if (HasEntity(guid))
            {
                throw new Exception("the entity guid is already exist");
            }
            GameEntity entity = new GameEntity(this, name, guid);
            entitys.Add(entity);
            return entity;
        }

        /// <summary>
        /// 加载逻辑模块
        /// </summary>
        /// <param name="scriptble"></param>
        public void AddScriptble(IScriptble scriptble)
        {
            runnables.Add(scriptble);
        }

        /// <summary>
        /// 加载逻辑模块
        /// </summary>
        /// <param name="table"></param>
        public void AddLuaScriptble(LuaTable table)
        {
            AddScriptble(new LuaScriptbleAdapter(table));
        }

        /// <summary>
        /// 释放当前世界
        /// </summary>
        public void Dispose()
        {
            if (dispose != null)
            {
                dispose.Call(table, this);
            }
            UIManager.Dispose();
            AudioManager.Dispose();
            skybox.Dispose();
            if (current == this)
            {
                current = null;
            }
            for (var i = 0; i < entitys.Count; i++)
            {
                entitys[i].Dispose();
            }
            entitys.Clear();
            foreach (var item in runnables)
            {
                item.Dispose();
            }
            runnables.Clear();
            dispose = null;
            fixedUpdate = null;
            setActive = null;
            table.Dispose();
        }

        /// <summary>
        /// 获取所有实体对象
        /// </summary>
        /// <returns></returns>
        public List<IEntity> GetEntitys()
        {
            return entitys;//.Cast<IEntity>();
        }

        /// <summary>
        /// 获取所有实体对象
        /// </summary>
        /// <returns></returns>
        public Context[] GetEntitys(int tags)
        {
            return contexts.Where(x => x.Contains(tags) && x.Count > 0).ToArray();
        }

        internal void INTERNAL_EntityChangeComponent(IEntity entity, int oldTag)
        {
            contexts.Find(x => x.Contains(oldTag))?.Remove(entity);
            var context = contexts.Find(x => x.tag == entity.tag);
            if (context == null)
            {
                context = new Context(entity.tag);
                contexts.Add(context);
            }
            context.Add(entity);
        }

        /// <summary>
        /// 获取指定的实体对象
        /// </summary>
        /// <param name="guid">实体唯一ID</param>
        /// <returns></returns>
        public IEntity GetEntity(string guid)
        {
            guid = guid.Replace("-", "");
            return entitys.Find(x => x.guid == guid);
        }

        /// <summary>
        /// 通过实体名称获取实体对象
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public IEntity GetEntityWithName(string name)
        {
            return entitys.Find(x => x.name == name);
        }

        /// <summary>
        /// 是否存在实体对象
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public bool HasEntity(string guid)
        {
            guid = guid.Replace("-", "");
            return GetEntity(guid) != null;
        }

        /// <summary>
        /// 移除实体对象
        /// </summary>
        /// <param name="guid"></param>
        public void RemoveEntity(string guid)
        {
            guid = guid.Replace("-", "");
            IEntity entity = GetEntity(guid);
            if (entity == null)
            {
                return;
            }
            contexts.ForEach(x => x.Remove(entity));
            entitys.Remove(entity);
            entity.Dispose();
        }

        /// <summary>
        /// 设置可见性
        /// </summary>
        /// <param name="active"></param>
        public void SetActive(bool active)
        {
            this.active = active;
            UIManager.SetActive(active);
            AudioManager.SetActive(active);
            skybox.SetActive(active);
            if (setActive != null)
            {
                setActive.Call(table, this);
            }
            if (!active)
            {
                return;
            }
            if (current != null && current != this)
            {
                current.SetActive(false);
                current = null;
            }
            current = this;
        }

        private void UpdateScript()
        {
            for (var i = 0; i < runnables.Count; i++)
            {
                runnables[i].Executed(this);
            }
        }
    }
}