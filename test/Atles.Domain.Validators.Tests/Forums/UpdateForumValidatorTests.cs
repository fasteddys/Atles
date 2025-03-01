﻿using System;
using Atles.Domain.Models.Forums.Commands;
using Atles.Domain.Models.Forums.Rules;
using Atles.Domain.Models.PermissionSets.Rules;
using Atles.Domain.Validators.Forums;
using Atles.Infrastructure;
using Atles.Infrastructure.Queries;
using AutoFixture;
using FluentValidation.TestHelper;
using Moq;
using NUnit.Framework;

namespace Atles.Domain.Validators.Tests.Forums
{
    [TestFixture]
    public class UpdateForumValidatorTests : TestFixtureBase
    {
        [Test]
        public void Should_have_validation_error_when_name_is_empty()
        {
            var command = Fixture.Build<UpdateForum>().With(x => x.Name, string.Empty).Create();

            var dispatcher = new Mock<IDispatcher>();

            var sut = new UpdateForumValidator(dispatcher.Object);

            sut.ShouldHaveValidationErrorFor(x => x.Name, command);
        }

        [Test]
        public void Should_have_validation_error_when_name_is_too_long()
        {
            var command = Fixture.Build<UpdateForum>().With(x => x.Name, new string('*', 51)).Create();

            var dispatcher = new Mock<IDispatcher>();

            var sut = new UpdateForumValidator(dispatcher.Object);

            sut.ShouldHaveValidationErrorFor(x => x.Name, command);
        }

        [Test]
        public void Should_have_validation_error_when_name_is_not_unique()
        {
            var command = Fixture.Create<UpdateForum>();

            var dispatcher = new Mock<IDispatcher>();
            dispatcher.Setup(x => x.Get(new IsForumNameUnique { SiteId = command.SiteId, CategoryId = command.CategoryId, Name = command.Name, Id = command.Id })).ReturnsAsync(false);

            var sut = new UpdateForumValidator(dispatcher.Object);

            sut.ShouldHaveValidationErrorFor(x => x.Name, command);
        }

        [Test]
        public void Should_have_validation_error_when_slug_is_empty()
        {
            var command = Fixture.Build<UpdateForum>().With(x => x.Slug, string.Empty).Create();

            var dispatcher = new Mock<IDispatcher>();

            var sut = new UpdateForumValidator(dispatcher.Object);

            sut.ShouldHaveValidationErrorFor(x => x.Slug, command);
        }

        [Test]
        public void Should_have_validation_error_when_slug_is_too_long()
        {
            var command = Fixture.Build<UpdateForum>().With(x => x.Slug, new string('*', 51)).Create();

            var dispatcher = new Mock<IDispatcher>();

            var sut = new UpdateForumValidator(dispatcher.Object);

            sut.ShouldHaveValidationErrorFor(x => x.Slug, command);
        }

        [Test]
        public void Should_have_validation_error_when_slug_is_not_unique()
        {
            var command = Fixture.Create<UpdateForum>();

            var dispatcher = new Mock<IDispatcher>();
            dispatcher.Setup(x => x.Get(new IsForumSlugUnique { SiteId = command.SiteId, Slug = command.Slug, Id = command.Id })).ReturnsAsync(false);

            var sut = new UpdateForumValidator(dispatcher.Object);

            sut.ShouldHaveValidationErrorFor(x => x.Slug, command);
        }

        [Test]
        public void Should_have_validation_error_when_description_is_too_long()
        {
            var command = Fixture.Build<UpdateForum>().With(x => x.Description, new string('*', 201)).Create();

            var dispatcher = new Mock<IDispatcher>();

            var sut = new UpdateForumValidator(dispatcher.Object);

            sut.ShouldHaveValidationErrorFor(x => x.Description, command);
        }

        [Test]
        public void Should_have_validation_error_when_permission_set_is_not_valid()
        {
            var command = Fixture.Create<UpdateForum>();

            var querySiteId = Guid.NewGuid();
            var queryPermissionSetId = Guid.NewGuid();

            var dispatcher = new Mock<IDispatcher>();
            dispatcher
                .Setup(x => x.Get(It.IsAny<IsPermissionSetValid>()))
                .Callback<IQuery<bool>>(q =>
                {
                    var query = q as IsPermissionSetValid;
                    querySiteId = query.SiteId;
                    queryPermissionSetId = query.Id;
                })
                .ReturnsAsync(false);

            var sut = new UpdateForumValidator(dispatcher.Object);

            sut.ShouldHaveValidationErrorFor(x => x.PermissionSetId, command);
            Assert.AreEqual(command.SiteId, querySiteId);
            Assert.AreEqual(command.PermissionSetId.Value, queryPermissionSetId);
        }
    }
}
