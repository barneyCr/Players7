using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Players7Server.GameLogic
{
    
    public sealed class CardPack : IEnumerable<Card>, IPacketData
    {
        private Queue<Card> Cards;

        public CardPack()
        {
            Cards = new Queue<Card>(52);
        }

        public void Initialize() 
        {
			for (int i = 0; i < 52; i++)
			{
				Card c = new Card();
				byte type = (byte)(1 << (i % 4));
				byte val = (byte)((i % 13) + 2);
				c.Type = (CardType)type;
				c.Value = (CardValue)val;
                Cards.Enqueue(c);
			}
        }
        public void Shuffle()
        {
            lock (Cards)
                Cards = new Queue<Card>(Cards.OrderBy(x => Helper.Randomizer.Next()));
        }

        public Card Pop()
        {
            lock (Cards)
            {
                if (Cards.Count == 0)
                {
                    Program.Write(Enums.LogMessageType.Error, "Card pack already empty");
                    return default(Card);
                }
                return Cards.Dequeue();
            }
        }

        public Card RemoveCard(Card c) {
			lock (Cards)
			{
				if (Cards.Count == 0)
				{
					Program.Write(Enums.LogMessageType.Error, "Card pack already empty");
                    return default(Card);
				}

                //Card ret = Cards.SingleOrDefault(x=>x==c);
                Cards = new Queue<Card>(Cards.Where(x=>x!=c));
                return c;
			}
        }

        public void AddCard(Card c)
        {
            lock (Cards)
            {
                if (Cards.Count == 52)
                {
                    Program.Write(Enums.LogMessageType.Error, "Cannot add more than 52 cards");
                }
                Cards.Enqueue(c);
            }
        }

        public IEnumerator<Card> GetEnumerator()
        {
            return ((IEnumerable<Card>)Cards).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<Card>)Cards).GetEnumerator();
        }

        public IEnumerable<string> Pack()
        {
            foreach (var card in Cards)
            {
                yield return string.Format("{0}\'{1}", (byte)card.Value, (byte)card.Type);
            }
        }

        public void UnpackFrom(string data)
        {
            throw new NotImplementedException();
        }
    }

    public struct Card
    {
        public CardType Type;
        public CardValue Value;

        public Card(CardType t, CardValue val)
        {
            Type = t;
            Value = val;
        }

        public Card(int type, int val)
        {
            Type = (CardType)type;
            Value = (CardValue)val;
        }

        public static bool operator ==(Card c1, Card c2) {
            return c1.Type == c2.Type && c1.Value == c2.Value;
        }

		public static bool operator !=(Card c1, Card c2)
		{
			return c1.Type != c2.Type || c1.Value != c2.Value;
		}

        public override string ToString()
        {
            return string.Format("{0} of {1}", (int)Value, Type.ToString());
        }
    }

    public enum CardValue : byte {
        Two = 2,
        Three = 3,
        Four = 4,
        Five = 5, 
        Six = 6,
        Seven = 7,
        Eight = 8,
        Nine = 9,
		Ten = 10,
		Ace = 11,
        JCard = 12,
        Queen = 13,
        King = 14
    }

    [Flags]
    public enum CardType : byte
    {
        Heart = 1,
        Spades = Heart << 1, // 2
        Trifle = Heart << 2,  // 4
        Romb = Heart << 3 // 8
    }
}
