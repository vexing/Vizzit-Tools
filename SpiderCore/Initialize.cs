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
        //private List<string> customerStringList;
        private List<Customer> customerList;
        private int crawlCounter;
        private int custNo;
        private bool sendFile;
        private bool dailyCheck;

        /// <summary>
        /// Used to initialize the crawl
        /// </summary>
        /// <param name="threadMaxCount"></param>
        /// <param name="customersToParse"></param>
        /// <param name="sendFile"></param>
        /// <param name="dailyCheck"></param>
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
            bool threadPool = false;

            // We don't use the threadPool it is only for testing.
            if (threadPool)
            {
                while (customersToParse.Count > counter)
                    if (runningThreads < threadMaxCount)
                    {
                        if (!newlyStarted)
                        {
                            newlyStarted = true;
                            counter++;
                            runningThreads++;
                            ThreadPool.SetMaxThreads(1, 1);
                            ThreadPool.QueueUserWorkItem(new WaitCallback(ThreadProc));                            
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
            else
            {
                // Iterate until no more customers are in the list
                while (customersToParse.Count > counter)
                    // Make sure we don't start too many threads
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
                            // Ugly hack to not start 2 threads at the same time
                            Thread.Sleep(2000);
                            newlyStarted = false;
                        }
                    }
                    else
                        Thread.Sleep(2000);
            }
        }

        /// <summary>
        /// Used with threadPool
        /// </summary>
        /// <param name="stateInfo"></param>
        private void ThreadProc(object stateInfo)
        {
            Output output = new Core(customerList[++crawlCounter].Domain, customerList[crawlCounter].Id, customerList[crawlCounter].Database).
                StartSpider(++custNo, customerList[crawlCounter].Startpage, customerList[crawlCounter].Id, sendFile, dailyCheck);
            runningThreads--;
        }

        /// <summary>
        /// Starting the actual crawl
        /// </summary>
        private void startCrawlList()
        {
            Thread thread = Thread.CurrentThread;
            Output output = new Core(customerList[++crawlCounter].Domain, customerList[crawlCounter].Id, customerList[crawlCounter].Database).
                StartSpider(++custNo, customerList[crawlCounter].Startpage, customerList[crawlCounter].Id, sendFile, dailyCheck);
            runningThreads--;

            // Old shit when we thought we had memory problems
            output.Dispose();
            GC.Collect();
        }

        /// <summary>
        /// Creating and starting the thread
        /// </summary>
        private void startThreadList()
        {
            Thread thread = new Thread(new ThreadStart(startCrawlList));
            thread.Start();
        }
    }
}
