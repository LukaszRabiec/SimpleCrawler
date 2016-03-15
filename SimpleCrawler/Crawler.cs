#region Using

using HtmlAgilityPack;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;

#endregion

namespace SimpleCrawler
{
    public class Crawler
    {
        private readonly string _boardUrl;
        private readonly ConcurrentQueue<string> _threadsUrls = new ConcurrentQueue<string>();

        public Crawler(string boardUrl)

        {
            _boardUrl = boardUrl;
        }

        public void StartCrawling()
        {
            Console.WriteLine("Started...\nGetting available threads...");
            PrepareThreadsUrls();
            Console.WriteLine("Done.\nDownloading images... Please wait.");

            ThreadPool.SetMaxThreads(10, 10);

            while (_threadsUrls.Count != 0)
            {
                ThreadPool.QueueUserWorkItem(x =>
                {
                    string result;
                    if (_threadsUrls.TryDequeue(out result))
                    {
                        DownloadImgsFromUrl(result);
                    }
                });
            }

            Console.WriteLine("Done.\nNavigate to crawler directory for your images.");
        }

        private void PrepareThreadsUrls()
        {
            try
            {
                string responseFromServer;

                using (WebClient client = new WebClient())
                {
                    responseFromServer = client.DownloadString(_boardUrl);
                }

                HtmlDocument dom = new HtmlDocument();
                dom.LoadHtml(responseFromServer);

                List<HtmlNode> listOfNodes =
                    dom.DocumentNode.Descendants()
                        .Where(x => x.Name == "a" && x.InnerHtml.Contains("View Thread"))
                        .ToList();

                foreach (string threadUrl in listOfNodes.Select(hNode => hNode.Attributes["href"].Value))
                {
                    _threadsUrls.Enqueue(threadUrl);
                    Debug.WriteLine(threadUrl);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void DownloadImgsFromUrl(string threadUrl)
        {
            try
            {
                string responseFromServer;
                string fullThreadUrl = _boardUrl + threadUrl;
                List<string> listOfImgsUrls = new List<string>();

                using (WebClient client = new WebClient())
                {
                    responseFromServer = client.DownloadString(fullThreadUrl);
                }

                HtmlDocument dom = new HtmlDocument();
                dom.LoadHtml(responseFromServer);

                List<HtmlNode> listOfNodes =
                    dom.DocumentNode.Descendants()
                        .Where(x => x.Name == "img")
                        .ToList();

                string saveLocation = @"thread\" + threadUrl.Split('/').Last() + @"\";
                System.IO.Directory.CreateDirectory(saveLocation);

                foreach (string img in listOfNodes.Select(imgUrl => imgUrl.Attributes["src"].Value))
                {
                    string imageUrl = "http:" + img;
                    using (WebClient client = new WebClient())
                    {
                        client.DownloadFile(imageUrl, saveLocation + img.Split('/').Last());
                    }

                    listOfImgsUrls.Add(imageUrl);
                    Debug.WriteLine(imageUrl);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}