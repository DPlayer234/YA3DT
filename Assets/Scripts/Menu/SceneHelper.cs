using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.SceneManagement;

namespace SAE.YA3DT.Menu
{
    /// <summary>
    ///     Supplies functions that help handling scenes
    /// </summary>
    public static class SceneHelper
    {
        /// <summary>
        ///     Loads a scene
        /// </summary>
        /// <param name="sceneName">The name of the scene to load</param>
        static public void StartLoadScene(string sceneName)
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
    }
}
