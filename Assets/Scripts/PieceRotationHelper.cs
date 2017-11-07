//-----------------------------------------------------------------------
// <copyright file="PieceRotationHelper.cs" company="SAE">
//     Copyright (c) Darius Kinstler, SAE. All rights reserved.
// </copyright>
// <author>Darius Kinstler</author>
//-----------------------------------------------------------------------
namespace SAE.YA3DT
{
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    ///     Provides variables and functions to rotate pieces.
    /// </summary>
    public static class PieceRotationHelper
    {
        /// <summary> Assigns any <seealso cref="Direction"/> to a respective <seealso cref="Vector3"/> </summary>
        private static Dictionary<Direction, Vector3> directionToVector;

        /// <summary>
        ///     Initializes static members of the <see cref="PieceRotationHelper"/> class
        /// </summary>
        static PieceRotationHelper()
        {
            directionToVector = new Dictionary<Direction, Vector3>();

            directionToVector[Direction.XBackwards] = new Vector3(90f, 0f, 0f);
            directionToVector[Direction.XForwards] = new Vector3(-90f, 0f, 0f);

            directionToVector[Direction.YLeft] = new Vector3(0f, -90f, 0f);
            directionToVector[Direction.YRight] = new Vector3(0f, 90f, 0f);

            directionToVector[Direction.ZLeft] = new Vector3(0f, 0f, 90f);
            directionToVector[Direction.ZRight] = new Vector3(0f, 0f, -90f);
        }

        /// <summary>
        ///     Supplies possible rotation directions.
        /// </summary>
        public enum Direction : byte
        {
            XBackwards, XForwards,
            YLeft, YRight,
            ZLeft, ZRight
        }

        /// <summary>
        ///     Returns a <seealso cref="Vector3"/> based on the given <seealso cref="Direction"/>.
        ///     Uses the values from <seealso cref="directionToVector"/>.
        /// </summary>
        /// <param name="direction">The direction to get a Vector from</param>
        /// <returns>A Vector</returns>
        public static Vector3 GetVectorByDirection(Direction direction)
        {
            // Theoretically impossible not to get a result
            // (all possible values for Direction are assigned),
            // but let's be safe
            Vector3 result;
            directionToVector.TryGetValue(direction, out result);
            return result;
        }

        /// <summary>
        ///     Rotates a vector around the origin by 90° in the given direction.
        /// </summary>
        /// <param name="input">The original vector</param>
        /// <param name="direction">The direction to rotate it in</param>
        /// <returns>The rotated vector</returns>
        public static Vector3 RotateVector(Vector3 input, Direction direction)
        {
            Vector3 output = Vector3.zero;

            switch (direction)
            {
                case Direction.XBackwards:
                    output.x = input.x;
                    output.y = -input.z;
                    output.z = input.y;
                    break;
                case Direction.XForwards:
                    output.x = input.x;
                    output.y = input.z;
                    output.z = -input.y;
                    break;
                case Direction.YLeft:
                    output.x = -input.z;
                    output.y = input.y;
                    output.z = input.x;
                    break;
                case Direction.YRight:
                    output.x = input.z;
                    output.y = input.y;
                    output.z = -input.x;
                    break;
                case Direction.ZLeft:
                    output.x = -input.y;
                    output.y = input.x;
                    output.z = input.z;
                    break;
                case Direction.ZRight:
                    output.x = input.y;
                    output.y = -input.x;
                    output.z = input.z;
                    break;
            }

            return output;
        }

        /// <summary>
        ///     Rotates all given blocks in the defined direction according to <seealso cref="RotateVector(Vector3, Direction)"/>
        /// </summary>
        /// <param name="blocks">The blocks to rotate</param>
        /// <param name="direction">The direction to rotate them in</param>
        public static void RotateBlocks(Block[] blocks, Direction direction)
        {
            foreach (Block block in blocks)
            {
                block.transform.localPosition = RotateVector(block.transform.localPosition, direction);
            }
        }
    }
}
