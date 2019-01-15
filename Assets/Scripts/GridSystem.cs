using System.Collections.Generic;
using UnityEngine;
using System.IO;

/// <summary>
/// Stores a list and sublists of all tiles on the board
/// </summary>
public class GridSystem : MonoBehaviour 
{
    // The number of tiles that pieces on a given side have access to
    private static readonly int TILE_COUNT = 14;

    /// <summary>
    /// Gets or sets a value indicating whether this <see cref="T:GridSystem"/>'s
    /// grid write function is enabled or not.
    /// Used for debugging.
    /// </summary>
    /// <value><c>true</c> if grid write enabled; otherwise, <c>false</c>.</value>
    internal bool GridWriteEnabled { get; set; }

    [SerializeField]
    private Tile[] allTiles = new Tile[20];

    [SerializeField]
    private Tile[] accessibleTilesPlayer = new Tile[TILE_COUNT];

    [SerializeField]
    private Tile[] accessibleTilesEnemy = new Tile[TILE_COUNT];

    private void Start()
    {
        for (int i = 0; i < TILE_COUNT; i++)
        {
            accessibleTilesEnemy[i].TileNumber = i;
            accessibleTilesPlayer[i].TileNumber = i;
        }
        if (GridWriteEnabled)
        {
            ClearBoardOutputFile(); // clear the board output file on restart
        }
    }

   /// <summary>
   /// Takes in the desired index of a piece and returns the tile to move it to
   /// </summary>
   /// <returns>The tile object to land on.</returns>
   /// <param name="destTileIdx">Index of desired tile</param>
    internal Tile TileToLandOn(int destTileIdx, Turn.SideName playerName)
    {
        if (playerName == Turn.SideName.PlayerSide)
        {
            return accessibleTilesPlayer[destTileIdx];
        }
        else if (playerName == Turn.SideName.EnemySide)
        {
            return accessibleTilesEnemy[destTileIdx];
        }
        Debug.LogError("The phase names are wrong");
        return null;
    }

    /// <summary>
    /// Clears the board output file so we can write newly to it on startup
    /// </summary>
    private void ClearBoardOutputFile()
    {
        string path = "Assets/Resources/GridStatuses.txt";

        // Setup writer
        StreamWriter writer = new StreamWriter(path, false);
        writer.WriteLine("");
    }

    /// <summary>
    /// Prints a string that shows how many pieces 
    /// of each side a tile has on top of it, if any
    /// </summary>
    public void WriteBoardStatusToFile()
    {
        string path = "Assets/Resources/GridStatuses.txt";

        // Setup writer
        StreamWriter writer = new StreamWriter(path, true);


        int tileIdx = 0;
        writer.WriteLine("-------Tile Statuses-------");
        foreach (Tile tile in allTiles)
        {
            Dictionary<Turn.SideName, int> sidePieceCount = new Dictionary<Turn.SideName, int>();
            sidePieceCount.Add(Turn.SideName.PlayerSide, 0); 
            sidePieceCount.Add(Turn.SideName.EnemySide, 0);
            writer.WriteLine("{TILE " + tile.TileID + "}:");
            foreach (Piece piece in tile.PiecesOnTop)
            {
                Turn.SideName sidename = piece.SideName;
                if (sidename == Turn.SideName.PlayerSide)
                {
                    // add 1 to the current count of that side
                    sidePieceCount[Turn.SideName.PlayerSide]++;
                }
                else if (sidename == Turn.SideName.EnemySide)
                {
                    sidePieceCount[Turn.SideName.EnemySide]++;
                }
            }

            foreach (Turn.SideName turn in sidePieceCount.Keys)
            {
                if (turn == Turn.SideName.EnemySide)
                {
                    writer.WriteLine("EPieces: " + sidePieceCount[turn]);

                }
                else if (turn == Turn.SideName.PlayerSide)
                {
                    writer.WriteLine("PPieces: " + sidePieceCount[turn]);
                }
            }


            tileIdx++;
        }
        writer.WriteLine("-----End Tile Statuses-----");

        // Close writer
        writer.Close();

        //Re-import the file to update the reference in the editor
        //AssetDatabase.ImportAsset(path);
        TextAsset asset = (TextAsset)Resources.Load("test");

    }
}
