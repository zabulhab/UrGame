using UnityEngine;

/// <summary>
/// Handles setup and overall control flow of turns 
/// </summary>
public class StateController : MonoBehaviour
{
    /// <summary>
    /// The turn that is currently active
    /// </summary>
    private Turn activeTurn;

    /// <summary>
    /// The turn that is currently inactive
    /// </summary>
    private Turn inactiveTurn;

    /// <summary>
    /// Reference to the instance of the player turn
    /// </summary>
    [SerializeField] private PlayerTurn playerTurn;

    /// <summary>
    /// Reference to the instance of the opponent turn, for manual 2P mode
    /// </summary>
    [SerializeField] private Turn enemyTurn;

    /// <summary>
    /// Reference to the instance of the computer turn
    /// </summary>
    [SerializeField] private AIController cpuTurn;

    /// <summary>
    /// The player who goes first
    /// </summary>
    private Turn player1;

    /// <summary>
    /// The player who goes second
    /// </summary>
    private Turn player2;

    /// <summary>
    /// The intial panel for choosing the game mode
    /// </summary>
    [SerializeField]
    private GameObject chooseModePanel;

    /// <summary>
    /// Called when the mode is selected. Handles setting UI active/inactive
    /// </summary>
    public void CloseModeChooserPane()
    {
        chooseModePanel.SetActive(false);
    }

    /// <summary>
    /// Starts the vs CPU Mode.
    /// </summary>
    public void StartVsCPUMode()
    {
        cpuTurn.AssignPlayerRef(playerTurn);
        SetupTurns(cpuTurn);
    }

    /// <summary>
    /// Starts the test version of the game, with both sides played by 1 person
    /// </summary>
    public void StartManual2PMode()
    {
        SetupTurns(enemyTurn);
    }

    /// <summary>
    /// Sets up the turn order and activates P1.
    /// </summary>
    /// <param name="opponentTurn">Which kind of opponent to use.</param>
    private void SetupTurns(Turn opponentTurn)
    {
        if (opponentTurn is AIController)
        {
            // Cast to AI first so we can access correct methods
            opponentTurn = ((AIController)opponentTurn);
        }

        opponentTurn.TurnSetup(Turn.SideName.EnemySide);
        playerTurn.TurnSetup(Turn.SideName.PlayerSide);

        activeTurn = ChooseStartSide(opponentTurn);
        player1 = activeTurn;

        player2 = (player1 == playerTurn) ? opponentTurn : playerTurn;
        inactiveTurn = player2; // initialize inactive turn

        activeTurn.ActivatePhase();
        activeTurn.SetTurnTitleVisible(true);
    }

    /// <summary>
    /// Returns the Turn object of the currently active turn
    /// </summary>
    /// <returns>The current turn.</returns>
    internal Turn GetActiveTurn()
    {
        return activeTurn;
    }

    /// <summary>
    /// Returns the turn that is not active right now.
    /// </summary>
    /// <returns>The waiting turn.</returns>
    internal Turn GetInactiveTurn()
    {
        return inactiveTurn;
    }

    /// <summary>
    /// Switches the turn to the other player. Called from the end turn button.
    /// </summary>
    public void SwitchTurn()
    {
        if (activeTurn.GetInstanceID() == player1.GetInstanceID())
        {
            activeTurn = player2;
            inactiveTurn = player1;

        }
        else if (activeTurn.GetInstanceID() == player2.GetInstanceID())
        {
            activeTurn = player1;
            inactiveTurn = player2;
        }
        activeTurn.ActivatePhase();
        SwapTurnTitleText();
    }

    /// <summary>
    /// Switches which turn title is showing.
    /// Used when the turn is switched.
    /// </summary>
    private void SwapTurnTitleText()
    {
        inactiveTurn.SetTurnTitleVisible(false);
        activeTurn.SetTurnTitleVisible(true);
    }

    /// <summary>
    /// Randomly returns which side is player1
    /// </summary>
    private Turn ChooseStartSide(Turn opponentTurn)
    {
        Turn[] turnList = new Turn[2];
        turnList[0] = playerTurn;
        turnList[1] = opponentTurn;
        return turnList[Random.Range(0, turnList.Length)];
    }

}