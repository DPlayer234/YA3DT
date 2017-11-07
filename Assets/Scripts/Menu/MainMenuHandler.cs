//-----------------------------------------------------------------------
// <copyright file="MainMenuHandler.cs" company="SAE">
//     Copyright (c) Darius Kinstler, SAE. All rights reserved.
// </copyright>
// <author>Darius Kinstler</author>
//-----------------------------------------------------------------------
namespace SAE.YA3DT.Menu
{
    using System;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    ///     Handles the main menu
    /// </summary>
    public class MainMenuHandler : MonoBehaviour
    {
        /// <summary> The minimum selectable starting difficulty </summary>
        public double minimumInitialDifficulty = 1.0;

        /// <summary> The maximum selectable starting difficulty </summary>
        public double maximumInitialDifficulty = 10.0;

        /// <summary> The current menu state </summary>
        public MenuState state;

        /// <summary> GameObject that is active when the state is <see cref="MenuState.Main"/> </summary>
        public GameObject mainStateObject;

        /// <summary> GameObject that is active when the state is <see cref="MenuState.DifficultySelection"/> </summary>
        public GameObject difficultySelectionStateObject;

        /// <summary> GameObject that is active when the state is <see cref="MenuState.Credits"/> </summary>
        public GameObject creditsStateObject;

        /// <summary> The difficulty slider object </summary>
        public Slider difficultySlider;

        /// <summary> The text displaying the selected difficulty </summary>
        public Text difficultyDisplay;

        /// <summary> The text to display in front of the difficulty value </summary>
        public string difficultyDisplayText;

        /// <summary>
        ///     Possible states for the menu
        /// </summary>
        public enum MenuState
        {
            /// <summary> Main Menu State (Selection of next state) </summary>
            Main,

            /// <summary> State in which you select the difficulty and can start the game </summary>
            DifficultySelection,
            
            /// <summary> State which displays the Credits </summary>
            Credits
        }

        /// <summary>
        ///     Called by Unity to initialize the MainMenuHandler
        /// </summary>
        private void Start()
        {
            // Make sure the difficulty values are valid
            try
            {
                if (minimumInitialDifficulty <= 0) throw new Exception("MainMenuHandler.minimumInitialDifficulty must be greater than 0!");
            }
            catch (Exception exception)
            {
                print(exception);
                minimumInitialDifficulty = 1;
            }

            try
            {
                if (maximumInitialDifficulty < minimumInitialDifficulty) throw new Exception(
                    "MainMenuHandler.maximumInitialDifficulty must be at least the " +
                    "value of MainMenuHandler.minimumInitialDifficulty!");
            }
            catch (Exception exception)
            {
                print(exception);
                maximumInitialDifficulty = minimumInitialDifficulty;
            }

            // Proper initialization
            ChangeState(state);

            difficultySlider.minValue = (float)minimumInitialDifficulty;
            difficultySlider.maxValue = (float)maximumInitialDifficulty;
            difficultySlider.value = (float)GameStateHandler.InitialDifficulty;

            difficultySlider.onValueChanged.AddListener(OnDifficultySliderChanged);
            OnDifficultySliderChanged(difficultySlider.value);
        }

        /// <summary>
        ///     Called when the difficulty slider is moved
        /// </summary>
        /// <param name="newValue">The new value of the slider</param>
        public void OnDifficultySliderChanged(float newValue)
        {
            GameStateHandler.InitialDifficulty = newValue;
            difficultyDisplay.text = difficultyDisplayText + newValue.ToString("F2");
        }

        /// <summary>
        ///     Changes the current Menu State
        /// </summary>
        /// <param name="toState">To which state to change</param>
        public void ChangeState(MenuState toState)
        {
            mainStateObject.SetActive(false);
            difficultySelectionStateObject.SetActive(false);
            creditsStateObject.SetActive(false);

            switch (toState)
            {
                case MenuState.Main:
                    mainStateObject.SetActive(true);
                    break;
                case MenuState.DifficultySelection:
                    difficultySelectionStateObject.SetActive(true);
                    break;
                case MenuState.Credits:
                    creditsStateObject.SetActive(true);
                    break;
            }

            state = toState;
        }
    }
}
