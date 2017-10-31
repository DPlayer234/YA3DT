using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Menu
{
    public class Buttons : MonoBehaviour
    {
        /// <summary>
        /// Loads a scene
        /// </summary>
        /// <param name="sceneName">The name of the scene to load</param>
        private void StartLoadScene(string sceneName)
        {
            Scene scene = SceneManager.GetSceneByName(sceneName);

            if (!scene.isLoaded)
            {
                SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
            }
            else
            {
                SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());
                SceneManager.SetActiveScene(scene);
            }
        }

        /// <summary>
        /// Loads the main scene and starts the game
        /// </summary>
        public void PlayGame()
        {
            StartLoadScene("Main");
        }

        /// <summary>
        /// Exits the game
        /// </summary>
        public void ExitGame()
        {
            Application.Quit();
        }

        /// <summary>
        /// Goes back to the menu
        /// </summary>
        public void GoToMenu()
        {
            StartLoadScene("Menu");
        }
    }
}
