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
           * keywords(14) -> Title (12) -> description (11) -> H1 (10) -> H2 (8) -> H3 (6) -> H4 (5) -> H5 (5) -> H6 (5) -> b or strong (3) -> P or others (1)
           *
           ***/

        private static readonly Dictionary<string, int> Ranker = new Dictionary<string, int>(){
           {"keywords",14 },
           {"title",12},
           {"description",11 },
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

        private string SourceLink;


        public HtmlParser(string html, string link)
        {
            this.SourceLink = link;
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

        private string FixLink(string link)
        {
            if (!link.StartsWith(@"http://") && !link.StartsWith(@"https://") && !link.StartsWith(@"mailto:") && !link.StartsWith(@"tel:"))
            {
                if (link.StartsWith("//"))
                    return "http:" + link;
                else
                    return new Uri(new Uri(SourceLink), link).AbsoluteUri;
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
                    current_link = FixLink(node.Attributes["href"].Value);
                    if (current_link.StartsWith("http"))
                        yield return current_link;
                }
            }

        }

        public string GetTitle()
        {
            var title = doc.DocumentNode.SelectSingleNode("//title");
            if (title != null)
                return HttpUtility.HtmlDecode(title.InnerHtml);
            return null;
        }

        public string GetMetaKeywords()
        {
            var Keywords = doc.DocumentNode.SelectSingleNode("//meta[translate(@name, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz')='keywords']");
            if (Keywords != null)
                return HttpUtility.HtmlDecode(Keywords.Attributes["content"].Value);
            return null;
        }

        public string GetMetaDescription()
        {
            var Description = doc.DocumentNode.SelectSingleNode("//meta[translate(@name, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz')='description']");
            if (Description != null)
                return HttpUtility.HtmlDecode(Description.Attributes["content"].Value);
            return null;
        }

        public string PlainText()
        {
            string text = "";
            string description = GetMetaDescription();
            string keywords = GetMetaKeywords();
            if (description != null)
            {
                text += description;
            }
            if (keywords != null)
            {
                text += keywords;
            }
            return text + HttpUtility.HtmlDecode(string.Join(" ", doc.DocumentNode.Descendants()
                        .Where(n => !n.HasChildNodes && !string.IsNullOrWhiteSpace(n.InnerText))
                        .Select(n => n.InnerText)));
        }

        public Dictionary<string, int> KeywordsVectors()
        {
            Dictionary<string, int> dictionary = new Dictionary<string, int>();

            GetMeta(dictionary);

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

        private void GetMeta(Dictionary<string, int> dictionary)
        {
            string description = GetMetaDescription();

            if (description != null)
            {
                var arr = description.Split(' ');
                foreach (var w in arr)
                {
                    if (!stopwords.Contains(w))
                    {
                        IStemmer p = new Porter2();
                        string stemmed = p.stem(w);
                        if (dictionary.ContainsKey(stemmed))
                            dictionary[stemmed] += Ranker["keywords"];
                        else
                            dictionary.Add(stemmed, Ranker["keywords"]);
                    }
                }
            }

            string keywords = GetMetaKeywords();

            if (keywords != null)
            {
                var arr = keywords.Split(',');
                foreach (var w in arr)
                {

                    IStemmer p = new Porter2();
                    string stemmed = p.stem(w);

                    if (dictionary.ContainsKey(stemmed))
                        dictionary[stemmed] += Ranker["description"];
                    else
                        dictionary.Add(stemmed, Ranker["description"]);

                }

            }
        }
    }
}