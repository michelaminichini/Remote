using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Template.Services.Shared
{
    public class RichiesteIndexDTO
    {
        public IEnumerable<Request> Richieste { get; set; }

        public class Request
        {
            public Guid Id { get; set; }
            public string Tipologia { get; set; }
            public DateTime DataInizio { get; set; }
            public DateTime DataFine { get; set; }
            public TimeSpan? OraInizio { get; set; }
            public TimeSpan? OraFine { get; set; }
        }
    }

    public partial class SharedService
    {
        public async Task<RichiesteIndexDTO> Query()
        {
            var richieste = await _dbContext.Requests
                .Select(x => new RichiesteIndexDTO.Request
                {
                    Id = x.Id,
                    Tipologia = x.Tipologia,
                    DataInizio = x.DataInizio,
                    DataFine = x.DataFine,
                    OraInizio = x.OraInizio,
                    OraFine = x.OraFine
                })
                .ToArrayAsync(); 

            return new RichiesteIndexDTO
            {
                Richieste = richieste
            };
        }

        public async Task<List<Request>> GetUserRequest(string userEmail)
        {
            return await _dbContext.Requests
           .Where(r => r.UserName == userEmail) 
           .ToListAsync();
        }

        public async Task<List<Request>> GetManagerRequest(string teamName)
        {
            var richieste = await (from user in _dbContext.Users
                                   join request in _dbContext.Requests on user.Email equals request.UserName
                                   where user.TeamName == teamName
                                   select request).ToListAsync();
            return richieste;
        }

    }
}
