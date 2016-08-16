using System;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.IO;
//using System.Windows.Forms;
using System.Diagnostics;
using System.Security.AccessControl;
using System.Net;

namespace _2chBrowser
{
    /// <summary>
    /// UC_Explore.xaml の相互作用ロジック
    /// </summary>
    public partial class UC_BoardList : CBasePage
    {
        public UC_BoardList()
        {
            InitializeComponent();
        }

        private void listFolderItem_MouseDoubleClick(object sender, RoutedEventArgs e)
        {
            //TreeViewItem item = (TreeViewItem)sender;
            //Board dat = item.DataContext as Board;

            TreeViewItem item = (TreeViewItem)listFolder.SelectedItem;
            if (item == null) return;
            Board dat = item.DataContext as Board;

            Debug.WriteLine(string.Format("UC_BoardList::listFolderItem_MouseDoubleClick Name={0}, Url={1}, Category={2}", dat.Name, dat.Url, dat.Category));

            if (dat.Url == "") return;

            m_EventHandler(MainWindow.EventID.ShowThread, dat);
        }

        public void LoadBoardList(string url)
        {
            listFolder.Items.Clear();

            WebRequest wr = WebRequest.Create(url);
            WebResponse ws = wr.GetResponse();
            string boardListHtml;
            using (StreamReader sr = new StreamReader(ws.GetResponseStream(), Encoding.GetEncoding("Shift-Jis")))
            {
                boardListHtml = sr.ReadToEnd();
                sr.Close();
            }

            State state = State.Category;
            string category = "", Boardurl = "", boardName = "";
            for (int i = 0; i < boardListHtml.Length;)
            {
                switch (state)
                {
                    case State.Category:
                        int tmp = boardListHtml.IndexOf("<B>", i) + 3;
                        int tmp2 = boardListHtml.IndexOf("</B>", tmp);
                        if (tmp != -1 && tmp2 != -1)
                        {
                            category = boardListHtml.Substring(tmp, tmp2 - tmp);
                            i = tmp2 + 4;
                            state++;
                        }
                        break;
                    case State.Url:
                        int tmp1 = boardListHtml.IndexOf("HREF=", i) + 5;
                        int tmp12 = boardListHtml.IndexOf(">", tmp1);
                        if (tmp1 != -1 && tmp12 != -1)
                        {
                            Boardurl = boardListHtml.Substring(tmp1, tmp12 - tmp1);
                            i = tmp12 + 1;
                            state++;
                        }
                        break;
                    case State.BoardName:
                        int tmp3 = boardListHtml.IndexOf("</A>", i);
                        if (tmp3 != -1)
                        {
                            boardName = boardListHtml.Substring(i, tmp3 - i);
                            i = tmp3 + 4;
                            addItem(category, Boardurl, boardName);
                            int hrefIndex = boardListHtml.IndexOf("HREF=", i);
                            int bTagIndex = boardListHtml.IndexOf("<B>", i);
                            if (hrefIndex == -1)
                            {
                                return;
                            }
                            else
                            {
                                if (hrefIndex > bTagIndex && bTagIndex != -1)
                                {
                                    state = State.Category;
                                }
                                else
                                {
                                    state = State.Url;
                                }
                            }

                        }
                        break;
                }
            }
        }

        private enum State
        {
            Category,
            Url,
            BoardName,
        }

        private void addItem(string category, string url, string name)
        {
            Board newBoard = new Board() { Category = category, Url = url, Name = name };

            bool isExistCategory = false;

            foreach (TreeViewItem parent in listFolder.Items)
            {
                Board parent_data = (Board)parent.DataContext;
                if (parent_data.Category == newBoard.Category)
                {
                    TreeViewItem item = new TreeViewItem();
                    item.Header = newBoard;
                    item.DataContext = newBoard;
                    item.FontSize = (double)Resources["fontsize"];
                    parent.Items.Add(item);

                    isExistCategory = true;
                    break;
                }
            }

            if(isExistCategory == false)
            {
                //ルートカテゴリの場合
                TreeViewItem item_category = new TreeViewItem();

                Board newCategory = new Board() { Category = category, Url = "", Name = category };

                item_category.Header = newCategory;
                item_category.DataContext = newCategory;
                item_category.FontSize = (double)Resources["fontsize"];
                item_category.IsExpanded = true;
                listFolder.Items.Add(item_category);

                TreeViewItem item_board = new TreeViewItem();
                item_board.Header = newBoard;
                item_board.DataContext = newBoard;
                item_board.FontSize = (double)Resources["fontsize"];
                item_category.Items.Add(item_board);
            }
        }

        public void SetSelectedBoard(string category, string name)
        {
            bool is_found = false;
            foreach (TreeViewItem item in listFolder.Items)
            {
                Board data = (Board)item.DataContext;
                if (data.Category != category || item.Items.Count <= 0) continue;

                foreach (TreeViewItem item_sub in item.Items)
                {
                    Board data_sub = (Board)item_sub.DataContext;
                    if (data_sub.Name == name)
                    {
                        item_sub.IsSelected = true;
                        item_sub.BringIntoView();
                        is_found = true;
                        break;
                    }
                }
                if (is_found == true)
                {
                    break;
                }
            }
        }
    }
}
