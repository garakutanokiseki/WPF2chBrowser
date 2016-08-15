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
using System.Diagnostics;
using System.ComponentModel;

namespace _2chBrowser
{
    /// <summary>
    /// UC_ThreadList.xaml の相互作用ロジック
    /// </summary>
    public partial class UC_ThreadList : CBasePage
    {
        public UC_ThreadList()
        {
            InitializeComponent();
        }

        #region メソッド
        private void AddHistory_Base(ComboBox cmb, string szString)
        {
            Debug.WriteLine("AddHistory_Base <Start>");
            if (szString == "") return;
            bool bIsExist = false;
            foreach (Object objCmbBox in cmb.Items)
            {
                Debug.WriteLine("AddHistory_Base : " + (string)objCmbBox);
                if (szString == (string)objCmbBox)
                {
                    bIsExist = true;
                    break;
                }
            }
            if (bIsExist == false)
            {
                cmb.Items.Add(szString);
            }

            if (cmb.Items.Count > 5)
            {
                cmb.Items.RemoveAt(0);
            }
            Debug.WriteLine("AddHistory_Base <End>");
        }

        private void listThread_sort()
        {
            Debug.WriteLine("listThread_sort >>");
            foreach (Thread tread_data in listThread.Items)
            {
                if (cmdNarrowingWord.Text == null || cmdNarrowingWord.Text == "")
                {
                    tread_data.visible = false;
                    continue;
                }
                if (tread_data.Title.IndexOf(cmdNarrowingWord.Text) >= 0)
                {
                    tread_data.visible = true;
                }
                else
                {
                    tread_data.visible = false;
                }
            }

            //ソースを取得する
            ICollectionView viewList = listThread.Items;

            //ソートする
            viewList.SortDescriptions.Clear();

            //ソートする
            viewList.SortDescriptions.Add(new SortDescription("visible", ListSortDirection.Descending));
            viewList.SortDescriptions.Add(new SortDescription("Number", ListSortDirection.Descending));

            //ビューを更新する
            viewList.Refresh();
            Debug.WriteLine("listThread_sort <<");
        }

        public bool SetThreadList(string threadlist)
        {
            try
            {
                listThread.Items.Clear();

                foreach (string row in threadlist.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries))
                {
                    string[] tmp = row.Split(new string[] { "<>" }, StringSplitOptions.RemoveEmptyEntries);
                    listThread.Items.Add(new Thread() { Number = tmp[0], Title = tmp[1] });
                }

                listThread_sort();

                return true;
            }

            catch (Exception ex)
            {
                Debug.WriteLine("SetThreadList : [Error]" + ex.Message);
            }

            return false;
        }
        #endregion

        #region イベントハンドラ
        private void listFolderItem_MouseDoubleClick(object sender, RoutedEventArgs e)
        {
            Thread dat = ((ListBoxItem)sender).DataContext as Thread;
            m_EventHandler(MainWindow.EventID.ShowMessage, dat);
        }

        private void cmdNarrowingWord_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;

            if (cmdNarrowingWord.Text != "")
            {
                AddHistory_Base(cmdNarrowingWord, cmdNarrowingWord.Text);
            }

            listThread_sort();
        }

        private void cmdNarrowingWord_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            cmdNarrowingWord.Text = (string)cmdNarrowingWord.SelectedValue;
            listThread_sort();

        }

        private void btnDeleteNarrowingWord_Click(object sender, RoutedEventArgs e)
        {
            cmdNarrowingWord.Text = "";
            listThread_sort();
        }
        #endregion
    }
}
