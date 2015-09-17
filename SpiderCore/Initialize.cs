using Spider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SpiderCore
{
    public class Initialize
    {
        private int counter;
        private int runningThreads;
        private int threadMaxCount;
        private List<string> customerStringList;
        private List<Customer> customerList;
        private int crawlCounter;
        private int custNo;

        public Initialize(int threadMaxCount)
        {
            this.threadMaxCount = threadMaxCount;
            runningThreads = 0;
            counter = 0;
            customerStringList = Db.Queries.SelectQuery.GetCustomerDbList();

            while (customerList.Count > counter)
                if (runningThreads < threadMaxCount)
                    startThread();
                else
                    Thread.Sleep(1000);
        }

        /*public Initialize(int threadMaxCount, List<Tuple<string, string>> urlsToParse)
        {
            this.custNo = 0;
            this.crawlCounter = 0;
            customerList = urlsToParse;
            this.threadMaxCount = threadMaxCount;
            this.runningThreads = 0;
            this.counter = 0;

            while (urlsToParse.Count > counter)
                if (runningThreads < threadMaxCount)
                {
                    counter++;
                    runningThreads++;
                    startThread();
                }
                else
                    Thread.Sleep(1000);
        }*/

        public Initialize(int threadMaxCount, List<Customer> customersToParse)
        {
            this.custNo = 0;
            this.crawlCounter = -1;
            customerList = customersToParse;
            this.threadMaxCount = threadMaxCount;
            this.runningThreads = 0;
            this.counter = 0;
            bool newlyStarted = false;

            while (customersToParse.Count > counter)
                if (runningThreads < threadMaxCount)
                {
                    if (!newlyStarted)
                    {
                        newlyStarted = true;
                        counter++;
                        runningThreads++;
                        startThreadList();
                    }
                    else
                    {
                        Thread.Sleep(2000);
                        newlyStarted = false;
                    }
                }
                else
                    Thread.Sleep(2000);
        }

        public Initialize(int threadMaxCount, List<string> urlsToParse)
        {
            this.custNo = 0;
            this.crawlCounter = 0;
            customerStringList = urlsToParse;
            this.threadMaxCount = threadMaxCount;
            this.runningThreads = 0;
            this.counter = 0;
            bool newlyStarted = false;

            while (urlsToParse.Count > counter)
                if (runningThreads < threadMaxCount)
                {
                    counter++;
                    runningThreads++;
                    if (!newlyStarted)
                    {
                        newlyStarted = true;
                        startThread();
                    }
                    else
                    {
                        Thread.Sleep(5000);
                        newlyStarted = false;
                    }
                }
                else
                    Thread.Sleep(5000);
        }

        private void startCrawlList()
        {
            Thread thread = Thread.CurrentThread;
            Output output = new Core(customerList[++crawlCounter].Domain, customerList[crawlCounter].Id).
                StartSpider(++custNo, customerList[crawlCounter].Startpage, customerList[crawlCounter].Id);
            runningThreads--;
        }

        private void startThreadList()
        {
            Thread thread = new Thread(new ThreadStart(startCrawlList));
            thread.Start();
        }

        private void startCrawl()
        {            
            Thread thread = Thread.CurrentThread;
            var spider = new Core(customerStringList[crawlCounter++], crawlCounter.ToString());
            Output output = spider.StartSpider(++custNo, "/");
            StringCompressor.CreateZipFile(output.jsonFileName);
            runningThreads--;
        }

        private void startThread()
        {
            Thread thread = new Thread(new ThreadStart(startCrawl));
            thread.Start();
        }
    }
}
