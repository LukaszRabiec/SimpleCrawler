namespace SimpleCrawler
{
    class Program
    {
        public delegate void MethodInvoker();

        private static void Main(string[] args)
        {
            const string mainUrl = "http://boards.4chan.org/k/";
            
            Crawler crawler = new Crawler(mainUrl);
            crawler.StartCrawling();
        }
    }
}