﻿using Atles.Data;
using Atles.Domain.Users;
using Atles.Domain.Users.Commands;
using Microsoft.EntityFrameworkCore;
using OpenCqrs.Commands;
using System.Data;
using System.Threading.Tasks;

namespace Atles.Domain.Handlers.Users.Commands
{
    public class DeleteUserHandler : ICommandHandler<DeleteUser>
    {
        private readonly AtlesDbContext _dbContext;

        public DeleteUserHandler(AtlesDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task Handle(DeleteUser command)
        {
            var user = await _dbContext.Users
                .FirstOrDefaultAsync(x =>
                    x.Id == command.Id &&
                    x.IdentityUserId == command.IdentityUserId &&
                    x.Status != UserStatusType.Deleted);

            if (user == null)
            {
                throw new DataException($"User with Id {command.Id} not found.");
            }

            user.Delete();

            _dbContext.Events.Add(new Event(command.SiteId,
                command.UserId,
                EventType.Deleted,
                typeof(User),
                command.Id));

            await _dbContext.SaveChangesAsync();
        }
    }
}