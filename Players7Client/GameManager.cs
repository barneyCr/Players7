using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Players7Client
{
    static public class GameManager
    {
        public static UIProperty<CardPack> PlayedCardsPack;
        public static UIProperty<CardPack> PackOnTable;
        public static UIProperty<CardPack> MyPack;

        public static UIProperty<Player> PlayerOnTurn;
        public static UIProperty<int> CardsFloated;

        // todo add List of players in current game

        static GameManager() {
            PlayedCardsPack = new UIProperty<CardPack>(new CardPack());
            PackOnTable = new UIProperty<CardPack>(new CardPack());
            MyPack = new UIProperty<CardPack>(new CardPack());

            PlayerOnTurn = new UIProperty<Player>(null);
            CardsFloated = new UIProperty<int>(0);
        }


       // public static List<Game> Games = new List<Game>();
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

        public string Name { get; set; }

        public Button Btn { get; set; }
    }
    
}