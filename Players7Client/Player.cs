using System.Collections.Generic;

namespace Players7Client
{
    public class Player
    {
        static Player()
        {
            All = new Dictionary<int, Player>(25);
            Me = new Player(0, "");
        }
        public static Player Me;
        public UIProperty<double> MyLeverage { get; set; }

        public Player(int id, string p)
        {
            this.UserID = id;
            this.Username = p;
            this.MyLeverage = new UIProperty<double>(1);
        }
        public int UserID { get; set; }
        public string Username { get; set; }
        public double Rating { get; set; }

        public static Dictionary<int, Player> All { get; set; }
    }
}