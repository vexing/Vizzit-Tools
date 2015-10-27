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
            worker = new BackgroundWorker();
            worker.WorkerSupportsCancellation = true;
            worker.DoWork += worker_DoWork;
        }

        void GuiLogger_LogAdded(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() => 
            {
                DebugTextBlock.Text = DebugTextBlock.Text + Environment.NewLine + GuiLogger.GetLastLog();
            })); 
        }

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
                        item.Database = SelectQuery.GetDatabase(item.Id)[0];
                    }
                    catch(Exception ex)
                    {
                        GuiLogger.Log(ex.Message);
                        item.Database = null;
                    }
                    customerList.Add(item);
                }
            }
            else if (DbRadioBtn.IsChecked.Value)
            {
                List<string> dbList = SelectQuery.GetCustomerDbList();                

                foreach (string customerDb in dbList)
                {
                    try
                    {
                        string shouldSpider = SelectQuery.GetShouldSpider(customerDb)[0];
                        if (customerDb.StartsWith("v2_rs_") || customerDb.Contains("intranet") || shouldSpider.Equals("False"))
                            continue;
                    }
                    catch(Exception ex)
                    {
                        GuiLogger.Log(ex.Message);
                        continue;
                    }

                    Customer customer = new Customer();
                    try
                    {
                        customer.Domain = createFullurl(SelectQuery.GetDomain(customerDb)[0]);
                        customer.Id = SelectQuery.GetCustomerId(customerDb)[0];
                        customer.Startpage = firstPageFix(SelectQuery.GetStartPage(customerDb)[0], customer.Id);
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

        private string firstPageFix(string url, string customerId)
        {
            if (customerId == "plannja.com")
                return "/international/";
            else
                return url;
        }

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            Initialize ini = new Initialize(threads, customerList, sendFile, dailyCheck);
        }                                

        private string createFullurl(string url)
        {
            if (url.EndsWith(@"/"))
                url = url.Remove(url.Length - 1);

            if (!url.StartsWith(@"http://") && !url.StartsWith(@"https://"))
                url = @"http://" + url;
            return url;
        }

        private void TempHomeFunc()
        {
            List<string> urlList = new List<string>(); 

            urlList.Add(@"http://www.molndal.se");

            Initialize ini = new Initialize(1, urlList);
        }

        private bool DomainValidation()
        {
            if (String.IsNullOrEmpty(DomainTextBox.Text))
                return false;
            else
                return true;
        }

        private void AddBtn_Click(object sender, RoutedEventArgs e)
        {
            CustomerLsv.Items.Add(new Customer { Domain = DomainTextBox.Text, Startpage = StartPageTextBox.Text, Id = CustomerIdTextBox.Text });            
        }

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

        private void ClrBtn_Click(object sender, RoutedEventArgs e)
        {
            CustomerLsv.Items.Clear();
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            // Cancel the asynchronous operation.
            worker.CancelAsync();

            // Disable the Cancel button.
            cancelButton.IsEnabled = false;
        }

        private void quitBtn_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
