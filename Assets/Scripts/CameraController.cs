//-----------------------------------------------------------------------
// <copyright file="CameraController.cs" company="SAE">
//     Copyright (c) Darius Kinstler, SAE. All rights reserved.
// </copyright>
// <author>Darius Kinstler</author>
//-----------------------------------------------------------------------
namespace SAE.YA3DT
{
    using UnityEngine;

    /// <summary>
    ///     Controls the camera
    /// </summary>
    public class CameraController : MonoBehaviour
    {
        /// <summary> The active game state handler </summary>
        public GameStateHandler gameStateHandler;

        /// <summary> The H value for the background color on high difficulties </summary>
        private const float BackgroundColorHighDifficultyHue = 0.0f;

        /// <summary> The H value for the background color on low difficulties </summary>
        private const float BackgroundColorLowDifficultyHue = 0.6f;

        /// <summary> [0..1] Lower values make the background color fade to the high-difficulty H value faster. </summary>
        private const float BackgroundColorDifficultyFadeRelation = 0.4f;

        /// <summary> The S value for the background color </summary>
        private const float BackgroundColorSaturation = 0.6f;

        /// <summary> The V value for the background color </summary>
        private const float BackgroundColorValue = 0.6f;

        /// <summary> The camera object attached to this GameObject </summary>
        private new Camera camera;

        /// <summary> The difficulty last frame; used to make sure not to change the background color every frame </summary>
        private double lastDifficulty;

        /// <summary>
        ///     Called by Unity to initialize the CameraController
        /// </summary>
        private void Start()
        {
            Cursor.lockState = CursorLockMode.None;

            camera = GetComponent<Camera>();

            lastDifficulty = -1;
        }

        /// <summary>
        ///     Called by Unity every frame to update the CameraController
        /// </summary>
        private void Update()
        {
            SetBackgroundColorBasedOnDifficulty();

            CursorLockMode thisLockMode;
            if (!gameStateHandler.Paused && ((Input.GetMouseButton(0) && !gameStateHandler.GameOver) || Input.GetMouseButton(1)))
            {
                thisLockMode = CursorLockMode.Locked;
            }
            else
            {
                thisLockMode = CursorLockMode.None;
            }

            if (thisLockMode != Cursor.lockState)
            {
                Cursor.lockState = thisLockMode;
            }

            if (!gameStateHandler.Paused)
            {
                // Move camera
                if (Input.GetMouseButton(1))
                {
                    Quaternion rotation = transform.parent.rotation;
                    Vector3 eulerAngles = rotation.eulerAngles;

                    eulerAngles.y += Input.GetAxis("Mouse X");
                    if (eulerAngles.y > 180f) eulerAngles.y -= 360f;
                    if (eulerAngles.y > 45f) eulerAngles.y = 45f;
                    else if (eulerAngles.y < -45f) eulerAngles.y = -45f;

                    eulerAngles.x += Input.GetAxis("Mouse Y");
                    if (eulerAngles.x > 180f) eulerAngles.x -= 360f;
                    if (eulerAngles.x > 45f) eulerAngles.x = 45f;
                    else if (eulerAngles.x < -45f) eulerAngles.x = -45f;

                    rotation.eulerAngles = eulerAngles;
                    transform.parent.rotation = rotation;
                }
            }
        }

        /// <summary>
        ///     Sets the background color based on the current difficulty.
        /// </summary>
        private void SetBackgroundColorBasedOnDifficulty()
        {
            if (lastDifficulty != gameStateHandler.Difficulty)
            {
                lastDifficulty = gameStateHandler.Difficulty;

                float h = BackgroundColorHighDifficultyHue -
                    (BackgroundColorHighDifficultyHue - BackgroundColorLowDifficultyHue) *
                    Mathf.Pow(
                        BackgroundColorDifficultyFadeRelation,
                        (float)(gameStateHandler.Difficulty - GameStateHandler.InitialDifficulty));

                camera.backgroundColor = Color.HSVToRGB(h, BackgroundColorSaturation, BackgroundColorValue);
            }
        }
    }
}
