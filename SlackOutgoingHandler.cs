
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using CalMan.Utils;
using CalMan.Entities;
using CalMan.Services;
using CalMan.Commands;
using Microsoft.Extensions.Configuration;
using System.Runtime.InteropServices;

namespace CalMan
{
    public static class SlackOutgoingHandler
    {
        static bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        static SlackOutgoingHandler()
        {
            var builder = new ConfigurationBuilder()
                            .AddJsonFile("local.settings.json", true)
                            .AddEnvironmentVariables();

            Configuration = builder.Build();
        }

        private static IConfigurationRoot Configuration { get; }

        static void InitializeServices(ILogger log)
        {
            var googleAPIDataStore = new GoogleAPIOnMemoryDataStore(new Dictionary<string, dynamic>
            {
                ["user"] = new Google.Apis.Auth.OAuth2.Responses.TokenResponse
                {
                    AccessToken = Configuration.GetValue("MYGOOGLEAPITOKEN_access_token", "YOU_MUST_SET_YOUR_MYGOOGLEAPITOKEN_access_token"),
                    TokenType = Configuration.GetValue("MYGOOGLEAPITOKEN_token_type", "YOU_MUST_SET_YOUR_MYGOOGLEAPITOKEN_token_type"),
                    ExpiresInSeconds = long.Parse(Configuration.GetValue("MYGOOGLEAPITOKEN_expires_in", "3600")),
                    RefreshToken = Configuration.GetValue("MYGOOGLEAPITOKEN_refresh_token", "YOU_MUST_SET_YOUR_MYGOOGLEAPITOKEN_refresh_token"),
                    Scope = Configuration.GetValue("MYGOOGLEAPITOKEN_scope", "YOU_MUST_SET_YOUR_MYGOOGLEAPITOKEN_scope"),
                    Issued = DateTime.Parse(Configuration.GetValue("MYGOOGLEAPITOKEN_Issued", "2000-01-01T09:00:00.000+09:00")),
                    IssuedUtc = DateTime.Parse(Configuration.GetValue("MYGOOGLEAPITOKEN_IssuedUtc", "2000-01-01T00:00:00.000Z")),
                }
            });
            // var googleAPIDataStore = new FileDataStore(Configuration.GetValue(IsWindows ? "CALENDAR_TOKEN_PATH_WINDOWS" : "CALENDAR_TOKEN_PATH_xNIX", "REPLACE"));

            SlackAPIClient.Instance.SlackAPIKey = Configuration.GetValue("SLACK_API_KEY", "YOU_MUST_SET_YOUR_SLACK_API_KEY");
            GoogleCalendarClient.Instance.SetCredentials(Configuration.GetValue("GOOGLE_CALENDAR_CLIENT_ID", "REPLACE"),
            Configuration.GetValue("GOOGLE_CALENDAR_CLIENT_SECRET", "REPLACE"),
            googleAPIDataStore);
            GoogleCalendarClient.Instance.CalendarId = Configuration.GetValue("MANAGE_CALENDAR_ID", "primary");
        }

        [FunctionName("SlackOutgoingHandler")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]HttpRequest req, ILogger log)
        {
            log.LogInformation("Slack request recieved, starting process.");
            InitializeServices(log);

            var slackParams = new SlackPostEntity(req.Form);
            log.LogInformation(JsonConvert.SerializeObject(slackParams));

            var outgoingToken = Configuration.GetValue("SLACK_OUTGOING_TOKEN", "show: https://[YOUR-SLACK_GROUP].slack.com/apps/manage/custom-integrations");
            if (outgoingToken != slackParams.Token)
            {
                return new BadRequestResult();
            }

            // 'calman <command> <args>'
            var splittedText = slackParams.Text.Split(new char[0], StringSplitOptions.RemoveEmptyEntries).Skip(1);
            var commandStr = splittedText.FirstOrDefault() ?? CommandType.Help.ToCommandString();
            CommandType command;

            try
            {
                command = EnumHelper<CommandType>.FromString(commandStr);
            }
            catch (Exception e)
            {
                log.LogTrace(e, $"Undefined command from {slackParams.UserId} => {slackParams.Text}");
                return new BadRequestResult();
            }

            switch (command)
            {
                case CommandType.Help:
                    return new OkObjectResult(new
                    {
                        Text = await new Help(log, Configuration.GetValue("EMAIL_DOMAIN", "gmail.com")).Run(slackParams, splittedText)
                    });
                case CommandType.Add:
                    return new OkObjectResult(new
                    {
                        Text = await new Add(log, Configuration.GetValue("EMAIL_DOMAIN", "gmail.com")).Run(slackParams, splittedText.Skip(1))
                    });
                default:
                    return new BadRequestResult();
            }
        }
    }
}
