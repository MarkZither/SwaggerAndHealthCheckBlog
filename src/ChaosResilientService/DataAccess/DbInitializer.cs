using ChaosResilientService.Data;
using ChaosResilientService.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChaosResilientService.DataAccess
{
    public static class DbInitializer
    {
        public static void Initialize(ApplicationDbContext context)
        {
            context.Database.EnsureCreated();

            // Look for any students.
            if (context.Teams.Any())
            {
                return;   // DB has been seeded
            }

            var teams = new Team[]
            {
            new Team{TeamId = 1, Name = "Nottingham Forest", Url="https://blah" },
            new Team{TeamId = 2, Name = "Nottingham Forest", Url="https://blah" },
            new Team{TeamId = 3, Name = "Nottingham Forest", Url="https://blah" },
            new Team{TeamId = 4, Name = "Nottingham Forest", Url="https://blah" },
            new Team{TeamId = 5, Name = "Nottingham Forest", Url="https://blah" },
            new Team{TeamId = 6, Name = "Nottingham Forest", Url="https://blah" },
            new Team{TeamId = 7, Name = "Nottingham Forest", Url="https://blah" },
            new Team{TeamId = 8, Name = "Nottingham Forest", Url="https://blah" }
            };
            foreach (Team t in teams)
            {
                context.Teams.Add(t);
            }
            context.SaveChanges();
        }
    }
}
