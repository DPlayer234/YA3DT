using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace YA3DT
{
    /// <summary>
    /// Provides variables and functions to rotate pieces.
    /// </summary>
    static class PieceRotationHelper
    {
        /// <summary>
        /// Suplies possible rotation directions
        /// </summary>
        public enum Direction : byte
        {
            XBackwards, XForwards,
            YLeft, YRight,
            ZLeft, ZRight
        }

        public static readonly Dictionary<Direction, Vector3> DirectionToVector;

        static PieceRotationHelper()
        {
            DirectionToVector = new Dictionary<Direction, Vector3>();

            DirectionToVector[Direction.XBackwards] = new Vector3(90f, 0f, 0f);
            DirectionToVector[Direction.XForwards] = new Vector3(-90f, 0f, 0f);

            DirectionToVector[Direction.YLeft] = new Vector3(0f, -90f, 0f);
            DirectionToVector[Direction.YRight] = new Vector3(0f, 90f, 0f);

            DirectionToVector[Direction.ZLeft] = new Vector3(0f, 0f, 90f);
            DirectionToVector[Direction.ZRight] = new Vector3(0f, 0f, -90f);
        }

        /// <summary>
        /// Rotates a vector around the origin by 90° in the given direction.
        /// </summary>
        /// <param name="input">The original vector</param>
        /// <param name="direction">The direction to rotate it in</param>
        /// <returns>The rotated vector</returns>
        public static Vector3 RotateVector(Vector3 input, Direction direction)
        {
            Vector3 output = Vector3.zero;

            switch (direction) {
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
        /// Rotates all given blocks in the defined direction according to <seealso cref="RotateVector(Vector3, Direction)"/>
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
