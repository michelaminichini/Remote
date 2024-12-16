﻿using Template.Services.Shared;
using System;
using System.Linq;
using Template.Services;

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
                    Email = "email1@test.it",
                    Password = "M0Cuk9OsrcS/rTLGf5SY6DUPqU2rGc1wwV2IL88GVGo=", // SHA-256 of text "Prova"
                    FirstName = "Nome1",
                    LastName = "Cognome1",
                    TeamName = "Team A",
                    Role = "Manager"
                },
                new User
                {
                    Id = Guid.Parse("a030ee81-31c7-47d0-9309-408cb5ac0ac7"), // Forced to specific Guid for tests
                    Email = "email2@test.it",
                    Password = "Uy6qvZV0iA2/drm4zACDLCCm7BE9aCKZVQ16bg80XiU=", // SHA-256 of text "Test"
                    FirstName = "Nome2",
                    LastName = "Cognome2",
                    TeamName = "Team A",
                    Role = "Dipendente",
                    DataRichiesta = DateTime.Now.AddMonths(-3),
                    Tipologia = "Permesso",
                    DataInizio = DateTime.Now.AddMonths(-3).AddHours(14),  // Inizio permesso alle 14
                    DataFine = DateTime.Now.AddMonths(-3).AddHours(16),   // Fine permesso alle 16
                    Durata = "2 ore"

                },
                new User
                {
                    Id = Guid.Parse("bfdef48b-c7ea-4227-8333-c635af267354"), // Forced to specific Guid for tests
                    Email = "email3@test.it",
                    Password = "Uy6qvZV0iA2/drm4zACDLCCm7BE9aCKZVQ16bg80XiU=", // SHA-256 of text "Test"
                    FirstName = "Nome3",
                    LastName = "Cognome3",
                    TeamName = "Team A",
                    Role = "Dipendente",
                    DataRichiesta = DateTime.Now.AddMonths(-2),
                    Tipologia = "Ferie",
                    DataInizio = DateTime.Now.AddMonths(-2).AddDays(2),  // Inizio ferie il 2 del mese
                    DataFine = DateTime.Now.AddMonths(-2).AddDays(3),   // Fine ferie il 3 del mese
                    Durata = "2 giorni"

                },
                new User
                {
                    Id = Guid.NewGuid(),
                    Email = "mario.rossi@teama.it",
                    Password = "Password123!", // Usando una password semplice per il test
                    FirstName = "Mario",
                    LastName = "Rossi",
                    TeamName = "Team A",
                    Role = "Dipendente",
                    DataRichiesta = DateTime.Now.AddMonths(-3),
                    Tipologia = "Permesso",
                    DataInizio = DateTime.Now.AddMonths(-3).AddHours(14),  // Inizio permesso alle 14
                    DataFine = DateTime.Now.AddMonths(-3).AddHours(16),   // Fine permesso alle 16
                    Durata = "2 ore"
                },
                new User
                {
                    Id = Guid.NewGuid(),
                    Email = "anna.ferrari@company.com",
                    Password = "Password1",
                    FirstName = "Anna",
                    LastName = "Ferrari",
                    TeamName = "Team A",
                    Role = "Dipendente",
                    DataRichiesta = DateTime.Now.AddMonths(-1),
                    Tipologia = "Ferie",
                    DataInizio = DateTime.Now.AddMonths(-1).AddDays(5),  // Inizio ferie il 5 del mese
                    DataFine = DateTime.Now.AddMonths(-1).AddDays(10),   // Fine ferie il 10 del mese
                    Durata = "5 giorni"
                },
                new User
                {
                    Id = Guid.NewGuid(),
                    Email = "giuseppe.conti@company.com",
                    Password = "Password8",
                    FirstName = "Giuseppe",
                    LastName = "Conti",
                    TeamName = "Team B",
                    Role = "Dipendente",
                    DataRichiesta = DateTime.Now.AddMonths(-1),
                    Tipologia = "Permesso",
                    DataInizio = DateTime.Now.AddMonths(-1).AddHours(9),  // Inizio permesso alle 9
                    DataFine = DateTime.Now.AddMonths(-1).AddHours(12),   // Fine permesso alle 12
                    Durata = "Mezza giornata"
                },
                new User
                {
                    Id = Guid.NewGuid(),
                    Email = "sara.moretti@company.com",
                    Password = "Password5",
                    FirstName = "Sara",
                    LastName = "Moretti",
                    TeamName = "Team B",
                    Role = "Manager"
                });

            context.SaveChanges();
        }
    }
}
