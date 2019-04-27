using System.Threading.Tasks;
using Contact.API.Dtos;

namespace Contact.API.Service
{
    public interface IUserService
    {
        /// <summary>
        /// 获取用户信息
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<BaseUserInfo> GetBaseUserInfoAsync(int userId);
    }
}