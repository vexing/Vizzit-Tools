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
using System.Web;
using System.Net;

namespace TestApp
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

        private void startButton_Click(object sender, RoutedEventArgs e)
        {
            testPage();
        }

        private void testPage()
        {
            try
            {
                Uri uri = new Uri(
                    "http://www.folkuniversitetet.se/Global/DOKUMENTBANKEN/Lokala%20dokument/Dokument_Region_Vast/G%c3%b6teborg/Diplomutbildningar/Ekvivaleringsblankett_familjeterapi_ht14.pdf", UriKind.Absolute);

                Uri encodedUri = encodeUri(uri);

                WebRequest.DefaultWebProxy = null;

                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(encodedUri);
                webRequest.Proxy = null;
                webRequest.AllowAutoRedirect = true;
                webRequest.MaximumAutomaticRedirections = 10;

                var webResponse = (HttpWebResponse)webRequest.GetResponse();
            }
            catch(Exception e)
            {
                textblock.Text = e.Message;
            }
        }

        private Uri encodeUri(Uri origUri)
        {
            string host = origUri.Scheme + @"://" + origUri.Host;
            string pathAndQuery = HttpUtility.UrlPathEncode(origUri.PathAndQuery);
            string fullUrl = host + pathAndQuery;

            return new Uri(fullUrl, UriKind.Absolute);
        }
    }
}
