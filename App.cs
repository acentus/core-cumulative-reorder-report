using System;

namespace CoreCumulativeReorderReport
{
    public static class App
    {
        private static DateTime firstDayMonth;
        private static DateTime lastDayMonth;
        private static DateTime firstDay3MonthsAgo;
        private static DateTime lastDay3MonthsAgo;

        static App()
        {
            DateTime dtToday = DateTime.Today;
            DateTime firstDayOfAccountingMonth = Utils.GetFirstDay(dtToday);
            DateTime lastDayOfAccountingMonth = Utils.GetLastDay(dtToday);
            firstDayMonth = firstDayOfAccountingMonth;
            lastDayMonth = lastDayOfAccountingMonth;

            DateTime dt3MonthsAgo = DateTime.Today.AddMonths(-3);
            DateTime firstDayOfAccounting3MonthsAgo = Utils.GetFirstDay(dtToday);
            DateTime lastDayOfAccounting3MonthsAgo = Utils.GetLastDay(dtToday);
            firstDay3MonthsAgo = firstDayOfAccounting3MonthsAgo;
            lastDay3MonthsAgo = lastDayOfAccounting3MonthsAgo;

            Log.write("First Day Month: " + firstDayMonth.ToShortDateString());
            Log.write("Last Day Month: " + lastDayMonth.ToShortDateString());
        }

        public static DateTime FirstDayMonth
        {
            get
            {
                return firstDayMonth;
            }
        }

        public static DateTime LastDayMonth
        {
            get
            {
                return lastDayMonth;
            }
        }

        public static DateTime FirstDay3MonthsAgo
        {
            get
            {
                return firstDay3MonthsAgo;
            }
        }

        public static DateTime LastDay3MonthsAgo
        {
            get
            {
                return lastDay3MonthsAgo;
            }
        }
    }
}