using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

/// <summary>
/// The basic turn class, from which we create the player and enemy turns for 
/// manual 2p mode, and also the AI controller for the vs CPU mode. PlayerTurn
/// derives from this class, while the manual 2p mode enemy is an instance of this.
/// </summary>
public class Turn : MonoBehaviour
{
    [SerializeField]
    protected GridSystem grid;

    /// <summary>
    /// The click sfx.
    /// </summary>
    [SerializeField]
    protected AudioSource clickSFX;

    // The total number of pieces per side
    private static readonly int PIECE_COUNT = 7;

    /// <summary>
    /// A list of all pieces that this turn starts off with
    /// </summary>
    [SerializeField]
    [Header("Don't change size from 7!")]
    protected List<Piece> allPieces = new List<Piece>();

    /// <summary>
    /// List of starting locations for pieces, so they can teleport back
    /// </summary>
    internal List<Vector3> pieceStartLocations = new List<Vector3>();

    /// <summary>
    /// The piece that the user clicks on that will move
    /// </summary>
    private Piece selectedPiece;

    /// <summary>
    /// The last number that has been rolled on this side's turn.
    /// </summary>
    protected int rolledNumber;

    /// <summary>
    /// Displays at the end of the turn; used to end 
    /// this phase and start the other player's phase
    /// </summary>
    [SerializeField]
    protected GameObject turnEndPanel;

    /// <summary>
    /// The panel that shows up for the user to roll the dice
    /// </summary>
    [SerializeField]
    private GameObject rollPhasePanel;

    /// <summary>
    /// The light blue panel that indicates that a turn is currently frozen
    /// </summary>
    [SerializeField]
    private GameObject freezeBGPanel;

    /// <summary>
    /// The panel to display when this turn ends the game
    /// </summary>
    [SerializeField]
    private GameObject gameOverPanel;

    /// <summary>
    /// The button that restarts the game. Moves to center on game over
    /// </summary>
    [SerializeField]
    private GameObject restartButton;

    /// <summary>
    /// Displays the turn name on the top of the screen.
    /// Always visible while this turn is active
    /// </summary>
    [SerializeField]
    private GameObject turnTitleText;

    /// <summary>
    /// Reference to the text object that displays the
    /// rolled number, shared by both sides
    /// </summary>
    [SerializeField]
    protected GameObject rolledNumberText;

    /// <summary>
    /// Whether or not the user is allowed to click on a piece yet to move it.
    /// This is active only after rolling the dice
    /// </summary>
    /// <value><c>true</c> if piece selection phase; otherwise, <c>false</c>.</value>
    internal bool PieceSelectionPhase { get; set; }

    /// <summary>
    /// Can either be "PlayerSide" or "EnemySide". Used
    /// to know which grid spaces to access
    /// </summary>
    [HideInInspector]
    public enum SideName { PlayerSide, EnemySide };

    /// <summary>
    /// The name of this side
    /// </summary>
    internal SideName TurnSideName;

    // Whether or not this side has been frozen from 
    // moving pieces already on the board. Lasts one turn.
    protected bool isFrozen;

    /// <summary>
    /// Gets or sets a value indicating whether the game is over. 
    /// Used by the AI's call to ActivateGameOver method to decide whether
    /// or not to allow the player UI to appear after
    /// </summary>
    /// <value><c>true</c> if game is over; otherwise, <c>false</c>.</value>
    private bool GameIsOver { get; set; }


    /// <summary>
    /// Sets up this turn's information. Used by all sides, with 
    /// different turn side names passed in through overloading
    /// </summary>
    /// <param name="sideName">Side name.</param>
    internal virtual void TurnSetup(SideName sideName)
    {
        this.TurnSideName = sideName;
        int i = 0;
        foreach (Piece piece in allPieces)
        {
            piece.SideName = TurnSideName;
            piece.SetAssociatedTurnObject(this);

            // store start location and index of each piece
            pieceStartLocations.Add(piece.transform.position);
            piece.StartIndex = i;
            i++;
        }
    }

    /// <summary>
    /// Begins the player phase by opening the initial panel.
    /// Called from the StateController's SwitchTurn method.
    /// </summary>
    internal virtual void ActivatePhase()
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

    /// <summary>
    /// Called by a hit piece; tries to move it
    /// to that one
    /// </summary>
    internal void PieceHitTryMove(Piece hitPiece)
    {

        selectedPiece = hitPiece;
        rolledNumberText.SetActive(false);
        clickSFX.Play(0); // play the SFX
        MovePiece();

    }

    /// <summary>
    /// Checks for the correct piece array length in the editor
    /// </summary>
    private void OnValidate()
    {
        if (allPieces.Count != PIECE_COUNT)
        {
            Debug.LogError("The length of the All Pieces array must be 7!");
        }
    }

    /// <summary>
    /// Sets the freeze pane to visible or invisible
    /// </summary>
    protected void SetFreezePanelVisible(bool visibleStatus)
    {
        this.freezeBGPanel.SetActive(visibleStatus);
    }

    /// <summary>
    /// Brings up the Panel that waits for the user to roll
    /// </summary>
    public void OpenRollUI()
    {
        turnEndPanel.SetActive(false);
        rollPhasePanel.SetActive(true);
    }

    // Set the title of the turn to showing/hidden in the UI
    public void SetTurnTitleVisible(bool activeStatus)
    {
        turnTitleText.SetActive(activeStatus);
    }

    /// <summary>
    /// Takes away the movement UI, signalling the player turn's end.
    /// Activated by landing on tiles other than repeat tiles, or 
    /// when exiting the board at the finish line.
    /// Auto-ends without the End Turn button if AITurnEnded is true.
    /// </summary>
    public void EndTurn(bool AITurnEnded = false, StateController phaseController = null)
    {
        SetFreezePanelVisible(false);
        turnEndPanel.SetActive(true);

        UnfreezeBoardPieces();

        if (AITurnEnded) 
        {
            // bypass having to press the turn end button with AI
            if (!GameIsOver)
                phaseController.SwitchTurn();
            else
                turnEndPanel.SetActive(false);
        }
        // Disable clicking pieces
        PieceSelectionPhase = false;
        SetAllPiecesUnSelectable();
    }

    /// <summary>
    /// Activated by button; tells the dice to roll
    /// </summary>
    public void RollDice()
    {
        rolledNumber = DiceRoll.Roll();

        UpdateRolledNumber(rolledNumber);
        rolledNumberText.SetActive(true);
        rollPhasePanel.SetActive(false);
        if (rolledNumber == 0 || !PostRollOpenSpacesAvailable())
        {
            rolledNumberText.SetActive(true);
        }
    }

    /// <summary>
    /// Sets the roll text to the number that was rolled
    /// </summary>
    private void UpdateRolledNumber(int rolledNum)
    {
        rolledNumberText.GetComponent<Text>().text = "Rolled: " + rolledNum;
    }

    /// <summary>
    /// Sets appropriate pieces on this side 
    /// selectable/unselectable for potential movement.
    /// If frozen, sets only undeployed pieces selectable.
    /// </summary>
    protected void SetPiecesSelectable()
    {
        if (!isFrozen) // allow selecting all pieces
        {
            foreach (Piece piece in allPieces)
            {
                piece.PieceCanMove = piece.CheckPieceCanMoveToTile(rolledNumber);
            }

        }
        else // only allow selection of undeployed pieces
        {

            if (!PostRollOpenSpacesAvailable())
            {
                EndTurn();
            }
            else
            {
                foreach (Piece piece in allPieces)
                {
                    if (piece.Status == Piece.PieceStatus.Undeployed)
                    {
                        piece.PieceCanMove = piece.CheckPieceCanMoveToTile(rolledNumber);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Sets all of the pieces to be unselectabe
    /// </summary>
    protected void SetAllPiecesUnSelectable()
    {
        foreach (Piece piece in allPieces)
        {
            piece.PieceCanMove = false;
        }
    }

    /// <summary>
    /// If user has not rolled a 0, sets the appropriate pieces to be selectable
    /// </summary>
    public void TryActivateSelectionPhase()
    {
        if (rolledNumber != 0 && PostRollOpenSpacesAvailable())
        {
            PieceSelectionPhase = true;
            SetPiecesSelectable();
        }
        else
        {
            PieceSelectionPhase = false;
            SetAllPiecesUnSelectable();
            EndTurn();
        }
    }

    /// <summary>
    /// Activated by clicking on a piece; tells the 
    /// piece to move by the last number rolled
    /// </summary>
    public void MovePiece()
    {
        selectedPiece.SetTargetTile(rolledNumber);
        selectedPiece.UnHighlight();
        SetAllPiecesUnSelectable(); // disable selecting other pieces

        UnfreezeBoardPieces();

        if (rolledNumber != 0)
        {
            selectedPiece.MovePieceForward();
        }
    }

    /// <summary>
    /// Used by the repeat tiles. Activates this turn again.
    /// Overridden in the AI controller.
    /// </summary>
    internal virtual void SetTurnRepeat()
    {
        ActivatePhase();
    }

    /// <summary>
    /// Called when the other player lands on an imprisonment tile. 
    /// Prevents moving any pieces that are already on the board for one turn.
    /// Pieces not yet on the board may still be deployed.
    /// </summary>
    internal void FreezeBoardPieces()
    {
        isFrozen = true;
        SetFreezePanelVisible(true);
        foreach (Piece piece in allPieces)
        {
            if (piece.Status == Piece.PieceStatus.Undeployed)
            {
                piece.PieceCanMove = false;
            }
        }
    }

    /// <summary>
    /// Unfreeze pieces on board if they were frozen this turn
    /// </summary>
    private void UnfreezeBoardPieces()
    {
        if (isFrozen)
        {
            isFrozen = false;
            foreach (Piece piece in allPieces)
            {
                if (piece.Status == Piece.PieceStatus.Undeployed)
                {
                    piece.PieceCanMove = true;
                }
            }
        }
    }

    /// <summary>
    /// Determine if no piece can be moved during a frozen turn
    /// </summary>
    /// <returns><c>true</c>, if all pieces frozen, <c>false</c> otherwise.</returns>
    internal bool AreAllPiecesFrozen()
    {
        if (isFrozen)
        {
            foreach (Piece piece in allPieces)
            {
                // Return false if there are any undeployed pieces
                if (piece.Status == Piece.PieceStatus.Undeployed)
                {
                    return false;
                }
            }
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// ONLY use AFTER rolling the dice.
    /// Returns true if there are any tiles for any piece to move to, 
    /// meaning that the tile does not already have the maximum number
    /// of same-side pieces on top of it.
    /// </summary>
    /// <returns><c>true</c>, if no spaces free, <c>false</c> otherwise.</returns>
    internal bool PostRollOpenSpacesAvailable()
    { 
        foreach (Piece piece in allPieces)
        {
            Tile targetTile = piece.GetTargetTile(rolledNumber);

            if (targetTile != null && 
                (!targetTile.IsMaxNumSamePieceOnTop()))
            {
                // ignore if the piece is deployed already during frozen turn
                if (isFrozen && !(piece.Status == Piece.PieceStatus.Undeployed))
                {
                    ;
                }
                else // some piece can move by that roll amount
                {
                    return true;
                }
            }
        }
        return false;
    }

    /// <summary>
    /// ONLY use BEFORE rolling the dice.
    /// Returns true if it is possible for a certain rolled number
    /// to result in a piece being able to move to a space,
    /// meaning that the tile does not already have the maximum number
    /// of same-side pieces on top of it.
    /// </summary>
    /// <returns><c>true</c>, if spaces available for any possible roll, 
    ///                                 <c>false</c> otherwise.</returns>
    internal bool PreRollOpenSpacesAvailable()
    {
        foreach (Piece piece in allPieces)
        {
            // Try each roll number possibility, from 1 to 3
            for (int rollNum = 1; rollNum <= 3; rollNum++)
            {
                Tile targetTile = piece.GetTargetTile(rollNum);

                if (targetTile != null &&
                    (!targetTile.IsMaxNumSamePieceOnTop()))
                {
                    // ignore if the piece is deployed already during frozen turn
                    if (isFrozen && !(piece.Status == Piece.PieceStatus.Undeployed))
                    {
                        ;
                    }
                    else // some piece is able to move
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    /// <summary>
    /// Returns the value of the current state of the board, for the CPU's side.
    /// Intended for future use in accounting for risk factor with the AI.
    /// </summary>
    /// <returns>The value for this side.</returns>
    internal int GetSideValue()
    {
        // check if no piece is deployed yet
        foreach (Piece piece in allPieces)
        {
            if (piece.Status != Piece.PieceStatus.Undeployed)
            {
                break;
            }
        }
        int totalBoardValue = 0;
        foreach (Piece piece in allPieces)
        {
            switch (piece.Status)
            {
                case Piece.PieceStatus.Finished:
                    totalBoardValue += 15;
                    break;
                case Piece.PieceStatus.Deployed:
                    totalBoardValue += piece.CurrentTileIdx;
                    break;
                case Piece.PieceStatus.Undeployed:
                    totalBoardValue += 0;
                    break;
            }
        }
        return totalBoardValue;
    }

    /// <summary>
    /// Returns true if all of the pieces on this side are finished, 
    /// signalling the end of the game
    /// </summary>
    /// <returns><c>true</c>, if all pieces finished, <c>false</c> otherwise.</returns>
    internal bool AreAllPiecesFinished()
    {
        foreach (Piece piece in allPieces)
        {
            // if any piece is not finished yet
            if (piece.Status != Piece.PieceStatus.Finished)
            {
                return false;
            }
        }
        // else all are done
        return true;
    }

    /// <summary>
    /// Brings up the UI for when the game has ended.
    /// </summary>
    internal void ActivateGameOver()
    {
        // close unneccesary UI and just show game over UI
        GameIsOver = true;
        turnEndPanel.SetActive(false);
        rollPhasePanel.SetActive(false);
        rolledNumberText.SetActive(false);
        SetTurnTitleVisible(false);
        gameOverPanel.SetActive(true);

        // center restart button
        float buttonHeightHalf = restartButton.GetComponent<RectTransform>().rect.height / 2;
        float buttonWidthHalf = restartButton.GetComponent<RectTransform>().rect.width / 2;
        // move button
        restartButton.transform.position = 
            (new Vector2((Screen.width/2)+buttonWidthHalf, 
                         (Screen.height/2)-buttonHeightHalf));

    }

}