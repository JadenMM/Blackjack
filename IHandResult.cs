using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blackjack;

public interface IHandResult
{
    public float Reward { get;}
}

public class LoseResult : IHandResult
{
    public float Reward { get => -1;}
}

public class WinResult : IHandResult
{
    public float Reward { get => 1; }
}

public class PushResult : IHandResult
{
    public float Reward { get => 0;}
}

public class NaturalResult : IHandResult
{
    public float Reward { get => 1.5f; }
}
