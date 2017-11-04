using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SAE.YA3DT.Menu
{
    /// <summary>
    ///     Handles the pause menu
    /// </summary>
    public class PauseMenuHandler : MonoBehaviour
    {
        /// <summary> The active game state handler </summary>
        public GameStateHandler gameStateHandler;

        public GameObject menuItemParent;

        /// <summary>
        ///     Called by Unity to initialize the PauseMenuHandler
        /// </summary>
        private void Start()
        {
            menuItemParent.SetActive(false);
        }

        /// <summary>
        ///     Changes whether the menu is opened and the game is paused.
        /// </summary>
        /// <param name="open">Is it supposed to be opened?</param>
        public void SetPauseMenuOpened(bool open)
        {
            menuItemParent.SetActive(open);
            gameStateHandler.Paused = open;
        }
    }
}
