using XLua;

namespace GameFramework.Runtime.Data
{
    public sealed class LuaDatable : IGameDatable
    {
        public string group { get; private set; }

        public string guid { get; private set; }
        private LuaTable table;

        private LuaFunction _dispose;



        public void Dispose()
        {
            if (_dispose == null)
            {
                return;
            }
            _dispose.Call(table);
        }

        public void Init(object data)
        {
            table = (LuaTable)data;
            if (table == null)
            {
                return;
            }
            _dispose = table.Get<LuaFunction>("dispose");
            LuaFunction _init = table.Get<LuaFunction>("init");
            if (_init == null)
            {
                return;
            }
            _init.Call(table);
        }
    }
}
