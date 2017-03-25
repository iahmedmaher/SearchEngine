using System;
using System.IO;
using System.Web;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace Spider
{
    class HtmlParser
    {
        /*** Ranking:
           * Title (12) -> H1 (10) -> H2 (8) -> H3 (6) -> H4 (5) -> H5 (5) -> H6 (5) -> b or strong (3) -> P or others (1)
           *
           ***/

        private static readonly Dictionary<string, int> Ranker = new Dictionary<string, int>(){
           {"title",12},
           {"h1",10},
           {"h2",8},
           {"h3",6},
           {"h4",5},
           {"h5",5},
           {"h6",5},
           {"b",3},
           {"strong",3},
           {"body",1}
        };

        private static HashSet<string> stopwords;

        private HtmlDocument doc;

        private string link;


        public HtmlParser(string html, string link)
        {
            this.link = link;
            doc = new HtmlDocument();

            doc.LoadHtml(html);

            doc.DocumentNode.Descendants().Where(n => n.Name == "script" || n.Name == "style" || n.NodeType == HtmlAgilityPack.HtmlNodeType.Comment).ToList().ForEach(n => n.Remove());
        }

        public static void IntializeStopWords()
        {
            string FileName = Properties.Settings.Default.PATH + Properties.Settings.Default.StopWordsFile;
            Stream TestFileStream = File.OpenRead(FileName);
            BinaryFormatter deserializer = new BinaryFormatter();
            stopwords = (HashSet<string>)deserializer.Deserialize(TestFileStream);
            TestFileStream.Close();
        }

        private string FixLink(string BaseUrl,string link)
        {
            if (!link.StartsWith(@"http://") && !link.StartsWith(@"https://") && !link.StartsWith(@"mailto:"))
            {
                if (link.StartsWith("//"))
                    return "http:" + link;
                else
                    return new Uri(new Uri(BaseUrl), link).AbsoluteUri;
            }

            return link;
        }

        public IEnumerable<String> GetOutGoingLinks()
        {
            var links_nodes = doc.DocumentNode.SelectNodes("//a[@href]");
            string current_link;
            if (links_nodes != null)
            {
                foreach (var node in links_nodes)
                {
                    current_link = FixLink(link, node.Attributes["href"].Value);
                    if (current_link.StartsWith("http"))
                        yield return current_link;
                }
            }
            
        }

        public string GetTitle()
        {
            var title = doc.DocumentNode.SelectSingleNode("//title");
            if (title != null)
                return HtmlEntity.DeEntitize(title.InnerHtml);
            return null;
        }


        public string PlainText()
        {
            return HttpUtility.HtmlDecode(string.Join(" ", doc.DocumentNode.Descendants()
                        .Where(n => !n.HasChildNodes && !string.IsNullOrWhiteSpace(n.InnerText))
                        .Select(n => n.InnerText)));
        }

        public Dictionary<string, int> KeywordsVectors()
        {
            Dictionary<string,int> dictionary=new Dictionary<string,int>();

            string word;

            foreach (var NodeType in Ranker)
            {
                var Nodes = doc.DocumentNode.Descendants(NodeType.Key);

                if (Nodes.Count() == 0)
                    continue;

                foreach (var Node in Nodes)
                {
                    var KeywordsMatches = Regex.Matches(HttpUtility.HtmlDecode(
                        string.Join(" ", Node.Descendants()
                        .Where(n => !n.HasChildNodes && !string.IsNullOrWhiteSpace(n.InnerText))
                        .Select(n => n.InnerText))
                        ), @"\b\w{2,}\b", RegexOptions.Compiled);

                    foreach (Match KeywordMatch in KeywordsMatches)
                    {
                        word = KeywordMatch.Value.ToLowerInvariant();

                        if (!stopwords.Contains(word))
                        {
                            IStemmer p = new Porter2();
                            word = p.stem(word);
                            if (dictionary.ContainsKey(word))
                                dictionary[word] += NodeType.Value;
                            else
                                dictionary.Add(word, NodeType.Value);
                        }
                    }
                }

                Nodes.ToList().ForEach(N => N.Remove());
            }

            return dictionary;
        }
    }
}
