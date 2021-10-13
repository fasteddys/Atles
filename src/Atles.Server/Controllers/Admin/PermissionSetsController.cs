﻿using System;
using System.Threading.Tasks;
using Atles.Domain.PermissionSets.Commands;
using Atles.Domain.PermissionSets.Rules;
using Atles.Models.Admin.PermissionSets;
using Atles.Models.Admin.PermissionSets.Queries;
using Atles.Server.Services;
using Microsoft.AspNetCore.Mvc;
using OpenCqrs;

namespace Atles.Server.Controllers.Admin
{
    [Route("api/admin/permission-sets")]
    public class PermissionSetsController : AdminControllerBase
    {
        private readonly IContextService _contextService;
        private readonly ISender _sender;

        public PermissionSetsController(IContextService contextService, ISender sender) : base(sender)
        {
            _contextService = contextService;
            _sender = sender;
        }

        [HttpGet("list")]
        public async Task<IndexPageModel> List()
        {
            var site = await CurrentSite();

            return await _sender.Send(new GetPermissionSetsIndex { SiteId = site.Id });
        }

        [HttpGet("create")]
        public async Task<FormComponentModel> Create()
        {
            var site = await CurrentSite();

            return await _sender.Send(new GetPermissionSetCreateForm { SiteId = site.Id });
        }

        [HttpPost("save")]
        public async Task<ActionResult> Save(FormComponentModel.PermissionSetModel model)
        {
            var site = await CurrentSite();
            var user = await _contextService.CurrentUserAsync();

            var command = new CreatePermissionSet
            {
                Name = model.Name,
                Permissions = model.Permissions.ToPermissionCommands(),
                SiteId = site.Id,
                UserId = user.Id
            };

            await _sender.Send(command);

            return Ok();
        }

        [HttpGet("edit/{id}")]
        public async Task<ActionResult<FormComponentModel>> Edit(Guid id)
        {
            var site = await CurrentSite();

            var result = await _sender.Send(new GetPermissionSetEditForm { SiteId = site.Id, Id = id });

            if (result == null)
            {
                return NotFound();
            }

            return result;
        }

        [HttpPost("update")]
        public async Task<ActionResult> Update(FormComponentModel.PermissionSetModel model)
        {
            var site = await CurrentSite();
            var user = await _contextService.CurrentUserAsync();

            var command = new UpdatePermissionSet
            {
                Id = model.Id,
                Name = model.Name,
                Permissions = model.Permissions.ToPermissionCommands(),
                SiteId = site.Id,
                UserId = user.Id
            };

            await _sender.Send(command);

            return Ok();
        }

        [HttpDelete("delete/{id}")]
        public async Task<ActionResult> Delete(Guid id)
        {
            var site = await CurrentSite();
            var user = await _contextService.CurrentUserAsync();

            var command = new DeletePermissionSet
            {
                Id = id,
                SiteId = site.Id,
                UserId = user.Id
            };

            await _sender.Send(command);

            return Ok();
        }

        [HttpGet("is-name-unique/{name}")]
        public async Task<IActionResult> IsNameUnique(string name)
        {
            var site = await CurrentSite();
            var query = new IsPermissionSetNameUnique { SiteId = site.Id, Name = name };
            var isNameUnique = await _sender.Send(query);
            return Ok(isNameUnique);
        }

        [HttpGet("is-name-unique/{name}/{id}")]
        public async Task<IActionResult> IsNameUnique(string name, Guid id)
        {
            var site = await CurrentSite();
            var query = new IsPermissionSetNameUnique { SiteId = site.Id, Name = name, Id = id };
            var isNameUnique = await _sender.Send(query);
            return Ok(isNameUnique);
        }
    }
}
