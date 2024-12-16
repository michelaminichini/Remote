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
            var week = new List<DayViewModel>();

            while (current.Month == monthStart.Month || week.Any())
            {
                if (current.Month != monthStart.Month && current.DayOfWeek == DayOfWeek.Monday)
                {
                    break;
                }

                week.Add(new DayViewModel { Date = current, Events = new List<string>() });

                if (current.DayOfWeek == DayOfWeek.Sunday)
                {
                    Weeks.Add(week);
                    week = new List<DayViewModel>();
                }

                current = current.AddDays(1);
            }
        }
    }

    public class DayViewModel
    {
        public DateTime Date { get; set; }
        public List<string> Events { get; set; }
    }
}
