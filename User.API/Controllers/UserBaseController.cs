using Microsoft.AspNetCore.Mvc;
using User.API.Dtos;

namespace User.API.Controllers
{
    public class UserBaseController : ControllerBase
    {
        protected UserIdentity UserIdentity => new UserIdentity() { UserId = 1, Name = "jesse" };
    }
}