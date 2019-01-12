namespace User.Identity.Services
{
    public class TestAuthService:IAuthService
    {
        public bool Validate(string phone, string authCode)
        {
            return true;
        }
    }
}