using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blackjack;

public class Player
{

    public List<Hand> Hands;

    public Player()
    {
        Hands = new List<Hand>();
    }

    public void ClearHands()
    {
        Hands.Clear();
    }

}
