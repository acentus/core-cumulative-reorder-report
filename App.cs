using System;
using System.Configuration;

namespace CoreCumulativeReorderReport
{
    public static class App
    {
        private static DateTime firstDayMonth;
        private static DateTime lastDayMonth;
        //private static DateTime firstDayOfAccountingMonth;
        //private static DateTime lastDayOfAccountingMonth;
        private static DateTime firstDay3MonthsAgo;

        private static string ForceMonth = ConfigurationManager.AppSettings["ForceMonth"];
        private static string MonthBack = ConfigurationManager.AppSettings["MonthBack"];

        static App()
        {
            DateTime dtToday = DateTime.Today;

            //firstDayOfAccountingMonth = Utils.GetFirstDay(dtToday);
            //lastDayOfAccountingMonth = Utils.GetLastDay(dtToday);
            
            firstDayMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            lastDayMonth = firstDayMonth.AddMonths(1).AddTicks(-1);

            firstDay3MonthsAgo = firstDayMonth.AddMonths(-3);

            //Log.write("firstDayOfAccountingMonth: " + firstDayOfAccountingMonth.ToString());
            //Log.write("lastDayOfAccountingMonth: " + lastDayOfAccountingMonth.ToString());
            Log.write("firstDayMonth: " + firstDayMonth.ToString());
            Log.write("lastDayMonth: " + lastDayMonth.ToString());
            Log.write("firstDay3MonthsAgo: " + firstDay3MonthsAgo.ToString());

            if (ForceMonth == "True")
            {
                int backMonths = Convert.ToInt32(MonthBack);
                firstDayMonth = firstDayMonth.AddMonths(-backMonths);
                lastDayMonth = lastDayMonth.AddMonths(-backMonths);

                Log.write("In FORCEMONTH mode. New dates: " + firstDayMonth + " to " + lastDayMonth);
            }
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

        //public static DateTime FirstDayAccountingMonth
        //{
        //    get
        //    {
        //        return firstDayOfAccountingMonth;
        //    }
        //}

        //public static DateTime LastDayAccountingMonth
        //{
        //    get
        //    {
        //        return lastDayOfAccountingMonth;
        //    }
        //}

        public static DateTime FirstDay3MonthsAgo
        {
            get
            {
                return firstDay3MonthsAgo;
            }
        }
    }
}