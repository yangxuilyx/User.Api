using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using DnsClient;
using Microsoft.Extensions.Options;
using User.Identity.Dtos;

namespace User.Identity.Services
{
    public class UserService : IUserService
    {
        private readonly string _userServiceUrl;
        private HttpClient _httpClient;

        public UserService(HttpClient httpClient, IDnsQuery dnsQuery, IOptions<ServiceDiscoveryOptions> serviceOptions)
        {
             _httpClient = httpClient;

            var address = dnsQuery.ResolveService("service.consul", serviceOptions.Value.UserServiceName);
            var addressList = address.First().AddressList;

            var host = addressList.Any() ? addressList.First().ToString() : address.First().HostName;
            var port = address.First().Port;

            _userServiceUrl = $"http://{host}:{port}/";

        }

    public async Task<int> CheckOrCreate(string phone)
    {
        var formUrlEncodedContent = new FormUrlEncodedContent(new Dictionary<string, string>()
            {
                {"phone", phone}
            });
        var response = await _httpClient.PostAsync(_userServiceUrl + "api/user/check-or-create", formUrlEncodedContent);

        if (response.IsSuccessStatusCode)
        {
            var userId = await response.Content.ReadAsStringAsync();

            int.TryParse(userId, out int intUserId);

            return intUserId;
        }

        return 0;
    }
}
}