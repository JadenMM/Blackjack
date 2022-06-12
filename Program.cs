using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.Text;

namespace Blackjack;

public class Program
{

    public static List<Card> DeckCards = new();
    private static List<Card> Discard = new();

    public static Player Dealer = new();
    private static Player Player = new();


    public static readonly Random Random = new();

    private static float StartingBet = 0; // Bet the player starts the round off with
    private static int CurrentHand = 1;

    private const int DECK_COUNT = 6; // Number of card decks to simulate.
    private const int RESHUFFLE_PLACE = 75; // "Reshuffle" decks after this amount of cards are remaining. Simulates the plastic insert card.

    private static Stats PlayerStats = FileService.LoadStats();

    static void Main()
    {
        StartGame();
    }

    private static void StartGame()
    {
        Console.ForegroundColor = ConsoleColor.White;

        DeckCards = new();
        Discard = new();
        Dealer = new();
        Player = new();

        for (int i = 0; i < DECK_COUNT; i++)
        {
            PopulateDeck();
        }

        CollectBet();

    }

    /*
    * Populate deck with cards.
    */
    private static void PopulateDeck()
    {
        foreach (CardType cardType in Enum.GetValues(typeof(CardType)))
        {
            DeckCards.Add(new Card(cardType, CardSuit.SPADE));
            DeckCards.Add(new Card(cardType, CardSuit.CLUB));
            DeckCards.Add(new Card(cardType, CardSuit.HEART));
            DeckCards.Add(new Card(cardType, CardSuit.DIAMOND));
        }
    }

    /*
     * Collects the bet to start off the round.
     */
    private static void CollectBet()
    {

        if (PlayerStats.Coins <= 0)
        {
            Console.WriteLine($"You have been given 100 coins to continue playing.");
            PlayerStats.Coins = 100;
            FileService.SaveStats(PlayerStats);
        }

        Console.WriteLine($"Please enter your bet. You have: {PlayerStats.Coins}.");

        string request = Console.ReadLine();

        if (request.Equals("stats", StringComparison.InvariantCultureIgnoreCase))
        {
            PlayerStats.ShowStats();
            CollectBet();
            return;
        }

        int bet = 0;
        try
        {
            bet = int.Parse(request);
        }
        catch (Exception)
        {
            Console.WriteLine("Incorrect bet.");
            CollectBet();
        }

        if (bet <= 0)
        {
            Console.WriteLine("Bet must be more than 0.");
            CollectBet();
            return;
        }

        if (bet > PlayerStats.Coins)
        {
            Console.WriteLine("You cannot afford that bet.");
            CollectBet();
            return;
        }

        StartingBet = bet;
        StartRound();
    }

    private static void StartRound()
    {
        Dealer.Hands.Add(new Hand());
        Player.Hands.Add(new Hand(StartingBet));
        
        // Dealer will always only have one hand.
        var dealerHand = Dealer.Hands[0];

        dealerHand.DealCard();

        // If dealer hits a ten card, second card is revealed.
        if (dealerHand.CalculateValue() == 10)
        {
            dealerHand.DealCard();
        }

        // Deals first two player cards.
        Player.Hands[0].DealCard();
        Player.Hands[0].DealCard();

        BroadcastCards();

        // Checks if player or dealer hit a blackjack.
        if (Dealer.Hands[0].CalculateValue() == 21)
        {
            HandleNatural(Dealer);
            return;
        }
        else if (Player.Hands[0].CalculateValue() == 21)
        {
            HandleNatural(Player);
            return;
        }

        WaitForPlay(Player.Hands[0]);
    }

    /*
     * Print out player's and dealer's cards.
     * Result boolean is used to determine if these cards are being broadcasted at the end of the game or not.
     * If cards are being broadcasted at the end of the game, won't try to check for the current hand & will try to check hand results.
     */
    private static void BroadcastCards(bool results = false)
    {
        Console.Clear();
        Console.WriteLine($"Dealer ({Dealer.Hands[0].CalculateValue()}) ");
        foreach (Card card in Dealer.Hands[0].Cards)
        {
            Console.WriteLine(card.ToString());
        }


        for (int i = 0; i < Player.Hands.Count; i++)
        {
            Hand hand = Player.Hands[i];

            bool multipleHands = Player.Hands.Count > 1;

            if (multipleHands && CurrentHand-1 == i && !results || results && hand.Result is WinResult)
            {
                Console.ForegroundColor = ConsoleColor.Green;
            } else if (results && hand.Result is LoseResult)
            {
                Console.ForegroundColor = ConsoleColor.Red;
            } else if (results && hand.Result is PushResult)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
            } else if (results && hand.Result is NaturalResult)
            {
                Console.ForegroundColor = ConsoleColor.DarkGreen;
            }

            // Ternary used to display the corresponding hand if player has more than one active hand.
            Console.WriteLine($"Player {(multipleHands ? "Hand " + (i+1) : "")} ({hand.CalculateValue()})"); 

            foreach (Card card in hand.Cards)
            {
                Console.WriteLine(card.ToString());
            }

            Console.ForegroundColor = ConsoleColor.White;
        }

    }

    /*
     * Give player's options and respond to commands.
     */
    public static void WaitForPlay(Hand hand)
    {
        List<string> validCommands = new() { "Hit", "Stand" };

        // Check if player has enough money to split or double.
        if (GetAllBets() + hand.Bet * 2 > PlayerStats.Coins)
        {
            // Check if player is eligible to double down.
            if (hand.CalculateValue() >= 9 && hand.CalculateValue() <= 11 && hand.Cards.Count == 2)
            {
                validCommands.Add("Double");
            }

            // Check if player is eligible to split their pairs.
            if (hand.Cards.Count == 2 && hand.Cards[0].CardType == hand.Cards[1].CardType)
            {
                validCommands.Add("Split");
            }
        }


        if (validCommands.Count == 2)
        {
            Console.WriteLine($"{validCommands[0]} or {validCommands[1]}");
        }
        else
        {
            StringBuilder builder = new($"{validCommands[0]}, {validCommands[1]}");
            if (validCommands.Count == 3)
            {
                builder.Append($" or {validCommands[2]}");
            }
            else
            {
                builder.Append($", {validCommands[2]}, or {validCommands[3]}");
            }

            Console.WriteLine(builder.ToString());
        }

        var command = Console.ReadLine();

        if (command is null || !validCommands.Contains(command, StringComparer.OrdinalIgnoreCase))
        {
            Console.WriteLine("Incorrect command.");
            WaitForPlay(hand);
            return;
        }

        // Player wants an additional card.
        if (command.Equals("hit", StringComparison.InvariantCultureIgnoreCase))
        {
            hand.DealCard();
            BroadcastCards();

            hand.Handle();
        }

        // Player chooses to stick with their hand.
        else if (command.Equals("stand", StringComparison.InvariantCultureIgnoreCase))
        {
            hand.Handle(true);
        }

        // Allows player to double their bet, but they can only receive one additional card.
        else if (command.Equals("double", StringComparison.InvariantCultureIgnoreCase))
        {
            hand.Bet *= 2;
            hand.DealCard();

            BroadcastCards();

            hand.Handle(true);
        }

        // Allows player to split their pairs into two separate hands
        else if (command.Equals("split", StringComparison.InvariantCultureIgnoreCase))
        {
            // Gets second card to split into the new hand.
            var pair = hand.Cards[1];
            hand.Cards.Remove(pair);

            // Creates new hand
            var newHand = new Hand(StartingBet);
            newHand.Cards.Add(pair);

            Player.Hands.Add(newHand);

            // If splitting aces, only one card may be dealt.
            if (pair.CardType is CardType.ACE)
            {
                // Deals another card and then makes that hand stand
                hand.DealCard();
                newHand.DealCard();

                // Force the new hand to stand so that when it's that hand's turn, it stands.
                newHand.ForceStand = true;

                // Starts to handle the first hand, will stand.
                hand.Handle(true);

                BroadcastCards();
                return;
            }

            BroadcastCards();

            WaitForPlay(hand);
        }

    }

    /*
    * Used when the player/dealer receives a natural blackjack.
    */
    private static void HandleNatural(Player player)
    {

        // If dealer hit natural blackjack
        if (Dealer == player)
        {
            if (Player.Hands[0].CalculateValue() == 21)
            {
                Player.Hands[0].EndHand(new PushResult());
                EndRound();
            }
            else
            {
                Player.Hands[0].EndHand(new LoseResult());
                EndRound();
            }
        }
        // If player hit natural blackjack
        else if (Player == player)
        {
            Player.Hands[0].EndHand(new NaturalResult());
            EndRound();
        }

    }

    /*
     * Handles dealer's play.
     */
    public static void HandleDealerCard()
    {
        while (Dealer.Hands[0].CalculateValue() < 17)
        {
            Dealer.Hands[0].DealCard();
        }

        BroadcastCards();
    }


    /*
     * Checks if player has another hand to play and plays it.
     */
    public static void NextHand()
    {
        if (Player.Hands.Count >= CurrentHand+1) // If player has another hand
        {
            CurrentHand++;
            BroadcastCards();
            WaitForPlay(Player.Hands[CurrentHand-1]);
            
        }
        else
        {
            HandleDealerCard();
            EndRound();
        }
    }

    /*
     * End hand, handle points.
     */
    private static void EndRound()
    {

        // Need to loop through first to do the first evaluations for proper colour display at the end.
        foreach (Hand hand in Player.Hands.Where(h => h.Result == null))
        {
            hand.EvaluateScore();
        }

        BroadcastCards(true);


        for (int i = 0; i < Player.Hands.Count; i++)
        {
            Hand hand = Player.Hands[i];

            var reward = hand.Bet * hand.Result.Reward;

            if (Player.Hands.Count > 1)
                Console.WriteLine($"Hand {i+1}:");

            if (hand.Result is LoseResult)
            {
                Console.WriteLine($"You lost ${Math.Abs(reward)}!");
                PlayerStats.Losses++;
            }
            else if (hand.Result is WinResult)
            {
                Console.WriteLine($"You won ${reward} with: {hand.CalculateValue()}!");
                PlayerStats.Wins++;
            }
            else if (hand.Result is PushResult)
            {
                Console.WriteLine($"You tied with the dealer! You receive ${hand.Bet} back.");
                PlayerStats.Ties++;
            }
            else if (hand.Result is NaturalResult)
            {
                Console.WriteLine($"Blackjack! You won ${reward}!");
                PlayerStats.Blackjacks++;
            }

            PlayerStats.Coins += reward;
        }

        FileService.SaveStats(PlayerStats);

        ClearRound();
        CollectBet();
    }

    /*
     * Clears current hand and adds cards to discard pile.
     * Checks if active deck is small enough for a "reshuffle".
     */
    private static void ClearRound()
    {

        // Loops through dealer's cards and handles adding them to the discord piles.
        foreach (Card card in Dealer.Hands[0].Cards)
        {
            // Removes from deck.
            DeckCards.Remove(card);

            // Adds to discard pile
            Discard.Add(card);
        }

        foreach (Hand hand in Player.Hands)
        {
            foreach (Card card in hand.Cards)
            {
                // Removes from deck.
                DeckCards.Remove(card);

                // Adds to discard pile
                Discard.Add(card);
            }
        }

        // Clears player and dealers hands
        Dealer.Hands.Clear();
        Player.Hands.Clear();


        // Checks if it's time to reshuffle deck.
        if (DeckCards.Count < RESHUFFLE_PLACE)
        {
            Console.WriteLine("Shuffling decks.");
            foreach (Card card in Discard)
            {
                DeckCards.Add(card);
            }

            Discard.Clear();
        }

        StartingBet = 0;
        CurrentHand = 1;
    }

    private static float GetAllBets()
    {
        float amount = 0;
        foreach (Hand hand in Player.Hands)
        {
            amount += hand.Bet;
        }

        return amount;
    }
}
