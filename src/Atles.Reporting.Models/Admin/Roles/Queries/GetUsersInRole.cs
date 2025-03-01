﻿using System.Collections.Generic;
using Atles.Infrastructure.Queries;

namespace Atles.Reporting.Models.Admin.Roles.Queries
{
    public class GetUsersInRole : QueryBase<IList<IndexPageModel.UserModel>>
    {
        public string RoleName { get; set; }
    }
}
