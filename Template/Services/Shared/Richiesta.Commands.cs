using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Template.Services.Shared
{
    public class AddRequestCommand
    {
        public Guid? Id { get; set; }
        public string Tipologia { get; set; }
        public DateTime DataInizio { get; set; }
        public DateTime DataFine { get; set; }
        public TimeSpan OraInizio { get; set; }  
        public TimeSpan OraFine { get; set; }
        public string UserName { get; set; }
        public string Stato { get; set; }
    }

    public partial class SharedService
    {
        public async Task<Guid> HandleRequest(AddRequestCommand cmd)
        {
            var request = new Request
            {
                UserName = cmd.UserName,
                Tipologia = cmd.Tipologia,
                DataInizio = cmd.DataInizio,
                DataFine = cmd.DataFine,
                OraInizio = cmd.OraInizio,
                OraFine = cmd.OraFine,
                Stato = cmd.Stato
            };
            _dbContext.Requests.Add(request);
            await _dbContext.SaveChangesAsync();

            return request.Id;
        }

        public async Task<bool> UpdateStatus(Guid id, string stato)
        {
            var richiesta = await _dbContext.Requests.FindAsync(id);
            if (richiesta == null) return false;

            richiesta.Stato = stato; 
            await _dbContext.SaveChangesAsync();
            return true;
        }


    }
}
