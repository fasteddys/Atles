﻿using Atles.Data;
using Atles.Domain.Users;
using Atles.Models.Public;
using Atles.Reporting.Handlers.Services;
using Atles.Reporting.Public.Queries;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OpenCqrs;
using OpenCqrs.Queries;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Atles.Reporting.Handlers.Public
{
    public class GetCurrentUserHandler : IQueryHandler<GetCurrentUser, CurrentUserModel>
    {
        private readonly AtlesDbContext _dbContext;
        private readonly ISender _sender;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IGravatarService _gravatarService;

        public GetCurrentUserHandler(AtlesDbContext dbContext,
                                     ISender sender,
                                     IHttpContextAccessor httpContextAccessor,
                                     IGravatarService gravatarService)
        {
            _dbContext = dbContext;
            _sender = sender;
            _httpContextAccessor = httpContextAccessor;
            _gravatarService = gravatarService;
        }

        public async Task<CurrentUserModel> Handle(GetCurrentUser query)
        {
            var result = new CurrentUserModel();

            var claimsPrincipal = _httpContextAccessor.HttpContext.User;

            if (claimsPrincipal.Identity.IsAuthenticated)
            {
                var identityUserId = _httpContextAccessor.HttpContext.User.Identities.First().Claims
                    .FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;

                if (!string.IsNullOrEmpty(identityUserId))
                {
                    var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.IdentityUserId == identityUserId);

                    if (user != null)
                    {
                        result = new CurrentUserModel
                        {
                            Id = user.Id,
                            IdentityUserId = user.IdentityUserId,
                            Email = user.Email,
                            DisplayName = user.DisplayName,
                            GravatarHash = _gravatarService.GenerateEmailHash(user.Email),
                            IsSuspended = user.Status == UserStatusType.Suspended,
                            IsAuthenticated = true
                        };
                    }
                }
            }

            return result;
        }
    }
}