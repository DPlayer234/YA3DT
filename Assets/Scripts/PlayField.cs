using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YA3DT
{
    /// <summary>
    /// A play field with fixed blocks
    /// </summary>
    public class PlayField : MonoBehaviour
    {
        /// <summary> Width of the play field (X) </summary>
        public int width = 10;
        /// <summary> Depth of the play field (Z) </summary>
        public int depth = 10;
        /// <summary> Height of the play field (Y) </summary>
        public int height = 20;

        /// <summary> All the blocks already locked in this play field </summary>
        private Block[,,] blocks;

        /// <summary> The active game state handler </summary>
        public GameStateHandler gameStateHandler;

        /// <summary>
        /// Called by Unity to initialize the PlayField
        /// </summary>
        void Start()
        {
            blocks = new Block[width, height, depth];

            transform.position = Vector3.zero;

            gameStateHandler.StartGame();
        }

        /// <summary>
        /// Locks a block into the play field
        /// </summary>
        /// <param name="block">The new block to add</param>
        public void AddBlock(Block block)
        {
            Vector3 position = block.transform.position;
            int x, y, z;
            ConvertVectorToInts(position, out x, out y, out z);

            if (IsInPlayArea(x, y, z))
            {
                block.transform.parent = transform;
                block.transform.localPosition = position;
                block.lockedInPlace = true;
                blocks[x, y, z] = block;
            }
            else
            {
                Destroy(block);
                print("Block added out of play area?");
            }
        }

        /// <summary>
        /// Locks all given blocks into the play field
        /// </summary>
        /// <param name="blocks">The new blocks to add</param>
        public void AddBlocks(Block[] blocks)
        {
            foreach (Block block in blocks)
            {
                AddBlock(block);
            }
        }

        /// <summary>
        /// Checks for and clears any filled planes.
        /// </summary>
        public void CheckForAndClearFilledPlanes()
        {
            // How many planes were cleared during this check
            int planesCleared = 0;

            for (int y = height - 1; y >= 0; y--)
            {
                // Check whether plane is cleared
                bool filled = true;
                for (int x = 0; x < width; x++)
                {
                    for (int z = 0; z < depth; z++)
                    {
                        if (blocks[x, y, z] == null)
                        {
                            filled = false;
                            goto notFilled;
                        }
                    }
                }

            notFilled:
                if (filled)
                {
                    planesCleared++;
                    // If it is, move all planes above it one down, deleting this plane as a result
                    for (int yr = y + 1; yr < height; yr++)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            for (int z = 0; z < depth; z++)
                            {
                                Block block = blocks[x, yr, z];
                                Block blockReplace = blocks[x, yr - 1, z];

                                if (blockReplace != null)
                                {
                                    Destroy(blockReplace.gameObject);
                                }

                                if (block != null)
                                {
                                    Vector3 localPosition = block.transform.localPosition;
                                    localPosition.y -= 1;
                                    block.transform.localPosition = localPosition;

                                    blocks[x, yr - 1, z] = block;
                                }
                                else
                                {
                                    blocks[x, yr - 1, z] = null;
                                }

                                blocks[x, yr, z] = null;
                            }
                        }
                    }
                }
            }

            if (planesCleared > 0)
            {
                gameStateHandler.Score += (int)(planesCleared * planesCleared * gameStateHandler.scoreOnPlaneClearFactor * gameStateHandler.Difficulty);
                gameStateHandler.Difficulty += planesCleared * gameStateHandler.difficultyIncrementOnPlaneClear;
            }
        }

        /// <summary>
        /// Returns whether there is a block at the given position.
        /// Also returns true when the position is out-of-bounds.
        /// </summary>
        /// <param name="position">The position to check against.</param>
        /// <returns>Whether the position is considered to have a block</returns>
        public bool HasBlockAt(Vector3 position)
        {
            int x, y, z;
            ConvertVectorToInts(position, out x, out y, out z);
            
            if (IsInPlayArea(x, y, z))
            {
                return blocks[x, y, z] != null;
            }
            else if (IsInBounds(x, y, z))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Converts the position of vector into 3 integer values.
        /// </summary>
        /// <param name="vector">The original Vector</param>
        /// <param name="x">Its X component as an Int</param>
        /// <param name="y">Its Y component as an Int</param>
        /// <param name="z">Its Z component as an Int</param>
        private void ConvertVectorToInts(Vector3 vector, out int x, out int y, out int z)
        {
            x = Mathf.RoundToInt(vector.x);
            y = Mathf.RoundToInt(vector.y);
            z = Mathf.RoundToInt(vector.z);
        }

        /// <summary>
        /// Returns whether the coordinate is in bounds.
        /// A coordinate is in bounds if it is within the walls and higher than the origin.
        /// </summary>
        private bool IsInBounds(int x, int y, int z)
        {
            return (x >= 0 && x < width && z >= 0 && z < depth && y >= 0);
        }

        /// <summary>
        /// Returns whether the given coordinates are within the play field.
        /// A coordinate is within the play area if it is less than <seealso cref="width"/>, <seealso cref="height"/>, and <seealso cref="depth"/>
        /// but greater than or equal to 0.
        /// </summary>
        private bool IsInPlayArea(int x, int y, int z)
        {
            return (IsInBounds(x, y, z) && y < height);
        }
    }
}
