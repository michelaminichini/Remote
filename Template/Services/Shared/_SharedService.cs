using System;
using System.Collections.Generic;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using System.Linq;
using Template.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Template.Services.Shared
{
    public partial class SharedService
    {
        private readonly TemplateDbContext _dbContext;

        public SharedService(TemplateDbContext dbContext)
        {
            _dbContext = dbContext;
        }
    }
}
