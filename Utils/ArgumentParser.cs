using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace CalMan.Utils
{
    public static class ArgumentParser
    {
        public static IEnumerable<string> GetDomainEmails(IEnumerable<string> args, string domain)
        {
            var regex = new Regex(@"^(\w+)@" + domain + @"$", RegexOptions.Compiled);
            return args.Select(v => regex.Match(v)).Where(v => v.Success).Select(v => v.Value);
        }

        public static IEnumerable<string> GetSlackUserIds(IEnumerable<string> args)
        {
            var regex = new Regex(@"^<@(\w+)>$", RegexOptions.Compiled);
            return args.Select(v => regex.Match(v)).Where(v => v.Success).Select(v => v.Groups[1].Value);
        }

        public static IEnumerable<string> GetSlackGroupIds(IEnumerable<string> args)
        {
            var regex = new Regex(@"^<!subteam\^(\w+)\|@\w+>$", RegexOptions.Compiled);
            return args.Select(v => regex.Match(v)).Where(v => v.Success).Select(v => v.Groups[1].Value);
        }

        public static string GetUrlFromQuotedSlackText(string arg)
        {
            var match = new Regex(@"^<(http.?.+)>$").Match(arg);
            return match.Success ? match.Groups[1].Value : null;
        }
    }
}