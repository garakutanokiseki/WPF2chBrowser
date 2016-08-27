using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

using NVelocity;
using NVelocity.App;
using NVelocity.Exception;
using System.Diagnostics;

namespace _2chBrowser
{
    class MessageData
    {
        public string rescount { get; set; }
        public string name { get; set; }
        public string mail { get; set; }
        public string date { get; set; }
        public string message { get; set; }

    }

    /// <summary>
    /// UC_Message.xaml の相互作用ロジック
    /// </summary>
    public partial class UC_Message : CBasePage
    {
        VelocityContext m_velocityctx;
        string [] m_themes = WPF.Themes.ThemeManager.GetThemes();

        public UC_Message()
        {
            InitializeComponent();

            m_ButtonHomeVisibility = Visibility.Visible;

            //NVelocityの初期化
            Velocity.Init();
            m_velocityctx = new VelocityContext();
        }

        public void ShowDat(string dat)
        {
            if(dat != "")
            {
                List<MessageData> messages = new List<MessageData>();
                string title;
                int font_size;

                font_size = 4 - Properties.Settings.Default.font_size;

                Regex regex = new Regex("((s?https?|ttp)://[-_.!~*'()a-zA-Z0-9;/?:@&=+$,%#]+)");

                string[] datElements = dat.Split(new string[] { "<>" }, StringSplitOptions.None);

                title = datElements[4].Split(new char[] { '\n' })[0];

                int resCount = 1;
                for (int i = 0; i < datElements.Length - 4; i = i + 4)
                {
                    MessageData message_data = new MessageData();
                    message_data.rescount = resCount.ToString();
                    message_data.name = i == 4 ? datElements[i].Split(new char[] { '\n' })[1] : datElements[i];//datElements[i];
                    message_data.mail = datElements[i + 1];
                    message_data.date = datElements[i + 2];
                    message_data.message = regex.Replace(datElements[i + 3], "<a href=\"$1\" target=\"_blank\">$1</a>");

                    messages.Add(message_data);

                    resCount++;
                }

                string basedirectory = System.IO.Directory.GetCurrentDirectory().Replace("\\", "/") + "/";

                m_velocityctx.Put("title", title);
                m_velocityctx.Put("basedirectory", basedirectory);
                m_velocityctx.Put("font_size", font_size);
                m_velocityctx.Put("messages", messages);
            }

            try
            {
                string template_file = ".\\Template\\" + m_themes[Properties.Settings.Default.theme] + "\\message.html";

                Debug.WriteLine(template_file);

                System.IO.StringWriter resultWriter = new System.IO.StringWriter();
                Velocity.MergeTemplate(template_file, "UTF-8", m_velocityctx, resultWriter);
                string result = resultWriter.GetStringBuilder().ToString();
                browser.NavigateToString(result);
            }
            catch (ResourceNotFoundException ex)
            {
            }
            catch (ParseErrorException ex)
            {
            }

        }
    }
}
