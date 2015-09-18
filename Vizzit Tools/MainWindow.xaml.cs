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

namespace Vizzit_Tools
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            ListRadioBtn.IsChecked = true;
            GuiLogger.LogAdded += new EventHandler(GuiLogger_LogAdded);
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
            if(ListRadioBtn.IsChecked.Value)
            {
                List<Customer> customerList = new List<Customer>();

                foreach (Customer item in CustomerLsv.Items)
                    customerList.Add(item);

                int threads = Int32.Parse(coreTextBox.Text);

                Initialize ini = new Initialize(threads, customerList);

                DebugTextBlock.Text = "Started spider from list";
            }
            else if(DbRadioBtn.IsChecked.Value)
            {
                List<Customer> customerList = new List<Customer>();
                List<string> dbList = new List<string>();

                try
                {
                    Connector connector = new Connector();
                    dbList = SelectQuery.GetCustomerDbList();
                }
                catch (Exception ex)
                {
                    DebugTextBlock.Text += ex.Message;
                }

                foreach (string customerDb in dbList)
                {
                    Customer customer = new Customer();
                    try
                    {
                        customer.Domain = createFullurl(SelectQuery.GetDomain(customerDb)[0]);
                        customer.Id = SelectQuery.GetCustomerId(customerDb)[0];
                        customer.Startpage = SelectQuery.GetStartPage(customerDb)[0];
                        customerList.Add(customer);
                    }
                    catch (Exception ex)
                    {
                        DebugTextBlock.Text += ex.Message;
                    }
                }

                int threads = Int32.Parse(coreTextBox.Text);
                DebugTextBlock.Text = "Started spider from database";
                Initialize ini = new Initialize(threads, customerList);
                
            }
            else
                TempHomeFunc();
        }

        private string createFullurl(string url)
        {
            if (url.EndsWith(@"/"))
                url = url.Remove(url.Length - 1);

            if (!url.StartsWith(@"http://") || !url.StartsWith(@"https://"))
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
    }
}
