namespace _2chBrowser
{
    class Thread {
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

        //更新フラグ
        public bool is_update { get; set; }

        //現在のデータ数
        public int current_count { set; get; }

        //サーバー上の存在有無
        public bool is_exist_in_server { set; get; }

        //文字列変換
        public override string ToString()
        {
            return Title;
        }
    }
}
