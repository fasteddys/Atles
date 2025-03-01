﻿using System;
using Atles.Domain.Models.Categories.Commands;
using Atles.Domain.Models.Categories.Rules;
using Atles.Domain.Models.PermissionSets.Rules;
using Atles.Domain.Validators.Categories;
using Atles.Infrastructure;
using Atles.Infrastructure.Queries;
using AutoFixture;
using FluentValidation.TestHelper;
using Moq;
using NUnit.Framework;

namespace Atles.Domain.Validators.Tests.Categories
{
    [TestFixture]
    public class CreateCategoryValidatorTests : TestFixtureBase
    {
        [Test]
        public void Should_have_validation_error_when_name_is_empty()
        {
            var command = Fixture.Build<CreateCategory>().With(x => x.Name, string.Empty).Create();

            var dispatcher = new Mock<IDispatcher>();

            var sut = new CreateCategoryValidator(dispatcher.Object);

            sut.ShouldHaveValidationErrorFor(x => x.Name, command);
        }

        [Test]
        public void Should_have_validation_error_when_name_is_too_long()
        {
            var command = Fixture.Build<CreateCategory>().With(x => x.Name, new string('*', 51)).Create();

            var dispatcher = new Mock<IDispatcher>();

            var sut = new CreateCategoryValidator(dispatcher.Object);

            sut.ShouldHaveValidationErrorFor(x => x.Name, command);
        }

        [Test]
        public void Should_have_validation_error_when_name_is_not_unique()
        {
            var command = Fixture.Create<CreateCategory>();

            var dispatcher = new Mock<IDispatcher>();
            dispatcher.Setup(x => x.Get(It.IsAny<IsCategoryNameUnique>())).ReturnsAsync(false);

            var sut = new CreateCategoryValidator(dispatcher.Object);

            sut.ShouldHaveValidationErrorFor(x => x.Name, command);
        }

        [Test]
        public void Should_have_validation_error_when_permission_set_is_not_valid()
        {
            var command = Fixture.Create<CreateCategory>();

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

            var sut = new CreateCategoryValidator(dispatcher.Object);

            sut.ShouldHaveValidationErrorFor(x => x.PermissionSetId, command);
            Assert.AreEqual(command.SiteId, querySiteId);
            Assert.AreEqual(command.PermissionSetId, queryPermissionSetId);
        }
    }
}
