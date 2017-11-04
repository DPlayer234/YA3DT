using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SAE.YA3DT.Menu
{
    /// <summary>
    ///     Stores the functions for the buttons in the main menu
    /// </summary>
    public class MainMenu : MonoBehaviour
    {
        /// <summary> The assigned menu handler </summary>
        public MainMenuHandler menuHandler;

        /// <summary>
        ///     Loads the main scene and starts the game
        /// </summary>
        public void PlayGame()
        {
            SceneHelper.StartLoadScene("Main");
        }

        /// <summary>
        ///     Exits the game
        /// </summary>
        public void ExitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        /// <summary>
        ///     Goes to the HighScore Display
        /// </summary>
        public void GoToHighScoreScreen()
        {
            SceneHelper.StartLoadScene("HighScores");
        }

        /// <summary>
        ///     Goes to the Credits Screen
        /// </summary>
        public void GoToCreditsScreen()
        {
            SceneHelper.StartLoadScene("Credits");
        }

        /// <summary>
        ///     Goes back to the menu
        /// </summary>
        public void GoToMenu()
        {
            SceneHelper.StartLoadScene("Menu");
        }

        /// <summary>
        ///     Changes the menu state to the difficulty selection
        /// </summary>
        public void SelectDifficulty()
        {
            menuHandler.ChangeState(MainMenuHandler.MenuState.DifficultySelection);
        }

        /// <summary>
        ///     Changes the menu back to the main state
        /// </summary>
        public void GoBackToMainState()
        {
            menuHandler.ChangeState(MainMenuHandler.MenuState.Main);
        }
    }
}
