using System.ComponentModel;

namespace _2chBrowser
{
    class Thread : INotifyPropertyChanged
    {
        // -- データベースに記録するデータ --
        //データID
        public int nID { set; get; }

        //スレッドID
        public string Number { set; get; }

        //スレッドタイトル
        public string Title { set; get; }

        //取得済み数
        public int countobtained_count { set; get; }

        //-- 表示使用するデータ --
        //絞り込みフラグ
        public bool visible { set; get; }

        //(旧)状態　0:通常、1:新規、2:更新あり、3:DAT落ち、4:既読
        //状態　0:通常、1:DAT落ち、2:既読、3:新規、4:更新あり
        private int _status = 0;
        public int status
        {
            get
            {
                return _status;
            }
            set
            {
                _status = value;
                OnPropertyChanged("status");
            }
        }

        //現在のデータ数
        public int current_count { set; get; }

        //サーバー上の存在有無
        public bool is_exist_in_server { set; get; }

        //文字列変換
        public override string ToString()
        {
            return Title;
        }

        // イベント宣言
        public event PropertyChangedEventHandler PropertyChanged;

        // イベントに対応するOnPropertyChanged メソッドを作る
        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}
