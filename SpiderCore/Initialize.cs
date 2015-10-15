using Spider;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private bool sendFile;
        private bool dailyCheck;

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

        public Initialize(List<string> urlsToParse, string customer_id)
        {
            Core core = new Core(customer_id);
            core.StartSpider(urlsToParse);
        }

        public Initialize(int threadMaxCount, List<Customer> customersToParse, bool sendFile, bool dailyCheck)
        {
            this.dailyCheck = dailyCheck;
            this.sendFile = sendFile;
            this.custNo = 0;
            this.crawlCounter = -1;
            customerList = customersToParse;
            this.threadMaxCount = threadMaxCount;
            this.runningThreads = 0;
            this.counter = 0;
            bool newlyStarted = false;

            Process currentProc = Process.GetCurrentProcess();
            GuiLogger.Log("Using " + currentProc.PrivateMemorySize64.ToString() + " before we start");

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

        private void startCrawlList()
        {
            Thread thread = Thread.CurrentThread;
            Output output = new Core(customerList[++crawlCounter].Domain, customerList[crawlCounter].Id, customerList[crawlCounter].Database).
                StartSpider(++custNo, customerList[crawlCounter].Startpage, customerList[crawlCounter].Id, sendFile, dailyCheck);
            runningThreads--;

            GC.Collect();
            Process currentProc = Process.GetCurrentProcess();
            GuiLogger.Log("Using " + currentProc.PrivateMemorySize64.ToString() + " when all is done");
           
        }

        private void startThreadList()
        {
            Thread thread = new Thread(new ThreadStart(startCrawlList));
            thread.Start();
        }

        private void startCrawl()
        {            
            Thread thread = Thread.CurrentThread;
            var spider = new Core(customerStringList[crawlCounter++], crawlCounter.ToString(), customerList[crawlCounter].Database);
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
