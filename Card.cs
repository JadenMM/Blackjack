using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blackjack;

public class CardSuit
{
    public string Value { get; private set; }

    private CardSuit(string value) { Value = value; }

    public static CardSuit SPADE { get { return new CardSuit("♠"); } }
    public static CardSuit CLUB { get { return new CardSuit("♣"); } }
    public static CardSuit HEART { get { return new CardSuit("♥"); } }
    public static CardSuit DIAMOND { get { return new CardSuit("♦"); } }

}

public enum CardType
{
    ACE = 1, TWO = 2, THREE = 3, FOUR = 4, FIVE = 5, SIX = 6, SEVEN = 7, EIGHT = 8, NINE = 9, TEN = 10, JACK = 11, QUEEN = 12, KING = 13
}

    public class Card
    {
        public CardType CardType { get; set; }
        public CardSuit CardSuit { get; set; }

        public Card(CardType type, CardSuit suit)
        {
            CardType = type;
            CardSuit = suit;
        }

        public int GetValue(bool lowAce = false)
        {
            int value = CardType switch
            {
                CardType.JACK => 10,
                CardType.QUEEN => 10,
                CardType.KING => 10,
                _ => (int)CardType,
            };

            if (CardType == CardType.ACE && !lowAce)
            {
                value = 11;
            }

            return value;
        }

        public override string ToString()
        {

            string str = CardType switch
            {
                CardType.ACE => "A",
                CardType.TEN => "T",
                CardType.JACK => "J",
                CardType.QUEEN => "Q",
                CardType.KING => "K",
                _ => ((int)CardType).ToString(),
            };

            return $"{CardSuit.Value} {str}";
        }
    }
