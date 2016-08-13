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
using System.Net;
using System.IO;

namespace _2chBrowser
{
    /// <summary>
    /// UC_ThreadList.xaml の相互作用ロジック
    /// </summary>
    public partial class UC_ThreadList : CBasePage
    {
        public Board m_Board { set; get; }

        public UC_ThreadList()
        {
            InitializeComponent();
        }

        private void listFolderItem_MouseDoubleClick(object sender, RoutedEventArgs e)
        {
            Thread dat = ((ListBoxItem)sender).DataContext as Thread;
            m_EventHandler(MainWindow.EventID.ShowMessage, dat);
        }

        public void GetThreadList(Board board)
        {
            this.m_Board = board;
            listThread.Items.Clear();
            WebRequest wr = WebRequest.Create(board.Url + "subject.txt");
            WebResponse ws = wr.GetResponse();
            string threadListText;
            using (StreamReader sr = new StreamReader(ws.GetResponseStream(), Encoding.GetEncoding("Shift-Jis")))
            {
                threadListText = sr.ReadToEnd();
                sr.Close();
            }

            foreach (string row in threadListText.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries))
            {
                string[] tmp = row.Split(new string[] { "<>" }, StringSplitOptions.RemoveEmptyEntries);
                listThread.Items.Add(new Thread() { Number = tmp[0], Title = tmp[1] });
            }
        }
    }
}
