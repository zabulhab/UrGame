using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The basic dice rolling class
/// </summary>
public class DiceRoll : MonoBehaviour
{
   /// <summary>
   /// Implements the rolling of the dice.
   /// </summary>
   /// <returns>The random number rolled</returns>
    public static int Roll()
    {
        int rolledNum = Random.Range(0, 4);
        Debug.Log("Rolled a " + rolledNum + "!");
        return rolledNum;
    }
}
