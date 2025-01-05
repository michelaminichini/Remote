using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Template.Infrastructure;

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
        public TimeSpan Durata { get; set; }
        public string UserName { get; set; }
        public string Stato { get; set; }
    }

    public partial class SharedService
    {
        public async Task<Guid> HandleRequest(AddRequestCommand cmd)
        {
            var durata = GetDuration(cmd.DataInizio, cmd.DataFine, cmd.OraInizio, cmd.OraFine, cmd.Tipologia);
            var request = new Request
            {
                UserName = cmd.UserName,
                Tipologia = cmd.Tipologia,
                DataInizio = cmd.DataInizio,
                DataFine = cmd.DataFine,
                OraInizio = cmd.OraInizio,
                OraFine = cmd.OraFine,
                Durata = durata,
                Stato = cmd.Stato
            };

            if (cmd.UserName == "ceo@ceo.it")
            {
                DataGenerator.AddEventForUser(_dbContext, request);
            }
            else
            {
                _dbContext.Requests.Add(request);
            }
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
        public TimeSpan GetDuration(DateTime dataInizio, DateTime dataFine, TimeSpan? oraInizio, TimeSpan? oraFine, string tipologia)
        {
            TimeSpan durata = TimeSpan.Zero;

            if (tipologia == "Ferie")
            {
                // Verifica che le date siano valide
                if (dataInizio > dataFine)
                {
                    throw new ArgumentException("La data di inizio non può essere successiva alla data di fine.");
                }

                // Inizializza la data corrente per l'iterazione
                var dataCorrente = dataInizio.Date;

            
                // Itera per tutti i giorni tra la data di inizio e la data di fine
                while (dataCorrente <= dataFine.Date)
                {
                    // Se il giorno corrente non è un weekend (sabato o domenica)
                    if (dataCorrente.DayOfWeek != DayOfWeek.Saturday && dataCorrente.DayOfWeek != DayOfWeek.Sunday)
                    {
                        durata += TimeSpan.FromDays(1);
                    }
                    dataCorrente = dataCorrente.AddDays(1); // Passa al giorno successivo
                }
            }
            else if (tipologia == "Permessi")
            {
                // Verifica che le ore siano valide
                if (oraInizio.HasValue && oraFine.HasValue)
                {
                    if (oraInizio.Value > oraFine.Value)
                    {
                        throw new ArgumentException("L'ora di inizio non può essere successiva all'ora di fine.");
                    }

                    // Calcola la durata come differenza tra oraFine e oraInizio
                    durata = oraFine.Value - oraInizio.Value;  // durata in ore e minuti
                }
                else
                {
                    // Se non sono fornite le ore, restituisce TimeSpan.Zero
                    durata = TimeSpan.Zero;
                }
            }
            else
            {
                throw new ArgumentException("Tipologia non valida. Deve essere 'Ferie' o 'Permessi'.");
            }

            // Restituisce sempre la durata come TimeSpan
            return durata;
        }


    }
}
