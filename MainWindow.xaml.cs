﻿using System;
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

        //現在の表示ページ
        CBasePage m_CurrentPage;

        //ページ
        UC_BoardList m_ucBoardList;
        UC_ThreadList m_ucThreadList;
        UC_Message m_ucMessage;

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
        }

        void SaveConfig()
        {
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
                    font_size = 18;
                    break;
                case 1:
                    font_size = 16;
                    break;
                default:
                    font_size = 12;
                    break;
            }

            return font_size;

        }

        #region ページ遷移処理
        void ChangePage(CBasePage dest, TrasitionType trasision,
            System.Windows.Visibility toolbar,
            System.Windows.Visibility back_button)
        {
            //メイン画面のボタン表示を設定する
            //btnBack.Visibility = back_button;

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
            ChangePage(m_ucBoardList, TrasitionType.Trasition_SlideLeft, Visibility.Visible, Visibility.Collapsed);
        }

        private void OnShowThread(Board board)
        {
            //表示ボードを保持する
            m_CurrentBoard = board;

            WebRequest wr = WebRequest.Create(board.Url + "subject.txt");
            WebResponse ws = wr.GetResponse();
            string threadListText;
            using (StreamReader sr = new StreamReader(ws.GetResponseStream(), Encoding.GetEncoding("Shift-Jis")))
            {
                threadListText = sr.ReadToEnd();
                sr.Close();
            }

            //スレッドを取得する
            m_ucThreadList.GetThreadList(threadListText);

            //スレッドを保存する
            string szFile = GetLogDirectory(m_CurrentBoard) + "\\subject.txt";
            using (StreamWriter sw = new System.IO.StreamWriter(szFile, false))
            {
                sw.Write(threadListText);
                sw.Close();
            }

            //ページを表示する
            ChangePage(m_ucThreadList, TrasitionType.Trasition_SlideLeft, Visibility.Visible, Visibility.Visible);
        }

        private void OnShowMessage(Thread thread)
        {
            //表示スレッドを保持する
            m_CurrentThread = thread;

            //メッセージをダウンロードする
            string url = m_CurrentBoard.Url + "dat/" + thread.Number;

            HttpWebRequest hwreq = (HttpWebRequest)(HttpWebRequest.Create(url));
            hwreq.UserAgent = "Monazilla";
            WebResponse res = hwreq.GetResponse();
            string dat = "";
            using (StreamReader sr = new StreamReader(res.GetResponseStream(), Encoding.GetEncoding("Shift-Jis")))
            {
                dat = sr.ReadToEnd();
                sr.Close();
            }

            //ログを保存する
            string szFile = GetLogDirectory(m_CurrentBoard) + "\\" + thread.Number.ToString();
            using (StreamWriter sw = new System.IO.StreamWriter(szFile, false))
            {
                sw.Write(dat);
                sw.Close();
            }

            //メッセージを表示する
            m_ucMessage.ShowDat(dat);

            //ページを切り替える
            ChangePage(m_ucMessage, TrasitionType.Trasition_SlideLeft, Visibility.Visible, Visibility.Visible);
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
            }
        }
        #endregion

        #region イベンドハンドラ
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadConfig();

            m_ucBoardList.LoadBoardList(Properties.Settings.Default.bbs_menu_url);

            ChangePage(m_ucBoardList, TrasitionType.Trasition_None, Visibility.Visible, Visibility.Collapsed);
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
                ChangePage(m_ucBoardList, TrasitionType.Trasition_SlideRight, Visibility.Visible, Visibility.Collapsed);
            }
            else if (m_CurrentPage == m_ucMessage)
            {
                ChangePage(m_ucThreadList, TrasitionType.Trasition_SlideRight, Visibility.Visible, Visibility.Collapsed);
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
        #endregion
    }
}
