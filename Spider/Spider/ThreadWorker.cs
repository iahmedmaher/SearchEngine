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

            string link = (obj as ThreadParameter).link;
            ConcurrentQueue<string> Scheduled_links = (obj as ThreadParameter).queue;

            bool Revisted = false;

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

            if (!RobotstxtParser.Approved(link))
            {
                return;
            }


            if (Controller.OperationCancelled)
                return;

            reporter.Invoke(reporter.ReportStartProcessing, link);

            var response = HttpDownloader.GetHtml(link);

            if (Controller.OperationCancelled)
                return;

            if (response == null)
                return;
            
            HtmlParser doc = new HtmlParser(response.Item1, link);

            link = response.Item2;  //Get reponse redirect link

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

            Dictionary<string, double> imagesDictionary = doc.KeywordsVectorsFromImages(images);

            Dictionary<string, double> textDictionary = doc.KeywordsVectorsFromText();

            Dictionary<string, double> dictionary = doc.MergeDictionaries(textDictionary, imagesDictionary);

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
            }
            else
            {
                Database.AddLink(link, title, linkscount);
                Database.AddPageVector(link, dictionary);
                Database.AddPageContent(link, PlainText);
                Database.AddPageImages(link, images);
            }


            if (Controller.OperationCancelled)
                return;
          
            reporter.Invoke(reporter.ReportStatistics, dictionary);
 
            reporter.Invoke(reporter.ReportProcessed, link);

        }

    }
}
