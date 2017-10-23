using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Players7Client
{
    static public class GameManager
    {
        public static List<Game> Games = new List<Game>();
    }

    public class Game
    {
        [DisplayName("Game ID")]
        public int ID { get; set; }

        [DisplayName("Game creator")]
        public string GameCreator { get; set; }

        [DisplayName("Bet")]
        public int Bet { get; set; }

        [DisplayName("Capacity of players")]
        public int PlayerCapacity { get; set; }
    }
}