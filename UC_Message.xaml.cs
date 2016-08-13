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

namespace _2chBrowser
{
    /// <summary>
    /// UC_Message.xaml の相互作用ロジック
    /// </summary>
    public partial class UC_Message : CBasePage
    {
        public UC_Message()
        {
            InitializeComponent();
        }

        public void ShowDat(string dat)
        {
            StringBuilder sb = new StringBuilder("<HTML><HEAD><META http-equiv=\"Content-Type\" content=\"text/html; charset=UTF-8\"></HEAD><BODY><font face=\"ＭＳ ゴシック\">");
            string[] datElements = dat.Split(new string[] { "<>" }, StringSplitOptions.None);
            int resCount = 1;

            sb.Append("<H2>" + datElements[4].Split(new char[] { '\n' })[0] + "</H2>");

            sb.Append(resCount.ToString() + ": " + "NAME:" + datElements[0] + "[" + datElements[1] + "] DATE:" + datElements[2] + "<br><br>" + datElements[3] + "<br>");
            resCount++;
            for (int i = 4; i < datElements.Length - 4; i = i + 4)
            {
                sb.Append("<hr>");
                sb.Append(resCount.ToString() + ": " + "NAME:" + (i == 4 ? datElements[i].Split(new char[] { '\n' })[1] : datElements[i]) + "[" + datElements[i + 1] + "] DATE:" + datElements[i + 2] + "<br><br>" + datElements[i + 3] + "<br>");
                resCount++;
            }
            sb.Replace("<b>", "");
            sb.Replace("</b>", "");
            sb.Append("</font></BODY></HTML>");
            browser.NavigateToString(sb.ToString());
        }
    }
}
