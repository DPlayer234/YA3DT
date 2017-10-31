using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace YA3DT
{
    /// <summary>
    /// Controls the camera
    /// </summary>
    public class CameraController : MonoBehaviour
    {
        /// <summary> The H value for the background color on high difficulties </summary>
        private const float BACKGROUND_COLOR_HIGH_DIFFICULTY_H = 0.0f;

        /// <summary> The H value for the background color on low difficulties </summary>
        private const float BACKGROUND_COLOR_LOW_DIFFICULTY_H = 0.6f;

        /// <summary> [0..1] Lower values make the background color fade to the high-difficulty H value faster. </summary>
        private const float BACKGROUND_COLOR_DIFFICULTY_FADE_RELATION = 0.4f;

        /// <summary> The S value for the background color </summary>
        private const float BACKGROUND_COLOR_S = 0.6f;

        /// <summary> The V value for the background color </summary>
        private const float BACKGROUND_COLOR_V = 0.6f;

        /// <summary> The active game state handler </summary>
        public GameStateHandler gameStateHandler;

        /// <summary> The camera object attached to this GameObject </summary>
        new private Camera camera;

        /// <summary>
        /// Called by Unity to initialize the CameraController
        /// </summary>
        void Start()
        {
            Cursor.lockState = CursorLockMode.None;

            camera = GetComponent<Camera>();
        }

        /// <summary>
        /// Sets the background color based on the current difficulty.
        /// </summary>
        private void SetBackgroundColorBasedOnDifficulty()
        {
            float h = BACKGROUND_COLOR_HIGH_DIFFICULTY_H -
                (BACKGROUND_COLOR_HIGH_DIFFICULTY_H - BACKGROUND_COLOR_LOW_DIFFICULTY_H) *
                Mathf.Pow(
                    BACKGROUND_COLOR_DIFFICULTY_FADE_RELATION,
                    (float)(gameStateHandler.Difficulty - gameStateHandler.initialDifficulty));

            camera.backgroundColor = Color.HSVToRGB(h, BACKGROUND_COLOR_S, BACKGROUND_COLOR_V);
        }

        /// <summary>
        /// Called by Unity every frame to update the CameraController
        /// </summary>
        void Update()
        {
            SetBackgroundColorBasedOnDifficulty();

            CursorLockMode thisLockMode;
            if ((Input.GetMouseButton(0) && !gameStateHandler.GameOver) || Input.GetMouseButton(1))
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
}
