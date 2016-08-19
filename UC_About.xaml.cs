using System;
//using System.Collections.Generic;
//using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
//using System.Windows.Data;
//using System.Windows.Documents;
//using System.Windows.Input;
//using System.Windows.Media;
//using System.Windows.Media.Imaging;
//using System.Windows.Navigation;
//using System.Windows.Shapes;
using System.Diagnostics;
using System.IO;

namespace _2chBrowser
{
    /// <summary>
    /// UC_About.xaml の相互作用ロジック
    /// </summary>
    public partial class UC_About : CBasePage
    {
        public UC_About()
        {
            m_ButtonHomeVisibility = Visibility.Visible;
            InitializeComponent();
        }

        private void hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            if (e.Uri != null && string.IsNullOrEmpty(e.Uri.OriginalString) == false)
            {
                string uri = e.Uri.AbsoluteUri;
                Process.Start(new ProcessStartInfo(uri));

                e.Handled = true;
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                using (StreamReader sr = File.OpenText("ver.txt"))
                {
                    string szStr = sr.ReadLine();
                    Debug.WriteLine("About::Window_Loaded szStr = " + szStr);
                    if (szStr != null)
                        textVersion.Text = szStr;
                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine("About::Window_Loaded Error = " + ex.Message);

            }

        }
    }
}
