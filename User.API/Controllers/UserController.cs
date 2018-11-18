using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using User.API.Data;
using User.API.Models;

namespace User.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : UserBaseController
    {
        private readonly UserContext _userContext;

        public UserController(UserContext userContext)
        {
            _userContext = userContext;
        }

        public async Task<ActionResult<AppUser>> Get()
        {
            var user = await _userContext.AppUsers.AsNoTracking()
                .Include(p=>p.Properties)
                .FirstOrDefaultAsync(p=>p.Id == UserIdentity.UserId);

            if (user == null)
                return NotFound();

            return user;
        }
    }
}