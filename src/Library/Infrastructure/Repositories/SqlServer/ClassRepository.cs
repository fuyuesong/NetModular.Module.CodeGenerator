﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NetModular.Lib.Data.Abstractions;
using NetModular.Lib.Data.Core;
using NetModular.Lib.Data.Query;
using NetModular.Module.Admin.Domain.Account;
using NetModular.Module.CodeGenerator.Domain.Class;
using NetModular.Module.CodeGenerator.Domain.Class.Models;

namespace NetModular.Module.CodeGenerator.Infrastructure.Repositories.SqlServer
{
    public class ClassRepository : RepositoryAbstract<ClassEntity>, IClassRepository
    {
        public ClassRepository(IDbContext context) : base(context)
        {
        }

        public async Task<IList<ClassEntity>> Query(ClassQueryModel model)
        {
            var paging = model.Paging();
            var query = Db.Find(m => m.ProjectId == model.ProjectId);
            query.WhereNotNull(model.Name, m => m.Name.Contains(model.Name) || m.Remarks.Contains(model.Name));

            var joinQuery = query.LeftJoin<AccountEntity>((x, y) => x.CreatedBy == y.Id);

            if (!paging.OrderBy.Any())
            {
                joinQuery.OrderByDescending((x, y) => x.Id);
            }

            joinQuery.Select((x, y) => new { x, Creator = y.Name });

            var list = await joinQuery.PaginationAsync(paging);
            model.TotalCount = paging.TotalCount;
            return list;
        }

        public Task<IList<ClassEntity>> QueryAllByProject(Guid projectId)
        {
            return Db.Find(m => m.ProjectId == projectId).ToListAsync();
        }

        public Task<bool> Exists(ClassEntity entity)
        {
            return Db.Find(m => m.ProjectId == entity.ProjectId && m.Name.Equals(entity.Name))
                .WhereNotEmpty(entity.Id, m => m.Id != entity.Id)
                .ExistsAsync();
        }

        public Task<bool> DeleteByProject(Guid projectId, IUnitOfWork uow)
        {
            return Db.Find(m => m.ProjectId == projectId).UseUow(uow).DeleteAsync();
        }
    }
}
