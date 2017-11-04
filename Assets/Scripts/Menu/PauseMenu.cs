using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SAE.YA3DT.Menu
{
    /// <summary>
    ///     Stores the functions for the buttons in the pause menu
    /// </summary>
    public class PauseMenu : MonoBehaviour
    {
        /// <summary> The current pause menu handler </summary>
        public PauseMenuHandler menuHandler;

        /// <summary>
        ///     Resumes and unpauses the game
        /// </summary>
        public void ResumeGame()
        {
            menuHandler.SetPauseMenuOpened(false);
        }

        /// <summary>
        ///     Returns to the main menu
        /// </summary>
        public void ReturnToMainMenu()
        {
            SceneHelper.StartLoadScene("Menu");
        }
    }
}
