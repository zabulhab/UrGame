using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The turn controller for the player
/// </summary>
public class PlayerTurn :  Turn
{
    internal override void TurnSetup()
    {
        turnSideName = SideName.PlayerSide;
        int i = 0;
        foreach (Piece piece in allPieces)
        {
            piece.SideName = turnSideName;
            piece.SetAssociatedTurnObject(this);

            // store start location and index of each piece
            pieceStartLocations.Add(piece.transform.position);
            piece.SetStartIndex(i);
            i++;
        }
    }

    /// <summary>
    /// Begins the player phase
    /// </summary>
    internal override void ActivatePhase()
    {
        if (isFrozen)
        {
            SetFreezePanelVisible(true);
        }

        // Write the grid status to a file, if desired
        if (grid.GridWriteEnabled)
        {
            grid.WriteBoardStatusToFile();
        }

        rolledNumberText.SetActive(false); // get rid of old rolled number

        if (!AreAllPiecesFrozen() && PreRollOpenSpacesAvailable())
        {
            OpenRollUI();
        }
        else
        {
            EndTurn();
        }
    }

    internal override void SetTurnRepeat()
    {
        ActivatePhase();
    }

    /// <summary>
    /// Provided a list of tiles that the enemy could be on for the next turn,
    /// returns a list of pieces that this playerturn is within range to kill
    /// on the next turn if the enemy does decide to move to that tile.
    /// </summary>
    /// <returns>The killable AI pieces next turn.</returns>
    /// <param name="pieceToTileDict">Map of AI pieces to tiles they can land on</param>
    internal List<Piece> GetKillableAIPiecesNextTurn(Dictionary<Piece, Tile> pieceToTileDict)
    {
        List<Piece> killablePieces = new List<Piece>();
        foreach (KeyValuePair<Piece,Tile> entry in pieceToTileDict)
        {
            Piece piece = entry.Key;
            Tile tile = entry.Value;
            // Checks if any player piece can move to that tile
            foreach (Piece playerPiece in this.allPieces)
            {
                if (playerPiece.GetPieceStatus()==Piece.PieceStatus.Deployed)
                {
                    for (int i = 1; i < 4; i++)
                    {
                        /// Checks, given a piece on this playerSide, if the tile
                        /// it will be able to land on given a potential rollNum
                        /// is equal to a tile that the AI may move a piece to
                        if (playerPiece.GetTargetTile(i) == tile)
                        {
                            killablePieces.Add(piece);
                        }
                    }
                }
            }
        }
        return killablePieces;
    }
}
