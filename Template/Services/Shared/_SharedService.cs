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

        //public async Task<User> GetUserByName(string userName)
        //{
        //    return await _dbContext.Users.FirstOrDefaultAsync(x => x.Email == userName);
        //}

        //public async Task<List<Event>> GetRequestByTeam(string teamName)
        //{
        //    return await _dbContext.Events
        //        .Where(e => e.TeamName == teamName)
        //        .ToListAsync();
        //}

    }
}
