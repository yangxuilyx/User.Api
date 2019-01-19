using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Resilience
{
    /// <summary>
    /// 自定义请求接口
    /// </summary>
    public interface IHttpClient
    {
        /// <summary>
        /// Post请求
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url"></param>
        /// <param name="item"></param>
        /// <param name="authorizationToken"></param>
        /// <param name="requestId"></param>
        /// <param name="authorizationMethod"></param>
        /// <returns></returns>
        Task<HttpResponseMessage> PostAsync<T>(string url, T item, string authorizationToken = null, string requestId = null,
            string authorizationMethod = "Bearer");

        Task<HttpResponseMessage> PostAsync(string url, Dictionary<string, string> item, string authorizationToken = null, string requestId = null,
            string authorizationMethod = "Bearer");
    }
}