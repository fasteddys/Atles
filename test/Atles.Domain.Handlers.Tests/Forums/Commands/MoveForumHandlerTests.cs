﻿using System;
using System.Threading.Tasks;
using Atles.Data;
using Atles.Data.Caching;
using Atles.Domain.Handlers.Forums.Commands;
using Atles.Domain.Models;
using Atles.Domain.Models.Categories;
using Atles.Domain.Models.Forums;
using Atles.Domain.Models.Forums.Commands;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;

namespace Atles.Domain.Handlers.Tests.Forums.Commands
{
    [TestFixture]
    public class MoveForumHandlerTests : TestFixtureBase
    {
        [Test]
        public async Task Should_move_forum_up_and_add_events()
        {
            var options = Shared.CreateContextOptions();

            var categoryId = Guid.NewGuid();
            var siteId = Guid.NewGuid();

            var category = new Category(categoryId, siteId, "Category", 1, Guid.NewGuid());

            var forum1 = new Forum(categoryId, "Forum 1", "my-forum-1", "Forum 1", 1);
            var forum2 = new Forum(categoryId, "Forum 2", "my-forum-2", "Forum 2", 2);

            using (var dbContext = new AtlesDbContext(options))
            {
                dbContext.Categories.Add(category);
                dbContext.Forums.Add(forum1);
                dbContext.Forums.Add(forum2);
                await dbContext.SaveChangesAsync();
            }

            using (var dbContext = new AtlesDbContext(options))
            {
                var command = new MoveForum
                {
                    Id = forum2.Id,
                    Direction = Direction.Up,
                    SiteId = siteId
                };

                var cacheManager = new Mock<ICacheManager>();

                var sut = new MoveForumHandler(dbContext,
                    cacheManager.Object);

                await sut.Handle(command);

                var updatedForum1 = await dbContext.Forums.FirstOrDefaultAsync(x => x.Id == forum1.Id);
                var updatedForum2 = await dbContext.Forums.FirstOrDefaultAsync(x => x.Id == forum2.Id);

                var event1 = await dbContext.Events.FirstOrDefaultAsync(x => x.TargetId == forum1.Id);
                var event2 = await dbContext.Events.FirstOrDefaultAsync(x => x.TargetId == forum2.Id);

                Assert.AreEqual(forum2.SortOrder, updatedForum1.SortOrder);
                Assert.AreEqual(forum1.SortOrder, updatedForum2.SortOrder);
                Assert.NotNull(event1);
                Assert.NotNull(event2);
            }
        }

        [Test]
        public async Task Should_move_forum_down_and_add_events()
        {
            var options = Shared.CreateContextOptions();

            var categoryId = Guid.NewGuid();
            var siteId = Guid.NewGuid();

            var category = new Category(categoryId, siteId, "Category", 1, Guid.NewGuid());

            var forum1 = new Forum(categoryId, "Forum 1", "my-forum-1", "Forum 1", 1);
            var forum2 = new Forum(categoryId, "Forum 2", "my-forum-2", "Forum 2", 2);

            using (var dbContext = new AtlesDbContext(options))
            {
                dbContext.Categories.Add(category);
                dbContext.Forums.Add(forum1);
                dbContext.Forums.Add(forum2);
                await dbContext.SaveChangesAsync();
            }

            using (var dbContext = new AtlesDbContext(options))
            {
                var command = new MoveForum
                {
                    Id = forum1.Id,
                    Direction = Direction.Down,
                    SiteId = siteId
                };

                var cacheManager = new Mock<ICacheManager>();
                var createValidator = new Mock<IValidator<CreateForum>>();
                var updateValidator = new Mock<IValidator<UpdateForum>>();

                var sut = new MoveForumHandler(dbContext,
                    cacheManager.Object);

                await sut.Handle(command);

                var updatedForum1 = await dbContext.Forums.FirstOrDefaultAsync(x => x.Id == forum1.Id);
                var updatedForum2 = await dbContext.Forums.FirstOrDefaultAsync(x => x.Id == forum2.Id);

                var event1 = await dbContext.Events.FirstOrDefaultAsync(x => x.TargetId == forum1.Id);
                var event2 = await dbContext.Events.FirstOrDefaultAsync(x => x.TargetId == forum2.Id);

                Assert.AreEqual(forum2.SortOrder, updatedForum1.SortOrder);
                Assert.AreEqual(forum1.SortOrder, updatedForum2.SortOrder);
                Assert.NotNull(event1);
                Assert.NotNull(event2);
            }
        }
    }
}
