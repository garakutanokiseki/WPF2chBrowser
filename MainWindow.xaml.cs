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
using PixelLab.Wpf.Transitions;
using System.Net;
using System.IO;
using System.Diagnostics;

using System.Windows.Interop;
using System.Runtime.InteropServices;
using UtilWindowRestore;
using System.Windows.Media.Animation;

namespace _2chBrowser
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        //イベントID
        public enum EventID
        {
            //ページ表示命令
            ShowConfig = 1,
            ShowBoard,
            ShowThread,
            ShowMessage,

            //警告表示
            ShowAlert,

            //ページ遷移命令
            Back,
        }

        //トラジションのID
        public enum TrasitionType
        {
            Trasition_None = 0,
            Trasition_Fade,
            Trasition_SlideRight,
            Trasition_SlideLeft,
            Transition_MaxCount,
        }

        //トラジション
        private PixelLab.Wpf.Transitions.Transition[] m_Transition;
        private const int TRASITION_MAX_COUNT = 4;

        //表示スレッド保持
        Board m_CurrentBoard;
        Thread m_CurrentThread;

        //スレッド
        System.Threading.Thread m_threadGetThread;

        //現在の表示ページ
        CBasePage m_CurrentPage;

        //ページ
        UC_BoardList m_ucBoardList;
        UC_ThreadList m_ucThreadList;
        UC_Message m_ucMessage;

        double m_Borad_VerticalOffset = 0;

        public MainWindow()
        {
            InitializeComponent();

            //トラジションをリソースから読み込む
            m_Transition = new Transition[TRASITION_MAX_COUNT];
            m_Transition[(int)TrasitionType.Trasition_None] = (Transition)FindResource("Transition_Base");
            m_Transition[(int)TrasitionType.Trasition_Fade] = (Transition)FindResource("Transition_Fade");
            m_Transition[(int)TrasitionType.Trasition_SlideRight] = (Transition)FindResource("Transition_SlideRight");
            m_Transition[(int)TrasitionType.Trasition_SlideLeft] = (Transition)FindResource("Transition_SlideLeft");

            //トラジション終了ハンドラを設定する
            mainViewTrasision.CompleteTransisionHandler = OnTransitionCompleted;

            m_ucBoardList = new UC_BoardList();
            m_ucBoardList.m_EventHandler = EventHandler;

            m_ucThreadList = new UC_ThreadList();
            m_ucThreadList.m_EventHandler = EventHandler;

            m_ucMessage = new UC_Message();
            m_ucMessage.m_EventHandler = EventHandler;

            //設定をバインディングする
            DataContext = Properties.Settings.Default;

            //テーマを読み込む
            cmbThemes.ItemsSource = WPF.Themes.ThemeManager.GetThemes();
        }

        private string GetAppDataPath()
        {
            string szDir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\WPF2chBrowser";
            if (Directory.Exists(szDir) == false)
            {
                Directory.CreateDirectory(szDir);
            }
            return szDir;
        }

        void LoadConfig()
        {
            //検索履歴を読み込む(スレッド)
            if (Properties.Settings.Default.NarrowingWord0 != "")
                m_ucThreadList.cmdNarrowingWord.Items.Add(Properties.Settings.Default.NarrowingWord0);
            if (Properties.Settings.Default.NarrowingWord1 != "")
                m_ucThreadList.cmdNarrowingWord.Items.Add(Properties.Settings.Default.NarrowingWord1);
            if (Properties.Settings.Default.NarrowingWord2 != "")
                m_ucThreadList.cmdNarrowingWord.Items.Add(Properties.Settings.Default.NarrowingWord2);
            if (Properties.Settings.Default.NarrowingWord3 != "")
                m_ucThreadList.cmdNarrowingWord.Items.Add(Properties.Settings.Default.NarrowingWord3);
            if (Properties.Settings.Default.NarrowingWord4 != "")
                m_ucThreadList.cmdNarrowingWord.Items.Add(Properties.Settings.Default.NarrowingWord4);

            //板一覧を読み込む
            if(m_ucBoardList.LoadBoardfromFile(GetAppDataPath() + "\\boardlist.xml") == false)
            {
                //ファイルが無い場合は、指定URLから読み込む
                m_ucBoardList.LoadBoardList(Properties.Settings.Default.bbs_menu_url);
            }
        }

        void SaveConfig()
        {
            //履歴を保存する(スレッド)
            try
            {
                Properties.Settings.Default.NarrowingWord0 = (string)m_ucThreadList.cmdNarrowingWord.Items[0];
                Properties.Settings.Default.NarrowingWord1 = (string)m_ucThreadList.cmdNarrowingWord.Items[1];
                Properties.Settings.Default.NarrowingWord2 = (string)m_ucThreadList.cmdNarrowingWord.Items[2];
                Properties.Settings.Default.NarrowingWord3 = (string)m_ucThreadList.cmdNarrowingWord.Items[3];
                Properties.Settings.Default.NarrowingWord4 = (string)m_ucThreadList.cmdNarrowingWord.Items[4];
            }
            catch (Exception ex)
            {
                Debug.WriteLine("SaveSetting : Error => " + ex.Message);
            }

            //板一覧を保存する
            m_ucBoardList.SaveBoard(GetAppDataPath() + "\\boardlist.xml");

            //現在のスレッドを保存する
            if(m_CurrentBoard != null)
            {
                string szFile = GetLogDirectory(m_CurrentBoard) + "\\obtained.db";
                m_ucThreadList.Save(szFile);
            }

            //ウインドウの位置を保存する
            WINDOWPLACEMENT wp = CUtilWindowRestore.Get(this);
            Properties.Settings.Default.WindowPlacement = wp;

            Properties.Settings.Default.Save();
        }

        private string GetLogDirectory(Board board)
        {
            string szDirectory = GetAppDataPath() + "\\" + board.Category;
            if (Directory.Exists(szDirectory) == false)
            {
                Directory.CreateDirectory(szDirectory);
            }
            szDirectory = szDirectory + "\\" + board.Name;
            if (Directory.Exists(szDirectory) == false)
            {
                Directory.CreateDirectory(szDirectory);
            }

            return szDirectory;
        }

        double GetFontSize()
        {
            double font_size;

            switch (Properties.Settings.Default.font_size)
            {
                case 0:
                    font_size = 20;
                    break;
                case 1:
                    font_size = 18;
                    break;
                case 2:
                    font_size = 16;
                    break;
                case 3:
                    font_size = 12;
                    break;
                case 4:
                    font_size = 10;
                    break;
                default:
                    font_size = 16;
                    break;
            }

            return font_size;

        }

        private ScrollViewer GetScrollViewer(UIElement uiParent)
        {
            int nCount = VisualTreeHelper.GetChildrenCount(uiParent);

            try
            {
                for (int i = 0; i < nCount; ++i)
                {
                    UIElement uielement = VisualTreeHelper.GetChild(uiParent, i) as UIElement;
                    if (uielement.GetType() == typeof(System.Windows.Controls.ScrollViewer))
                    {
                        return (ScrollViewer)uielement;
                    }
                    ScrollViewer scrollviewer = GetScrollViewer(uielement);
                    if (scrollviewer != null) return scrollviewer;
                }
            }

            catch (Exception ex)
            {
                Debug.WriteLine("GetScrollViewer : Error => " + ex.Message);
            }

            return null;
        }

        private void UpdateThread(bool is_show_thread_page, Board board)
        {
            if (m_threadGetThread != null && m_threadGetThread.IsAlive)
            {
                return;
            }
            m_threadGetThread = null;

            //表示ボードを保持する
            m_CurrentBoard = board;

            Properties.Settings.Default.selected_board_category = board.Category;
            Properties.Settings.Default.selected_board_name = board.Name;

            //既読用データベースを開く
            string szFile = GetLogDirectory(m_CurrentBoard) + "\\obtained.db";
            m_ucThreadList.Load(szFile);

            //スレッドファイル名を作成する
            szFile = GetLogDirectory(m_CurrentBoard) + "\\subject.txt";

            //スレッドを取得する
            string threadListText = "";
            string pastListText = "";

            //過去のデータを読み込む
            if (File.Exists(szFile))
            {
                using (StreamReader sr = new StreamReader(szFile))
                {
                    pastListText = sr.ReadToEnd();
                    sr.Close();
                }
            }

            //取得済みスレッドを表示する
            m_ucThreadList.SetThreadList(pastListText, "");

            //ページを表示する
            if(is_show_thread_page == true)
                ChangePage(m_ucThreadList, TrasitionType.Trasition_SlideLeft, Visibility.Visible, m_ucThreadList.m_ButtonHomeVisibility);

            m_threadGetThread = new System.Threading.Thread(() =>
            {
                if (System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable() == true)
                {
                    this.Dispatcher.Invoke((Action)(() =>
                    {
                        textStatus.Text = board.Name + "を取得中・・・";
                    }));

                    //Webから読み込む
                    WebRequest wr = WebRequest.Create(board.Url + "subject.txt");
                    WebResponse ws = wr.GetResponse();
                    using (StreamReader sr = new StreamReader(ws.GetResponseStream(), Encoding.GetEncoding("Shift-Jis")))
                    {
                        threadListText = sr.ReadToEnd();
                        sr.Close();
                    }

                    //スレッドを保存する
                    using (StreamWriter sw = new System.IO.StreamWriter(szFile, false))
                    {
                        sw.Write(threadListText);
                        sw.Close();
                    }

                    this.Dispatcher.Invoke((Action)(() =>
                    {
                        textStatus.Text = "";

                        if (m_ucThreadList.SetThreadList(threadListText, pastListText) == false)
                        {
                            //throw new Exception("Can' read thread");
                            //TODO:エラー表示を行う(Toast)
                        }

                    }));
                }
            });
            m_threadGetThread.Start();
        }

        private void UpdateMessage(bool is_show_message_page, Thread thread)
        {
            string dat = "";
            long obtained_file_length = 0;

            //表示スレッドを保持する
            m_CurrentThread = thread;
            //ファイル名を作成する
            string szFile = GetLogDirectory(m_CurrentBoard) + "\\" + thread.Number.ToString();

            //過去のデータを読み込む
            if (File.Exists(szFile))
            {
                FileInfo info = new FileInfo(szFile);
                obtained_file_length = info.Length;

                if(is_show_message_page == true)
                {
                    using (StreamReader sr = new StreamReader(szFile, Encoding.GetEncoding("Shift-jis")))
                    {
                        dat = sr.ReadToEnd();
                        sr.Close();
                    }
                }
            }

            //ページを切り替える
            if(is_show_message_page == true)
            {
                //メッセージを表示する
                m_ucMessage.ShowDat(dat, thread.countobtained_count);

                //取得済みのメッセージまでスクロールする
                if (thread.countobtained_count > 0)
                {
                    m_ucMessage.Browser_ScrollToID(thread.countobtained_count.ToString());
                }

                //ページを切り替える
                ChangePage(m_ucMessage, TrasitionType.Trasition_SlideLeft, Visibility.Visible, m_ucMessage.m_ButtonHomeVisibility);

                //データを取得済みの場合は取得しない
                if (thread.countobtained_count == thread.current_count)
                {
                    //ツールチップを設定する
                    m_ucMessage.SetTooltipForRes();

                    return;
                }
            }

            //ネットワークに接続していない場合は取得しない
            if (System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable() == false) return;

            System.Threading.Thread download_thread = new System.Threading.Thread(() =>
            {
                this.Dispatcher.Invoke((Action)(() =>
                {
                    textStatus.Text = thread.Title + "を取得中・・・";
                }));

                try
                {
                    //メッセージをダウンロードする
                    string url = m_CurrentBoard.Url + "dat/" + thread.Number;

                    HttpWebRequest hwreq = (HttpWebRequest)(HttpWebRequest.Create(url));
                    hwreq.UserAgent = "Monazilla";
                    if (obtained_file_length != 0)
                        hwreq.AddRange(obtained_file_length);
                    WebResponse res = hwreq.GetResponse();

                    //応答データをファイルに書き込む
                    byte[] readData = new byte[1024];
                    Stream strm = res.GetResponseStream();
                    System.IO.MemoryStream ms = new System.IO.MemoryStream();

                    for (;;)
                    {
                        //データを読み込む
                        int readSize = strm.Read(readData, 0, readData.Length);
                        if (readSize == 0)
                        {
                            //すべてのデータを読み込んだ時
                            break;
                        }
                        //読み込んだデータをファイルに書き込む
                        //sw.Write(readData, 0, readSize);
                        ms.Write(readData, 0, readSize);
                    }
                    using (FileStream sw = new System.IO.FileStream(szFile, System.IO.FileMode.Append, System.IO.FileAccess.Write))
                    {
                        sw.Write(ms.ToArray(), 0, (int)ms.Length);
                        sw.Close();
                    }

                    this.Dispatcher.Invoke((Action)(() =>
                    {
                        string downloaded_dat = Encoding.GetEncoding("Shift-JIS").GetString(ms.ToArray());
                        if (dat == "" && m_ucMessage.get_rescount() == 0)
                        {
                            //初期取得の場合
                            m_ucMessage.ShowDat(downloaded_dat, thread.countobtained_count);
                            m_ucMessage.SetTooltipForRes();
                        }
                        else
                        {
                            //追加取得の場合
                            m_ucMessage.InsertDat(downloaded_dat, m_ucMessage.get_rescount());
                            m_ucMessage.SetTooltipForRes();
                        }
                    }));
                }

                catch (Exception ex)
                {
                    Debug.WriteLine("OnShowMessage : Error => " + ex.Message);
                }

                this.Dispatcher.Invoke((Action)(() =>
                {
                    textStatus.Text = "";
                }));
            });
            download_thread.Start();
        }

        #region ページ遷移処理
        void ChangePage(CBasePage dest, TrasitionType trasision,
            System.Windows.Visibility toolbar,
            System.Windows.Visibility back_button)
        {
            //現在の表示位置を保存する
            if (m_CurrentPage == m_ucBoardList)
            {
                ScrollViewer sv = GetScrollViewer(m_ucBoardList.listFolder);
                if(sv != null)
                {
                    m_Borad_VerticalOffset = sv.VerticalOffset;
                    Debug.WriteLine("ChangePage : m_Borad_VerticalOffset = " + m_Borad_VerticalOffset.ToString());
                }
            }

            //メイン画面のボタン表示を設定する
            btnBack.Visibility = back_button;

            //メニューの表示
            //btnMenu.Visibility = toolbar;
            //content_menu.Visibility = toolbar;

            //ボタンの状態を保持する
            dest.m_ButtonHomeVisibility = back_button;
            dest.m_ToolbarVisibility = toolbar;

            //現在のページを保持する
            m_CurrentPage = dest;

            //メニューの状態を変更する
            //UpdateMenuStatus();

            //ページを遷移する
            mainViewTrasision.Transition = m_Transition[(int)trasision];
            mainViewTrasision.Content = dest;
        }

        private bool OnTransitionCompleted(Object sender)
        {
            if (m_CurrentPage == m_ucThreadList && m_ucThreadList.listThread.SelectedItem != null)
            {
                m_ucThreadList.listThread.ScrollIntoView(m_ucThreadList.listThread.SelectedItem);
            }
            else if (m_CurrentPage == m_ucBoardList)
            {
                ScrollViewer sv = GetScrollViewer(m_ucBoardList.listFolder);
                if(sv != null)
                {
                    sv.ScrollToVerticalOffset(m_Borad_VerticalOffset);
                }
            }

            return true;
        }
        #endregion

        #region 各ページイベントハンドラ
        private bool EventHandler(EventID nEventID, Object data)
        {
            switch (nEventID)
            {
                case EventID.Back:
                    break;
                case EventID.ShowBoard:
                    OnShowBoard();
                    break;
                case EventID.ShowThread:
                    OnShowThread((Board)data);
                    break;
                case EventID.ShowMessage:
                    OnShowMessage((Thread)data);
                    break;
            }

            return true;
        }

        private void OnShowBoard()
        {
            ChangePage(m_ucBoardList, TrasitionType.Trasition_SlideLeft, Visibility.Visible, m_ucBoardList.m_ButtonHomeVisibility);
        }

        private void OnShowThread(Board board)
        {
            try
            {
                UpdateThread(true, board);
            }

            catch (Exception ex)
            {
                Debug.WriteLine("OnShowThread : [Error]" + ex.Message);
                MessageBox.Show(this, "スレッドの取得に失敗しました。");
            }
        }

        private void OnShowMessage(Thread thread)
        {
            UpdateMessage(true, thread);
        }

        void OnShowAbout()
        {
            if (ucAbout.Visibility == System.Windows.Visibility.Collapsed)
            {
                //ストーリーボードを停止する
                Storyboard storyboard = (Storyboard)FindResource("Storyboard_InfoClose");
                storyboard.Stop();
                storyboard = (Storyboard)FindResource("Storyboard_InfoOpen");
                storyboard.Stop();

                //ストリーボードを実行する
                DoubleAnimation animate = (DoubleAnimation)storyboard.Children[0];
                animate.From = Width;
                storyboard.Begin();

                //戻るボタンを表示する
                btnBack.Visibility = System.Windows.Visibility.Visible;
                btnBack.IsEnabled = true;
                btnMenu.Visibility = System.Windows.Visibility.Collapsed;

                //メニューの選択を外す
                listMenu.SelectedIndex = -1;
            }
        }
        #endregion

        #region イベンドハンドラ
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadConfig();

            ChangePage(m_ucBoardList, TrasitionType.Trasition_None, Visibility.Visible, Visibility.Collapsed);

            m_ucBoardList.SetSelectedBoard(
                Properties.Settings.Default.selected_board_category,
                Properties.Settings.Default.selected_board_name);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveConfig();
        }

        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            try
            {
                //ウインドウの位置を復元する
                WINDOWPLACEMENT wp = (WINDOWPLACEMENT)Properties.Settings.Default.WindowPlacement;
                if (wp.length > 0)
                {
                    wp.length = Marshal.SizeOf(typeof(WINDOWPLACEMENT));
                    wp.flags = 0;
                    wp.showCmd = (wp.showCmd == CUtilWindowRestore.SW_SHOWMINIMIZED ? CUtilWindowRestore.SW_SHOWNORMAL : wp.showCmd);
                    CUtilWindowRestore.Set(this, wp);
                }
            }

            catch { }
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            if (ucAbout.Visibility == System.Windows.Visibility.Visible)
            {
                Storyboard storyboard = (Storyboard)FindResource("Storyboard_InfoOpen");
                storyboard.Stop();

                storyboard = (Storyboard)FindResource("Storyboard_InfoClose");
                DoubleAnimation animate = (DoubleAnimation)storyboard.Children[0];
                animate.To = Width;
                storyboard.Begin();

                //各ボタンの状態を元に戻す
                btnBack.Visibility = m_CurrentPage.m_ButtonHomeVisibility;
                btnMenu.Visibility = Visibility.Visible;

                //メニューの状態を更新する
                //UpdateMenuStatus();

                return;
            }
            else if (m_CurrentPage == m_ucThreadList)
            {
                string szFile = GetLogDirectory(m_CurrentBoard) + "\\obtained.db";
                m_ucThreadList.Save(szFile);
                ChangePage(m_ucBoardList, TrasitionType.Trasition_SlideRight, Visibility.Visible, m_ucBoardList.m_ButtonHomeVisibility);
            }
            else if (m_CurrentPage == m_ucMessage)
            {
                ChangePage(m_ucThreadList, TrasitionType.Trasition_SlideRight, Visibility.Visible, m_ucThreadList.m_ButtonHomeVisibility);
            }
        }

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            if(m_CurrentPage == m_ucBoardList)
            {
                m_ucBoardList.LoadBoardList(Properties.Settings.Default.bbs_menu_url);
            }
            else if (m_CurrentPage == m_ucThreadList)
            {
                try
                {
                    UpdateThread(false, m_CurrentBoard);
                }

                catch (Exception ex)
                {
                    Debug.WriteLine("OnShowThread : [Error]" + ex.Message);
                    MessageBox.Show(this, "スレッドの取得に失敗しました。");
                }
            }
            else if (m_CurrentPage == m_ucMessage)
            {
                UpdateMessage(false, m_CurrentThread);
            }
        }


        private void content_menu_MouseLeave(object sender, MouseEventArgs e)
        {
            btnMenu.IsChecked = false;
        }

        private void btnAbout_Click(object sender, RoutedEventArgs e)
        {
            OnShowAbout();
        }

        private void cmbFontSize_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Double font_size = GetFontSize();
            Resources["fontsize"] = font_size;
            m_ucBoardList.Resources["fontsize"] = font_size;
            m_ucThreadList.Resources["fontsize"] = font_size;
        }

        private void menu_sort_normal_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.thread_sort_type = 0;
            m_ucThreadList.listThread_sort();
        }

        private void menu_sort_lastest_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.thread_sort_type = 1;
            m_ucThreadList.listThread_sort();
        }

        private void menu_sort_title_up_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.thread_sort_type = 2;
            m_ucThreadList.listThread_sort();
        }

        private void menu_sort_title_down_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.thread_sort_type = 3;
            m_ucThreadList.listThread_sort();
        }
        #endregion
    }
}
