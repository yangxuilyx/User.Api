using System.Threading.Tasks;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Validation;
using User.Identity.Services;

namespace User.Identity.Authentication
{
    public class SmsAuthCodeValidator : IExtensionGrantValidator
    {
        private readonly IAuthService _authService;
        private readonly IUserService _userService;

        public SmsAuthCodeValidator(IAuthService authService, IUserService userService)
        {
            _authService = authService;
            _userService = userService;
        }

        public async Task ValidateAsync(ExtensionGrantValidationContext context)
        {
            var phone = context.Request.Raw.Get("phone");
            var authCode = context.Request.Raw.Get("auth_code");
            var errorResult = new GrantValidationResult(TokenRequestErrors.InvalidGrant);

            if (phone.IsNullOrEmpty() || authCode.IsNullOrEmpty())
            {
                context.Result = errorResult;
                return;
            }

            if (!_authService.Validate(phone, authCode))
            {
                context.Result = errorResult;
                return;
            }

            var userId = await _userService.CheckOrCreate(phone);

            if (userId <= 0)
            {
                context.Result = errorResult;
                return;
            }

            context.Result = new GrantValidationResult(userId.ToString(), GrantType);
            return;
        }

        public string GrantType => "sms_auth_code";
    }
}