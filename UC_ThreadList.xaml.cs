using System;
using System.Collections;
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
using Community.CsharpSqlite.SQLiteClient;

namespace _2chBrowser
{
    /// <summary>
    /// UC_ThreadList.xaml の相互作用ロジック
    /// </summary>
    public partial class UC_ThreadList : CBasePage
    {
        List<Thread> m_listThread = new List<Thread>();
        private SqliteConnection m_Sql;
        private System.Data.Common.DbCommand m_SqlCmd;

        public UC_ThreadList()
        {
            InitializeComponent();

            m_Sql = new SqliteConnection();
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

                    Thread thread_data = new Thread();

                    thread_data.Number = tmp[0];
                    thread_data.Title = tmp[1];
                    thread_data.nID = -1;
                    thread_data.visible = false;
                    thread_data.is_update = false;
                    thread_data.countobtained_count = 0;
                    thread_data.current_count = 0;

                    int start = thread_data.Title.LastIndexOf("(") + 1;
                    int end = thread_data.Title.LastIndexOf(")");
                    if(start > 0 && end > start)
                    {
                        string str_count = thread_data.Title.Substring(start, end - start);
                        thread_data.current_count = int.Parse(str_count);
                    }

                    //TODO:取得済みのデータの有無を確認する

                    listThread.Items.Add(thread_data);
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

        #region SQLite操作
        public bool Save(string szFile)
        {
            try
            {
                if (Open(szFile) == false) return false;

                foreach (Thread data in m_listThread)
                {
                    if(Add(data) == -1)
                    {
                        Update(data);
                    }
                }

                Close();
            }

            catch (Exception ex)
            {
                Debug.WriteLine(string.Format("CTextreamMessage::Save << err = {0}", ex.Message));
                return false;
            }

            return true;
        }

        //取得済みスレッド情報をロードする
        public bool Load(string szFile)
        {
            try
            {
                if (Open(szFile) == false) return false;

                m_listThread.Clear();

                m_SqlCmd.CommandText = "SELECT * FROM data";

                //コマンドを実行する
                System.Data.Common.DbDataReader reader = m_SqlCmd.ExecuteReader();

                while (reader.Read())
                {
                    Thread data = GetData(reader);
                    m_listThread.Add(data);
                }

                Close();
            }

            catch (Exception ex)
            {
                return false;
            }

            return true;
        }

        private bool Open(string szFile)
        {
            Debug.WriteLine("Open");

            int nRet;

            //データベースを開く
            m_Sql.ConnectionString = string.Format("Version=3,uri=file:{0}", szFile);
            m_Sql.Open();

            //テーブルがあるかを確認する
            m_SqlCmd = m_Sql.CreateCommand();

            //テーブルの有無を確認して必要に応じてテーブルを作成する
            try
            {
                m_SqlCmd.CommandText = "SELECT count(*) FROM data";

                //コマンドを実行する
                nRet = m_SqlCmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                //データベースが開けない場合はエラー
                if (ex.Message.Contains("no such table") == false) return false;
                //テーブルを作成する
                m_SqlCmd.CommandText = "CREATE TABLE data(ID INTEGER PRIMARY KEY, Number INTEGER, Title TEXT, countobtained_count INTEGER)";
                nRet = m_SqlCmd.ExecuteNonQuery();
            }

            return true;
        }

        public bool Close()
        {
            Debug.WriteLine("Close");
            if (m_Sql == null) return true;
            m_Sql.Close();
            return true;
        }

        private int Add(Thread dat)
        {
            Debug.WriteLine("Add");
            int nRet;

            //同じデータがあるかを確認する
            //テーブルの有無を確認して必要に応じてテーブルを作成する
            try
            {
                m_SqlCmd.CommandText = "SELECT count(*) FROM data WHERE Number=\"" + dat.Number + "\" limit 1";

                //コマンドを実行する
                nRet = int.Parse(m_SqlCmd.ExecuteScalar().ToString());

                if (nRet > 0)
                {
                    return -1;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Add : Error(1) " + ex.Message);
                Debug.WriteLine("Add : Error(1) " + m_SqlCmd.CommandText);
                return 0;
            }

            //データを追加する
            try
            {
                string title = dat.Title;
                string szStr = "INSERT INTO data(ID, Number, Title, countobtained_count) VALUES(NULL, \"" + dat.Number + "\" , \"" + dat.Title + "\" , '" + dat.countobtained_count.ToString() + "')";
                m_SqlCmd.CommandText = szStr;
                m_SqlCmd.CommandType = System.Data.CommandType.Text;
                nRet = m_SqlCmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Add : Error(2) " + ex.Message);
                Debug.WriteLine("Add : Error(2) " + m_SqlCmd.CommandText);
                if (ex.Message.IndexOf("unrecognized token") >= 0) return -2;
                return 0;
            }

            return 1;
        }

        private bool Delete(int nID)
        {
            //データを削除する
            int nRet;
            try
            {
                m_SqlCmd.CommandText = "DELETE FROM data WHERE ID=" + nID.ToString();
                nRet = m_SqlCmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                return false;
            }

            if (nRet == 0) return false;
            return true;
        }

        private Thread GetData(System.Data.Common.DbDataReader reader)
        {
            Thread dat = new Thread();

            dat.Number = GetStringFromSQL(reader, "Number");
            dat.countobtained_count = GetIntFromSQL(reader, "countobtained_count");
            dat.Title = GetStringFromSQL(reader, "Title");
            dat.nID = GetIntFromSQL(reader, "ID");

            return dat;
        }

        private Thread Get(int nIdx)
        {

            m_SqlCmd.CommandText = "SELECT * FROM data limit 1 offset " + nIdx.ToString();

            //コマンドを実行する
            System.Data.Common.DbDataReader reader = m_SqlCmd.ExecuteReader();

            if (reader.Read() == false) return null;

            return GetData(reader);
        }

        private int Count()
        {
            int nRet = 0;
            //テーブルの有無を確認して必要に応じてテーブルを作成する
            try
            {
                m_SqlCmd.CommandText = "SELECT count(*) FROM data";

                //コマンドを実行する
                nRet = int.Parse(m_SqlCmd.ExecuteScalar().ToString());
            }
            catch (Exception ex)
            {
            }

            return nRet;
        }

        private bool Update(Thread dat)
        {
            //テーブルの有無を確認して必要に応じてテーブルを作成する
            try
            {
                m_SqlCmd.CommandText = string.Format("UPDATE data SET countobtained_count='{0}' WHERE Number='{1}'",
                    dat.countobtained_count, dat.Number);

                //コマンドを実行する
                m_SqlCmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Update Error : " + ex.Message);
                Debug.WriteLine("Update Error : " + m_SqlCmd.CommandText);
                return false;
            }

            return true;

        }

        private int GetIntFromSQL(System.Data.Common.DbDataReader reader, string name)
        {
            try
            {
                return reader.GetInt32(reader.GetOrdinal(name));
            }
            catch (Exception ex)
            {
                Debug.WriteLine("GetIntFromSQL : Error : " + ex.Message);
            }

            return 0;
        }

        private string GetStringFromSQL(System.Data.Common.DbDataReader reader, string name)
        {
            try
            {
                return reader.GetString(reader.GetOrdinal(name));
            }
            catch (Exception ex)
            {
                Debug.WriteLine("GetStringFromSQL : Error : " + ex.Message);
            }

            return "";
        }
        #endregion

        #region イベントハンドラ
        private void listFolderItem_MouseDoubleClick(object sender, RoutedEventArgs e)
        {
            Thread dat = ((ListBoxItem)sender).DataContext as Thread;

            bool is_found = false;
            for(int i = 0; i < m_listThread.Count(); ++i)
            {
                if(m_listThread[i].Number == dat.Number)
                {
                    m_listThread[i].countobtained_count = dat.current_count;
                    is_found = true;
                    break;
                }
            }

            if(is_found == false)
            {
                dat.countobtained_count = dat.current_count;
                m_listThread.Add(dat);
            }

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
