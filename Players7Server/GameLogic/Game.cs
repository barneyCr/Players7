using System;
using System.Collections.Generic;
using System.Linq;
using Players7Server.Enums;
using Players7Server.Networking;

namespace Players7Server.GameLogic
{
    public sealed class Game
    {
        #region General
        #region Properties and Fields
        /// <summary>
        /// List of Players
        /// </summary>
        /// <value>The players.</value>
        public List<Client> Players { get; set; }
        private Rewards RewardManager { get; set; }
        public int PlayerCount { get; private set; }
        public int Win { get; private set; }
        public string Name { get; private set; }
        public string GameID { get; private set; }
        public Client Creator { private get; set; }
        #endregion

        #region Packs
        public /*volatile*/ CardPack PackOnTable;
        public CardPack PlayedCardsPack;

        /// <summary>
        /// Dictionary that maps the players' card-packs.
        /// </summary>
        /// <value>The packs</value>
        public Dictionary<Client, CardPack> Packs { get; set; }
        #endregion
        #endregion General



        public Game(int pCount, int win, string name, string id)
        {
            this.Players = new List<Client>(pCount);
            this.PlayerCount = pCount;
            this.Win = win;
            this.Name = name;
            this.GameID = id;

            this.PackOnTable = new CardPack();
            this.PlayedCardsPack = new CardPack();
            this.Packs = new Dictionary<Client, CardPack>(pCount);
        }

        CardPack GetLastPlayedCards(int take)
        {
            var enumerable = PlayedCardsPack.Take(take);
            var pack = new CardPack();
            foreach (var item in enumerable)
                pack.AddCard(item);
            return pack;
        }



        private Dictionary<Client, bool> Readiness;
        private int _turn = 0;

        private bool _playersAdded, _gameInitialized;

        public bool CanAddPlayers
        {
            get
            {
                return !_playersAdded;
            }
        }
        public void SetReady(Client player)
        {
            if (Players.Contains(player))
            {
                Readiness[player] = true;
            }
            if (Readiness.All(p => p.Value == true))
            {
                InitializeGame();
            }
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
            RewardManager = new Rewards(this.Win, Players.Select(p => p.UserID).ToArray());
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

        void SendCardsInfo()
        {
            CardPack lastPlayed = GetLastPlayedCards(3);
            lock (Players)
            {
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

        void EndTurn()
        {
            if (_umflate == 0)
            {
                if (Packs.Any(pair => pair.Value.Count() == 0))
                {
                    Client winner = Packs.Single(p => p.Value.Count() == 0).Key;
                    int place = this.RewardManager.AssignPlayer(winner.UserID);
                    lock (Players)
                    {
                        foreach (var p in Players)
                        {
                            p.Send(Packet.CreatePacket(HeaderTypes.GAME_PLAYER_FINISHED_PLACE, p.UserID, place));
                        }
                    }
                }
            }
            SetNextTurn();
        }

        void SetNextTurn()
        {
            Client onTurn;
            do
            {
                _turn = (PlayerCount - 1 == _turn) ? 0 : _turn + 1;
                onTurn = Players[_turn];
            } while (this.RewardManager.PlayerIDsAndPlaces.ContainsKey(onTurn.UserID));
            foreach (var player in Players)
            {
                player.Send(Packet.CreatePacket(HeaderTypes.GAME_TURN_OF, onTurn.UserID));
            }
        }

        /// <summary>
        /// Gives a card to a player.
        /// </summary>
        /// <param name="card">Card, already removed from a pack</param>
        /// <param name="client">Client.</param>
        public void GiveCardToPlayer(Card card, Client client)
        {
            //todo packet ?
            Packs[client].AddCard(card);
            SendCardsInfo();
        }

        /// <summary>
        /// Moves the card, removing it and adding it.
        /// </summary>
        /// <param name="from">From.</param>
        /// <param name="to">To.</param>
        /// <param name="c">the card</param>
        public void MoveCard(CardPack from, CardPack to, Card c)
        {
            from.RemoveCard(c);
            to.AddCard(c);
            SendCardsInfo();
            // todo packet ?
        }

        void OnCardsOwed()
        {
            // todo

            _umflate = 0; //dezumfla
            lock (Players)
            {
                foreach (var player in Players)
                {
                    player.Send(Packet.CreatePacket(HeaderTypes.GAME_CARDS_FLOAT_SET, 0));
                }
            }
        }

        int _umflate = 0;
        void umfla(int n)
        {
            _umflate += n;
            lock (Players)
            {
                foreach (var player in Players)
                {
                    player.Send(Packet.CreatePacket(HeaderTypes.GAME_CARDS_FLOAT_SET, _umflate));
                }
            }
        }

        internal void HandlePutCardCommand(Client sender, byte val, byte type)
        {
            Client onTurn = Players[this._turn];
            if (sender == onTurn)
            { // todo check this check
                Card putCard = new Card(type, val);
                CardPack playerPack = this.Packs[onTurn];
                Card lastCard = PlayedCardsPack.Peek();

                if (playerPack.Contains(putCard)) // check if player actually has the card
                {
                    if (lastCard.Umflator)
                    {
                        // umflatorii se pun doar ca valoare ambivalenta, nu poti pune 3 INIMA peste 2 Inima
                        if (putCard.Umflator)
                        { // COREEECT
                            umfla(2);
                        }
                        else //if (!playerPack.Any(c => c.Umflator))
                        {
                            // jucatorul ALEGE SA NU DEA un umflator sau NU ARE SA DEA
                            OnCardsOwed();
                        }
                        SetNextTurn();
                    }
                    else // daca nu e umflator
                    {
                        CardValue equivalentValue = lastCard.Value;
                        if (lastCard.Type == putCard.Type)
                        {
                            MoveCard(playerPack, PlayedCardsPack, putCard);
                            // END TURN HERE
                            EndTurn();
                        }
                        else if (equivalentValue == CardValue.Queen || equivalentValue == CardValue.King)
                        {
                            equivalentValue = (CardValue)(equivalentValue - 10);
                            //goto checkOnValue; // probably redundant
                        }
                    //checkOnValue:
                        if (equivalentValue == putCard.Value)
                        {
                            MoveCard(playerPack, PlayedCardsPack, putCard);
                            // END TURN HERE
                            EndTurn();
                        }

                        // daca am ajuns aici, inseamna ca nu are sa dea carte 
                        // daca este As => sta o tura
                        // daca nu este As=> primeste una
                        if (equivalentValue == CardValue.Ace)
                        {
                            // sit
                        }
                        else
                        {
                            Card onTop = this.PackOnTable.Pop();
                            GiveCardToPlayer(onTop, onTurn);
                        }
                        EndTurn();
                    }
                }
            }
            else
            { // not his turn
                sender.Send(Packet.CreatePacket(HeaderTypes.GAME_PLAYER_PUT_CARD_ERROR));
            }
        }
    }
}