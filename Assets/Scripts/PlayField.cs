using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SAE.YA3DT
{
    /// <summary>
    ///     A play field with fixed blocks
    /// </summary>
    public class PlayField : MonoBehaviour
    {
#if UNITY_EDITOR
        /// <summary> How much of the play field to fill at the game start. 0=Nothing, 1=Everything </summary>
        public float fillAmountDEBUG = 0f;

        /// <summary> Which prefab to take from the GameStateHandler for filling the play field. </summary>
        public int fillPrefabIndexDEBUG = 0;
#endif

        /// <summary> Width of the play field (X) </summary>
        public int width = 10;

        /// <summary> Height of the play field (Y) </summary>
        public int height = 20;

        /// <summary> Depth of the play field (Z) </summary>
        public int depth = 10;

        /// <summary> All the blocks already locked in this play field </summary>
        private Block[,,] blocks;

        /// <summary>
        ///     All the active blocks broken after being added to this play field.
        ///     Only blocks in this List are updated via <seealso cref="Block.UpdateWhileBroken"/>.
        ///     They are not added to this List on a game over.
        /// </summary>
        private List<Block> brokenBlocks;

        /// <summary> The active game state handler </summary>
        public GameStateHandler GameStateHandler { get; set; }

        /// <summary>
        ///     Called by Unity to initialize the PlayField
        /// </summary>
        private void Start()
        {
            // Make sure the dimensions are valid
            try
            {
                if (width < 1) throw new Exception("PlayField.width must be greater than 0!");
            }
            catch (Exception exception)
            {
                print(exception);
                width = 1;
            }

            try
            {
                if (height < 1) throw new Exception("PlayField.height must be greater than 0!");
            }
            catch (Exception exception)
            {
                print(exception);
                height = 1;
            }

            try
            {
                if (depth < 1) throw new Exception("PlayField.depth must be greater than 0!");
            }
            catch (Exception exception)
            {
                print(exception);
                depth = 1;
            }

            // Proper setup
            blocks = new Block[width, height, depth];
            brokenBlocks = new List<Block>(width * depth);

            transform.position = Vector3.zero;

            GameStateHandler.StartGame();

#if UNITY_EDITOR
            // Debug code.
            if (fillAmountDEBUG > 0)
            {
                // Fills the play field with random blocks based on the first prefab
                // the GameStateHandler has.
                Piece piecePrefab = null;
                try
                {
                    piecePrefab = GameStateHandler.piecePrefabs[fillPrefabIndexDEBUG];
                }
                catch (Exception e)
                {
                    print(e);
                }

                if (piecePrefab != null)
                {
                    Block blockPrefab = piecePrefab.blockPrefab;
                    Material blockMaterial = piecePrefab.blockMaterial;

                    // Optimally, there are actually prefabs set...
                    for (int x = 0; x < width; x++)
                    {
                        for (int y = 0; y < height; y++)
                        {
                            for (int z = 0; z < depth; z++)
                            {
                                if (UnityEngine.Random.value < fillAmountDEBUG)
                                {
                                    Block block = Instantiate(blockPrefab, this.transform);
                                    block.GetComponent<MeshRenderer>().material = blockMaterial;

                                    block.transform.localPosition = new Vector3(x, y, z);

                                    blocks[x, y, z] = block;
                                }
                            }
                        }
                    } // Loop end
                } // piecePrefab != null
            }
#endif

            // Add the game over event
            GameStateHandler.GameOverEvents.Add(OnGameOver);
        }

        /// <summary>
        ///     Called by Unity to update the PlayField
        /// </summary>
        private void Update()
        {
            Block block;
            for (int i = brokenBlocks.Count - 1; i >= 0; i--)
            {
                block = brokenBlocks[i];
                if (block != null && block.UpdateWhileBroken())
                {
                    brokenBlocks.Remove(block);
                    Destroy(block.gameObject);
                }
            }
        }

        /// <summary>
        ///     Locks a block into the play field
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
                blocks[x, y, z] = block;
            }
            else
            {
                Destroy(block);
                print("Block added out of play area?");
            }
        }

        /// <summary>
        ///     Locks all given blocks into the play field
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
        ///     Checks for and clears any filled planes.
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
                                    blockReplace.BreakMe();
                                    brokenBlocks.Add(blockReplace);
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
                } // if (filled)
            } // Plane loop

            if (planesCleared > 0)
            {
                GameStateHandler.Score += (ulong)(planesCleared * planesCleared * GameStateHandler.scoreOnPlaneClearFactor * GameStateHandler.Difficulty);
                GameStateHandler.Difficulty += planesCleared * GameStateHandler.difficultyIncrementOnPlaneClear;
            }
        }

        /// <summary>
        ///     To be called when the game is over.
        ///     Causes all blocks to go flying wherever.
        /// </summary>
        private void OnGameOver()
        {
            Block block;
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    for (int z = 0; z < depth; z++)
                    {
                        block = blocks[x, y, z];
                        if (block != null)
                        {
                            block.BreakMe();
                            blocks[x, y, z] = null;
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Returns whether there is a block at the given position.
        ///     Also returns true when the position is out-of-bounds.
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
        ///     Converts the position of vector into 3 integer values.
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
        ///     Returns whether the coordinate is in bounds.
        /// A coordinate is in bounds if it is within the walls and higher than the origin.
        /// </summary>
        /// <returns>Whether the coordinate is within bounds</returns>
        private bool IsInBounds(int x, int y, int z)
        {
            return (x >= 0 && x < width && z >= 0 && z < depth && y >= 0);
        }

        /// <summary>
        ///     Returns whether the given coordinates are within the play field.
        ///     A coordinate is within the play area if it is less than <seealso cref="width"/>, <seealso cref="height"/>, and <seealso cref="depth"/>
        ///     but greater than or equal to 0.
        /// </summary>
        /// <returns>Whether the coordinate is within the play area</returns>
        private bool IsInPlayArea(int x, int y, int z)
        {
            return (IsInBounds(x, y, z) && y < height);
        }
    }
}
