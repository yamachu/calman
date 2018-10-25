using System.Collections.Generic;
using System.Threading.Tasks;
using CalMan.Entities;
using Microsoft.Extensions.Logging;

namespace CalMan.Commands
{
    public class Help : IComamnd
    {
        private string EmailDomain;
        private ILogger logger;

        public Help(ILogger log, string domain)
        {
            EmailDomain = domain;
            logger = log;
        }

        public Task<string> Run(SlackPostEntity postParameter, IEnumerable<string> args)
        {
            return Task.FromResult(
                @"usage: 'calman command [args...]'
                * help
                  => show this message
                * add event_url [users...]
                  => invite users to target event
                  event_url: target event url
                    ex) https://calendar.google.com/calendar/r/eventedit/SOME_HASHED_STR
                  user: space sepatated target id or email (if no args, set target as sender)
                    ex) @target_user @g_target_group target_email_address@{ARGS_EMAIL_DOMAIN}
                ".Replace("{ARGS_EMAIL_DOMAIN}", EmailDomain));
        }
    }
}