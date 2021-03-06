﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using DnsClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Resilience;
using User.Identity.Dtos;

namespace User.Identity.Services
{
    public class UserService : IUserService
    {
        private readonly string _userServiceUrl;
        private readonly IHttpClient _httpClient;
        private readonly ILogger<UserService> _logger;

        public UserService(IHttpClient httpClient, IDnsQuery dnsQuery, IOptions<ServiceDiscoveryOptions> serviceOptions, ILogger<UserService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;

            var address = dnsQuery.ResolveService("service.consul", serviceOptions.Value.UserServiceName);
            var addressList = address.First().AddressList;

            var host = addressList.Any() ? addressList.First().ToString() : address.First().HostName;
            var port = address.First().Port;

            _userServiceUrl = $"http://{host}:{port}/";
        }

        public async Task<int> CheckOrCreate(string phone)
        {
            var form = new Dictionary<string, string>()
            {
                {"phone", phone}
            };

            try
            {
                var response = await _httpClient.PostAsync(_userServiceUrl + "api/user/check-or-create", form);

                if (response.IsSuccessStatusCode)
                {
                    var userId = await response.Content.ReadAsStringAsync();

                    int.TryParse(userId, out int intUserId);

                    return intUserId;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("CheckOrCreate 在重试之后失败，" + ex.StackTrace);

                throw ex;
            }


            return 0;
        }
    }
}