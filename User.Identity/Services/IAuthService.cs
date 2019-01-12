namespace User.Identity.Services
{
    public interface IAuthService
    {
        /// <summary>
        /// 验证验证码
        /// </summary>
        /// <param name="phone"></param>
        /// <param name="authCode"></param>
        /// <returns></returns>
        bool Validate(string phone, string authCode);
    }
}