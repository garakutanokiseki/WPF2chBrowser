﻿using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows;

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
        public bool is_new { get; set; }

    }

    /// <summary>
    /// UC_Message.xaml の相互作用ロジック
    /// </summary>
    public partial class UC_Message : CBasePage
    {
        VelocityContext m_velocityctx;
        string [] m_themes = WPF.Themes.ThemeManager.GetThemes();

        bool m_is_insert;
        string m_Inserttext = "";

        public UC_Message()
        {
            InitializeComponent();

            m_ButtonHomeVisibility = Visibility.Visible;

            //ブラウザイベント処理
            browser.LoadCompleted += Browser_LoadCompleted;

            //NVelocityの初期化
            Velocity.Init();
            m_velocityctx = new VelocityContext();
        }

        private void Browser_LoadCompleted(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            if (m_is_insert == false) return;

            AddTextToContent(m_Inserttext);

            m_is_insert = false;
            m_Inserttext = "";
        }

        private string CreateMessageList(string [] datElements,int resCount, int obtained_count)
        {
            List<MessageData> messages = new List<MessageData>();

            Regex regex = new Regex("((s?https?|ttp)://[-_.!~*'()a-zA-Z0-9;/?:@&=+$,%#]+)");

            //int resCount = 1;
            for (int i = 0; i < datElements.Length - 4; i = i + 4)
            {
                MessageData message_data = new MessageData();
                message_data.rescount = resCount.ToString();
                message_data.name = i == 4 ? datElements[i].Split(new char[] { '\n' })[1] : datElements[i];//datElements[i];
                message_data.mail = datElements[i + 1];
                message_data.date = datElements[i + 2];
                message_data.message = regex.Replace(datElements[i + 3], "<a href=\"$1\" target=\"_blank\">$1</a>");
                message_data.is_new = resCount > obtained_count ? true : false;
                messages.Add(message_data);

                resCount++;
            }

            //テンプレートを適用する
            m_velocityctx.Put("messages", messages);

            string template_file = ".\\Template\\" + m_themes[Properties.Settings.Default.theme] + "\\content.html";

            Debug.WriteLine(template_file);

            System.IO.StringWriter resultWriter = new System.IO.StringWriter();
            Velocity.MergeTemplate(template_file, "UTF-8", m_velocityctx, resultWriter);
            return resultWriter.GetStringBuilder().ToString();
        }

        public void ShowDat(string dat, int obtained_count)
        {
            string title;
            string content;

            string basedirectory = System.IO.Directory.GetCurrentDirectory().Replace("\\", "/") + "/";
            int font_size = 4 - Properties.Settings.Default.font_size;

            if (dat == "")
            {
                title = "メッセージが読み込めない。または、取得中です。";
                content = "";

            }
            else
            {
                string[] datElements = dat.Split(new string[] { "<>" }, StringSplitOptions.None);
                title = datElements[4].Split(new char[] { '\n' })[0];
                content = CreateMessageList(datElements, 1, obtained_count);
            }

            //テンプレートを適用する
            m_velocityctx.Put("title", title);
            m_velocityctx.Put("basedirectory", basedirectory);
            m_velocityctx.Put("font_size", font_size);
            m_velocityctx.Put("content", content);

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
                Debug.WriteLine("MessageData::ShowDat(ResourceNotFoundException) Error = " + ex.Message);
            }

            catch (ParseErrorException ex)
            {
                Debug.WriteLine("MessageData::ShowDat(ParseErrorException) Error = " + ex.Message);
            }

        }

        public void InsertDat(string dat, int obtained_count)
        {
            if (dat == "") return;

            string[] datElements = dat.Split(new string[] { "<>" }, StringSplitOptions.None);
            string content = CreateMessageList(datElements, obtained_count + 1, obtained_count);

            AddTextToContent(content);
        }


        void AddTextToContent(string html)
        {
            if(browser.IsLoaded == false)
            {
                m_is_insert = true;
                m_Inserttext = html;
                return;
            }

            MSHTML.HTMLDocument doc = (MSHTML.HTMLDocument)browser.Document;
            var element_content = doc.getElementById("content");

            if(element_content == null)
            {
                Debug.WriteLine("MessageData::AddTextToContent element_content is null");
                return;
            }
            element_content.innerHTML = element_content.innerHTML + html;
        }
    }
}
