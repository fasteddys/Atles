﻿using System;
using System.Threading.Tasks;
using Atlas.Domain;
using Atlas.Server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Atlas.Domain.Forums.Commands;
using Atlas.Domain.Forums;
using Atlas.Shared.Admin.Forums;
using Atlas.Shared.Admin.Forums.Models;

namespace Atlas.Server.Controllers.Admin
{
    [Authorize(Policy = "Admin")]
    [Route("api/admin/forums")]
    [ApiController]
    public class ForumsController : ControllerBase
    {
        private readonly IContextService _contextService;
        private readonly IForumService _forumService;
        private readonly IForumRules _forumRules;
        private readonly IForumModelBuilder _modelBuilder;

        public ForumsController(IContextService contextService,
            IForumService forumService,
            IForumRules forumRules,
            IForumModelBuilder modelBuilder)
        {
            _contextService = contextService;
            _forumService = forumService;
            _forumRules = forumRules;
            _modelBuilder = modelBuilder;
        }

        [HttpGet("index-model")]
        public async Task<IndexPageModel> Index()
        {
            var site = await _contextService.CurrentSiteAsync();

            return await _modelBuilder.BuildIndexPageModelAsync(site.Id);
        }

        [HttpGet("index-model/{categoryId}")]
        public async Task<IndexPageModel> Index(Guid categoryId)
        {
            var site = await _contextService.CurrentSiteAsync();

            return await _modelBuilder.BuildIndexPageModelAsync(site.Id, categoryId);
        }

        [HttpGet("create")]
        public async Task<FormComponentModel> Create()
        {
            var site = await _contextService.CurrentSiteAsync();

            return await _modelBuilder.BuildCreateFormModelAsync(site.Id);
        }

        [HttpGet("create/{categoryId}")]
        public async Task<FormComponentModel> Create(Guid categoryId)
        {
            var site = await _contextService.CurrentSiteAsync();

            return await _modelBuilder.BuildCreateFormModelAsync(site.Id, categoryId);
        }

        [HttpPost("save")]
        public async Task<ActionResult> Save(FormComponentModel.ForumModel model)
        {
            var site = await _contextService.CurrentSiteAsync();
            var member = await _contextService.CurrentMemberAsync();

            var command = new CreateForum
            {
                CategoryId = model.CategoryId,
                Name = model.Name,
                PermissionSetId = model.PermissionSetId == Guid.Empty ? (Guid?)null : model.PermissionSetId,
                SiteId = site.Id,
                MemberId = member.Id
            };

            await _forumService.CreateAsync(command);

            return Ok();
        }

        [HttpGet("edit/{id}")]
        public async Task<ActionResult<FormComponentModel>> Edit(Guid id)
        {
            var site = await _contextService.CurrentSiteAsync();

            var result = await _modelBuilder.BuildEditFormModelAsync(site.Id, id);

            if (result == null)
            {
                return NotFound();
            }

            return result;
        }

        [HttpPost("update")]
        public async Task<ActionResult> Update(FormComponentModel.ForumModel model)
        {
            var site = await _contextService.CurrentSiteAsync();
            var member = await _contextService.CurrentMemberAsync();

            var command = new UpdateForum
            {
                Id = model.Id,
                CategoryId = model.CategoryId,
                Name = model.Name,
                PermissionSetId = model.PermissionSetId == Guid.Empty ? (Guid?)null : model.PermissionSetId,
                SiteId = site.Id,
                MemberId = member.Id
            };

            await _forumService.UpdateAsync(command);

            return Ok();
        }

        [HttpPost("move-up")]
        public async Task<ActionResult> MoveUp([FromBody] Guid id)
        {
            var site = await _contextService.CurrentSiteAsync();
            var member = await _contextService.CurrentMemberAsync();

            var command = new MoveForum
            {
                Id = id,
                SiteId = site.Id,
                MemberId = member.Id,
                Direction = Direction.Up
            };

            await _forumService.MoveAsync(command);

            return Ok();
        }

        [HttpPost("move-down")]
        public async Task<ActionResult> MoveDown([FromBody] Guid id)
        {
            var site = await _contextService.CurrentSiteAsync();
            var member = await _contextService.CurrentMemberAsync();

            var command = new MoveForum
            {
                Id = id,
                SiteId = site.Id,
                MemberId = member.Id,
                Direction = Direction.Down
            };

            await _forumService.MoveAsync(command);

            return Ok();
        }

        [HttpDelete("delete/{id}")]
        public async Task<ActionResult> Delete(Guid id)
        {
            var site = await _contextService.CurrentSiteAsync();
            var member = await _contextService.CurrentMemberAsync();

            var command = new DeleteForum
            {
                Id = id,
                SiteId = site.Id,
                MemberId = member.Id
            };

            await _forumService.DeleteAsync(command);

            return Ok();
        }

        [HttpGet("is-name-unique/{categoryId}/{name}")]
        public async Task<IActionResult> IsNameUnique(Guid categoryId, string name)
        {
            var site = await _contextService.CurrentSiteAsync();
            var isNameUnique = await _forumRules.IsNameUniqueAsync(categoryId, name);
            return Ok(isNameUnique);
        }

        [HttpGet("is-name-unique/{categoryId}/{name}/{id}")]
        public async Task<IActionResult> IsNameUnique(Guid categoryId, string name, Guid id)
        {
            var site = await _contextService.CurrentSiteAsync();
            var isNameUnique = await _forumRules.IsNameUniqueAsync(categoryId, name, id);
            return Ok(isNameUnique);
        }
    }
}
