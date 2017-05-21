using System;
using System.IO;
using System.Text;
using System.Net;
using System.Net.Mime;
using System.Threading;
using System.Collections.Concurrent;

namespace Spider
{
    class HttpDownloader
    {
        private static long Totaldata = 0;
        private ConcurrentDictionary<string, string> DownloadedLinks;
        private static HttpDownloader singleton;

        public static HttpDownloader GetInstance()
        {
            if (singleton == null)
            {
                singleton = new HttpDownloader();
            }
            return singleton;
        }

        public static long DataTransmitted
        {
            get
            {
                return Totaldata;
            }
        }

        private HttpDownloader()
        {
            DownloadedLinks = new ConcurrentDictionary<string, string>();
        }

        private string __getrequiredcontent(string Link, string ContentType)
        {
            if (DownloadedLinks.ContainsKey(Link))
                return null;
            
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Link);

                //"Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1; .NET CLR 1.0.3705;)";
                //"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/56.0.2924.87 Safari/537.36";
                request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/56.0.2924.87 Safari/537.36";

                //prefer english language
                request.Headers.Add("Accept-Language", "en-US,en;q=0.9");

                request.KeepAlive = false;

                using (WebResponse response = request.GetResponse())
                {
                    if (!response.Headers[HttpResponseHeader.ContentType].Contains(ContentType))
                    {
                        DownloadedLinks.TryAdd(Link, Link);
                        return null;
                    }

                    DownloadedLinks.TryAdd(Link, response.ResponseUri.AbsoluteUri);
                    
                    var encoding = Encoding.UTF8; //The default of the web is UTF-8

                    var contentType = new ContentType(response.Headers[HttpResponseHeader.ContentType]);

                    if (!String.IsNullOrEmpty(contentType.CharSet))
                    {
                        encoding = Encoding.GetEncoding(contentType.CharSet);
                    }

                    if (Controller.OperationCancelled)
                    {
                        return null;
                    }

                    Stream dataStream = response.GetResponseStream();

                    using (StreamReader reader = new StreamReader(dataStream, encoding))
                    {
                        if (Controller.OperationCancelled)
                        {
                            return null;
                        }
                        string html = reader.ReadToEnd();
                        Interlocked.Add(ref Totaldata, encoding.GetByteCount(html));
                        return html;
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public string GetHtml(string link)
        {
            return __getrequiredcontent(link, "html");
        }

        public string GetRobotsTxt(string domain)
        {
            return __getrequiredcontent("http://" + domain + "/robots.txt", "text");
        }

        public string GetRedirectOf(string link)
        {
            return DownloadedLinks[link];
        }

        public bool IsVisited(string Link)
        {
            return DownloadedLinks.ContainsKey(Link);
        }

    }
}
