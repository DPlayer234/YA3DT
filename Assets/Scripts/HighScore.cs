//-----------------------------------------------------------------------
// <copyright file="HighScore.cs" company="SAE">
//     Copyright (c) Darius Kinstler, SAE. All rights reserved.
// </copyright>
// <author>Darius Kinstler</author>
//-----------------------------------------------------------------------
namespace SAE.YA3DT
{
    using System.Text.RegularExpressions;

    // Partial class to have HighScore by a part of HighScorehandler
    /// <summary>
    ///     Stores and handles High Scores
    /// </summary>
    public partial class HighScoreHandler
    {
        /// <summary>
        ///     Stores information about a single HighScore
        /// </summary>
        public struct HighScore
        {
            /// <summary>
            ///     The user who obtained the HighScore
            /// </summary>
            public string user;

            /// <summary>
            ///     The HighScore's value
            /// </summary>
            public ulong score;

            /// <summary>
            ///     Initializes a new instance of the <see cref="HighScore"/> struct
            /// </summary>
            /// <param name="user">The user who obtained the HighScore</param>
            /// <param name="score">The HighScore's value</param>
            public HighScore(string user, ulong score)
            {
                this.user = user;
                this.score = score;
            }

            /// <summary>
            ///     Gets a string used to save the highscore to disk.
            /// </summary>
            public string SaveString
            {
                get
                {
                    return user + " " + score.ToString();
                }
            }

            // Equals and GetHashCode only defined to not cause a (Unity) warning
            /// <summary>
            ///     Returns whether this struct equals another object
            /// </summary>
            /// <param name="o">The object to check against</param>
            /// <returns>Whether they are equal</returns>
            public override bool Equals(object o)
            {
                return this.GetType() == o.GetType() && this == (HighScore)o;
            }

            /// <summary>
            ///     Returns a hash code based on this object's data
            /// </summary>
            /// <returns>A hash code</returns>
            public override int GetHashCode()
            {
                return base.GetHashCode();
            }

            /// <summary>
            ///     Comparison method to compare which HighScore is greater
            /// </summary>
            /// <param name="a">The first HighScore</param>
            /// <param name="b">The second HighScore</param>
            /// <returns>0 if they are equal, -1 if a is greater and 1 if b is greater</returns>
            public static int Compare(HighScore a, HighScore b)
            {
                return a.score == b.score ? 0 :
                    (a.score > b.score ? -1 : 1);
            }

            /// <summary>
            ///     Parses a string, as outputed by <seealso cref="SaveString"/> and sets its values.
            ///     Returns whether the operation was successful.
            /// </summary>
            /// <param name="saveString">The string to parse from</param>
            /// <param name="user">The user who obtained the HighScore</param>
            /// <param name="score">The HighScore's value</param>
            /// <returns>Whether the operation was successful</returns>
            public static bool TryParse(string saveString, out string user, out ulong score)
            {
                // Try to match the name and score
                Match userMatch = Regex.Match(saveString, @"^.*\s");
                Match scoreMatch = Regex.Match(saveString, @"\d+$");

                if (userMatch.Success && scoreMatch.Success)
                {
                    // Try to parse the score into a number (ulong) value
                    bool success = ulong.TryParse(scoreMatch.Value, out score);

                    if (success)
                    {
                        user = Regex.Replace(userMatch.Value, @"\s$", string.Empty);
                        return true;
                    }
                }

                // Failed parsing
                score = 0;
                user = string.Empty;
                return false;
            }

            /// <summary>
            ///     Checks whether two HighScores are equal
            /// </summary>
            /// <param name="left">The first operand</param>
            /// <param name="right">The second operand</param>
            /// <returns>Whether both operands are equal</returns>
            public static bool operator ==(HighScore left, HighScore right)
            {
                return left.score == right.score && left.user == right.user;
            }

            /// <summary>
            ///     Checks whether two HighScores aren't equal
            /// </summary>
            /// <param name="left">The first operand</param>
            /// <param name="right">The second operand</param>
            /// <returns>Whether both operands aren't equal</returns>
            public static bool operator !=(HighScore left, HighScore right)
            {
                return left.score != right.score || left.user != right.user;
            }
        }
    }
}
