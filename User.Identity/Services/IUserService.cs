using System.Threading.Tasks;

namespace User.Identity.Services
{
    public interface IUserService
    {
        /// <summary>
        /// 检查或创建用户
        /// </summary>
        /// <param name="phone"></param>
        /// <returns></returns>
        Task<int> CheckOrCreate(string phone);
    }
}