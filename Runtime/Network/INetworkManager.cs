using System.Collections.Generic;
using System.Threading.Tasks;

namespace GameFramework.Runtime.Network
{
    /// <summary>
    /// 网络管理器
    /// </summary>
    public interface INetworkManager : GModule
    {
        /// <summary>
        /// 连接网络
        /// </summary>
        /// <param name="name">连接名</param>
        /// <param name="url">连接地址</param>
        /// <param name="channelHandler">消息管道</param>
        /// <typeparam name="T">管道类型</typeparam>
        void Connect<T>(string name, string url, IChannelHandler channelHandler) where T : IChannel;

        /// <summary>
        /// 断开连接
        /// </summary>
        /// <param name="name">连接名</param>
        /// <returns></returns>
        void Disconnect(string name);

        /// <summary>
        /// 请求web数据
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <returns>响应结果</returns>
        string Request(string url);

        /// <summary>
        /// 请求web数据
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <typeparam name="T">响应结果类型</typeparam>
        /// <returns>响应结果</returns>
        T Request<T>(string url);

        /// <summary>
        /// 请求web数据
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <returns>响应结果</returns>
        Task<string> RequestAsync(string url);

        /// <summary>
        /// 请求web数据
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <typeparam name="T">响应结果类型</typeparam>
        /// <returns>响应结果</returns>
        Task<T> RequestAsync<T>(string url);

        /// <summary>
        /// 提交数据
        /// </summary>
        /// <param name="url">提交地址</param>
        /// <param name="header">标头</param>
        /// <param name="data">提交的数据</param>
        /// <returns>响应结果</returns>
        string Post(string url, Dictionary<string, string> header, Dictionary<string, object> data);

        /// <summary>
        /// 提交数据
        /// </summary>
        /// <param name="url">提交地址</param>
        /// <param name="header">标头</param>
        /// <param name="data">提交的数据</param>
        /// <typeparam name="T">响应结果类型</typeparam>
        /// <returns>响应结果</returns>
        T Post<T>(string url, Dictionary<string, string> header, Dictionary<string, object> data);

        /// <summary>
        /// 提交数据
        /// </summary>
        /// <param name="url">提交地址</param>
        /// <param name="header">标头</param>
        /// <param name="data">提交的数据</param>
        /// <returns>响应结果</returns>
        Task<string> PostAsync(string url, Dictionary<string, string> header, Dictionary<string, object> data);

        /// <summary>
        /// 提交数据
        /// </summary>
        /// <param name="url">提交地址</param>
        /// <param name="header">标头</param>
        /// <param name="data">提交的数据</param>
        /// <typeparam name="T">响应结果类型</typeparam>
        /// <returns>响应结果</returns>
        Task<T> PostAsync<T>(string url, Dictionary<string, string> header, Dictionary<string, object> data);
    }
}

