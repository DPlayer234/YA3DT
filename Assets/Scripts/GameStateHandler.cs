//-----------------------------------------------------------------------
// <copyright file="GameStateHandler.cs" company="SAE">
//     Copyright (c) Darius Kinstler, SAE. All rights reserved.
// </copyright>
// <author>Darius Kinstler</author>
//-----------------------------------------------------------------------
namespace SAE.YA3DT
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    ///     An Event to be called on a Game Over
    /// </summary>
    public delegate void GameOverEvent();

    /// <summary>
    ///     Handles and stores some information about the general game state.
    /// </summary>
    public class GameStateHandler : MonoBehaviour
    {
        /// <summary> The current pause menu handler </summary>
        public Menu.PauseMenuHandler menuHandler;

        /// <summary> An array of prefabs to use to create a new piece </summary>
        public Piece[] piecePrefabs;

        /// <summary> The prefab for the game over text object </summary>
        public GameObject gameOverTextPrefab;

        /// <summary> The position at which the game over text object should spawn </summary>
        public Vector3 gameOverTextPosition;

        /// <summary> Reference to the play field </summary>
        public PlayField playFieldPrefab;

        /// <summary> Reference to the UI element displaying the current score </summary>
        public Text scoreDisplay;

        /// <summary> The text to display before the score value </summary>
        public string scoreDisplayText;

        /// <summary> Reference to the UI element displaying the current difficulty </summary>
        public Text difficultyDisplay;

        /// <summary> The text to display before the difficulty value </summary>
        public string difficultyDisplayText;

        /// <summary> The position of the next piece in relation to this. </summary>
        public Vector3 nextPieceLocalPosition;

        /// <summary> The rotation of the next piece in relation to this. </summary>
        public Quaternion nextPieceLocalRotation;

        /// <summary> How much the difficulty increases for every plane that is cleared </summary>
        public double difficultyIncrementOnPlaneClear = 0.25;

        /// <summary> How much the difficulty increases for every piece that is set down </summary>
        public double difficultyIncrementOnPiecePlaced = 0.01;

        /// <summary> The multiplier for the score awarded when placing a piece down </summary>
        public double scoreOnPlaneClearFactor = 2000.0;

        /// <summary> The multiplier for the score awarded when placing a piece down </summary>
        public double scoreOnPiecePlacedFactor = 100.0;

        /// <summary> Affects how much the mouse must be moved to move a piece </summary>
        public float mouseSensitivity = 1.5f;

        /// <summary> The starting difficulty </summary>
        private static double initialDifficulty = 3.0;

        /// <summary> RNG for picking the pieces </summary>
        private System.Random random;

        /// <summary> The current score; use the property <see cref="Score"/> for access </summary>
        private ulong score;

        /// <summary> The current game difficulty; use the property <see cref="Difficulty"/> for access </summary>
        private double difficulty;

        /// <summary> Whether the game is currently paused; use the property <see cref="Paused"/> for access </summary>
        private bool paused;

        /// <summary>
        ///     The starting difficulty
        /// </summary>
        public static double InitialDifficulty
        {
            get { return initialDifficulty; }
            set { initialDifficulty = value; }
        }

        /// <summary> The currently active and controlled piece </summary>
        public Piece ActivePiece { get; private set; }

        /// <summary> The next piece that will be dropped </summary>
        public Piece NextPiece { get; private set; }

        /// <summary> The current play field </summary>
        public PlayField PlayField { get; private set; }

        /// <summary> Stores whether the game is over </summary>
        public bool GameOver { get; private set; }

        /// <summary>
        ///     A list of all events to run on a game over.
        /// </summary>
        public List<GameOverEvent> GameOverEvents { get; private set; }

        /// <summary>
        ///     The current score.
        ///     Setting it also updates the attached score display.
        /// </summary>
        public ulong Score
        {
            get
            {
                return score;
            }

            set
            {
                if (!GameOver)
                {
                    scoreDisplay.text = scoreDisplayText + value.ToString("D9");
                }

                score = value;
            }
        }

        /// <summary>
        ///     The current game difficulty.
        ///     Affects things like the speed at which the active piece falls and how much score is awarded.
        ///     Setting it also updates the attached difficulty display.
        ///     It cannot be set lower than the initial difficulty.
        /// </summary>
        public double Difficulty
        {
            get
            {
                return difficulty;
            }

            set
            {
                if (value < InitialDifficulty)
                {
                    difficulty = InitialDifficulty;
                }
                else
                {
                    difficulty = value;
                }

                difficultyDisplay.text = difficultyDisplayText + difficulty.ToString("F2");
            }
        }

        /// <summary>
        ///     Whether the game is currently paused.
        ///     Also updates the time scale.
        /// </summary>
        public bool Paused
        {
            get
            {
                return paused;
            }

            set
            {
                if (value)
                {
                    Time.timeScale = 0.05f;
                }
                else
                {
                    Time.timeScale = 1f;
                }

                paused = value;
            }
        }

        /// <summary>
        ///     Called by Unity to initialize the GameStateHandler
        /// </summary>
        private void Start()
        {
            // Make sure the starting difficulty is valid
            try
            {
                if (InitialDifficulty <= 0) throw new Exception("GameStateHandler.initialDifficulty must be greater than 0!");
            }
            catch (Exception exception)
            {
                print(exception);
                InitialDifficulty = 1;
            }

            // Proper setup
            random = new System.Random();

            GameOver = false;
            Score = 0;

            GameOverEvents = new List<GameOverEvent>();
            GameOverEvents.Add(OnGameOver);

            Difficulty = InitialDifficulty;

            PlayField = Instantiate(playFieldPrefab);
            PlayField.GameStateHandler = this;

            NextPiece = SetupNewPiece();

            Paused = false;
        }

        /// <summary>
        ///     Called by Unity every frame to update the GameStateHandler
        /// </summary>
        private void Update()
        {
            if (!GameOver)
            {
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    Paused = !Paused;
                    menuHandler.SetPauseMenuOpened(Paused);
                }
            }
            else if (Paused)
            {
                Paused = false;
                menuHandler.SetPauseMenuOpened(false);
            }
        }

        /// <summary>
        ///     Called by the PlayField to give the go that everything is setup
        /// </summary>
        public void StartGame()
        {
            GoToNextPiece();
        }

        /// <summary>
        ///     Progresses to the next piece.
        /// </summary>
        public void GoToNextPiece()
        {
            PlayField.CheckForAndClearFilledPlanes();

            if (ActivePiece != null)
            {
                Destroy(ActivePiece.gameObject);
            }

            ActivePiece = NextPiece;
            ActivatePiece(ActivePiece);

            if (ActivePiece.CheckCurrentCollision())
            {
                // Piece already overlaps something upon spawning
                SetGameOver();

                ActivePiece.Active = false;
                NextPiece = null;
            }
            else
            {
                // Progress normally
                NextPiece = SetupNewPiece();
            }
        }

        /// <summary>
        ///     Sets and initializes the game over state.
        /// </summary>
        public void SetGameOver()
        {
            GameOver = true;

            foreach (GameOverEvent gameOverEvent in GameOverEvents)
            {
                gameOverEvent();
            }
        }

        /// <summary>
        ///     Creates a new piece and sets its relevant variables.
        ///     It starts out inactive.
        /// </summary>
        /// <returns>Returns the new Piece.</returns>
        private Piece SetupNewPiece()
        {
            int pieceNumber = random.Next(piecePrefabs.Length);

            Piece newPiece = Instantiate(piecePrefabs[pieceNumber]);
            newPiece.Active = false;
            newPiece.MouseSensitivity = mouseSensitivity;
            newPiece.PlayField = PlayField;
            newPiece.GameStateHandler = this;

            newPiece.transform.parent = this.transform;
            newPiece.transform.localPosition = nextPieceLocalPosition;
            newPiece.transform.localRotation = nextPieceLocalRotation;

            return newPiece;
        }

        /// <summary>
        ///     Activates the given piece.
        /// </summary>
        /// <param name="piece">The piece to activate</param>
        private void ActivatePiece(Piece piece)
        {
            piece.transform.parent = null;
            piece.transform.rotation = new Quaternion(0f, 0f, 0f, 0f);
            piece.Active = true;
        }

        /// <summary>
        ///     To be called on a game over.
        ///     Sets misc. things, like the display and unhides the exit button.
        /// </summary>
        private void OnGameOver()
        {
            // Game Over Text Object falling from the sky...
            Instantiate(gameOverTextPrefab).transform.position = gameOverTextPosition;
        }
    }
}
