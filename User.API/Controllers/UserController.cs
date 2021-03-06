﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
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
                .Include(p => p.Properties)
                .FirstOrDefaultAsync(p => p.Id == UserIdentity.UserId);

            if (user == null)
                throw new UserException($"错误的用户上下文Id：{UserIdentity.UserId}");

            return user;
        }

        [HttpPatch]
        public async Task<ActionResult<AppUser>> Patch([FromBody] JsonPatchDocument<AppUser> patch)
        {
            var user = await _userContext.AppUsers.FirstOrDefaultAsync(p => p.Id == UserIdentity.UserId);

            patch.ApplyTo(user);

            var userProperties = await _userContext.UserProperties.Where(p => p.AppUserId == UserIdentity.UserId).ToListAsync();

            _userContext.UserProperties.RemoveRange(userProperties);

            _userContext.Update(user);
            await _userContext.SaveChangesAsync();

            return user;
        }

        [HttpPost]
        [Route("check-or-create")]
        public async Task<ActionResult> CheckOrCreate([FromForm]string phone)
        {
            var appUser = await _userContext.AppUsers.SingleOrDefaultAsync(u => u.PhoneNumber == phone);
            if (appUser == null)
            {
                appUser = new AppUser()
                {
                    PhoneNumber = phone
                };
                _userContext.AppUsers.Add(appUser);

                await _userContext.SaveChangesAsync();
            }
            return Ok(appUser.Id);
        }

        [HttpGet]
        [Route("tags")]
        public async Task<IActionResult> GetUserTags()
        {
            return Ok(await _userContext.UserTags.Where(u => u.UserId == UserIdentity.UserId).ToListAsync());
        }


        [HttpPost]
        [Route("search")]
        public async Task<IActionResult> Search(string phone)
        {
            return Ok(await _userContext.AppUsers.Include(p => p.Properties)
                .SingleOrDefaultAsync(p => p.Id == UserIdentity.UserId));
        }

        [HttpPut]
        [Route("tags")]
        public async Task<IActionResult> UpdateUserTags([FromBody] List<string> tags)
        {
            var userTags = await _userContext.UserTags.Where(p=>p.UserId == UserIdentity.UserId).ToListAsync();

            var newTags = tags.Except(userTags.Select(p=>p.Tag));

            await _userContext.UserTags.AddRangeAsync(newTags.Select(p=>new UserTag()
            {
                UserId = UserIdentity.UserId,
                Tag = p
            }));
            await _userContext.SaveChangesAsync();

            return Ok();
        }
    }
}