using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace _2chBrowser
{
    public class CBasePage : UserControl
    {
        //ページタイトル
        public string m_Title;

        //戻るボタンの表示状態
        public System.Windows.Visibility m_ButtonHomeVisibility;

        //ログアウトボタンの表示状態
        public System.Windows.Visibility m_ButtonLogoutVisibility;

        //ツールバーの表示状態
        public System.Windows.Visibility m_ToolbarVisibility;

        //イベントハンドラ
        public Func<MainWindow.EventID, Object, bool> m_EventHandler;
    }
}
