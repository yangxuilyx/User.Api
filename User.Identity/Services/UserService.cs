using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Resilience;

namespace User.Identity.Services
{
    public class UserService : IUserService
    {
        private readonly string _userServiceUrl = "http://localhost:5001/";
        private readonly IHttpClient _httpClient;
        private ILogger<UserService> _logger;

        public UserService(IHttpClient httpClient, ILogger<UserService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
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