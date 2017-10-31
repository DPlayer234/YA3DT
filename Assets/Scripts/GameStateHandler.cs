using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace YA3DT
{
    /// <summary>
    /// Handles and stores some information about the general game state.
    /// </summary>
    public class GameStateHandler : MonoBehaviour
    {
        /// <summary> The prefab for the play field </summary>
        public PlayField playFieldPrefab;

        /// <summary> An array of prefabs to use to create a new piece </summary>
        public Piece[] piecePrefabs;

        /// <summary> Reference to the UI element displaying the current score </summary>
        public Text scoreDisplay;

        /// <summary> Reference to the UI element displaying the current difficulty </summary>
        public Text difficultyDisplay;

        /// <summary> The button used to return to the main menu </summary>
        public GameObject backButton;

        /// <summary> The starting difficulty </summary>
        public double initialDifficulty = 1.0;

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

        /// <summary> RNG for picking the pieces </summary>
        private System.Random random;

        /// <summary> The current score; use the field <see cref="Score"/> for access </summary>
        private int score;

        /// <summary> The current game difficulty; use the field <see cref="Difficulty"/> for access </summary>
        private double difficulty;

        /// <summary> The current play field </summary>
        public PlayField PlayField { get; private set; }

        /// <summary> The currently active and controlled piece </summary>
        public Piece ActivePiece { get; private set; }

        /// <summary> The next piece that will be dropped </summary>
        public Piece NextPiece { get; private set; }

        /// <summary> Stores whether the game is over </summary>
        public bool GameOver { get; private set; }

        /// <summary>
        /// The current score.
        /// Setting it also updates the attached score display. </summary>
        public int Score
        {
            get { return score; }
            set
            {
                if (!GameOver)
                {
                    scoreDisplay.text = "Score: " + value.ToString("D9");
                }
                score = value;
            }
        }

        /// <summary>
        /// The current game difficulty.
        /// Affects things like the speed at which the active piece falls and how much score is awarded.
        /// Setting it also updates the attached difficulty display.
        /// </summary>
        public double Difficulty
        {
            get { return difficulty; }
            set
            {
                difficultyDisplay.text = "Difficulty: " + value.ToString("F2");
                difficulty = value;
            }
        }

        /// <summary>
        /// Called by Unity to initialize the GameStateHandler
        /// </summary>
        void Start()
        {
            random = new System.Random();

            GameOver = false;
            Score = 0;

            Difficulty = initialDifficulty;

            PlayField = Instantiate(playFieldPrefab);
            PlayField.gameStateHandler = this;

            NextPiece = SetupNewPiece();
        }

        /// <summary>
        /// Called by the PlayField to give the go that everything is setup
        /// </summary>
        public void StartGame()
        {
            GoToNextPiece();
        }

        /// <summary>
        /// Creates a new piece and sets its relevant variables.
        /// It starts out inactive.
        /// </summary>
        /// <returns>Returns the new Piece.</returns>
        private Piece SetupNewPiece()
        {
            int pieceNumber = random.Next(piecePrefabs.Length);

            Piece newPiece = Instantiate(piecePrefabs[pieceNumber]);
            newPiece.Active = false;
            newPiece.mouseSensitivity = mouseSensitivity;
            newPiece.playField = PlayField;
            newPiece.gameStateHandler = this;
            newPiece.transform.parent = this.transform;
            newPiece.transform.localPosition = new Vector3(-10f, 5f, -10f);
            newPiece.transform.localRotation = new Quaternion(0f, 0f, 0f, 0f);

            return newPiece;
        }

        /// <summary>
        /// Activates the given piece.
        /// </summary>
        /// <param name="piece">The piece to activate</param>
        private void ActivatePiece(Piece piece)
        {
            piece.transform.parent = null;
            piece.transform.rotation = new Quaternion(0f, 0f, 0f, 0f);
            piece.Active = true;
        }

        /// <summary>
        /// Progresses to the next piece.
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
        /// Sets and initializes the game over state.
        /// </summary>
        public void SetGameOver()
        {
            GameOver = true;

            scoreDisplay.text = "GAME OVER!!";

            backButton.SetActive(true);
        }
    }
}
