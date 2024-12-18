using System;
using System.Collections.Generic;

namespace Template.Infrastructure
{
    public class Calendar
    {
        public static List<List<DayViewModel>> GetWeeksInMonth(int year, int month, DateTime? dateFrom, DateTime? dateTo) //per gestione dal-al e delle settimane/mesi
        {
            var weeks = new List<List<DayViewModel>>();
            var firstDayOfMonth = new DateTime(year, month, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

            var startOfWeek = firstDayOfMonth.AddDays(-(int)firstDayOfMonth.DayOfWeek + (int)DayOfWeek.Monday);
            if (startOfWeek > firstDayOfMonth)
            {
                startOfWeek = startOfWeek.AddDays(-7);
            }

            var currentDay = startOfWeek;

            while (currentDay <= lastDayOfMonth)
            {
                var week = new List<DayViewModel>();

                for (int i = 0; i < 7; i++)
                {
                    bool isCurrentMonth = currentDay.Month == month;
                    bool isToday = currentDay.Date == DateTime.Today.Date;

                    week.Add(new DayViewModel
                    {
                        Date = currentDay,
                        IsCurrentMonth = isCurrentMonth,
                        IsToday = isToday,
                        IsInRange = (dateFrom == null || currentDay >= dateFrom) && (dateTo == null || currentDay <= dateTo),
                        Events = new List<string>()
                    });

                    currentDay = currentDay.AddDays(1);
                }

                weeks.Add(week);
            }

            return weeks;
        }
    }

    public class DayViewModel
    {
        public DateTime Date { get; set; }
        public bool IsCurrentMonth { get; set; }
        public bool IsToday { get; set; }
        public bool IsInRange { get; set; }  // Aggiunto per il filtro dell'intervallo
        public List<string> Events { get; set; }
    }
}
