﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Atles.Data;
using Atles.Data.Caching;
using Atles.Domain.Handlers.Posts.Commands;
using Atles.Domain.Handlers.Posts.Services;
using Atles.Domain.Models.Categories;
using Atles.Domain.Models.Forums;
using Atles.Domain.Models.Posts;
using Atles.Domain.Models.Posts.Commands;
using AutoFixture;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;

namespace Atles.Domain.Handlers.Tests.Posts.Commands
{
    [TestFixture]
    public class UpdateTopicHandlerTests : TestFixtureBase
    {
        [Test]
        public async Task Should_update_topic_and_add_event()
        {
            var options = Shared.CreateContextOptions();

            var siteId = Guid.NewGuid();
            var categoryId = Guid.NewGuid();
            var forumId = Guid.NewGuid();

            var category = new Category(categoryId, siteId, "Category", 1, Guid.NewGuid());
            var forum = new Forum(forumId, category.Id, "Forum", "my-forum", "My Forum", 1);
            var topic = Post.CreateTopic(forumId, Guid.NewGuid(), "Title", "slug", "Content", PostStatusType.Published);

            using (var dbContext = new AtlesDbContext(options))
            {
                dbContext.Categories.Add(category);
                dbContext.Forums.Add(forum);
                dbContext.Posts.Add(topic);

                await dbContext.SaveChangesAsync();
            }

            using (var dbContext = new AtlesDbContext(options))
            {
                var command = Fixture.Build<UpdateTopic>()
                    .With(x => x.Id, topic.Id)
                    .With(x => x.ForumId, forumId)
                    .With(x => x.SiteId, siteId)
                    .Create();

                var cacheManager = new Mock<ICacheManager>();

                var validator = new Mock<IValidator<UpdateTopic>>();
                validator
                    .Setup(x => x.ValidateAsync(command, new CancellationToken()))
                    .ReturnsAsync(new ValidationResult());

                var topicSlugGenerator = new Mock<ITopicSlugGenerator>();
                topicSlugGenerator
                    .Setup(x => x.GenerateTopicSlug(command.ForumId, command.Title))
                    .ReturnsAsync("slug");

                var sut = new UpdateTopicHandler(dbContext, validator.Object, cacheManager.Object, topicSlugGenerator.Object);

                await sut.Handle(command);

                var updatedTopic = await dbContext.Posts.FirstOrDefaultAsync(x => x.Id == command.Id);
                var @event = await dbContext.Events.FirstOrDefaultAsync(x => x.TargetId == command.Id);

                validator.Verify(x => x.ValidateAsync(command, new CancellationToken()));
                Assert.AreEqual(command.Title, updatedTopic.Title);
                Assert.NotNull(@event);
            }
        }
    }
}
