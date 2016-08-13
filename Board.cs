namespace _2chBrowser
{
    public class Board {
        public string Url { set; get; }
        public string Name { set; get; }
        public string Category { set; get; }

        public override string ToString(){
            return Name;
        }
    }
}
