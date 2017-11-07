//-----------------------------------------------------------------------
// <copyright file="MainMenu.cs" company="SAE">
//     Copyright (c) Darius Kinstler, SAE. All rights reserved.
// </copyright>
// <author>Darius Kinstler</author>
//-----------------------------------------------------------------------
namespace SAE.YA3DT.Menu
{
    using UnityEngine;

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
            HighScoreHandler.lastHighScorePosition = -1;
            SceneHelper.StartLoadScene("HighScores");
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
        ///     Changes the menu state to the credits screen
        /// </summary>
        public void ShowCredits()
        {
            menuHandler.ChangeState(MainMenuHandler.MenuState.Credits);
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
