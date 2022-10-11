using System.IO;
using System.Linq;
using System.Collections.Generic;
using System;
using UnityEngine;
using XLua;

namespace GameFramework.Runtime.Game
{
    /// <summary>
    /// Game Entity Object
    /// </summary>
    public sealed class GameEntity : IEntity
    {
        private bool isDispose;
        private List<ICompnent> compnents = new List<ICompnent>();



        /// <summary>
        /// Entity Name
        /// </summary>
        /// <value></value>
        public string name { get; }

        /// <summary>
        /// Entity ID
        /// </summary>
        /// <value></value>
        public string guid { get; }

        /// <summary>
        /// Êµï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½Ç?
        /// </summary>
        /// <value></value>
        public int tag { get; private set; }

        /// <summary>
        /// owner form world
        /// </summary>
        /// <value></value>
        public IWorld world { get; private set; }

        public string path { get; }


        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="name"></param>
        /// <param name="guid"></param>
        public GameEntity(IWorld world, string name, string guid)
        {
            this.name = Path.GetFileName(name);
            this.path = name;
            this.guid = guid;
            this.world = world;
        }

        /// <summary>
        /// reslease the game entity
        /// </summary>
        public void Dispose()
        {
            if (isDispose)
            {
                return;
            }
            isDispose = true;
            foreach (var item in compnents)
            {
                item.Dispose();
            }
            compnents.Clear();
            world = null;
            Debug.Log($"dispose game entity:{name}_{guid}");
        }

        public LuaTable AddComponent(LuaTable table)
        {
            LuaComponentAdapter adapter = new LuaComponentAdapter(table);
            AddComponent(adapter);
            return adapter.table;
        }


        /// <summary>
        /// add a component to the entity
        /// </summary>
        /// <param name="compnent">component</param>
        public ICompnent AddComponent(ICompnent compnent)
        {
            ICompnent temp = GetComponent(compnent.tag);
            if (temp != null)
            {
                throw new Exception("the component is already exsit");
            }
            int oldTag = tag;
            this.tag = this.tag | compnent.tag;
            compnents.Add(compnent);
            compnent.Awake(this);
            ((GameWorld)world).INTERNAL_EntityChangeComponent(this, oldTag);
            return compnent;
        }

        /// <summary>
        /// remove component for the entity
        /// </summary>
        /// <param name="tag">component tag</param>
        public void RemoveComponent(int tag)
        {
            ICompnent compnent = GetComponent(tag);
            if (compnent == null)
            {
                return;
            }
            int oldTag = tag;
            this.tag = this.tag & ~compnent.tag;
            compnent.Dispose();
            compnents.Remove(compnent);
            ((GameWorld)world).INTERNAL_EntityChangeComponent(this, oldTag);
        }

        public ICompnent GetComponent(int tag)
        {
            return compnents.Find(x => x.tag == tag);
        }

        /// <summary>
        /// gets all component arrry for the entity
        /// </summary>
        /// <returns></returns>
        public ICompnent[] GetComponents()
        {
            return compnents.ToArray();
        }

        /// <summary>
        /// get component array for the entity
        /// </summary>
        /// <param name="tags">component tags</param>
        /// <returns></returns>
        public ICompnent[] GetComponents(params int[] tags)
        {
            if (tags == null || tags.Length <= 0)
            {
                return Array.Empty<ICompnent>();
            }

            ICompnent[] temp = new ICompnent[tags.Length];
            for (var i = 0; i < tags.Length; i++)
            {
                temp[i] = GetComponent(tags[i]);
                if (temp[i] == null)
                {
                    return Array.Empty<ICompnent>();
                }
            }
            return temp;
        }

        public LuaTable GetLuaComponent(int tag)
        {
            LuaComponentAdapter adapter = (LuaComponentAdapter)GetComponent(tag);
            if (adapter == null)
            {
                return default;
            }
            return adapter.table;
        }

        public LuaTable[] GetLuaComponents()
        {
            ICompnent[] compnents = GetComponents();
            LuaTable[] result = new LuaTable[compnents.Length];
            for (var i = 0; i < compnents.Length; i++)
            {
                LuaComponentAdapter adapter = (LuaComponentAdapter)compnents[i];
                if (adapter == null)
                {
                    continue;
                }
                result[i] = adapter.table;
            }
            return result;
        }

        public LuaTable[] GetLuaComponents(params int[] tags)
        {
            ICompnent[] compnents = GetComponents(tags);
            LuaTable[] result = new LuaTable[compnents.Length];
            for (var i = 0; i < compnents.Length; i++)
            {
                LuaComponentAdapter adapter = (LuaComponentAdapter)compnents[i];
                if (adapter == null)
                {
                    continue;
                }
                result[i] = adapter.table;
            }
            return result;
        }
    }
}