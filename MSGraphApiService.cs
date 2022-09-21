//
// Copyright (c) 2022 by Acentus
// Developed by: Luis Cabrera
// gigocabrera@outlook.com
//

using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace CoreCumulativeReorderReport
{
    public class MSGraphApiService
    {
        private static object _lock = new object();
        private static MSGraphApiService _instance;
        private AppConfig _appConfig;
        static readonly HttpClient client = new HttpClient();
        private MSGraphApiService(AppConfig appConfig)
        {
            _appConfig = appConfig;
        }

        public static MSGraphApiService GetInstance(AppConfig appConfig)
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new MSGraphApiService(appConfig);
                    }
                }
            }
            return _instance;
        }


        public async Task SendEmail(string subject, string Body, string fromEmailAddress, List<string> toEmailAddresses, List<string> ccEmailAddresses = null, Dictionary<string, byte[]> attachments = null, int failureCount = 0)
        {
            try
            {
                var graphServiceClient = GetGraphClient();
                var toRecipients = new List<Recipient>();
                foreach (var toEmailAddress in toEmailAddresses)
                {
                    toRecipients.Add(
                    new Recipient
                    {
                        EmailAddress = new EmailAddress
                        {
                            Address = toEmailAddress
                        }
                    });
                }
                var message = new Message
                {
                    Subject = subject,
                    Body = new ItemBody
                    {
                        ContentType = BodyType.Html,
                        Content = Body
                    },
                    ToRecipients = toRecipients
                };
                if (ccEmailAddresses != null && ccEmailAddresses.Count > 0)
                {
                    var ccRecipients = new List<Recipient>();
                    foreach (var ccEmailAddress in ccEmailAddresses)
                    {
                        ccRecipients.Add(
                                            new Recipient
                                            {
                                                EmailAddress = new EmailAddress
                                                {
                                                    Address = ccEmailAddress
                                                }
                                            });
                    }
                    message.CcRecipients = ccRecipients;
                }

                if (attachments != null && attachments.Count > 0)
                {
                    message.Attachments = new MessageAttachmentsCollectionPage();
                    foreach (var attachment in attachments)
                    {
                        message.Attachments.Add(new FileAttachment
                        {
                            ContentBytes = attachment.Value,
                            Name = attachment.Key
                        });
                    }
                }
                await graphServiceClient.Users[fromEmailAddress].SendMail(message).Request().PostAsync();
            }
            catch (ServiceException ex)
            {
                string message = ex.Message;
                throw;
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                throw;
            }
        }

        private GraphServiceClient GetGraphClient()
        {
            var graphClient = new GraphServiceClient(new DelegateAuthenticationProvider((requestMessage) => {
                // get an access token for Graph
                var accessToken = GetAccessToken();

                requestMessage
                    .Headers
                    .Authorization = new AuthenticationHeaderValue("bearer", accessToken.Result);

                return Task.FromResult(0);
            }));
            return graphClient;
        }

        private async Task<string> GetAccessToken()
        {
            var _httpClient = new HttpClient();
            var url = String.Format("https://login.microsoftonline.com/{0}/oauth2/v2.0/token", _appConfig.TenantId);
            var content = new FormUrlEncodedContent(new Dictionary<string, string> {
              { "client_id", _appConfig.    AppId },
              { "grant_type", "client_credentials" },
              { "client_secret", _appConfig.AppSecret},
              { "scope", "https://graph.microsoft.com/.default" }
            });
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, new Uri(url))
            {
                Content = content
            };

            using (var response = await client.SendAsync(httpRequestMessage))
            {
                var responseStream = await response.Content.ReadAsStreamAsync();
                Office365TokenResponse myDeserializedClass = System.Text.Json.JsonSerializer.Deserialize<Office365TokenResponse>(responseStream);
                var token = myDeserializedClass.access_token;
                return token;
            }
        }
    }
}