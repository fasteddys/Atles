﻿using System.Threading.Tasks;
using Atles.Reporting.Models.Admin.Users;
using Atles.Reporting.Models.Admin.Users.Queries;
using OpenCqrs.Queries;

namespace Atles.Reporting.Handlers.Admin
{
    public class GetUserCreateFormHandler : IQueryHandler<GetUserCreateForm, CreatePageModel>
    {
        public async Task<CreatePageModel> Handle(GetUserCreateForm request)
        {
            await Task.CompletedTask;

            var result = new CreatePageModel();

            return result;
        }
    }
}
