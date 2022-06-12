using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blackjack
{

    public class Hand
    {
        public List<Card> Cards;

        public IHandResult Result;

        public float Bet;

        // For splitting aces, requires us to make the player force stand.
        public bool ForceStand = false;

        public Hand(float bet = 0)
        {
            Cards = new List<Card>();
            Bet = bet;
        }

        public int CalculateValue()
        {
            int value = 0;
            foreach (Card card in Cards)
            {
                value += card.GetValue();
            }
            int aces = GetAcesCount();

            for (int i = 0; i < aces; i++)
            {
                if (value > 21)
                {
                    value -= 10;
                }
            }

            return value;
        }

        private int GetAcesCount()
        {
            int count = 0;

            foreach (Card card in Cards)
            {
                if (card.CardType == CardType.ACE)
                {
                    count++;
                }
            }

            return count;
        }

        /*
        * Deal card to hand.
        */
        public Card DealCard()
        {
            var selectedCard = Program.DeckCards[Program.Random.Next(Program.DeckCards.Count)];

            Cards.Add(selectedCard);

            return selectedCard;
        }

        /*
         * Handles what to do with the hand next.
         */
        public void Handle(bool stand = false)
        {


            // Player has gotten over 21, hand is lost.
            if (CalculateValue() > 21)
            {
                EndHand(new LoseResult());
                Program.NextHand();
            }

            // Player has 21, doesn't need to play the hand further.
            else if (CalculateValue() == 21 || stand || ForceStand)
            {
                Program.NextHand();
            }

            Program.WaitForPlay(this);
        }

        public void EndHand(IHandResult result)
        {
            Result = result;
        }

        public void EvaluateScore()
        {
            var dealerValue = Program.Dealer.Hands[0].CalculateValue();
            var handValue = CalculateValue();

            if (dealerValue > 21)
                EndHand(new WinResult()); // Player automatically wins if dealer busts because we already checked for player busts before.
            else if (handValue > dealerValue)
                EndHand(new WinResult()); // Player has a higher hand than the dealer, win.
            else if (handValue == dealerValue)
                EndHand(new PushResult()); // Player has tied the dealer, tie.
            else
                EndHand(new LoseResult()); // Player has a lower hand than the dealer, loss.
        }
    }
}
