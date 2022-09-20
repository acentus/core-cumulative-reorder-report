//
// Copyright (c) 2022 by Acentus
// Developed by: Luis Cabrera
// gigocabrera@outlook.com
//

using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net.Mail;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CoreCumulativeReorderReport
{
    /// <summary>
    /// This abstract class provides commonly used utility methods
    /// </summary>
    public abstract class Utils
    {
        #region Conversion Methods

        public static int StringToInt(string value)
        {
            try
            {
                return System.Convert.ToInt32(value);
            }
            catch (Exception)
            {
                return -1;
            }
        }

        public static bool IsEmpty(string value)
        {
            return ((value == null) || (value.Trim().Equals(String.Empty)));
        }

        #endregion

        #region FormatDate(DateTime date)

        public static string FormatDate(DateTime date)
        {
            return FormatDate(date, "/");
        }

        #endregion

        #region FormatDate(DateTime date, string separator)

        public static string FormatDate(DateTime date, string separator)
        {
            return string.Format("{0:D4}" + separator + "{1:D2}" + separator + "{2:D2}", date.Year, date.Month, date.Day);
        }

        #endregion

        #region FormatTime(DateTime date)

        public static string FormatTime(DateTime date)
        {
            return FormatTime(date, ":");
        }

        #endregion

        #region FormatTime(DateTime date, string separator)

        public static string FormatTime(DateTime date, string separator)
        {
            return string.Format("{0:D2}" + separator + "{1:D2}" + separator + "{2:D2}", date.Hour, date.Minute, date.Second);
        }

        #endregion

        #region FormatDateTime(DateTime date)

        public static string FormatDateTime(DateTime date)
        {
            return FormatDate(date) + " " + FormatTime(date);
        }

        #endregion

        #region FormatDateTime(DateTime date, string dateSeparator, string timeSeparator)

        public static string FormatDateTime(DateTime date, string dateSeparator, string timeSeparator)
        {
            return FormatDate(date, dateSeparator) + " " + FormatTime(date, timeSeparator);
        }

        #endregion

        #region DateTimeToString(DateTime dt)

        public static string DateTimeToString(DateTime dt)
        {
            return (dt == DateTime.MinValue) ? string.Empty : dt.ToString("MM/dd/yyyy");
        }

        #endregion

        #region GetTemplate(string strTemplateName)

        public static string GetTemplate(string strTemplateName)
        {
            string loc = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string filepath = loc + "\\HtmlTemplates\\" + strTemplateName;
            string html = System.IO.File.ReadAllText(filepath);
            return html;
        }

        #endregion

        #region SendEmail(string EmailMessage, string emailTo, string EmailSubject)

        public static void SendEmail(string attachmentName)
        {
            try
            {
                string host = ConfigurationManager.AppSettings["sendEmailHost"];
                string emailTo = ConfigurationManager.AppSettings["emailTo"];
                string emailCC = ConfigurationManager.AppSettings["emailCC"];
                string emailFrom = ConfigurationManager.AppSettings["emailFrom"];
                string pwd = ConfigurationManager.AppSettings["pwd"];
                string emailSubject = ConfigurationManager.AppSettings["emailSubject"];

                Log.write("-------------------------------------------------------");

                string testMode = System.Configuration.ConfigurationManager.AppSettings["TestMode"];
                if (testMode == "True")
                {
                    Log.write("In Test Mode. Original recipient: " + emailTo);

                    // Send report via Email - Test Mode
                    emailTo = System.Configuration.ConfigurationManager.AppSettings["TestEmailTo"];
                    emailCC = System.Configuration.ConfigurationManager.AppSettings["TestEmailCC"];
                }

                // Create the mail message object
                var mail = new System.Net.Mail.MailMessage(emailFrom, emailTo, emailSubject, "");

                string enableCC = System.Configuration.ConfigurationManager.AppSettings["enableCC"];
                if (enableCC == "True")
                {
                    Log.write("CC enable, sending to: " + emailCC);
                    // Add a carbon copy recipient.
                    MailAddress copy = new MailAddress(emailCC);
                    mail.Bcc.Add(copy);

                }

                string FileName = System.IO.Path.GetFileName(attachmentName);
                mail.Attachments.Add(new Attachment(FileName));

                mail.IsBodyHtml = true;
                var smtpClient = new System.Net.Mail.SmtpClient(host, 587);
                smtpClient.UseDefaultCredentials = true;
                smtpClient.Credentials = new System.Net.NetworkCredential(emailFrom, pwd);
                smtpClient.EnableSsl = true;
                smtpClient.Send(mail);

                Log.write("Report sent successfully to : " + emailTo);
            }
            catch (Exception ex)
            {
                Log.write("EXCEPTION: " + ex.Message);
                throw;
            }
        }

        public static async Task SendEmailWithModernAuthentication(byte[] attachmentBytes, string fileName)
        {
            try
            {
                string emailTo = ConfigurationManager.AppSettings["emailTo"];
                string emailFrom = ConfigurationManager.AppSettings["emailFrom"];
                string emailSubject = ConfigurationManager.AppSettings["subject"];

                string testMode = System.Configuration.ConfigurationManager.AppSettings["TestMode"];
                if (testMode == "True")
                {
                    emailTo = System.Configuration.ConfigurationManager.AppSettings["TestEmailTo"];
                }

                AppConfig appConfig = new AppConfig
                {
                    AppId = ConfigurationManager.AppSettings["AppId"],
                    AppSecret = ConfigurationManager.AppSettings["AppSecret"],
                    TenantId = ConfigurationManager.AppSettings["TenantId"],
                };

                Dictionary<string, byte[]> attachments = new Dictionary<string, byte[]>();
                attachments.Add(fileName, attachmentBytes);

                await MSGraphApiService.GetInstance(appConfig).SendEmail(emailSubject, "", emailFrom, new List<string> { emailTo }, attachments: attachments);
                Log.write("Email sent to: " + emailTo);

            }
            catch (Exception ex)
            {
                Log.write("EXCEPTION : " + ex.Message);
            }
        }

        #endregion

    }
}
