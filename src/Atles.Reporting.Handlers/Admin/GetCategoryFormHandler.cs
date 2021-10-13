﻿using Atles.Data;
using Atles.Domain.Categories;
using Atles.Domain.PermissionSets;
using Atles.Models.Admin.Categories;
using Atles.Reporting.Admin.Categories;
using Microsoft.EntityFrameworkCore;
using OpenCqrs.Queries;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Atles.Reporting.Handlers.Admin.Categories
{
    public class GetCategoryFormHandler : IQueryHandler<GetCategoryForm, FormComponentModel>
    {
        private readonly AtlesDbContext _dbContext;

        public GetCategoryFormHandler(AtlesDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<FormComponentModel> Handle(GetCategoryForm query)
        {
            var result = new FormComponentModel();

            var permissionSets = await _dbContext.PermissionSets
                .Where(x => x.SiteId == query.SiteId && x.Status == PermissionSetStatusType.Published)
                .ToListAsync();

            if (query.Id != null)
            {
                var category = await _dbContext.Categories
                    .FirstOrDefaultAsync(x =>
                        x.SiteId == query.SiteId &&
                        x.Id == query.Id &&
                        x.Status != CategoryStatusType.Deleted);

                if (category == null)
                {
                    return null;
                }

                result.Category = new FormComponentModel.CategoryModel
                {
                    Id = category.Id,
                    Name = category.Name,
                    PermissionSetId = category.PermissionSetId
                };
            }
            else
            {
                result.Category = new FormComponentModel.CategoryModel
                {
                    PermissionSetId = permissionSets.FirstOrDefault()?.Id ?? Guid.Empty
                };
            }

            foreach (var permissionSet in permissionSets)
            {
                result.PermissionSets.Add(new FormComponentModel.PermissionSetModel
                {
                    Id = permissionSet.Id,
                    Name = permissionSet.Name
                });
            }

            return result;
        }
    }
}