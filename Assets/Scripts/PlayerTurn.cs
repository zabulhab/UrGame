using System.Collections.Generic;

/// <summary>
/// The turn controller for the player. Contains a method that analyzes 
/// the state of the pieces on this side for the AI's logic
/// </summary>
public class PlayerTurn :  Turn
{
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
                if (playerPiece.Status == Piece.PieceStatus.Deployed)
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
