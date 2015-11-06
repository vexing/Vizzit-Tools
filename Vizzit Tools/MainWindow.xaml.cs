using SpiderCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using FileHandler;
using SpiderCore.Db;
using SpiderCore.Db.Queries;
using System.Threading;
using System.ComponentModel;

namespace Vizzit_Tools
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        int threads;
        List<Customer> customerList;
        private readonly BackgroundWorker worker;
        private bool sendFile;
        private bool dailyCheck;

        public MainWindow()
        {
            InitializeComponent();
            cancelButton.IsEnabled = false;
            ListRadioBtn.IsChecked = true;
            GuiLogger.LogAdded += new EventHandler(GuiLogger_LogAdded);
            GuiLogger.RunningCustomerChanged += new EventHandler(GuiLogger_GetCustomerList);
            worker = new BackgroundWorker();
            worker.WorkerSupportsCancellation = true;
            worker.DoWork += worker_DoWork;
        }

        /// <summary>
        /// Adds from guilogger to debugTextBlock
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void GuiLogger_LogAdded(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() => 
            {
                DebugTextBlock.Text = DebugTextBlock.Text + Environment.NewLine + GuiLogger.GetLastLog();
            })); 
        }

        /// <summary>
        /// Checks for running customers
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void GuiLogger_GetCustomerList(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                List<RunningCustomer> runningCustomersList = GuiEvents.runningCustomerList(GuiLogger.getRunningCustomersList());
                foreach (RunningCustomer rc in runningCustomersList)
                    RunningCustomersLV.Items.Add(new RunningCustomer { Customer = rc.Customer, Time = rc.Time });
            }));
        }

        /// <summary>
        /// Starts the crawl
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (SendFileCheckBox.IsChecked == true)
                sendFile = true;
            else
                sendFile = false;

            if (dailyCheckBox.IsChecked == true)
                dailyCheck = true;
            else
                dailyCheck = false;

            threads = Int32.Parse(coreTextBox.Text);
            customerList = new List<Customer>();

            try
            {
                Connector connector = new Connector();
            }
            catch (Exception ex)
            {
                DebugTextBlock.Text += ex.Message;
            }

            if(ListRadioBtn.IsChecked.Value)
            {
                foreach (Customer item in CustomerLsv.Items)
                {
                    try
                    {
                        item.Database = SelectQueryModel.GetDatabase(item.Id)[0];
                    }
                    catch(Exception ex)
                    {
                        string em = ex.Message;
                        item.Database = null;
                    }
                    customerList.Add(item);
                }
            }
            else if (DbRadioBtn.IsChecked.Value)
            {
                List<string> dbList = SelectQueryModel.GetCustomerDbList();                

                foreach (string customerDb in dbList)
                {
                    try
                    {
                        string shouldSpider = SelectQueryModel.GetShouldSpider(customerDb)[0];
                        if (customerDb.StartsWith("v2_rs_") || customerDb.Contains("intranet") || shouldSpider.Equals("False"))
                            continue;
                    }
                    catch(Exception ex)
                    {
                        string em = ex.Message;
                        continue;
                    }

                    Customer customer = new Customer();
                    try
                    {
                        customer.Domain = createFullurl(SelectQueryModel.GetDomain(customerDb)[0]);
                        customer.Id = SelectQueryModel.GetCustomerId(customerDb)[0];
                        customer.Startpage = firstPageFix(SelectQueryModel.GetStartPage(customerDb)[0], customer.Id);
                        customer.Database = customerDb;
                        customerList.Add(customer);
                    }
                    catch (Exception ex)
                    {
                        DebugTextBlock.Text += ex.Message;
                    }
                }
            }

            if (worker.IsBusy != true)
            {
                cancelButton.IsEnabled = true;
                worker.RunWorkerAsync();
            }
        }

        /// <summary>
        /// Stupid hack for plannja
        /// </summary>
        /// <param name="url"></param>
        /// <param name="customerId"></param>
        /// <returns></returns>
        private string firstPageFix(string url, string customerId)
        {
            if (customerId == "plannja.com")
                return "/international/";
            else
                return url;
        }

        /// <summary>
        /// Do work! 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            Initialize ini = new Initialize(threads, customerList, sendFile, dailyCheck);
        }                                

        /// <summary>
        /// Make sure the first url is a valid url for a webrequest
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private string createFullurl(string url)
        {
            if (url.EndsWith(@"/"))
                url = url.Remove(url.Length - 1);

            if (!url.StartsWith(@"http://") && !url.StartsWith(@"https://"))
                url = @"http://" + url;
            return url;
        }

        /// <summary>
        /// Some validation, yay. More is needed:(
        /// </summary>
        /// <returns></returns>
        private bool DomainValidation()
        {
            if (String.IsNullOrEmpty(DomainTextBox.Text))
                return false;
            else
                return true;
        }

        /// <summary>
        /// Add a customer to the list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddBtn_Click(object sender, RoutedEventArgs e)
        {
            CustomerLsv.Items.Add(new Customer { Domain = DomainTextBox.Text, Startpage = StartPageTextBox.Text, Id = CustomerIdTextBox.Text });            
        }

        /// <summary>
        /// Remove a customer from the list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RemoveBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CustomerLsv.Items.RemoveAt(CustomerLsv.Items.IndexOf(CustomerLsv.SelectedItem));
            }
            catch(ArgumentOutOfRangeException)
            {
                DebugTextBlock.Text = "Select an item first.";
            }
        }

        /// <summary>
        /// Clear the list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClrBtn_Click(object sender, RoutedEventArgs e)
        {
            CustomerLsv.Items.Clear();
        }

        /// <summary>
        /// Should be removed, it is not working with the new way of handling threads...Will be alot of work to make the threads abortable.
        /// We would need some kind of check in the core code if the button is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            // Cancel the asynchronous operation.
            worker.CancelAsync();

            // Disable the Cancel button.
            cancelButton.IsEnabled = false;
        }

        /// <summary>
        /// Close the application. Threads is badly written so they will finish before they clear memory.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void quitBtn_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
