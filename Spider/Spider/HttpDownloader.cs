using System;
using System.IO;
using System.Text;
using System.Net;
using System.Net.Mime;
using System.Threading;

namespace Spider
{
    class HttpDownloader
    {
        private static long Totaldata = 0;

        public static long DataTransmitted
        {
            get
            {
                return Totaldata;
            }
        }

        private HttpDownloader() 
        {

        }
        

        private static string __getrequiredcontent(string Link,string ContentType)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest) WebRequest.Create(Link);

                request.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1; .NET CLR 1.0.3705;)";

                request.KeepAlive = false;

                //request.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1; .NET CLR 1.0.3705;)");

                //request.Headers.Add(HttpRequestHeader.UserAgent, "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/56.0.2924.87 Safari/537.36");

                using (WebResponse response = request.GetResponse())
                {
                    if (!response.Headers[HttpResponseHeader.ContentType].Contains(ContentType))
                    {
                        return null;
                    }

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

                    using (StreamReader reader = new StreamReader(dataStream,encoding))
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

        public static string GetHtml(string link)
        {
            return __getrequiredcontent(link, "html");
        }

        public static string GetRobotsTxt(string domain)
        {
            return __getrequiredcontent("http://" + domain + "/robots.txt", "text");                       
        }

    }
}
