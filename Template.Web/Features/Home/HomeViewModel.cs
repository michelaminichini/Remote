using System;
using System.Collections.Generic;
using System.Linq;

namespace Template.Web.Features.Home
{
    public class HomeViewModel
    {
        public List<List<DayViewModel>> Weeks { get; set; } = new();
        public string CurrentMonthName { get; set; }
        public int CurrentYear { get; set; }

        public HomeViewModel(DateTime monthStart)
        {
            CurrentMonthName = monthStart.ToString("MMMM");
            CurrentYear = monthStart.Year;

            var current = monthStart;

            // Allineamento con il primo giorno della settimana (Lunedì)
            var startOfWeek = current.AddDays(-(int)current.DayOfWeek + (int)DayOfWeek.Monday);
            var week = new List<DayViewModel>();

            while (current.Month == monthStart.Month || week.Any())
            {
                week.Add(new DayViewModel
                {
                    Date = startOfWeek,
                    Events = new List<string>()
                });

                startOfWeek = startOfWeek.AddDays(1);

                if (startOfWeek.DayOfWeek == DayOfWeek.Monday)
                {
                    Weeks.Add(week);
                    week = new List<DayViewModel>();
                }

                if (startOfWeek > current.AddMonths(1).AddDays(-1))
                    break;
            }
        }

    }

    public class DayViewModel
    {
        public DateTime Date { get; set; }
        public List<string> Events { get; set; }
    }
}
