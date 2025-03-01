﻿using System.Threading;
using System.Threading.Tasks;
using Atles.Data;
using Atles.Domain.Handlers.Users.Commands;
using Atles.Domain.Models.Users.Commands;
using AutoFixture;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;

namespace Atles.Domain.Handlers.Tests.Users.Commands
{
    [TestFixture]
    public class CreateUserHandlerTests : TestFixtureBase
    {
        [Test]
        public async Task Should_create_new_user_and_add_event()
        {
            using (var dbContext = new AtlesDbContext(Shared.CreateContextOptions()))
            {
                var command = Fixture.Create<CreateUser>();

                var validator = new Mock<IValidator<CreateUser>>();
                validator
                    .Setup(x => x.ValidateAsync(command, new CancellationToken()))
                    .ReturnsAsync(new ValidationResult());

                var sut = new CreateUserHandler(dbContext, validator.Object);

                await sut.Handle(command);

                var user = await dbContext.Users.FirstOrDefaultAsync(x => x.Id == command.Id);
                var @event = await dbContext.Events.FirstOrDefaultAsync(x => x.TargetId == command.Id);

                validator.Verify(x => x.ValidateAsync(command, new CancellationToken()));
                Assert.NotNull(user);
                Assert.NotNull(@event);
            }
        }
    }
}
