using System;
using System.IO;

namespace CoreCumulativeReorderReport
{
    internal class Log
    {
        public static void write(string str)
        {
            string logfile = "";
            string filename = "CoreCumulativeReorderReport_Log_" + DateTime.Now.Year.ToString() + "_" + DateTime.Now.Month.ToString() + "_" + DateTime.Now.Day.ToString() + ".txt";
            string appPath = AppDomain.CurrentDomain.BaseDirectory + "logs";
            logfile = Path.Combine(appPath, filename);

            try
            {
                if (!Directory.Exists(logfile))
                {
                    //Log.write("Path does not exist. Creating directory.");
                    DirectoryInfo di = Directory.CreateDirectory(appPath);
                }

                using (System.IO.StreamWriter outfile = new System.IO.StreamWriter(logfile, true))
                {
                    outfile.WriteLine(DateTime.Now + " : " + str);
                    outfile.Close();
                    outfile.Dispose();
                }
            }
            catch
            {
            }
        }
    }
}