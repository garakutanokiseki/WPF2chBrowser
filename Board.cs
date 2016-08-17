namespace _2chBrowser
{
    public class Board {
        public string Url { set; get; }
        public string Name { set; get; }
        public string Category { set; get; }
        public bool IsExpand { get; set; }

        public override string ToString(){
            return Name;
        }
    }
}
