using Template.Services.Shared;
using System;
using System.Linq;
using Template.Services;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using System.Collections;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Template.Infrastructure
{
    public class DataGenerator
    {

        public static void AddEventForUser(TemplateDbContext context, Request richiesta)
        {
            // Upload the user with explicit tracking including Events
            var user = context.Users
                .Include(u => u.Events)
                .FirstOrDefault(u => u.Email == richiesta.UserName);

            if (user != null)
            {
                var newEvent = new Event
                {
                    EventId = Guid.NewGuid(),
                    DataRichiesta = DateTime.Now,
                    Tipologia = richiesta.Tipologia,
                    DataInizio = richiesta.DataInizio,
                    DataFine = richiesta.DataFine,
                    Durata = richiesta.DataFine - richiesta.DataInizio,
                    Stato = richiesta.Stato
                };

                // If the Events collection is null, initialize it
                if (user.Events == null)
                {
                    user.Events = new List<Event>();
                }

                user.Events.Add(newEvent);
                context.Events.Add(newEvent);

                try
                {
                    context.SaveChanges();
                }
                catch (Exception ex)
                {
                    throw new Exception("Errore nel salvataggio dell'evento: " + ex.Message);
                }
            }
            else
            {
                throw new Exception("Utente non trovato");
            }
        }


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
                    Events = new List<Event> // Add events for the user
                    {
                        new Event
                        {
                            EventId = Guid.NewGuid(),
                            DataRichiesta = DateTime.Now.AddMonths(-3),
                            Tipologia = "Permessi",
                            DataInizio = DateTime.Now.AddMonths(-3).AddHours(14),
                            DataFine = DateTime.Now.AddMonths(-3).AddHours(16),
                            Durata = new TimeSpan(2, 0, 0),
                            Stato = "Rifiutata"
                        },
                        new Event
                        {
                            EventId = Guid.NewGuid(),
                            DataRichiesta = new DateTime(2024, 3, 15),  // richiesta effettuata il 15 marzo 2024
                            Tipologia = "Ferie",
                            DataInizio = new DateTime(2024, 8, 5),  // data inizio il 5 agosto 2024
                            DataFine = new DateTime(2024, 8, 9),  // data fine il 7 agosto 2024
                            Durata = new TimeSpan(5, 0, 0, 0),  
                            Stato = "Accettata"
                        }
                    }
                },
                new User
                {
                    Id = Guid.NewGuid(),
                    Email = "ceo@ceo.it",
                    Password = "Uy6qvZV0iA2/drm4zACDLCCm7BE9aCKZVQ16bg80XiU=", // SHA-256 of text "Test"
                    FirstName = "Paolo",
                    LastName = "Neri",
                    TeamName = "Supervisione",
                    Role = "CEO",
                    Img = "images/User/UserAdmin.png"
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
                    Events = new List<Event> // Add events for the user
                    {
                        new Event
                        {
                            EventId = Guid.NewGuid(),
                            DataRichiesta = new DateTime(2024, 9, 17),
                            Tipologia = "Trasferta",
                            DataInizio = new DateTime(2024, 11, 20, 0, 0, 0),
                            DataFine = new DateTime(2024, 11, 20, 23, 59, 59),
                            Durata = new TimeSpan(1, 0, 0, 0),  // 1 day
                            Stato = "Accettata",
                        },
                        new Event
                        {
                            EventId = Guid.NewGuid(),
                            DataRichiesta = new DateTime(2024, 9, 10),  // richiesta effettuata il 10 settembre 2024
                            Tipologia = "Permessi",
                            DataInizio = new DateTime(2024, 9, 12, 14, 0, 0),  // data inizio il 12 settembre 2024 alle 14:00
                            DataFine = new DateTime(2024, 9, 12, 16, 0, 0),  // data fine il 12 settembre 2024 alle 16:00
                            Durata = new TimeSpan(2, 0, 0),  // durata di 2 ore
                            Stato = "Accettata"
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
                    Events = new List<Event> // Add events for the user
                    {
                        new Event
                        {
                            EventId = Guid.NewGuid(),
                            DataRichiesta = new DateTime(2024, 10, 8),
                            Tipologia = "Permessi",
                            DataInizio = new DateTime(2024, 10, 10, 9, 0, 0),
                            DataFine = new DateTime(2024, 10, 10, 13, 0, 0),
                            Durata = new TimeSpan(4, 0, 0),
                            Stato = "Accettata"
                        }
                    }
                },
                new User
                {
                    Id = Guid.NewGuid(),
                    Email = "mario.rossi@team.it",
                    Password = "Uy6qvZV0iA2/drm4zACDLCCm7BE9aCKZVQ16bg80XiU=", // SHA-256 of text "Test"
                    FirstName = "Mario",
                    LastName = "Rossi",
                    TeamName = "Team A",
                    Role = "Manager",
                    Img = "images/User/User4.png",
                    Events = new List<Event> // Add events for the user
                    {
                        new Event
                        {
                            EventId = Guid.NewGuid(),
                            DataRichiesta = new DateTime(2024, 9, 26),
                            Tipologia = "Permessi",
                            DataInizio = new DateTime(2024, 9, 27, 14, 0, 0),
                            DataFine = new DateTime(2024, 9, 27, 16, 0, 0),
                            Durata = new TimeSpan(2, 0, 0),
                            Stato = "Accettata"
                        },
                        new Event
                        {
                            EventId = Guid.NewGuid(),
                            DataRichiesta = new DateTime(2024, 6, 5),  // richiesta effettuata il 5 giugno 2024
                            Tipologia = "Ferie",
                            DataInizio = new DateTime(2024, 7, 20),  // data inizio il 20 luglio 2024
                            DataFine = new DateTime(2024, 8, 3),  // data fine il 3 agosto 2024
                            Durata = new TimeSpan(14, 0, 0, 0),  // durata di 14 giorni (2 settimane)
                            Stato = "Rifiutata"
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
                    Events = new List<Event> // Add events for the user
                    {
                        new Event
                        {
                            EventId = Guid.NewGuid(),
                            DataRichiesta = DateTime.Now.AddMonths(-1),
                            Tipologia = "Ferie",
                            DataInizio = DateTime.Now.AddMonths(-1).AddDays(5),
                            DataFine = DateTime.Now.AddMonths(-1).AddDays(10),
                            Durata = new TimeSpan(5, 0, 0, 0),
                            Stato = "Rifiutata"
                        },
                        
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
                    Events = new List<Event> // Add events for the user
                    {
                        new Event
                        {
                            EventId = Guid.NewGuid(),
                            DataRichiesta = new DateTime(2024, 10, 24), 
                            Tipologia = "Permessi",
                            DataInizio = new DateTime(2024, 11, 20, 9, 0, 0),
                            DataFine = new DateTime(2024, 11, 20, 12, 0, 0),
                            Durata = new TimeSpan(3, 0, 0), // 3 hours
                            Stato = "Accettata"
                        },
                        new Event
                        {
                            EventId = Guid.NewGuid(),
                            DataRichiesta = new DateTime(2024, 11, 12),
                            Tipologia = "Trasferta",
                            DataInizio = new DateTime(2024, 11, 20, 9, 0, 0),
                            DataFine = new DateTime(2024, 11, 20, 13, 0, 0),
                            Durata = new TimeSpan(4, 0, 0),
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
                    Events = new List<Event> // Add events for the user
                    {
                        new Event
                        {
                            EventId = Guid.NewGuid(),
                            DataRichiesta = DateTime.Now.AddMonths(-1),
                            Tipologia = "Ferie",
                            DataInizio = new DateTime(2024, 11, 20),
                            DataFine = new DateTime(2024, 11, 22), 
                            Durata = new TimeSpan(2,0,0,0), // 2 days
                            Stato = "Accettata"
                        },
                        new Event
                        {
                            EventId = Guid.NewGuid(),
                            DataRichiesta = DateTime.Now.AddMonths(-1),
                            Tipologia = "Ferie",
                            DataInizio = new DateTime(2025, 1, 13),
                            DataFine = new DateTime(2025, 1, 17),
                            Durata = new DateTime(2025, 1, 17).Subtract(new DateTime(2025, 1, 13)),
                            Stato = "Accettata"
                        },
                        new Event
                        {
                            EventId = Guid.NewGuid(),
                            DataRichiesta = DateTime.Now.AddMonths(-1),
                            Tipologia = "Ferie",
                            DataInizio = new DateTime(2025, 1, 2),
                            DataFine = new DateTime(2025, 1, 3),
                            Durata = new DateTime(2025, 1, 3).Subtract(new DateTime(2025, 1, 2)),
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
                    Events = new List<Event> // Add events for the user
                    {
                        new Event
                        {
                            EventId = Guid.NewGuid(),
                            DataRichiesta = DateTime.Now.AddMonths(-2),
                            Tipologia = "Trasferta",
                            DataInizio = new DateTime(2024, 11, 20, 8, 0, 0),
                            DataFine = new DateTime(2024, 11, 20, 12, 0, 0),
                            Durata = new TimeSpan(4, 0, 0),
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
                return; // Data has already been initialized
            }

            // Load all users from database
            var users = context.Users
                .Where(u => !string.IsNullOrEmpty(u.TeamName)) // Consider only users with "TeamName"
                .ToList();

            // Group for TeamName
            var groupedUsers = users
                .GroupBy(u => u.TeamName)
                .ToList();

            foreach (var group in groupedUsers)
            {
                var teamName = group.Key;
                var teamManager = group.FirstOrDefault(u => u.Role == "Manager"); // Find the team manager

                // Create the team
                var team = new Teams
                {
                    TeamName = teamName,
                    TeamManager = teamManager?.LastName ?? "N/A", // Use manager’s last name, if present
                    Employee = group.ToList() // Add all users in the group to the team
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
