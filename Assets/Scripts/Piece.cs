﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YA3DT
{
    /// <summary>
    ///     Represents a game Piece
    /// </summary>
    public class Piece : MonoBehaviour
    {
        /// <summary> Multiplier for movement and rotation speed. </summary>
        private const float MOVEMENT_AND_ROTATION_SPEED_FACTOR = 4f;

        /// <summary> Base for movement and rotation speed. </summary>
        /// Must be in the range of 0-1! (0=instant; 1=no movement)
        private const float MOVEMENT_AND_ROTATION_SPEED_BASE = 0.2f;

        /// <summary> Volume multiplier for sounds played </summary>
		private const float SOUND_VOLUME = 1.0f;

        /// <summary> Blocks coordinates </summary>
        public Vector3[] blockPositions;

        /// <summary> Material that all blocks use </summary>
        public Material blockMaterial;

        /// <summary> Prefab for a single block </summary>
        public Block blockPrefab;

        /// <summary> Sound played when colliding with walls </summary>
        public AudioClip collideSound;

        /// <summary> Sound played when landing on the ground </summary>
        public AudioClip landSound;

        /// <summary> Sound played when rotating </summary>
        public AudioClip rotateSound;

        /// <summary> Where the object is considered to be </summary>
        private Vector3 realPosition;

        /// <summary> Rotation offset of the object </summary>
        private Vector3 rotationOffset;

        /// <summary> All blocks that this piece is made of </summary>
        private Block[] parts;

        /// <summary> This object's audio source </summary>
        private AudioSource audioSource;

        /// <summary> Whether this piece is currently active </summary>
        private bool active;

        /// <summary> Object-based timer for handling step delays </summary>
        private float localTimer;

        /// <summary> Used internally to make sure movement by mouse isn't too rapid </summary>
        private Vector2 mouseDistanceMoved;

        /// <summary> Affects how much the mouse must be moved to move a piece </summary>
        public float MouseSensitivity { get; set; }

        /// <summary> The active play field </summary>
        public PlayField PlayField { get; set; }

        /// <summary> The active game state handler </summary>
        public GameStateHandler GameStateHandler { get; set; }

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
        private float MovementDelay { get { return (float)((Input.GetKey(KeyCode.Space) ? 0.1f : 1.0f) / GameStateHandler.Difficulty); } }

        /// <summary>
        ///     Called by Unity to initialize the Piece
        /// </summary>
        void Start()
        {
            PartCount = blockPositions.Length;

            realPosition = new Vector3(PlayField.width / 2, PlayField.height, PlayField.depth / 2);
            rotationOffset = Vector3.zero;

            parts = new Block[PartCount];

            float highestPartYCoordinate = 0;
            for (int i = 0; i < PartCount; i++)
            {
                Block block = Instantiate(blockPrefab, transform);
                block.GetComponent<MeshRenderer>().material = blockMaterial;

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
        ///     Called by Unity every frame to update the Piece
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
                if (Input.GetKeyDown(KeyCode.UpArrow) || mouseDistanceMoved.y > MouseSensitivity)
                {
                    RotateAndCollide(PieceRotationHelper.Direction.XBackwards);
                    mouseDistanceMoved.y %= MouseSensitivity;
                }
                else if (Input.GetKeyDown(KeyCode.DownArrow) || mouseDistanceMoved.y < -MouseSensitivity)
                {
                    RotateAndCollide(PieceRotationHelper.Direction.XForwards);
                    mouseDistanceMoved.y %= -MouseSensitivity;
                }

                if (Input.GetKeyDown(KeyCode.LeftArrow) || mouseDistanceMoved.x < -MouseSensitivity)
                {
                    RotateAndCollide(PieceRotationHelper.Direction.YLeft);
                    mouseDistanceMoved.x %= MouseSensitivity;
                }
                else if (Input.GetKeyDown(KeyCode.RightArrow) || mouseDistanceMoved.x > MouseSensitivity)
                {
                    RotateAndCollide(PieceRotationHelper.Direction.YRight);
                    mouseDistanceMoved.x %= -MouseSensitivity;
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
        ///     Checks whether the given position overlaps any fixed block.
        /// </summary>
        /// <param name="atPosition">The position to check</param>
        /// <returns>Whether it collides/overlaps anything</returns>
        private bool CheckCollision(Vector3 atPosition)
        {
            return PlayField.HasBlockAt(atPosition);
        }

        /// <summary>
        ///     Check whether any of the given positions overlaps any fixed block.
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
        ///     Sets the clip of the audio source and plays it.
        /// </summary>
        /// <param name="clip">The clip to play</param>
        private void PlaySound(AudioClip clip)
        {
            AudioSource.PlayClipAtPoint(clip, transform.position, SOUND_VOLUME);
        }

        /// <summary>
        ///     Checks whether a rotation in that direction is possible and rotates them if it is.
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

                PlaySound(rotateSound);

                return false;
            }
            else
            {
                PlaySound(collideSound);

                return true;
            }
        }

        /// <summary>
        ///     Checks whether moving by the given vector is possible and moves if it is.
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
                PlaySound(collideSound);

                return true;
            }
        }

        /// <summary>
        ///     Checks whether the piece currenly overlaps any collision.
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
        ///     Updates the transform's position and rotation smoothly.
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
        ///     Instantly updates the transform, skipping any animations.
        /// </summary>
        private void UpdateTransformInstantly()
        {
            transform.position = realPosition;
            transform.rotation = new Quaternion();
        }

        /// <summary>
        ///     Called when the object hits the floor.
        /// </summary>
        private void OnLanding()
        {
            PlaySound(landSound);

            UpdateTransformInstantly();

            PlayField.AddBlocks(parts);

            GameStateHandler.Score += (ulong)(GameStateHandler.scoreOnPiecePlacedFactor * GameStateHandler.Difficulty);

            GameStateHandler.Difficulty += GameStateHandler.difficultyIncrementOnPiecePlaced;

            GameStateHandler.GoToNextPiece();
        }
    }
}
