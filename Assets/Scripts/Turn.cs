using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

/// <summary>
/// The basic turn class, from which we derive the player and enemy turn classes
/// </summary>
public abstract class Turn : MonoBehaviour
{
    [SerializeField]
    protected GridSystem grid;

    // Reference to the diceroller class so we can 
    // use its Roll() method (no instance needed)
    public static DiceRoll DiceRoller;

    [SerializeField]
    protected AudioSource ClickSFX;

    internal bool GameIsOver { get; set; }

    // The total number of pieces per side
    private static readonly int PIECE_COUNT = 7;

    // A list of all pieces that can be moved for this side
    [SerializeField]
    [Header("Don't change size from 7!")]
    protected List<Piece> allPieces = new List<Piece>();

    // List of starting locations for pieces
    internal List<Vector3> pieceStartLocations = new List<Vector3>();

    private Piece selectedPiece;

    // The last number that the user has rolled
    protected int rolledNumber;

    // TODO: Rename this to turnEndPanel
    // Displays at the end of the turn; used to start the other player's phase
    [SerializeField]
    private GameObject turnStartPanel;

    [SerializeField]
    private GameObject rollPhasePanel;

    [SerializeField]
    private GameObject movePhasePanel;

    [SerializeField]
    private GameObject freezeBGPanel;

    // The panel to display when this turn ends the game
    [SerializeField]
    private GameObject gameOverPanel;

    // The button that restarts the game. Moves to center on game over
    [SerializeField]
    private GameObject restartButton;

    // Displays the turn name on the top of the screen.
    // Always visible while this turn is active
    [SerializeField]
    private GameObject turnTitleText;

    // Reference to the text object that displays the rolled number.
    // Right now, both sides share one object for this
    [SerializeField]
    protected GameObject rolledNumberText;

    // Whether or not the user is allowed to click on a piece yet to move it.
    // This is active only after rolling the dice
    internal bool PieceSelectionPhase { get; set; }

    // Can either be "PlayerSide" or "EnemySide". Used
    // to know which grid spaces to access
    [HideInInspector]
    public enum SideName { PlayerSide, EnemySide };

    // side name of this turn
    protected SideName turnSideName;

    // Whether or not this side has been frozen from 
    // moving pieces already on the board. Lasts one turn.
    protected bool isFrozen;

    internal abstract void TurnSetup();

    /// <summary>
    /// Begins the player phase by opening the initial panel.
    /// Called from the StateController's SwitchTurn method.
    /// </summary>
    internal abstract void ActivatePhase();

    // Checks if a player clicked on a piece, and changes the selected piece
    // to that one
    private void Update()
    {
        // if player clicks during selection phase
        if (PieceSelectionPhase && Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // If we hit something, select that piece and open the move UI
            if (Physics.Raycast(ray, out hit))
            {
                // If we hit a piece
                if (hit.transform.gameObject.GetComponent<Piece>() != null)
                {
                    Piece hitPiece = hit.transform.gameObject.GetComponent<Piece>();
                    // If the piece is on our side
                    if (allPieces.Contains(hitPiece))
                    {
                        // if it should be able to move
                        if (!isFrozen && hitPiece.GetPieceCanMove() ||
                           isFrozen && (hitPiece.GetPieceStatus() ==
                                              Piece.PieceStatus.Undeployed) &&
                                                    hitPiece.GetPieceCanMove())
                        {
                            // Move the piece (and possibly end the turn)

                            selectedPiece = hitPiece;
                            rolledNumberText.SetActive(false);
                            ClickSFX.Play(0); // play the SFX
                            MovePiece();

                            // Tile Function will now activate.
                        }
                    }
                }
                // Else we didn't hit a piece
            }
        }
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
    /// Gets the side name enum of this side.
    /// </summary>
    /// <returns>The side name.</returns>
    internal Turn.SideName getSideName()
    {
        return turnSideName;
    }

    protected void DisableTurnStartPanel()
    {
        turnStartPanel.SetActive(false);
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
        turnStartPanel.SetActive(false);
        rollPhasePanel.SetActive(true);
    }

    /// <summary>
    /// Brings up the Panel that waits for the player to move a piece.
    /// Called after any piece has been selected
    /// </summary>
    private void OpenMoveUI()
    {
        movePhasePanel.SetActive(true);
    }

    // Set the title of the turn to showing/hidden in the UI
    public void SetTurnTitleVisible(bool activeStatus)
    {
        turnTitleText.SetActive(activeStatus);
    }

    // TODO: Make two separate panels so you can end one phase
    // and begin another from the same button press, linking
    // the appropriate turns by dragging them
    /// <summary>
    /// Takes away the movement UI, signalling the player turn's end.
    /// Activated by landing on tiles other than repeat tiles, or 
    /// when exiting the board at the finish line.
    /// Auto-ends without the End Turn button if AITurnEnded is true.
    /// </summary>
    public void EndTurn(bool AITurnEnded = false, StateController phaseController = null)
    {
        SetFreezePanelVisible(false);
        UnfreezeBoardPieces();
        movePhasePanel.SetActive(false);
        turnStartPanel.SetActive(true);
        if (AITurnEnded) // bypass having to press the turn end button
        {
            if (!GameIsOver)
                phaseController.SwitchTurn();
        }
        // Disable clicking pieces
        PieceSelectionPhase = false;
        SetAllPiecesUnSelectable();
        if (GameIsOver)
        {
            ActivateGameOver();
        }
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
                piece.SetPieceCanMove(piece.CheckPieceCanMoveToTile(rolledNumber));
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
                    if (piece.GetPieceStatus() == Piece.PieceStatus.Undeployed)
                    {
                        piece.SetPieceCanMove(piece.CheckPieceCanMoveToTile(rolledNumber));
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
            piece.SetPieceCanMove(false);
        }
    }

    //protected bool CheckPieceCanMoveToTile(Piece piece)
    //{
    //    //// if (get tile at piece position + spaces to move)
    //    //Tile targetTile = piece.GetTargetTile(rolledNumber);
    //    ////      has too many tiles of the same color on it already
    //    //if (targetTile != null && (!targetTile.IsMaxNumSamePieceOnTop()))
    //    //{
    //    //    piece.SetPieceCanMove(true);
    //    //    return true;
    //    //}
    //    //return false;
    //    Tile targetTile = piece.GetTargetTile(rolledNumber);
    //    if (targetTile != null && (!targetTile.IsMaxNumSamePieceOnTop()))
    //    {
    //        return true;
    //    }
    //    // If this piece is at the end of the board and
    //    // theoretically targeting a tile at the beginning 
    //    // of the board that has a piece on it already
    //    else if (targetTile != null && (targetTile.IsMaxNumSamePieceOnTop()))
    //    {
    //        if (targetTile.TileNumber < piece.CurrentTileIdx)
    //        {
    //            return true;
    //        }
    //    }
    //    return false;
    //}

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
        selectedPiece.SetNumSpacesToMove(rolledNumber);
        selectedPiece.UnHighlight();
        SetAllPiecesUnSelectable(); // disable selecting other pieces

        UnfreezeBoardPieces();

        if (rolledNumber != 0)
        {
            selectedPiece.MoveToTargetTile();
        }
    }

    /// <summary>
    /// Used by the repeat tiles. Activates this turn again.
    /// </summary>
    internal abstract void SetTurnRepeat();

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
            if (piece.GetPieceStatus() == Piece.PieceStatus.Undeployed)
            {
                piece.SetPieceCanMove(false);
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
                if (piece.GetPieceStatus() == Piece.PieceStatus.Undeployed)
                {
                    piece.SetPieceCanMove(true);
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
                if (piece.GetPieceStatus() == Piece.PieceStatus.Undeployed)
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
                if (isFrozen && !(piece.GetPieceStatus() == Piece.PieceStatus.Undeployed))
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
    /// <returns><c>true</c>, if roll is movement possible was pred, <c>false</c> otherwise.</returns>
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
                    if (isFrozen && !(piece.GetPieceStatus() == Piece.PieceStatus.Undeployed))
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
    /// Intended for use in determining risk factor for the AI.
    /// </summary>
    /// <returns>The value for this side.</returns>
    internal int GetSideValue()
    {
        // check if no piece is deployed yet
        foreach (Piece piece in allPieces)
        {
            if (piece.GetPieceStatus() != Piece.PieceStatus.Undeployed)
            {
                break;
            }
        }
        int totalBoardValue = 0;
        foreach (Piece piece in allPieces)
        {
            switch (piece.GetPieceStatus())
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
            if (piece.GetPieceStatus() != Piece.PieceStatus.Finished)
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
    private void ActivateGameOver()
    {
        turnStartPanel.SetActive(false);
        rollPhasePanel.SetActive(false);
        gameOverPanel.SetActive(true);
        SetTurnTitleVisible(false);

        // center restart button
        float buttonHeightHalf = restartButton.GetComponent<RectTransform>().rect.height / 2;
        float buttonWidthHalf = restartButton.GetComponent<RectTransform>().rect.width / 2;
        // move button
        restartButton.transform.position = 
            (new Vector2((Screen.width/2)+buttonWidthHalf, 
                         (Screen.height/2)-buttonHeightHalf));

    }

}