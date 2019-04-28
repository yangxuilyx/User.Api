using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Contact.API.Data;
using Contact.API.Models;
using Contact.API.Service;
using Contact.API.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Contact.API.Controllers
{
    [Route("api/[controller]")]
    public class ContactController : BaseController
    {
        private IContactApplyRequestRepository _contactApplyRequestRepository;
        private IContactRepository _contactRepository;

        private IUserService _userService;

        public ContactController(IContactApplyRequestRepository contactApplyRequestRepository, IUserService userService, IContactRepository contactRepository)
        {
            _contactApplyRequestRepository = contactApplyRequestRepository;
            _userService = userService;
            _contactRepository = contactRepository;
        }

        [HttpGet]
        [Route("")]
        public async Task<IActionResult> Get(CancellationToken cancellationToken)
        {
            return Ok(await _contactRepository.GetContactsAsync(UserIdentity.UserId, cancellationToken));
        }

        public async Task<IActionResult> TagContact([FromBody]TagContactInputViewModel viewModel, CancellationToken cancellationToken)
        {
            var result = await _contactRepository.TagContactAsync(UserIdentity.UserId, viewModel.ContactId, viewModel.Tags, cancellationToken);
            if (!result)
                return BadRequest();

            return Ok();
        }

        /// <summary>
        /// 获取好友申请列表
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("apply-requests")]
        public async Task<IActionResult> GetApplyRequests(CancellationToken cancellationToken)
        {
            return Ok(await _contactApplyRequestRepository.GetRequestList(UserIdentity.UserId, cancellationToken));
        }

        /// <summary>
        /// 添加好友请求
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("apply-requests")]
        public async Task<IActionResult> AddApplyRequest(int userId, CancellationToken cancellationToken)
        {
            var userInfo = await _userService.GetBaseUserInfoAsync(userId);

            var result = await _contactApplyRequestRepository.AddRequestAsync(new ContactApplyRequest()
            {
                UserId = userInfo.UserId,
                Name = userInfo.Name,
                Company = userInfo.Company,
                Title = userInfo.Title,
                PhoneNumber = userInfo.PhoneNumber,
                Avatar = userInfo.Avatar,
                ApplierId = UserIdentity.UserId,

                ApplyTime = DateTime.Now
            }, cancellationToken);

            if (!result)
                return BadRequest();

            return Ok();

        }

        /// <summary>
        /// 通过好友请求
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("apply-requests")]
        public async Task<IActionResult> ApprovalApplyRequest(int applierId, CancellationToken cancellationToken)
        {
            var result = await _contactApplyRequestRepository.ApprovalAsync(UserIdentity.UserId, applierId, cancellationToken);

            if (!result)
                return BadRequest();

            var userInfo = await _userService.GetBaseUserInfoAsync(applierId);
            await _contactRepository.AddContactAsync(UserIdentity.UserId, userInfo, cancellationToken);

            var user = await _userService.GetBaseUserInfoAsync(UserIdentity.UserId);
            await _contactRepository.AddContactAsync(applierId, user, cancellationToken);

            return Ok();
        }
    }
}