﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Atlas.Domain.PermissionSets;
using Atlas.Models;
using Atlas.Models.Public;
using Atlas.Server.Services;
using Markdig;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Atlas.Server.Controllers.Public
{
    [Route("api/public")]
    [ApiController]
    public class IndexController : ControllerBase
    {
        private readonly IContextService _contextService;
        private readonly IPublicModelBuilder _modelBuilder;
        private readonly ISecurityService _securityService;
        private readonly IPermissionModelBuilder _permissionModelBuilder;

        public IndexController(IContextService contextService, 
            IPublicModelBuilder modelBuilder,
            ISecurityService securityService,
            IPermissionModelBuilder permissionModelBuilder)
        {
            _contextService = contextService;
            _modelBuilder = modelBuilder;
            _securityService = securityService;
            _permissionModelBuilder = permissionModelBuilder;
        }

        [HttpGet("index-model")]
        public async Task<IndexPageModel> Index()
        {
            var site = await _contextService.CurrentSiteAsync();

            var modelToFilter = await _modelBuilder.BuildPublishedForumsModelAsync(site.Id);

            var filteredModel = await GetFilteredIndexModel(site.Id, modelToFilter);

            return filteredModel;
        }

        private async Task<IndexPageModel> GetFilteredIndexModel(Guid siteId, IndexPageModel modelToFilter)
        {
            var result = new IndexPageModel();

            foreach (var categoryToFilter in modelToFilter.Categories)
            {
                var category = new IndexPageModel.CategoryModel { Name = categoryToFilter.Name };

                foreach (var forumToFilter in categoryToFilter.Forums)
                {
                    var permissionSetId = forumToFilter.PermissionSetId ?? categoryToFilter.PermissionSetId;
                    var permissions = await _permissionModelBuilder.BuildPermissionModels(siteId, permissionSetId);
                    var canViewForum = _securityService.HasPermission(PermissionType.ViewForum, permissions);
                    if (!canViewForum) continue;
                    var canViewTopics = _securityService.HasPermission(PermissionType.ViewTopics, permissions);
                    var forum = new IndexPageModel.ForumModel
                    {
                        Id = forumToFilter.Id,
                        Name = forumToFilter.Name,
                        Description = forumToFilter.Description,
                        TotalTopics = forumToFilter.TotalTopics,
                        TotalReplies = forumToFilter.TotalReplies,
                        LastTopicId = forumToFilter.LastTopicId,
                        LastTopicTitle = forumToFilter.LastTopicTitle,
                        LastPostTimeStamp = forumToFilter.LastPostTimeStamp,
                        LastPostMemberId = forumToFilter.LastPostMemberId,
                        LastPostMemberDisplayName = forumToFilter.LastPostMemberDisplayName,
                        CanViewTopics = canViewTopics
                    };
                    category.Forums.Add(forum);
                }

                result.Categories.Add(category);
            }

            return result;
        }

        [Authorize]
        [HttpPost("preview")]
        public async Task<string> Preview([FromBody]string content)
        {
            return await Task.FromResult(Markdown.ToHtml(content));
        }

        [HttpGet("current-site")]
        public async Task<CurrentSiteModel> CurrentSite()
        {
            return await _contextService.CurrentSiteAsync();
        }

        [Authorize]
        [HttpGet("current-member")]
        public async Task<CurrentMemberModel> CurrentMember()
        {
            return await _contextService.CurrentMemberAsync();
        }

        [HttpGet("search")]
        public async Task<SearchPageModel> Search([FromQuery] int page = 1, [FromQuery] string search = null)
        {
            var site = await _contextService.CurrentSiteAsync();

            var currentForums = await _contextService.CurrentForumsAsync();

            var accessibleForumIds = new List<Guid>();

            foreach (var forum in currentForums)
            {
                var permissions = await _permissionModelBuilder.BuildPermissionModels(site.Id, forum.PermissionSetId);
                var canViewForum = _securityService.HasPermission(PermissionType.ViewForum, permissions);
                var canViewTopics = _securityService.HasPermission(PermissionType.ViewTopics, permissions);
                var canViewRead = _securityService.HasPermission(PermissionType.Read, permissions);
                if (canViewForum && canViewTopics && canViewRead)
                {
                    accessibleForumIds.Add(forum.Id);
                }
            }

            var model = await _modelBuilder.BuildSearchPageModelAsync(site.Id, accessibleForumIds, new QueryOptions(search, page));

            return model;
        }
    }
}
