﻿using Atles.Infrastructure.Queries;
using Atles.Models.Public.Posts;
using System;

namespace Atles.Reporting.Public.Queries
{
    public class GetCreatePostPage : QueryBase<PostPageModel>
    {
        public Guid ForumId { get; set; }
    }
}
