using UnityEngine; // for randomness

/// <summary>
/// The basic dice-rolling class
/// </summary>
public static class DiceRoll
{
   /// <summary>
   /// Implements the rolling of the dice.
   /// </summary>
   /// <returns>The random number rolled</returns>
    public static int Roll()
    {
        int rolledNum = Random.Range(1,4);
        return rolledNum;
    }
}
