//
// Copyright (c) 2022 by Acentus
// Developed by: Luis Cabrera
// gigocabrera@outlook.com
//

using System;
using System.Configuration;
using System.Data;
using System.IO;
using System.Threading.Tasks;

namespace CoreCumulativeReorderReport
{
    class Report
    {
        JobCenterHistoryLogger logHeader = new JobCenterHistoryLogger();
        JobCenterHistoryLoggerDetails logDetails = new JobCenterHistoryLoggerDetails();

        private DataAccess db = new DataAccess();

        public async Task RunReport()
        {
            try
            {
                //
                // START LOGGING
                //
                logHeader = new JobCenterHistoryLogger(DateTime.Now, DateTime.Now, JobCenterHistoryLogger.StatusEnum.Running, "Report started");
                logDetails = new JobCenterHistoryLoggerDetails(logHeader.HistoryId, "Test Mode: " + ConfigurationManager.AppSettings["TestMode"]);
                logDetails = new JobCenterHistoryLoggerDetails(logHeader.HistoryId, "Subject: " + ConfigurationManager.AppSettings["subject"]);
                logDetails = new JobCenterHistoryLoggerDetails(logHeader.HistoryId, "Email From: " + ConfigurationManager.AppSettings["emailFrom"]);
                logDetails = new JobCenterHistoryLoggerDetails(logHeader.HistoryId, "Email To: " + ConfigurationManager.AppSettings["emailTo"]);
                logDetails = new JobCenterHistoryLoggerDetails(logHeader.HistoryId, "Report Date: " + DateTime.Today.ToShortDateString());

                //
                // GET LIST OF SALES REPS
                //
                DataTable dtReps = db.GetSalesReps();
                if (dtReps != null && dtReps.Rows.Count > 0)
                {
                    Log.write("Total active sales reps: " + dtReps.Rows.Count.ToString());

                    logDetails = new JobCenterHistoryLoggerDetails(logHeader.HistoryId, "Total active sales reps: " + dtReps.Rows.Count.ToString());

                    string filename = "cumulative-reorder-report_" + DateTime.Now.Year.ToString() + "_" + DateTime.Now.Month.ToString() + "_" + DateTime.Now.Day.ToString() + ".csv";
                    string appPath = AppDomain.CurrentDomain.BaseDirectory + "files";
                    string csvfile = Path.Combine(appPath, filename);
                    if (!Directory.Exists(csvfile))
                    {
                        DirectoryInfo di = Directory.CreateDirectory(appPath);
                    }

                    StreamWriter sw = new StreamWriter(csvfile);
                    Item rep = new Item();

                    rep.TotalOrdersThisMonth = db.GetTotalConfirmedOrdersThisMonth();

                    string sHeader = "Rep ID, Sales Rep, Active Patients, Current # Order Renewals, Current Order Renewal %, # Exp Visit Notes, # Exp SWO, Order Renewal # Goal-87%, Order Renewal # Goal-93%";
                    sw.WriteLine(sHeader);

                    foreach (DataRow salesrep in dtReps.Rows)
                    {
                        //
                        // LOOP THROUGH EACH SALES REP TO GET DATA METRICS
                        //
                        rep.Id = salesrep.Field<int>("ID");
                        rep.SalesRep = salesrep.Field<string>("SALESREP");
                        rep.TotalActivePatients = db.GetTotalActivePatients(rep.Id);

                        if (rep.TotalActivePatients > 0)
                        {
                            rep.TotalExpVisitNotes = db.GetTotalExpVisitNotes(rep.Id);
                            rep.TotalExpSWO = db.GetTotalExpSWO(rep.Id);
                            rep.Renewal87 = Math.Round(rep.TotalActivePatients * 0.87, 0);
                            rep.Renewal93 = Math.Round(rep.TotalActivePatients * 0.93, 0);
                            rep.CurrentRenewal = db.GetCurrentRenewal(rep.Id);

                            rep.CurrentRenewalPercent = (int)Math.Round((double)(100 * rep.CurrentRenewal) / rep.TotalActivePatients);

                            string sLine = rep.Id.ToString() + "," + rep.SalesRep + "," + rep.TotalActivePatients + "," + rep.CurrentRenewal + "," + rep.CurrentRenewalPercent.ToString() + "," + rep.TotalExpVisitNotes + "," + rep.TotalExpSWO + "," + rep.Renewal87 + "," + rep.Renewal93;
                            sw.WriteLine(sLine);
                        }
                    }
                    sw.Close();
                    sw.Dispose();

                    var bytesFromFile = System.IO.File.ReadAllBytes(csvfile);
                    await Utils.SendEmailWithModernAuthentication(bytesFromFile, filename);

                    logDetails = new JobCenterHistoryLoggerDetails(logHeader.HistoryId, "Email: Sent Successfully");

                    logHeader.EndTime = DateTime.Now;
                    logHeader.Message = "Report Completed";
                    logHeader.Status = JobCenterHistoryLogger.StatusEnum.Success;
                    logHeader.Update();
                }
                else
                {
                    // No data was returned
                    logHeader.EndTime = DateTime.Now;
                    logHeader.Message = "No data found";
                    logHeader.Status = JobCenterHistoryLogger.StatusEnum.Success;
                    logHeader.Update();
                }
            }
            catch (Exception ex)
            {
                Log.write(string.Format("REPORT EXCEPTION: " + ex.Message));

                logHeader.EndTime = DateTime.Now;
                logHeader.Status = JobCenterHistoryLogger.StatusEnum.Error;
                logHeader.Message = ex.Message;
                logHeader.Update();
                throw;
            }
        }
    }
}