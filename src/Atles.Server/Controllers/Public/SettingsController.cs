﻿using System.Threading.Tasks;
using Atles.Domain.Models.Users.Commands;
using Atles.Domain.Models.Users.Rules;
using Atles.Infrastructure;
using Atles.Reporting.Models.Public;
using Atles.Reporting.Models.Public.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Atles.Server.Controllers.Public
{
    [Authorize]
    [Route("api/public/settings")]
    public class SettingsController : SiteControllerBase
    {
        private readonly IDispatcher _dispatcher;
        private readonly ILogger<SettingsController> _logger;

        public SettingsController(IDispatcher dispatcher, ILogger<SettingsController> logger) : base(dispatcher)
        {
            _dispatcher = dispatcher;
            _logger = logger;
        }

        [HttpGet("edit")]
        public async Task<ActionResult<SettingsPageModel>> Edit()
        {
            var site = await CurrentSite();
            var user = await CurrentUser();

            var model = await _dispatcher.Get(new GetSettingsPage { SiteId = site.Id, UserId = user.Id });

            return model;
        }

        [HttpPost("update")]
        public async Task<ActionResult> Update(SettingsPageModel model)
        {
            var site = await CurrentSite();
            var user = await CurrentUser();

            if (model.User.Id != user.Id || user.IsSuspended)
            {
                _logger.LogWarning("Unauthorized access to update settings.", new
                {
                    SiteId = site.Id,
                    UserId = model.User?.Id,
                    User = User.Identity.Name
                });

                return Unauthorized();
            }

            var command = new UpdateUser
            {
                Id = user.Id,
                DisplayName = model.User.DisplayName,
                SiteId = site.Id,
                UserId = user.Id
            };

            await _dispatcher.Send(command);

            return Ok();
        }

        [HttpGet("is-display-name-unique/{name}")]
        public async Task<IActionResult> IsDisplayNameUnique(string name)
        {
            var isNameUnique = await _dispatcher.Get(new IsUserDisplayNameUnique { DisplayName = name });
            return Ok(isNameUnique);
        }
    }
}
