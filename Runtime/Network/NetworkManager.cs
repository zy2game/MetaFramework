using System.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BestHTTP;

namespace GameFramework.Runtime.Network
{
    /// <summary>
    /// 网络管理器
    /// </summary>
    public sealed class NetworkManager : Singleton<NetworkManager>, INetworkManager
    {
        private Dictionary<string, IChannel> channels;

        public NetworkManager()
        {
            channels = new Dictionary<string, IChannel>();
        }

        /// <summary>
        /// 连接远程服务器
        /// </summary>
        /// <param name="name">连接名</param>
        /// <param name="url">远程地址</param>
        /// <param name="channelHandler">处理器</param>
        /// <typeparam name="T">连接类型</typeparam>
        public async void Connect<T>(string name, string url, IChannelHandler channelHandler) where T : IChannel
        {
            if (channels.TryGetValue(name, out IChannel channel))
            {
                await channel.Disconnect();
                if (channel.active)
                {
                    throw new Exception("the channel close error");
                }
                channels.Remove(name);
            }
            WebSocketChannel webSocketChannel = new WebSocketChannel();
            await webSocketChannel.Connect(name, url, channelHandler);
            if (webSocketChannel.active)
            {
                channels.Add(name, webSocketChannel);
            }
        }

        public void ConnectWebSocket(string name, string url, IChannelHandler channelHandler)
        {
            Connect<WebSocketChannel>(name, url, channelHandler);
        }

        /// <summary>
        /// 断开指定的连接
        /// </summary>
        /// <param name="name"></param>
        public async void Disconnect(string name)
        {
            if (channels.TryGetValue(name, out IChannel channel))
            {
                await channel.Disconnect();
                if (channel.active)
                {
                    throw new Exception("the channel close error");
                }
                channels.Remove(name);
            }
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        /// <returns></returns>
        public async void Dispose()
        {
            Task[] tasks = new Task[channels.Count];
            IChannel[] temps = channels.Values.ToArray();
            for (var i = 0; i < temps.Length; i++)
            {
                tasks[i] = temps[i].Disconnect();
            }
            await Task.WhenAll(tasks);
            channels.Clear();
        }

        public void Update()
        {
        }

        /// <summary>
        /// 请求web数据
        /// </summary>
        /// <param name="url">远程地址</param>
        /// <returns>响应数据</returns>
        public string Request(string url)
        {
            return GenerateHttpWebReponse(url, "GET", null, null);
        }

        /// <summary>
        /// 请求web数据
        /// </summary>
        /// <param name="url">远程地址</param>
        /// <typeparam name="T">响应数据类型</typeparam>
        /// <returns>响应数据</returns>
        public T Request<T>(string url)
        {
            string response = Request(url);
            if (string.IsNullOrEmpty(response))
            {
                return default;
            }
            return CatJson.JsonParser.ParseJson<T>(response);
        }

        /// <summary>
        /// 请求web数据
        /// </summary>
        /// <param name="url">远程地址</param>
        /// <returns>响应数据</returns>
        public Task<string> RequestAsync(string url)
        {
            return GenerateHttpWebReponseAsync(url, "GET", null, null);
        }

        /// <summary>
        /// 请求web数据
        /// </summary>
        /// <param name="url">远程地址</param>
        /// <typeparam name="T">响应数据类型</typeparam>
        /// <returns>响应数据</returns>
        public async Task<T> RequestAsync<T>(string url)
        {
            string response = await RequestAsync(url);
            if (string.IsNullOrEmpty(response))
            {
                return default;
            }
            return CatJson.JsonParser.ParseJson<T>(response);
        }

        /// <summary>
        /// 提交表单数据
        /// </summary>
        /// <param name="url">远程地址</param>
        /// <param name="header">标头数据</param>
        /// <param name="data">提交数据</param>
        /// <returns>响应数据</returns>
        public string Post(string url, Dictionary<string, string> header, Dictionary<string, object> data)
        {
            return GenerateHttpWebReponse(url, "POST", header, data);
        }

        /// <summary>
        /// 提交表单数据
        /// </summary>
        /// <param name="url">远程地址</param>
        /// <param name="header">标头数据</param>
        /// <param name="data">提交数据</param>
        /// <typeparam name="T">响应数据类型</typeparam>
        /// <returns>响应数据</returns>
        public T Post<T>(string url, Dictionary<string, string> header, Dictionary<string, object> data)
        {
            string response = Post(url, header, data);
            if (string.IsNullOrEmpty(response))
            {
                return default;
            }
            return CatJson.JsonParser.ParseJson<T>(response);
        }

        /// <summary>
        /// 提交表单数据
        /// </summary>
        /// <param name="url">远程地址</param>
        /// <param name="header">标头数据</param>
        /// <param name="data">提交数据</param>
        /// <returns>响应数据</returns>
        public Task<string> PostAsync(string url, Dictionary<string, string> header, Dictionary<string, object> data)
        {
            return GenerateHttpWebReponseAsync(url, "POST", header, data);
        }

        /// <summary>
        /// 提交表单数据
        /// </summary>
        /// <param name="url">远程地址</param>
        /// <param name="header">标头数据</param>
        /// <param name="data">提交数据</param>
        /// <typeparam name="T">响应数据类型</typeparam>
        /// <returns>响应数据</returns>
        public async Task<T> PostAsync<T>(string url, Dictionary<string, string> header, Dictionary<string, object> data)
        {
            string response = await PostAsync(url, header, data);
            if (string.IsNullOrEmpty(response))
            {
                return default;
            }
            return CatJson.JsonParser.ParseJson<T>(response);
        }

        /// <summary>
        /// 生成同步web请求对象
        /// </summary>
        /// <param name="url">远程地址</param>
        /// <param name="method">操作类型</param>
        /// <param name="header">表头</param>
        /// <param name="data">数据</param>
        /// <returns>响应数据</returns>
        private string GenerateHttpWebReponse(string url, string method, Dictionary<string, string> header, Dictionary<string, object> data)
        {
            HttpWebRequest request = WebRequest.CreateHttp(url);
            request.Method = method;
            request.Proxy = null;
            request.Timeout = 5000;
            request.KeepAlive = false;
            request.ServicePoint.Expect100Continue = false;
            request.ServicePoint.UseNagleAlgorithm = false;
            request.ServicePoint.ConnectionLimit = 65500;
            request.AllowWriteStreamBuffering = false;
            if (header != null && header.Count > 0)
            {
                foreach (var item in header)
                {
                    request.Headers.Add(item.Key, item.Value);
                }
            }
            if (data != null && data.Count > 0)
            {
                string postData = CatJson.JsonParser.ToJson(data);
                using (Stream stream = request.GetRequestStream())
                {
                    stream.Write(UTF8Encoding.UTF8.GetBytes(postData));
                }
            }
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                using (Stream resposeStream = response.GetResponseStream())
                {
                    using (StreamReader reader = new StreamReader(resposeStream))
                    {
                        string result = reader.ReadToEnd();
                        request.Abort();
                        return result;
                    }
                }
            }
        }

        /// <summary>
        /// 生成异步web请求对象
        /// </summary>
        /// <param name="url">远程地址</param>
        /// <param name="method">操作类型</param>
        /// <param name="header">表头</param>
        /// <param name="data">数据</param>
        /// <returns>响应数据</returns>
        private async Task<string> GenerateHttpWebReponseAsync(string url, string method, Dictionary<string, string> header, Dictionary<string, object> data)
        {
            HttpWebRequest request = WebRequest.CreateHttp(url);
            request.Method = method;
            request.Proxy = null;
            request.Timeout = 5000;
            request.KeepAlive = false;
            request.ServicePoint.Expect100Continue = false;
            request.ServicePoint.UseNagleAlgorithm = false;
            request.ServicePoint.ConnectionLimit = 65500;
            request.AllowWriteStreamBuffering = false;
            if (header != null && header.Count > 0)
            {
                foreach (var item in header)
                {
                    request.Headers.Add(item.Key, item.Value);
                }
            }
            if (data != null && data.Count > 0)
            {
                string postData = CatJson.JsonParser.ToJson(data);
                using (Stream stream = request.GetRequestStream())
                {
                    await stream.WriteAsync(UTF8Encoding.UTF8.GetBytes(postData));
                }
            }
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                using (Stream resposeStream = response.GetResponseStream())
                {
                    using (StreamReader reader = new StreamReader(resposeStream))
                    {
                        string result = await reader.ReadToEndAsync();
                        request.Abort();
                        return result;
                    }
                }
            }
        }
    }
}

