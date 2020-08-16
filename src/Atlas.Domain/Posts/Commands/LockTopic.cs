﻿using System;

namespace Atlas.Domain.Posts.Commands
{
    public class LockTopic : CommandBase
    {
        public Guid Id { get; set; }
        public Guid ForumId { get; set; }
        public bool Locked { get; set; }
    }
}