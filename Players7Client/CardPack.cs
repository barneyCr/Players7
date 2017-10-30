using System;
using System.Collections.Generic;

namespace Players7Client
{
    public class CardPack
    {
        public List<Card> Cards;
        // no need to create a Stack since this is only read from the server and we recreate it with each move and packet

        public CardPack()
        {
            Cards = new List<Card>(52);
        }

        public CardPack(List<Card> list) {
            this.Cards = list;
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

        public bool Umflator
        {
            get
            {
                return this.Value == CardValue.Two || this.Value == CardValue.JCard;
            }
        }

        public static bool operator ==(Card c1, Card c2)
        {
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

        public override bool Equals(object obj)
        {
            if (obj is Card)
            {
                return (Card)obj == this;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return this.ToString().GetHashCode();
        }
    }

    public enum CardValue : byte
    {
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