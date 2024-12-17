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
            public TimeSpan OraInizio { get; set; }
            public TimeSpan OraFine { get; set; }
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
    }
}
