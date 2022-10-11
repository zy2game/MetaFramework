using System.Collections.Generic;
namespace GameFramework.Runtime.Config
{
    using CatJson;
    /// <summary>
    /// 配置表
    /// </summary>
    public interface IConfig : GObject
    {
        /// <summary>
        /// 配置ID
        /// </summary>
        /// <remarks>同一类型的配置的唯一ID</remarks>
        int id { get; }

        /// <summary>
        /// 配置名
        /// </summary>
        string name { get; }
    }

    public sealed class LuaConfig : IConfig
    {
        public int id
        {
            get;
            private set;
        }

        public string name
        {
            get;
            private set;
        }

        private JsonObject jObject;

        public LuaConfig(JsonObject json)
        {
            jObject = json;
            if (json["id"] != null)
            {
                id = (int)json["id"].Type;
            }
            if (json["name"] != null)
            {
                name = json["name"];
            }
        }

        public double GetNumber(string key)
        {
            if (jObject[key] == null)
            {
                throw new KeyNotFoundException(key);
            }
            return jObject[key];
        }

        public bool GetBoolean(string key)
        {
            if (jObject[key] == null)
            {
                throw new KeyNotFoundException(key);
            }
            return jObject[key];
        }

        public string GetString(string key)
        {
            if (jObject[key] == null)
            {
                throw new KeyNotFoundException(key);
            }
            return jObject[key];
        }

        public void Dispose()
        {
        }

    }
}
