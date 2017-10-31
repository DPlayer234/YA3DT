using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YA3DT
{
    /// <summary>
    /// Represents a game Piece
    /// </summary>
    public class Piece : MonoBehaviour
    {
        /// <summary> Multiplier for movement and rotation speed. </summary>
        private const float MOVEMENT_AND_ROTATION_SPEED_FACTOR = 4f;

        /// <summary> Base for movement and rotation speed. </summary>
        /// Must be in the range of 0-1! (0=instant; 1=no movement)
        private const float MOVEMENT_AND_ROTATION_SPEED_BASE = 0.2f;

        /// <summary> Affects how much the mouse must be moved to move a piece </summary>
        public float mouseSensitivity;

        /// <summary> Blocks coordinates </summary>
        public Vector3[] blockPositions;

        /// <summary> Material that all blocks use </summary>
        public Material blockMaterial;

        /// <summary> Prefab for a single block </summary>
        public Block blockPrefab;

        /// <summary> The active play field </summary>
        public PlayField playField;

        /// <summary> The active game state handler </summary>
        public GameStateHandler gameStateHandler;

        /// <summary> Where the object is considered to be </summary>
        private Vector3 realPosition;

        /// <summary> Rotation offset of the object </summary>
        private Vector3 rotationOffset;

        /// <summary> All blocks that this piece is made of </summary>
        private Block[] parts;

        /// <summary> Whether this piece is currently active </summary>
        public bool active;

        /// <summary> Object-based timer for handling step delays </summary>
        private float localTimer;

        /// <summary> Used internally to make sure movement by mouse isn't too rapid </summary>
        private Vector2 mouseDistanceMoved;

        /// <summary> Amount of parts a piece has </summary>
        private int PartCount { get; set; }

        /// <summary> Whether this piece is currently active </summary>
        public bool Active {
            get
            {
                return active;
            }
            set
            {
                if (value)
                {
                    UpdateTransformInstantly();
                }
                active = value;
            }
        }

        /// <summary> Multiplier for movement speed </summary>
        private float MovementDelay { get { return (float)((Input.GetKey(KeyCode.Space) ? 0.1f : 1.0f) / gameStateHandler.Difficulty); } }

        /// <summary>
        /// Called by Unity to initialize the Piece
        /// </summary>
        void Start()
        {
            PartCount = blockPositions.Length;

            realPosition = new Vector3(playField.width / 2, playField.height, playField.depth / 2);
            rotationOffset = Vector3.zero;

            parts = new Block[PartCount];

            float highestPartYCoordinate = 0;
            for (int i = 0; i < PartCount; i++)
            {
                Block block = Instantiate(blockPrefab, transform);
                block.gameObject.GetComponent<MeshRenderer>().material = blockMaterial;
                
                block.transform.localPosition = blockPositions[i];

                if (blockPositions[i].y > highestPartYCoordinate)
                {
                    highestPartYCoordinate = blockPositions[i].y;
                }

                parts[i] = block;
            }

            realPosition.y -= highestPartYCoordinate + 1;

            mouseDistanceMoved = Vector2.zero;

            localTimer = -0.5f;
        }

        /// <summary>
        /// Called by Unity every frame to update the Piece
        /// </summary>
        void Update()
        {
            if (Active)
            {
                float delay = MovementDelay;

                localTimer += Time.deltaTime;
                if (localTimer > delay)
                {
                    // Move down
                    if (MoveAndCollide(new Vector3(0f, -1f, 0f)))
                    {
                        OnLanding();
                    }

                    localTimer %= delay;
                }

                if (Input.GetMouseButton(0))
                {
                    mouseDistanceMoved.x += Input.GetAxis("Mouse X");
                    mouseDistanceMoved.y += Input.GetAxis("Mouse Y");
                }

                // Move
                if (Input.GetKeyDown(KeyCode.W))
                {
                    MoveAndCollide(new Vector3(0f, 0f, 1f));
                }
                else if (Input.GetKeyDown(KeyCode.S))
                {
                    MoveAndCollide(new Vector3(0f, 0f, -1f));
                }

                if (Input.GetKeyDown(KeyCode.D))
                {
                    MoveAndCollide(new Vector3(1f, 0f, 0f));
                }
                else if (Input.GetKeyDown(KeyCode.A))
                {
                    MoveAndCollide(new Vector3(-1f, 0f, 0f));
                }

                // Rotate
                if (Input.GetKeyDown(KeyCode.UpArrow) || mouseDistanceMoved.y > mouseSensitivity)
                { 
                    RotateAndCollide(PieceRotationHelper.Direction.XBackwards);
                    mouseDistanceMoved.y %= mouseSensitivity;
                }
                else if (Input.GetKeyDown(KeyCode.DownArrow) || mouseDistanceMoved.y < -mouseSensitivity)
                {
                    RotateAndCollide(PieceRotationHelper.Direction.XForwards);
                    mouseDistanceMoved.y %= -mouseSensitivity;
                }

                if (Input.GetKeyDown(KeyCode.LeftArrow) || mouseDistanceMoved.x < -mouseSensitivity)
                {
                    RotateAndCollide(PieceRotationHelper.Direction.YLeft);
                    mouseDistanceMoved.x %= mouseSensitivity;
                }
                else if (Input.GetKeyDown(KeyCode.RightArrow) || mouseDistanceMoved.x > mouseSensitivity)
                {
                    RotateAndCollide(PieceRotationHelper.Direction.YRight);
                    mouseDistanceMoved.x %= -mouseSensitivity;
                }

                if (Input.GetKeyDown(KeyCode.Q))
                {
                    RotateAndCollide(PieceRotationHelper.Direction.ZLeft);
                }
                else if (Input.GetKeyDown(KeyCode.E))
                {
                    RotateAndCollide(PieceRotationHelper.Direction.ZRight);
                }

                UpdateTransform();
            }
        }

        /// <summary>
        /// Checks whether the given position overlaps any fixed block.
        /// </summary>
        /// <param name="atPosition">The position to check</param>
        /// <returns>Whether it collides/overlaps anything</returns>
        private bool CheckCollision(Vector3 atPosition)
        {
            return playField.HasBlockAt(atPosition);
        }

        /// <summary>
        /// Check whether any of the given positions overlaps any fixed block.
        /// </summary>
        /// <param name="positions">The positions to check</param>
        /// <returns>Whether it collides/overlaps anything</returns>
        private bool CheckCollision(Vector3[] positions)
        {
            foreach (Vector3 position in positions)
            {
                if (CheckCollision(position))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Checks whether a rotation in that direction is possible and rotates them if it is.
        /// </summary>
        /// <param name="direction">The direction to attempt to rotate all pieces in.</param>
        /// <returns>Whether it has collided with something</returns>
        private bool RotateAndCollide(PieceRotationHelper.Direction direction)
        {
            Vector3[] newPositions = new Vector3[PartCount];

            for (int i = 0; i < PartCount; i++)
            {
                newPositions[i] = realPosition + PieceRotationHelper.RotateVector(parts[i].transform.localPosition, direction);
            }

            if (!CheckCollision(newPositions))
            {
                PieceRotationHelper.RotateBlocks(parts, direction);
                Vector3 rotateBy = -PieceRotationHelper.DirectionToVector[direction];

                if (
                    (rotationOffset.x != 0 && rotateBy.x != 0) ||
                    (rotationOffset.y != 0 && rotateBy.y != 0) ||
                    (rotationOffset.z != 0 && rotateBy.z != 0)
                    )
                {
                    // Rotates in same direction
                    rotationOffset += rotateBy;
                }
                else
                {
                    // Rotates in another direction
                    rotationOffset = rotateBy;
                }

                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Checks whether moving by the given vector is possible and moves if it is.
        /// </summary>
        /// <param name="offset">The vector to add to the current position</param>
        /// <returns>Whether it has collided with something</returns>
        private bool MoveAndCollide(Vector3 offset)
        {
            Vector3[] newPositions = new Vector3[PartCount];

            for (int i = 0; i < PartCount; i++)
            {
                newPositions[i] = realPosition + parts[i].transform.localPosition + offset;
            }

            if (!CheckCollision(newPositions))
            {
                realPosition += offset;
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Checks whether the piece currenly overlaps any collision.
        /// </summary>
        /// <returns>Returns true if it overlaps anything, false otherwise</returns>
        public bool CheckCurrentCollision()
        {
            Vector3[] positions = new Vector3[PartCount];

            for (int i = 0; i < PartCount; i++)
            {
                positions[i] = realPosition + parts[i].transform.localPosition;
            }

            return CheckCollision(positions);
        }

        /// <summary>
        /// Updates the transform's position and rotation smoothly.
        /// </summary>
        private void UpdateTransform()
        {
            float transformSpeed = (MOVEMENT_AND_ROTATION_SPEED_FACTOR / MovementDelay) * Time.deltaTime;

            transform.position = realPosition - (realPosition - transform.position) * Mathf.Pow(MOVEMENT_AND_ROTATION_SPEED_BASE, transformSpeed);
            rotationOffset *= Mathf.Pow(MOVEMENT_AND_ROTATION_SPEED_BASE, transformSpeed);

            transform.rotation = new Quaternion()
            {
                eulerAngles = rotationOffset
            };
        }

        /// <summary>
        /// Instantly updates the transform, skipping any animations.
        /// </summary>
        private void UpdateTransformInstantly()
        {
            transform.position = realPosition;
            transform.rotation = new Quaternion();
        }

        /// <summary>
        /// Called when the object hits the floor.
        /// </summary>
        private void OnLanding()
        {
            UpdateTransformInstantly();
            
            playField.AddBlocks(parts);

            gameStateHandler.Score += (int)(gameStateHandler.scoreOnPiecePlacedFactor * gameStateHandler.Difficulty);

            gameStateHandler.Difficulty += gameStateHandler.difficultyIncrementOnPiecePlaced;

            gameStateHandler.GoToNextPiece();
        }
    }
}
