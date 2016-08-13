namespace _2chBrowser
{
    class Thread {
        public string Number { set; get; }
        public string Title { set; get; }
        public override string ToString() {
            return Title;
        }
    }
}
