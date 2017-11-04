using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace SAE.YA3DT.Menu
{
    /// <summary>
    ///     Handles the main menu
    /// </summary>
    public class MainMenuHandler : MonoBehaviour
    {
        /// <summary>
        ///     Possible states for the menu
        /// </summary>
        public enum MenuState
        {
            Main,
            DifficultySelection
        }

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

        public Slider difficultySlider;

        public Text difficultyDisplay;

        /// <summary> The text to display before the difficulty value </summary>
        public string difficultyDisplayText;

        /// <summary>
        ///     Called by Unity to initialize the MainMenuHandler
        /// </summary>
        private void Start()
        {
            ChangeState(state);

            difficultySlider.minValue = (float)minimumInitialDifficulty;
            difficultySlider.maxValue = (float)maximumInitialDifficulty;
            difficultySlider.value = (float)GameStateHandler.initialDifficulty;

            difficultySlider.onValueChanged.AddListener(OnDifficultySliderChanged);
            OnDifficultySliderChanged(difficultySlider.value);
        }

        /// <summary>
        ///     Called when the difficulty slider is moved
        /// </summary>
        /// <param name="newValue">The new value of the slider</param>
        public void OnDifficultySliderChanged(float newValue)
        {
            GameStateHandler.initialDifficulty = newValue;
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

            switch (toState)
            {
                case MenuState.Main:
                    mainStateObject.SetActive(true);
                    break;
                case MenuState.DifficultySelection:
                    difficultySelectionStateObject.SetActive(true);
                    break;
            }

            state = toState;
        }
    }
}
