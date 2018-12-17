using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Simple class used for the game's restart button
/// </summary>
public class RestartGame : MonoBehaviour 
{
    /// <summary>
    /// Restart this instance of the game by reloading the current scene
    /// </summary>
    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
