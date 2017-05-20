using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Spider
{
    class ThreadWorker
    {
        DBController Database;
        GUI reporter;

        public ThreadWorker(GUI RP)
        {
            Database = DBController.GetInstance();
            reporter = RP;
        }
        
        public void ThreadProc(object obj)
        {
            if (Controller.OperationCancelled)
                return;

            HttpDownloader downloader = HttpDownloader.GetInstance();

            string link = (obj as ThreadParameter).link;
            ConcurrentQueue<string> Scheduled_links = (obj as ThreadParameter).queue;

            bool Revisted = false;
            
            if(downloader.IsVisited(link))
            {
                Database.LinkHit(downloader.GetRedirectOf(link));
                return;
            }
            /*
            else if (Database.LinkExists(link))
            {
                Database.LinkHit(link);

                if ((DateTime.Now - Database.GetLinkDate(link).GetValueOrDefault()).TotalDays >= 7)
                {
                    Revisted = true;
                }
                else
                {
                    return;
                }
            }
            */
            if (!RobotstxtParser.Approved(link))
            {
                return;
            }


            if (Controller.OperationCancelled)
                return;

            reporter.Invoke(reporter.ReportStartProcessing, link);

            var response = downloader.GetHtml(link);

            if (Controller.OperationCancelled)
                return;

            if (response == null)
                return;
            
            HtmlParser doc = new HtmlParser(response, link);

            link = downloader.GetRedirectOf(link);  //Get reponse redirect link

            if (Database.LinkExists(link))
            {
                Database.LinkHit(link);

                if ((DateTime.Now - Database.GetLinkDate(link).GetValueOrDefault()).TotalDays > 7)
                {
                    Revisted = true;
                }
                else
                {
                    return;
                }
            }

            int linkscount = 0;


            if (!Revisted)
            {
                IEnumerable<string> links_list = doc.GetOutGoingLinks();
                HashSet<string> Distinct = new HashSet<string>(links_list);
                StringBuilder queued = new StringBuilder();


                foreach (string Link in Distinct)
                {
                    linkscount++;

                    Scheduled_links.Enqueue(Link);
                    
                    if (Controller.OperationCancelled)
                        return;

                    queued.Append(Link);
                    queued.Append(Environment.NewLine);
                }

                reporter.Invoke(reporter.ReportQueued, queued.ToString());
            }

            string title = doc.GetTitle();

            string PlainText = doc.PlainText();

            Dictionary<string, string> images = doc.ImagesVectors();
            //Dictionary<string, string> ordered_lists = doc.GetOrderedLists();

            Dictionary<string, double> dictionary = doc.KeywordsVectors();

            if (dictionary == null)
                return;


            if (Controller.OperationCancelled)
                return;


            //redundant re check but needed because race condition may occuar leading to deplicate entry
            if (Database.LinkExists(link) && !Revisted)
            {
                Database.LinkHit(link);
                return;
            }
            else if (Revisted)
            {
                Database.UpdateLinkDate(link);
                Database.UpdateLinkTitle(link, title);
                Database.UpdatePageVector(link, dictionary);
                Database.UpdatePageContent(link, PlainText);
                //if (ordered_lists.Count > 0)
                //   Database.UpdatePageStepsList(link, ordered_lists);
                if (images.Count > 0)
                    Database.AddPageImages(link, images);
            }
            else
            {
                Database.AddLink(link, title, linkscount);
                Database.AddPageVector(link, dictionary);
                Database.AddPageContent(link, PlainText);

                //if (ordered_lists.Count > 0) 
                //    Database.AddPageStepsList(link, ordered_lists);
                if (images.Count > 0)
                  Database.AddPageImages(link, images);
            }


            if (Controller.OperationCancelled)
                return;
          
            reporter.Invoke(reporter.ReportStatistics, dictionary);
 
            reporter.Invoke(reporter.ReportProcessed, link);

        }

    }
}
