using System;
using System.IO;
using System.Net;
using System.Text;
using System.Windows.Forms;

namespace _2ch {
    class ThreadList :ListBox {
        public Board Board { set; get; }
        public void GetThreadList(Board board) {
            this.Board = board;
            this.Items.Clear();
            WebRequest wr = WebRequest.Create(board.Url + "subject.txt");
            WebResponse ws = wr.GetResponse();
            string threadListText;
            using(StreamReader sr = new StreamReader(ws.GetResponseStream(), Encoding.GetEncoding("Shift-Jis"))) {
                threadListText = sr.ReadToEnd();
                sr.Close();
            }

            foreach(string row in threadListText.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries)) {
                string[] tmp = row.Split(new string[] { "<>" }, StringSplitOptions.RemoveEmptyEntries);
                this.Items.Add(new Thread() { Number = tmp[0], Title = tmp[1] });
            }
        }
    }
}