//-----------------------------------------------------------------------
// <copyright file="SceneHelper.cs" company="SAE">
//     Copyright (c) Darius Kinstler, SAE. All rights reserved.
// </copyright>
// <author>Darius Kinstler</author>
//-----------------------------------------------------------------------
namespace SAE.YA3DT.Menu
{
    using UnityEngine.SceneManagement;

    /// <summary>
    ///     Supplies functions that help handling scenes
    /// </summary>
    public static class SceneHelper
    {
        /// <summary>
        ///     Loads a scene
        /// </summary>
        /// <param name="sceneName">The name of the scene to load</param>
        public static void StartLoadScene(string sceneName)
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
