using System;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Polly;
using Resilience;

namespace User.Identity.Infrastructure
{
    public class ResilienceClientFactory
    {
        private readonly ILogger<ResilienceHttpClient> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        // 重试次数
        private readonly int _retryCount;

        // 熔断重试之前允许的异常次数
        private readonly int _exceptionCountAllowedBeforeBreaking;

        public ResilienceClientFactory(ILogger<ResilienceHttpClient> logger, IHttpContextAccessor httpContextAccessor, int retryCount, int exceptionCountAllowedBeforeBreaking)
        {
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _retryCount = retryCount;
            _exceptionCountAllowedBeforeBreaking = exceptionCountAllowedBeforeBreaking;
        }

        public ResilienceHttpClient GetResilienceHttpClient() => new ResilienceHttpClient(origin => CreatePolicy(origin), _logger, _httpContextAccessor);

        private Policy[] CreatePolicy(string origin)
        {
            return new Policy[]
            {
                // 重试
                Policy.Handle<HttpRequestException>()
                    .WaitAndRetryAsync(_retryCount,retryAttempt=>TimeSpan.FromSeconds(Math.Pow(2,retryAttempt)),
                        (exception, timeSpan, retryCount, context) =>
                        {
                            var msg = $"第{retryCount}次重试" +
                                      $"of {context.PolicyKey}" +
                                      $"at {context.OperationKey}" +
                                      $"due to {exception}.";

                            _logger.LogWarning(msg);
                            _logger.LogDebug(msg);
                        }),

                // 熔断
                Policy.Handle<HttpRequestException>().CircuitBreakerAsync(_exceptionCountAllowedBeforeBreaking,TimeSpan.FromMinutes(1),
                    (exception, duration) =>
                    {
                        _logger.LogTrace("熔断器打开");
                    }, () =>
                    {
                        _logger.LogTrace("熔断器关闭");
                    })
            };
        }
    }
}