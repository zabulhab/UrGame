using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Contains static helper methods for getting killable statuses of a given piece
/// so we can use it in AI logic
/// </summary>
public class AIHelperChecks : MonoBehaviour 
{

    /// <summary>
    /// Returns a list of bools reporting killable statuses about the given piece
    /// </summary>
    /// <returns>The killable stats booleans, in the order of able to kill this turn,
    /// able to kill next turn, and able to be killed next turn
    /// </returns>
    /// <param name="piece">Piece.</param>
    /// <param name="killableAI">Killable ai next turn.</param>
    /// <param name="killablePlyrsThisTurn">Killable players this turn.</param>
    /// <param name="killablePlyrsNextTurn">Killable players next turn.</param>
    internal static List<bool> GetPieceKillableStats
        (Piece piece, 
         Dictionary<Piece, List<Piece>> killablePlyrsThisTurn, 
         Dictionary<Piece, List<Piece>> killablePlyrsNextTurn,
         List<Piece> killableAI)
    {
        bool canKillThisTurn = PieceCanKillThisTurn(piece, killablePlyrsThisTurn);
        bool canKillNextTurn = PieceCanKillNextTurn(piece, killablePlyrsNextTurn);
        bool canBeKilledNextTurn = PieceCanBeKilledNextTurn(piece, killableAI);
        return new List<bool>{ canKillThisTurn, canKillNextTurn, canBeKilledNextTurn };
    }

    private static bool PieceCanKillThisTurn(Piece piece, Dictionary<Piece, List<Piece>> killablePlyrsThisTurn)
    {
        return killablePlyrsThisTurn.ContainsKey(piece);
    }

    private static bool PieceCanKillNextTurn(Piece piece, Dictionary<Piece, List<Piece>> killablePlyrsNextTurn)
    {
        return killablePlyrsNextTurn.ContainsKey(piece);
    }

    private static bool PieceCanBeKilledNextTurn(Piece piece, List<Piece> killableAI)
    {
        return killableAI.Contains(piece);
    }

}
