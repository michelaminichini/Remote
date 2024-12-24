using Template.Services.Shared;
using System;
using System.Linq;
using Template.Services;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using System.Collections;

namespace Template.Infrastructure
{
    public class DataGenerator
    {
        public static void InitializeUsers(TemplateDbContext context)
        {
            if (context.Users.Any())
            {
                return;   // Data was already seeded
            }

            context.Users.AddRange(
                new User
                {
                    Id = Guid.Parse("3de6883f-9a0b-4667-aa53-0fbc52c4d300"), // Forced to specific Guid for tests
                    Email = "luca.armani@test.it",
                    Password = "M0Cuk9OsrcS/rTLGf5SY6DUPqU2rGc1wwV2IL88GVGo=", // SHA-256 of text "Prova"
                    FirstName = "Luca",
                    LastName = "Armani",
                    TeamName = "Team A",
                    Role = "Dipendente",
                    Img = "images/User/User1.png",
                    Events = new List<Event> // Aggiungi eventi per l'utente
                    {
                        new Event
                        {
                            EventId = Guid.NewGuid(),
                            DataRichiesta = DateTime.Now.AddMonths(-3),
                            Tipologia = "Permessi",
                            DataInizio = DateTime.Now.AddMonths(-3).AddHours(14),  // Inizio permesso alle 14
                            DataFine = DateTime.Now.AddMonths(-3).AddHours(16),   // Fine permesso alle 16
                            Durata = new TimeSpan(2, 0, 0),
                            Stato = "Rifiutata"
                        }
                    }
                },
                new User
                {
                    Id = Guid.Parse("a030ee81-31c7-47d0-9309-408cb5ac0ac7"), // Forced to specific Guid for tests
                    Email = "luisa.verdi@test.it",
                    Password = "Uy6qvZV0iA2/drm4zACDLCCm7BE9aCKZVQ16bg80XiU=", // SHA-256 of text "Test"
                    FirstName = "Luisa",
                    LastName = "Verdi",
                    TeamName = "Team B",
                    Role = "Manager",
                    Img = "images/User/User2.png",
                    Events = new List<Event> // Aggiungi eventi per l'utente
                    {
                        new Event
                        {
                            EventId = Guid.NewGuid(),
                            DataRichiesta = DateTime.Now.AddMonths(-2),
                            Tipologia = "Presenza",  // Cambiamo la tipologia a "Presenza"
                            DataInizio = new DateTime(2024, 11, 20, 0, 0, 0),  // Inizio presenza il 20 novembre 2024, a mezzanotte
                            DataFine = new DateTime(2024, 11, 20, 23, 59, 59),  // Fine presenza il 20 novembre 2024, a mezzanotte
                            Durata = new TimeSpan(1, 0, 0, 0),  // Durata di 1 giorno (24 ore)
                            Stato = "Accettata",
                        }
                    }
                },
                new User
                {
                    Id = Guid.Parse("bfdef48b-c7ea-4227-8333-c635af267354"), // Forced to specific Guid for tests
                    Email = "ambrogio.pisani@test.it",
                    Password = "M0Cuk9OsrcS/rTLGf5SY6DUPqU2rGc1wwV2IL88GVGo=", // SHA-256 of text "Prova"
                    FirstName = "Ambrogio",
                    LastName = "Pisani",
                    TeamName = "Team A",
                    Role = "Dipendente",
                    Img = "images/User/User3.png",
                    Events = new List<Event> // Aggiungi eventi per l'utente
                    {
                        new Event
                        {
                            EventId = Guid.NewGuid(),
                            DataRichiesta = DateTime.Now.AddMonths(-2),
                            Tipologia = "Presenza",
                            DataInizio = new DateTime(2024, 11, 20),  // in presenza il 20 novembre 2024
                            DataFine = new DateTime(2024, 11, 20),
                            Durata = new TimeSpan(1, 0, 0, 0),        // Durata di 1 giorno
                            Stato = "Accettata"
                        }
                    }
                },
                new User
                {
                    Id = Guid.NewGuid(),
                    Email = "mario.rossi@teama.it",
                    Password = "M0Cuk9OsrcS/rTLGf5SY6DUPqU2rGc1wwV2IL88GVGo=", // SHA-256 of text "Prova"
                    FirstName = "Mario",
                    LastName = "Rossi",
                    TeamName = "Team A",
                    Role = "Dipendente",
                    Img = "images/User/User4.png",
                    Events = new List<Event> // Aggiungi eventi per l'utente
                    {
                        new Event
                        {
                            EventId = Guid.NewGuid(),
                            DataRichiesta = DateTime.Now.AddMonths(-3),
                            Tipologia = "Permessi",
                            DataInizio = DateTime.Now.AddMonths(-3).AddHours(14),  // Inizio permesso alle 14
                            DataFine = DateTime.Now.AddMonths(-3).AddHours(16),   // Fine permesso alle 16
                            Durata = new TimeSpan(2, 0, 0),
                            Stato = "Accettata"
                        }
                    }
                },
                new User
                {
                    Id = Guid.NewGuid(),
                    Email = "anna.ferrari@company.com",
                    Password = "M0Cuk9OsrcS/rTLGf5SY6DUPqU2rGc1wwV2IL88GVGo=", // SHA-256 of text "Prova"
                    FirstName = "Anna",
                    LastName = "Ferrari",
                    TeamName = "Team A",
                    Role = "Dipendente",
                    Img = "images/User/User5.png",
                    Events = new List<Event> // Aggiungi eventi per l'utente
                    {
                        new Event
                        {
                            EventId = Guid.NewGuid(),
                            DataRichiesta = DateTime.Now.AddMonths(-1),
                            Tipologia = "Ferie",
                            DataInizio = DateTime.Now.AddMonths(-1).AddDays(5),  // Inizio ferie il 5 del mese
                            DataFine = DateTime.Now.AddMonths(-1).AddDays(10),   // Fine ferie il 10 del mese
                            Durata = new TimeSpan(5, 0, 0, 0),
                            Stato = "Rifiutata"
                        },
                        new Event
                        {
                            EventId = Guid.NewGuid(),
                            DataRichiesta = DateTime.Now, // Data di richiesta dell'evento (ad esempio, oggi)
                            Tipologia = "Smartworking",
                            DataInizio = new DateTime(2024, 12, 24),  // Inizio smartworking il 24 dicembre 2024
                            DataFine = new DateTime(2024, 12, 24),    // Fine smartworking il 24 dicembre 2024
                            Stato = "Accettata"
                        },
                        new Event
                        {
                            EventId = Guid.NewGuid(),
                            DataRichiesta = DateTime.Now, // Data di richiesta dell'evento (ad esempio, oggi)
                            Tipologia = "Smartworking",
                            DataInizio = new DateTime(2024, 12, 27),  // Inizio smartworking il 27 dicembre 2024
                            DataFine = new DateTime(2024, 12, 27),    // Fine smartworking il 27 dicembre 2024
                            Stato = "Accettata"
                        }
                    }
                },
                new User
                {
                    Id = Guid.NewGuid(),
                    Email = "giuseppe.conti@company.com",
                    Password = "M0Cuk9OsrcS/rTLGf5SY6DUPqU2rGc1wwV2IL88GVGo=", // SHA-256 of text "Prova"
                    FirstName = "Giuseppe",
                    LastName = "Conti",
                    TeamName = "Team B",
                    Role = "Dipendente",
                    Img = "images/User/User6.png",
                    Events = new List<Event> // Aggiungi eventi per l'utente
                    {
                        new Event
                        {
                            EventId = Guid.NewGuid(),
                            DataRichiesta = new DateTime(2024, 10, 24),  // Data della richiesta 
                            Tipologia = "Permessi",
                            DataInizio = new DateTime(2024, 11, 20, 9, 0, 0),  // Inizio permesso alle 9:00 del 20 novembre
                            DataFine = new DateTime(2024, 11, 20, 11, 0, 0),   // Fine permesso alle 11:00 del 20 novembre
                            Durata = new TimeSpan(2, 0, 0),  // Durata di 2 ore
                            Stato = "Accettata" 
                        },
                        new Event
                        {
                            EventId = Guid.NewGuid(),
                            DataRichiesta = DateTime.Now.AddMonths(-1),  // Richiesta effettuata un mese fa
                            Tipologia = "Ferie",
                            DataInizio = new DateTime(2025, 1, 2),  // Inizio ferie il 2 gennaio 2025
                            DataFine = new DateTime(2025, 1, 3),     // Fine ferie il 3 gennaio 2025
                            Durata = new DateTime(2025, 1, 3).Subtract(new DateTime(2025, 1, 2)),  // Durata da 2 gennaio a 3 gennaio
                            Stato = "Accettata"
                        },
                        new Event
                        {
                            EventId = Guid.NewGuid(),
                            DataRichiesta = DateTime.Now, // Data di richiesta dell'evento (ad esempio, oggi)
                            Tipologia = "Smartworking",
                            DataInizio = new DateTime(2024, 11, 20),  // Inizio smartworking il 20 novembre 2024
                            DataFine = new DateTime(2024, 11, 20),    // Fine smartworking il 20 novembre 2024
                            Stato = "Accettata"
                        }
                    }
                },
                new User
                {
                    Id = Guid.NewGuid(),
                    Email = "sara.moretti@company.com",
                    Password = "M0Cuk9OsrcS/rTLGf5SY6DUPqU2rGc1wwV2IL88GVGo=", // SHA-256 of text "Prova"
                    FirstName = "Sara",
                    LastName = "Moretti",
                    TeamName = "Team B",
                    Role = "Dipendente",
                    Img = "images/User/User7.png",
                    Events = new List<Event> // Aggiungi eventi per l'utente
                    {
                        new Event
                        {
                            EventId = Guid.NewGuid(),
                            DataRichiesta = DateTime.Now.AddMonths(-1),  // Richiesta effettuata un mese fa
                            Tipologia = "Ferie",
                            DataInizio = new DateTime(2024, 11, 20),  // Inizio ferie 
                            DataFine = new DateTime(2024, 11, 22),     // Fine ferie 
                            Durata = new DateTime(2024, 11, 20).Subtract(new DateTime(2024, 11, 22)),  // Durata da 3 giorni
                            Stato = "Accettata"
                        },
                        new Event
                        {
                            EventId = Guid.NewGuid(),
                            DataRichiesta = DateTime.Now.AddMonths(-1),  // Richiesta effettuata un mese fa
                            Tipologia = "Ferie",
                            DataInizio = new DateTime(2025, 1, 13),  // Inizio ferie il 13 gennaio 2025
                            DataFine = new DateTime(2025, 1, 17),     // Fine ferie il 17 gennaio 2025
                            Durata = new DateTime(2025, 1, 17).Subtract(new DateTime(2025, 1, 13)),  // Durata da 10 gennaio a 17 gennaio
                            Stato = "Accettata"
                        },
                        new Event
                        {
                            EventId = Guid.NewGuid(),
                            DataRichiesta = DateTime.Now.AddMonths(-1),  // Richiesta effettuata un mese fa
                            Tipologia = "Ferie",
                            DataInizio = new DateTime(2025, 1, 2),  // Inizio ferie il 2 gennaio 2025
                            DataFine = new DateTime(2025, 1, 3),     // Fine ferie il 3 gennaio 2025
                            Durata = new DateTime(2025, 1, 3).Subtract(new DateTime(2025, 1, 2)),  // Durata da 2 gennaio a 3 gennaio
                            Stato = "Accettata"
                        }
                    }
                },
                new User
                {
                    Id = Guid.NewGuid(), 
                    Email = "mirco.alessi@test.it",
                    Password = "M0Cuk9OsrcS/rTLGf5SY6DUPqU2rGc1wwV2IL88GVGo=", // SHA-256 of text "Prova"
                    FirstName = "Mirco",
                    LastName = "Alessi",
                    TeamName = "Team B",
                    Role = "Dipendente",
                    Img = "images/User/User8.png",
                    Events = new List<Event> // Aggiungi eventi per l'utente
                    {
                        new Event
                        {
                            EventId = Guid.NewGuid(),
                            DataRichiesta = DateTime.Now.AddMonths(-2),
                            Tipologia = "Trasferta",  // Modificata la tipologia a "Trasferta"
                            DataInizio = new DateTime(2024, 11, 20, 8, 0, 0),  // Inizio trasferta il 20 novembre 2024 alle 8:00
                            DataFine = new DateTime(2024, 11, 20, 12, 0, 0),   // Fine trasferta il 20 novembre 2024 alle 12:00
                            Durata = new TimeSpan(4, 0, 0),                      // Durata della trasferta di 4 ore
                            Stato = "Accettata"
                        }
                    }
                });

            context.SaveChanges();
        }

        public static void InitializeTeam(TemplateDbContext context)
        {
            if (context.Teams.Any())
            {
                return; // I dati sono già stati inizializzati
            }

            // Carica tutti gli utenti dal database
            var users = context.Users
                .Where(u => !string.IsNullOrEmpty(u.TeamName)) // Considera solo utenti con TeamName specificato
                .ToList();

            // Raggruppa in memoria per TeamName
            var groupedUsers = users
                .GroupBy(u => u.TeamName)
                .ToList();

            foreach (var group in groupedUsers)
            {
                var teamName = group.Key;
                var teamManager = group.FirstOrDefault(u => u.Role == "Manager"); // Trova il manager del team

                // Crea il team
                var team = new Teams
                {
                    TeamName = teamName,
                    TeamManager = teamManager?.LastName ?? "N/A", // Usa il cognome del manager, se presente
                    Employee = group.ToList() // Aggiungi tutti gli utenti del gruppo al team
                };

                context.Teams.Add(team);
            }

            try
            {
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Errore nel salvataggio: " + ex.Message);
            }
        }
    }
}
