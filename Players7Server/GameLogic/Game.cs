using System;
using System.Collections.Generic;
using System.Linq;
using Players7Server.Enums;
using Players7Server.Networking;

namespace Players7Server.GameLogic
{
    public sealed class Game
    {
        #region Properties and Fields
        /// <summary>
        /// List of Players
        /// </summary>
        /// <value>The players.</value>
        public List<Client> Players { get; set; }

        /// <summary>
        /// Dictionary that maps the players' card-packs.
        /// </summary>
        /// <value>The packs</value>
        public Dictionary<Client, CardPack> Packs { get; set; }

        public int PlayerCount { get; private set; }
        public int Win { get; private set; }
        public string Name { get; private set; }
        public string GameID { get; private set; }
        #endregion

        public /*volatile*/ CardPack PackOnTable;
        public CardPack PlayedCardsPack;
        CardPack GetLastPlayedCards(int take) {
            var enumerable = PlayedCardsPack.Take(take);
            var pack = new CardPack();
            foreach (var item in enumerable)
                pack.AddCard(item);
            return pack;
        }

        public void SetReady(Client player) {
            if (Players.Contains(player)) {
                Readiness[player] = true;
            }
            if (Readiness.All(p=>p.Value == true)) {
                InitializeGame();
            }
        }

        private Dictionary<Client, bool> Readiness;
        private int _turn=0;

        private bool _playersAdded, _gameInitialized;

        public bool CanAddPlayers {get{
                return !_playersAdded;
            }}

        public Game(int pCount, int win, string name, string id)
        {
            this.Players = new List<Client>(pCount);
            this.PlayerCount = pCount;
            this.Win = win;
            this.Name = name;

            this.PackOnTable = new CardPack();
            this.PlayedCardsPack = new CardPack();
            this.Packs = new Dictionary<Client, CardPack>(pCount);
        }

        private List<Client> waitingPlayers;
        public void AddOnePlayer(Client pl)
        {
            lock (waitingPlayers)
            {
                if (waitingPlayers == null)
                {
                    waitingPlayers = new List<Client>();
                }

                waitingPlayers.Add(pl);
                // todo packet
                if (waitingPlayers.Count == PlayerCount)
                {
                    AddPlayers();
                }
                waitingPlayers.Clear();
            }
        }

        private void AddPlayers()
        {
            this.Players = waitingPlayers.Select(p => p).ToList(); // todo check if needed
            //this.PlayerCount = waitingPlayers.Count;
            foreach (Client c in Players)
            {
                Packs.Add(c, new CardPack());
                c.CurrentGame = this;
            }
            OnPlayersAdded();
        }

        void OnPlayersAdded()
        {
            _playersAdded = true;
            foreach (var player in Players)
            {
                // todo packetAdditionFinalized: check
                player.Send(Packet.CreatePacket(Enums.HeaderTypes.GAME_PLAYERS_ALL_ADDED, this.GameID));
            }
            Readiness = new Dictionary<Client, bool>(this.PlayerCount);
        }

        public void InitializeGame()
        {
            if (!_playersAdded)
            {
                Program.Write(Enums.LogMessageType.Error, "Cannot initialize a game without the players");
                return;
            }
            ShuffleCards();
            for (int i = 0; i < 4; i++)
            {
                // PROBLEM: THE LOCK ?
                for (int j = 0; j < PlayerCount; j++)
                {
                    Card c = PackOnTable.Pop();
                    GiveCardToPlayer(c, Players[j]);
                }
            }
            _gameInitialized = true;
            //todo packet
        }

        void SendCardsInfo() {
            CardPack lastPlayed = GetLastPlayedCards(3);
            lock (Players) {
                foreach (var player in Players)
                {
                    player.Send(Packet.CreatePacket(HeaderTypes.GAME_PACK_UPDATE_PutONTABLE, lastPlayed.Pack()));
                    player.Send(Packet.CreatePacket(HeaderTypes.GAME_PACK_UPDATE_SELF, this.Packs[player].Pack()));
                }
            }
            // todo packet of all cards in all packs
        }

        private void ShuffleCards()
        {
            this.PackOnTable.Shuffle();
            foreach (var player in Players)
            {
                player.Send(HeaderTypes.GAME_PACK_SHUFFLED.ToString());
            }

            // todox packet and/or SendCardsInfo() ?
            // no sense to send card info because players 
            // don't have any reasons to know what cards are to be dealt
        }

        public void SetNextTurn() {
            _turn = (PlayerCount - 1 == _turn) ? 0 : _turn + 1;
            Client onTurn = Players[_turn];
            foreach (var player in Players)
            {
                player.Send(Packet.CreatePacket(HeaderTypes.GAME_TURN_OF, onTurn.UserID));
            }
            // todox packet
        }

        /// <summary>
        /// Gives a card to a player.
        /// </summary>
        /// <param name="card">Card, already removed from a pack</param>
        /// <param name="client">Client.</param>
        public void GiveCardToPlayer(Card card, Client client) {
            //todo packet ?
            Packs[client].AddCard(card);
            SendCardsInfo();
        }

        public void MoveCard(CardPack from, CardPack to, Card c) {
            from.RemoveCard(c);
            to.AddCard(c);
            SendCardsInfo();
            // todo packet ?
        }

        internal void HandlePutCard(Client sender, byte val, byte type)
        {
            Client onTurn = Players[this._turn];
            if (sender == onTurn)
            { // todo check this check
                Card newCard = new Card(type, val);
                CardPack playerPack = this.Packs[onTurn];
                if (playerPack.Contains(newCard)) {
                    // check if player actually has the card

                }
            }
            else {
                sender.Send(Packet.CreatePacket(HeaderTypes.GAME_PLAYER_PUT_CARD_ERROR));
            }
        }
    }
}
