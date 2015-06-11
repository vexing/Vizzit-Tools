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
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            var spider = new Core(@"http://www.vizzit.se");
            string firstPage = "/";
            spider.StartSpider(firstPage);
            DebugTextBlock.Text = spider.errorMsg;

            /*if (DomainValidation())
            {
                DebugTextBlock.Text = "Crawl Started!";
                var spider = new Core(DomainTextBox.Text);
                spider.StartSpider();
            }
            else
                DebugTextBlock.Text = "Domain value incorrect!";*/
        }

        private bool DomainValidation()
        {
            if (String.IsNullOrEmpty(DomainTextBox.Text))
                return false;
            else
                return true;
        }
    }
}
