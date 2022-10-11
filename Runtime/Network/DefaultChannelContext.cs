using System;
using System.Threading.Tasks;

namespace GameFramework.Runtime.Network
{
    public sealed class DefaultChannelContext : IChannelContext
    {
        public IChannel Channel { get; private set; }

        public DefaultChannelContext(IChannel channel)
        {
            this.Channel = channel;
        }

        public void Dispose()
        {
            Channel = null;
        }

        public void Flush()
        {
            if (!Channel.active)
            {
                return;
            }
            Channel.Flush();
        }

        public async Task WriteAndFlushAsync(byte[] stream)
        {
            Task sendTask = WriteAsync(stream);
            Flush();
            await sendTask;
        }

        public Task WriteAsync(byte[] stream)
        {
            if (!Channel.active)
            {
                return Task.FromException(new Exception("the channel is not connected"));
            }
            return Channel.WriteAsync(stream);
        }
    }
}

