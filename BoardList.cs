using System.IO;
using System.Net;
using System.Text;
using System.Windows.Forms;

namespace _2ch {
    class BoardList :TreeView {
        public void LoadBoardList(string url) {
            WebRequest wr = WebRequest.Create(url);
            WebResponse ws = wr.GetResponse();
            string boardListHtml;
            using(StreamReader sr = new StreamReader(ws.GetResponseStream(), Encoding.GetEncoding("Shift-Jis"))) {
                boardListHtml = sr.ReadToEnd();
                sr.Close();
            }

            State state = State.Category;
            string category = "", Boardurl = "", boardName = "";
            for(int i = 0; i < boardListHtml.Length; ) {
                switch(state) {
                case State.Category:
                    int tmp = boardListHtml.IndexOf("<B>", i) + 3;
                    int tmp2 = boardListHtml.IndexOf("</B>", tmp);
                    if(tmp != -1 && tmp2 != -1) {
                        category = boardListHtml.Substring(tmp, tmp2 - tmp);
                        i = tmp2 + 4;
                        state++;
                    }
                    break;
                case State.Url:
                    int tmp1 = boardListHtml.IndexOf("HREF=", i) + 5;
                    int tmp12 = boardListHtml.IndexOf(">", tmp1);
                    if(tmp1 != -1 && tmp12 != -1) {
                        Boardurl = boardListHtml.Substring(tmp1, tmp12 - tmp1);
                        i = tmp12 + 1;
                        state++;
                    }
                    break;
                case State.BoardName:
                    int tmp3 = boardListHtml.IndexOf("</A>", i);
                    if(tmp3 != -1) {
                        boardName = boardListHtml.Substring(i, tmp3 - i);
                        i = tmp3 + 4;
                        addItem(category, Boardurl, boardName);
                        int hrefIndex = boardListHtml.IndexOf("HREF=", i);
                        int bTagIndex = boardListHtml.IndexOf("<B>", i);
                        if(hrefIndex == -1) {
                            return;
                        } else {
                            if(hrefIndex > bTagIndex && bTagIndex != -1) {
                                state = State.Category;
                            } else {
                                state = State.Url;
                            }
                        }

                    }
                    break;
                }
            }
        }

        private enum State {
            Category,
            Url,
            BoardName,
        }

        private void addItem(string category, string url, string name) {
            Board newBoard = new Board() { Category = category, Url = url, Name = name };
            if(!this.Nodes.ContainsKey(category)) {
                this.Nodes.Add(category, category);
            }
            this.Nodes[category].Nodes.Add(new TreeNode() { Name = name, Text = name, Tag = newBoard });
        }
    }
}