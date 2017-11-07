//-----------------------------------------------------------------------
// <copyright file="HighScoreHandler.cs" company="SAE">
//     Copyright (c) Darius Kinstler, SAE. All rights reserved.
// </copyright>
// <author>Darius Kinstler</author>
//-----------------------------------------------------------------------
namespace SAE.YA3DT
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text.RegularExpressions;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    ///     Stores and handles High Scores
    /// </summary>
    public partial class HighScoreHandler : MonoBehaviour
    {
        /// <summary> The position of the last high score obtained </summary>
        public static int lastHighScorePosition = -1;

        /// <summary> The active game state handler </summary>
        public GameStateHandler gameStateHandler;

        /// <summary> The input element to enter a name </summary>
        public InputField nameField;

        /// <summary> The button that progresses to the next scene </summary>
        public Button continueButton;

        /// <summary> The element to display all usernames </summary>
        public Text userDisplay;

        /// <summary> The element to display all scores </summary>
        public Text scoreDisplay;

        /// <summary> Colors to be used for the top scores </summary>
        public string[] displayColors;

        /// <summary> Color to use for the newly obtained high score </summary>
        public string newScoreDislayColor;

        /// <summary> Color to use for all scores beyond those </summary>
        public string defaultDisplayColor;

        /// <summary> The amount of scores displayed in-game </summary>
        public int displayedScoreCount = 20;

        /// <summary> Reference to the file storing all default scores </summary>
        public TextAsset defaultScoreAsset;

        /// <summary> The directory in which to save the high scores (starting at %AppData%) </summary>
        public string highScoreSaveDirectory;

        /// <summary> The file in which to save the high scores </summary>
        public string highScoreSaveFile;

        /// <summary> The full path to the directory storing the high scores </summary>
        private string highScoreSaveDirectoryFullPath;

        /// <summary> The full path to the file storing the high scores </summary>
        private string highScoreSaveFileFullPath;

        /// <summary> A list of all high scores in memory </summary>
        public List<HighScore> HighScores { get; private set; }

        /// <summary>
        ///     Called by Unity to initialize the HighScoreHandler
        /// </summary>
        private void Start()
        {
            // Check inspector values
            try
            {
                if (displayedScoreCount < 1) throw new Exception("HighScoreHandler.displayedScoreCount must be greater than 0!");
            }
            catch (Exception exception)
            {
                print(exception);
                displayedScoreCount = 1;
            }

            // Initialize
            HighScores = new List<HighScore>(displayedScoreCount);

            string appdataPath = Environment.GetEnvironmentVariable("appdata");
            if (!string.IsNullOrEmpty(appdataPath))
            {
                highScoreSaveDirectoryFullPath = appdataPath + "\\" + highScoreSaveDirectory;
                highScoreSaveFileFullPath = highScoreSaveDirectoryFullPath + "\\" + highScoreSaveFile;

                LoadHighScores();
            }
            else
            {
                throw new Exception("Could not find AppData directory?!");
            }

            if (gameStateHandler != null)
            {
                // Add Game Over Event
                gameStateHandler.GameOverEvents.Add(OnGameOver);
            }

            TryToDisplayScores();
        }

        /// <summary>
        ///     Loads all HighScores from file into memory
        /// </summary>
        public void LoadHighScores()
        {
            StreamReader reader = null;

            try
            {
                reader = new StreamReader(highScoreSaveFileFullPath);

                string line;
                do
                {
                    line = reader.ReadLine();

                    string user;
                    ulong score;
                    if (HighScore.TryParse(line, out user, out score))
                    {
                        AddScore(user, score, sort: false);
                    }
                }
                while (reader.Peek() != -1);
            }
            catch (DirectoryNotFoundException)
            {
                // Try creating the directory again...
                // Not that that will create any file.
                CreateSaveDirectory();
                LoadDefaultHighScores();
            }
            catch (FileNotFoundException)
            {
                print("No High Score file on disk. No problem.");
                LoadDefaultHighScores();
            }
            catch (Exception e)
            {
                print(e);
            }

            if (reader != null)
            {
                reader.Close();
            }

            SortHighScores();
        }

        /// <summary>
        ///     Loads the default high scores from the set TextAsset
        /// </summary>
        public void LoadDefaultHighScores()
        {
            if (defaultScoreAsset != null)
            {
                // Carriage returns cause issues... Additional LFs won't hurt however.
                string text = defaultScoreAsset.text.Replace('\r', '\n');
                foreach (Match match in Regex.Matches(text, @".*"))
                {
                    string user;
                    ulong score;
                    if (HighScore.TryParse(match.Value, out user, out score))
                    {
                        AddScore(user, score, sort: false);
                    }
                }
            }

            SortHighScores();
        }

        /// <summary>
        ///     Saves the HighScore table to disk.
        /// </summary>
        /// <param name="recursive">Whether this method called itself</param>
        public void SaveHighScores(bool recursive = false)
        {
            StreamWriter writer = null;

            try
            {
                writer = new StreamWriter(highScoreSaveFileFullPath, false);

                foreach (HighScore highScore in HighScores)
                {
                    writer.WriteLine(highScore.SaveString);
                }

                writer.Flush();
            }
            catch (DirectoryNotFoundException e)
            {
                if (!recursive)
                {
                    // Try creating the directory and retry
                    if (CreateSaveDirectory()) SaveHighScores(recursive: true);
                }
                else
                {
                    // Apparently, the cause is out of my control?
                    print(e);
                }
            }
            catch (Exception e)
            {
                print(e);
            }

            if (writer != null)
            {
                writer.Close();
            }
        }

        /// <summary>
        ///     Adds a score to the HighScore table.
        /// </summary>
        /// <param name="user">Who obtained the score</param>
        /// <param name="score">What value does the score have</param>
        /// <param name="newScore">Whether it's a new score</param>
        /// <param name="sort">Whether to immediately sort the HighScore table</param>
        public void AddScore(string user, ulong score, bool newScore = false, bool sort = true)
        {
            HighScore newHighScore = new HighScore(
                user: user,
                score: score);
            HighScores.Add(newHighScore);

            if (sort)
            {
                SortHighScores();
            }

            if (newScore)
            {
                // Determine which position the new score had
                lastHighScorePosition = -1;

                for (int i = 0; i < HighScores.Count; i++)
                {
                    HighScore highScore = HighScores[i];
                    if (highScore == newHighScore)
                    {
                        lastHighScorePosition = i;
                        break;
                    }
                }
            }
        }

        /// <summary>
        ///     Sorts the HighScore table according to <seealso cref="HighScore.Compare(HighScore, HighScore)"/>
        /// </summary>
        public void SortHighScores()
        {
            HighScores.Sort(HighScore.Compare);

            // Remove the entries beyond the display limit
            for (int i = HighScores.Count - 1; i >= displayedScoreCount; i--)
            {
                HighScores.RemoveAt(i);
            }
        }

        /// <summary>
        ///     Creates the save directory.
        /// </summary>
        /// <returns>true if successful, false if it failed or it already exists.</returns>
        private bool CreateSaveDirectory()
        {
            if (!Directory.Exists(highScoreSaveDirectoryFullPath))
            {
                try
                {
                    Directory.CreateDirectory(highScoreSaveDirectoryFullPath);
                    return true;
                }
                catch
                {
                    return false;
                }
            }

            return false;
        }

        /// <summary>
        ///     Writes all scores to <see cref="scoreDisplay"/> and <see cref="userDisplay"/> for displaying, if they are set.
        /// </summary>
        private void TryToDisplayScores()
        {
            if (userDisplay != null && scoreDisplay != null)
            {
                var userBuilder = new System.Text.StringBuilder();
                var scoreBuilder = new System.Text.StringBuilder();
                int position = 0;

                // Append all data
                foreach (HighScore highScore in HighScores)
                {
                    userBuilder.Append("<color=");
                    userBuilder.Append(
                        (position == lastHighScorePosition) ? newScoreDislayColor :
                        (displayColors.Length > position) ? displayColors[position] :
                        defaultDisplayColor);
                    userBuilder.Append(">");
                    userBuilder.Append(highScore.user);
                    userBuilder.Append(" </color>");
                    userBuilder.AppendLine();

                    scoreBuilder.Append(highScore.score);
                    scoreBuilder.AppendLine();

                    position++;
                }

                userDisplay.text = userBuilder.ToString();
                scoreDisplay.text = scoreBuilder.ToString();
            }
        }

        /// <summary>
        ///     To be called on a game over.
        ///     Stores the current score in the high score table, shows it and saves it to disk.
        /// </summary>
        private void OnGameOver()
        {
            continueButton.gameObject.SetActive(true);
            nameField.gameObject.SetActive(true);
        }

        /// <summary>
        ///     To be called when the continue button is pressed.
        /// </summary>
        public void OnContinueClick()
        {
            AddScore(
                !string.IsNullOrEmpty(nameField.text) ? nameField.text : "You",
                gameStateHandler.Score,
                newScore: true);

            SaveHighScores();
        }
    }
}
