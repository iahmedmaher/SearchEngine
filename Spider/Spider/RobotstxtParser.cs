using System.Collections.Concurrent;
using System.Text;
using System.Text.RegularExpressions;

namespace Spider
{
    class RobotstxtParser
    {
        static ConcurrentDictionary<string, MatchCollection> DisallowCache = new ConcurrentDictionary<string, MatchCollection>();
        static ConcurrentDictionary<string, MatchCollection> AllowCache = new ConcurrentDictionary<string, MatchCollection>();
        
        private RobotstxtParser()
        {

        }

        public static bool Approved(string link)
        {
            string domain = new System.Uri(link).Host;

            bool allowed = true;
            MatchCollection Disallows;
            MatchCollection Allows;

            if (DisallowCache.ContainsKey(domain))
            {
                Allows = AllowCache[domain];
                Disallows = DisallowCache[domain];
                
                if (Allows == null || Disallows == null)
                    return true;
            }
            else
            {
                string robotstxt = HttpDownloader.GetRobotsTxt(domain);

                if (robotstxt == null)
                {
                    DisallowCache.TryAdd(domain, null);
                    AllowCache.TryAdd(domain, null);
                    return true;
                }

                string robotsmatch = Regex.Match(robotstxt, @"(?<=User\-agent\: \*).+?(?=(User-agent\:)|$)", RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.Compiled).Value;

                Disallows = Regex.Matches(robotsmatch, @"(?<=Disallow: ).+?(?=(\u000D|( +#)|$))", RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Compiled);
                Allows = Regex.Matches(robotsmatch, @"(?<=[^(Dis)]allow: ).+?(?=(\u000D|( +#)|$))", RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Compiled);

                DisallowCache.TryAdd(domain, Disallows);
                AllowCache.TryAdd(domain, Allows);
            }

            StringBuilder PatternBuilder;
            string pattern;

            foreach (Match disallow in Disallows)
            {
                PatternBuilder = new StringBuilder(disallow.Value).Replace(@"\", @"\\").Replace(@"/", @"\/").Replace(".", @"\.").Replace("?", @"\?").Replace("+", @"\+").Replace("*", @".*?").Replace("(", @"\(").Replace(")", @"\)").Replace("^", @"\^").Replace("$", @"\$").Replace("[", @"\[").Replace("]", @"]").Replace("{", @"\}").Replace("}", @"\}").Replace("|",@"\|");
                pattern = PatternBuilder.ToString();
                if (Regex.IsMatch(link, pattern))
                {
                    allowed = false;
                    break;
                }
            }

            foreach (Match allow in Allows)
            {
                PatternBuilder = new StringBuilder(allow.Value).Replace(@"\", @"\\").Replace(@"/", @"\/").Replace(".", @"\.").Replace("?", @"\?").Replace("+", @"\+").Replace("*", @".*?").Replace("(", @"\(").Replace(")", @"\)").Replace("^", @"\^").Replace("$", @"\$").Replace("[", @"\[").Replace("]", @"]").Replace("{", @"\}").Replace("}", @"\}").Replace("|", @"\|");
                pattern = PatternBuilder.ToString();
                if (Regex.IsMatch(link, pattern))
                {
                    allowed = true;
                    break;
                }
            }

            return allowed;
        }
    }
}
