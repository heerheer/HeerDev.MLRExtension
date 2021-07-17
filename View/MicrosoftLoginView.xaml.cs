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
using System.Windows.Shapes;

namespace HeerDev.MLRExtension.View
{
    /// <summary>
    /// Interaction logic for MicrosoftLoginView.xaml
    /// </summary>
    public partial class MicrosoftLoginView : Window
    {
        public string Code { get; set; } = "";

        public MicrosoftLoginView()
        {
            InitializeComponent();

        }

        private void WebBrowser_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            if (e.Uri.ToString().StartsWith("https://login.live.com/oauth20_desktop.srf?code="))
            {
                browser.Visibility = Visibility.Collapsed;
                // MessageBox.Show(e.Uri.ToString());
                string uri = e.Uri.ToString().Split('?')[1].Split('&')[0].Split('=')[1];
                // MessageBox.Show(uri);

                this.DialogResult = true;
                this.Code = uri;
                this.Close();
                return;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            browser.Source = new Uri("https://login.live.com/oauth20_authorize.srf%20?client_id=00000000402b5328%20&response_type=code%20&scope=service%3A%3Auser.auth.xboxlive.com%3A%3AMBI_SSL%20&redirect_uri=https%3A%2F%2Flogin.live.com%2Foauth20_desktop.srf");

        }
    }
}
