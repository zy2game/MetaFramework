using System;
using XLua;

namespace GameFramework.Runtime.Network
{
    /// <summary>
    /// Web Socket 消息处理管道
    /// </summary>
    public sealed class WebSocketChannelHandleAdapter : IChannelHandler
    {
        private LuaTable luaScript;
        private LuaFunction _channelActived;
        private LuaFunction _channelInactived;
        private LuaFunction _channelReaded;
        private LuaFunction _channelErrored;
        private LuaFunction _dispose;
        private bool active;
        private bool isDisposed;
        public WebSocketChannelHandleAdapter(LuaTable table)
        {
            luaScript = table;
            _channelActived = luaScript.Get<LuaFunction>("channelActive");
            _channelInactived = luaScript.Get<LuaFunction>("channelInactive");
            _channelErrored = luaScript.Get<LuaFunction>("channelErrored");
            _channelReaded = luaScript.Get<LuaFunction>("channelReaded");
            _dispose = luaScript.Get<LuaFunction>("dispose");
        }

        /// <summary>
        /// 网络连接成功
        /// </summary>
        /// <param name="context"></param>
        public void ChannelActive(IChannelContext context)
        {
            if (active)
            {
                return;
            }
            active = true;
            if (_channelActived == null)
            {
                return;
            }
            _channelActived.Call(luaScript, context);
        }

        /// <summary>
        /// 错误消息
        /// </summary>
        /// <param name="context"></param>
        /// <param name="info"></param>
        public void ChannelErrored(IChannelContext context, string info)
        {
            if (_channelErrored == null)
            {
                return;
            }
            _channelErrored.Call(luaScript, context, info);
        }

        /// <summary>
        /// 网络关闭
        /// </summary>
        /// <param name="context"></param>
        public void ChannelInactive(IChannelContext context)
        {
            if (!active)
            {
                return;
            }
            active = false;
            if (_channelInactived == null)
            {
                return;
            }
            _channelInactived.Call(luaScript, context);
        }

        /// <summary>
        /// 消息处理
        /// </summary>
        /// <param name="context"></param>
        /// <param name="message"></param>
        public void ChannelRead(IChannelContext context, byte[] message)
        {
            if (!active)
            {
                return;
            }
            if (_channelReaded == null)
            {
                return;
            }
            short opcode = BitConverter.ToInt16(message, 0);
            byte[] bytes = new byte[message.Length - sizeof(short)];
            Array.Copy(message, sizeof(short), bytes, 0, bytes.Length);
            _channelReaded.Call(luaScript, context, opcode, bytes);
        }

        /// <summary>
        /// 释放管道
        /// </summary>
        public void Dispose()
        {
            if (isDisposed)
            {
                return;
            }
            isDisposed = true;
            _channelActived.Dispose();
            _channelActived = null;
            _channelErrored.Dispose();
            _channelErrored = null;
            _channelInactived.Dispose();
            _channelInactived = null;
            _channelReaded.Dispose();
            _channelReaded = null;
            luaScript.Dispose();
            if (_dispose == null)
            {
                return;
            }
            _dispose.Call(luaScript);
            _dispose = null;
        }
    }
}

