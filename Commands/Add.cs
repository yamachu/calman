using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CalMan.Entities;
using CalMan.Services;
using CalMan.Utils;
using Microsoft.Extensions.Logging;

namespace CalMan.Commands
{
    public class Add : IComamnd
    {
        private string EmailDomain;
        private ILogger logger;

        public Add(ILogger log, string domain)
        {
            EmailDomain = domain;
            logger = log;
        }

        public async Task<string> Run(SlackPostEntity postParameter, IEnumerable<string> args)
        {
            var eventUrl = ArgumentParser.GetUrlFromQuotedSlackText(args.Take(1).SingleOrDefault() ?? "");
            if (eventUrl == null) throw new System.ArgumentException("eventUrl is required");

            var domainEmails = ArgumentParser.GetDomainEmails(args.Skip(1), EmailDomain);
            var userIds = ArgumentParser.GetSlackUserIds(args.Skip(1));
            var groupIds = ArgumentParser.GetSlackGroupIds(args.Skip(1));

            var users = (userIds.Count() == 0 && groupIds.Count() == 0 && domainEmails.Count() == 0
            ? await GetInvitedUserData(new List<string> { postParameter.UserId }, new List<string>())
            : await GetInvitedUserData(userIds, groupIds));

            var emails = users.Select(v => v.Profile.Email).Union(domainEmails).ToList();

            var result = await GoogleCalendarClient.Instance.InviteUsers(eventUrl, emails);
            return result.Count() == 0
            ? "New attendee is None"
            : $"Invited {result.Count()} crew(s).";
        }

        private async Task<IEnumerable<SlackUserEntity>> GetInvitedUserData(IEnumerable<string> slackUsers, IEnumerable<string> slackGroups)
        {
            var groupUsers = await Task.WhenAll(
                slackGroups.Select(async v => await SlackAPIClient.Instance.GetGroupUsers(v))
            );

            var willInvitedUsers = groupUsers
            .SelectMany(v => v)
            .Union(slackUsers).ToList();

            var members = await SlackAPIClient.Instance.GetUsers();

            return members.Join(willInvitedUsers, allUser => allUser.Id, invitedUser => invitedUser, (user, _) => user);
        }
    }
}