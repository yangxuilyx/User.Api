using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace User.Identity.Services
{
    public class UserService : IUserService
    {
        private readonly string _userServiceUrl = "http://localhost:5001/";
        private HttpClient _httpClient;

        public UserService(HttpClient httpClient)
        {
            _httpClient = httpClient;
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