﻿using Atles.Domain.Categories.Commands;
using Atles.Domain.Categories.Rules;
using Atles.Domain.Categories.Validators;
using Atles.Domain.PermissionSets.Rules;
using AutoFixture;
using FluentValidation.TestHelper;
using Moq;
using NUnit.Framework;
using OpenCqrs.Queries;
using System;

namespace Atles.Domain.Tests.Categories.Validators
{
    [TestFixture]
    public class UpdateCategoryValidatorTests : TestFixtureBase
    {
        [Test]
        public void Should_have_validation_error_when_name_is_empty()
        {
            var command = Fixture.Build<UpdateCategory>().With(x => x.Name, string.Empty).Create();

            var querySender = new Mock<IQuerySender>();

            var sut = new UpdateCategoryValidator(querySender.Object);

            sut.ShouldHaveValidationErrorFor(x => x.Name, command);
        }

        [Test]
        public void Should_have_validation_error_when_name_is_too_long()
        {
            var command = Fixture.Build<UpdateCategory>().With(x => x.Name, new string('*', 51)).Create();

            var querySender = new Mock<IQuerySender>();

            var sut = new UpdateCategoryValidator(querySender.Object);

            sut.ShouldHaveValidationErrorFor(x => x.Name, command);
        }

        [Test]
        public void Should_have_validation_error_when_name_is_not_unique()
        {
            var command = Fixture.Create<UpdateCategory>();

            var querySender = new Mock<IQuerySender>();
            querySender.Setup(x => x.Send(It.IsAny<IsCategoryNameUnique>())).ReturnsAsync(false);

            var sut = new UpdateCategoryValidator(querySender.Object);

            sut.ShouldHaveValidationErrorFor(x => x.Name, command);
        }

        [Test]
        public void Should_have_validation_error_when_permission_set_is_not_valid()
        {
            var command = Fixture.Create<UpdateCategory>();

            var querySiteId = Guid.NewGuid();
            var queryPermissionSetId = Guid.NewGuid();

            var querySender = new Mock<IQuerySender>();
            querySender
                .Setup(x => x.Send(It.IsAny<IsPermissionSetValid>()))
                .Callback<IQuery<bool>>(q =>
                {
                    var query = q as IsPermissionSetValid;
                    querySiteId = query.SiteId;
                    queryPermissionSetId = query.Id;
                })
                .ReturnsAsync(false);

            var sut = new UpdateCategoryValidator(querySender.Object);

            sut.ShouldHaveValidationErrorFor(x => x.PermissionSetId, command);
            Assert.AreEqual(command.SiteId, querySiteId);
            Assert.AreEqual(command.PermissionSetId, queryPermissionSetId);
        }
    }
}
