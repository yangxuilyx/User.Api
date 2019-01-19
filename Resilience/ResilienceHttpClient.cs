using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Polly;
using Polly.Wrap;

namespace Resilience
{
    public class ResilienceHttpClient : IHttpClient
    {
        private readonly HttpClient _httpClient;

        // 根据url origin 创建policy
        private readonly Func<string, IEnumerable<Policy>> _policyCreator;

        // 把policy打包成组合policy wraper，进行本地缓存
        private readonly ConcurrentDictionary<string, PolicyWrap> _policyWrappers;
        private ILogger<ResilienceHttpClient> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ResilienceHttpClient(Func<string, IEnumerable<Policy>> policyCreator, ILogger<ResilienceHttpClient> logger, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = new HttpClient();
            _policyCreator = policyCreator;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _policyWrappers = new ConcurrentDictionary<string, PolicyWrap>();
        }


        public async Task<HttpResponseMessage> PostAsync<T>(string url, T item, string authorizationToken = null, string requestId = null,
            string authorizationMethod = "Bearer")
        {
            return await DoPostAsync(HttpMethod.Post, url, () => GetHttpRequestMessage(HttpMethod.Post, url, item), authorizationToken, requestId, authorizationMethod);
        }

        public async Task<HttpResponseMessage> PostAsync(string url, Dictionary<string, string> item, string authorizationToken = null, string requestId = null,
            string authorizationMethod = "Bearer")
        {
            return await DoPostAsync(HttpMethod.Post, url, () => GetHttpRequestMessage(HttpMethod.Post, url, item), authorizationToken, requestId, authorizationMethod);
        }

        private HttpRequestMessage GetHttpRequestMessage<T>(HttpMethod method, string url, T item)
        {
            return new HttpRequestMessage(method, url)
            {
                Content = new StringContent(JsonConvert.SerializeObject(item), Encoding.UTF8, "application/json")
            };
        }


        private HttpRequestMessage GetHttpRequestMessage(HttpMethod method, string url, Dictionary<string, string> form)
        {
            return new HttpRequestMessage(method, url)
            {
                Content = new FormUrlEncodedContent(form)
            };
        }

        public Task<HttpResponseMessage> DoPostAsync(HttpMethod method, string url, Func<HttpRequestMessage> requestMessageAction, string authorizationToken, string requestId = null,
            string authorizationMethod = "Bearer")
        {
            if (method != HttpMethod.Post && method != HttpMethod.Put)
            {
                throw new ArgumentException("value must be either post or put", nameof(method));
            }

            var origin = GetOriginFromUri(url);

            return HttpInvoker(origin, async () =>
            {
                var requestMessage = requestMessageAction();

                SetAuthorizationHeader(requestMessage);

                if (authorizationToken != null)
                {
                    requestMessage.Headers.Authorization = new AuthenticationHeaderValue(authorizationMethod, authorizationToken);
                }

                if (requestId != null)
                {
                    requestMessage.Headers.Add("x-requestId", requestId);
                }

                var responseMessage = await _httpClient.SendAsync(requestMessage);

                if (!responseMessage.IsSuccessStatusCode)
                {
                    throw new HttpRequestException();
                }

                return responseMessage;
            });
        }

        private async Task<T> HttpInvoker<T>(string origin, Func<Task<T>> action)
        {
            var normalizeOrigin = NormalizeOrigin(origin);
            if (!_policyWrappers.TryGetValue(normalizeOrigin, out var policyWrap))
            {
                policyWrap = Policy.WrapAsync(_policyCreator(normalizeOrigin).ToArray());
                _policyWrappers.TryAdd(normalizeOrigin, policyWrap);
            }

            return await policyWrap.ExecuteAsync(action);
        }

        private static string NormalizeOrigin(string origin)
        {
            return origin?.Trim()?.ToLower();
        }

        private static string GetOriginFromUri(string uri)
        {
            var url = new Uri(uri);

            var origin = $"{url.Scheme}://{url.DnsSafeHost}:{url.Port}";

            return origin;
        }

        private void SetAuthorizationHeader(HttpRequestMessage requestMessage)
        {
            var authorizationHeader = _httpContextAccessor.HttpContext.Request.Headers["Authorization"];

            if (!string.IsNullOrEmpty(authorizationHeader))
            {
                requestMessage.Headers.Add("Authorization", new List<string>() { authorizationHeader });
            }
        }


    }
}