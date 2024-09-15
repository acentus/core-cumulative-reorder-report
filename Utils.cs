//
// Copyright (c) 2022 by Acentus
// Developed by: Luis Cabrera
// gigocabrera@outlook.com
//

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Mail;
using System.Threading.Tasks;

namespace CoreCumulativeReorderReport
{
    /// <summary>
    /// This abstract class provides commonly used utility methods
    /// </summary>
    public abstract class Utils
    {
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
