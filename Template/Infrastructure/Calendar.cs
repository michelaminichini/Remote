﻿using System;
using System.Collections.Generic;

namespace Template.Infrastructure
{
    public class Calendar
    {
        public static List<List<DayViewModel>> GetWeeksInMonth(int year, int month, DateTime? dateFrom, DateTime? dateTo)
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
                        DayId = Guid.NewGuid(),  // Genera un ID univoco per ogni giorno
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
        public Guid DayId { get; set; }  // Aggiungi l'ID del giorno
        public DateTime Date { get; set; }
        public bool IsCurrentMonth { get; set; }
        public bool IsToday { get; set; }
        public bool IsInRange { get; set; }
        public List<string> Events { get; set; }  // Aggiunto per memorizzare gli eventi
    }
}
