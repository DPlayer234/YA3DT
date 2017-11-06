using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SAE.YA3DT
{
    /// <summary>
    ///     Represents a block of a Piece
    /// </summary>
    public class Block : MonoBehaviour
    {
        /// <summary> Defines the range for <seealso cref="GetRandomForce"/> </summary>
        private const float FORCE_VALUE_RANGE = 5000;

        /// <summary> How long it takes until the block is completely removed after being broken </summary>
        public float timeUntilRemoved = 1.0f;

		/// <summary> This block's box collider </summary>
		private BoxCollider boxCollider;

		/// <summary> This block's rigidbody </summary>
		private Rigidbody rigidbody;

		/// <summary>
		/// 	Called by Unity to initialize the Block
		/// </summary>
		private void Start()
		{
            boxCollider = GetComponent<BoxCollider>();
            rigidbody = GetComponent<Rigidbody>();
		}

        /// <summary>
        ///     Called when the Block is supposed to be removed from the PlayField
        /// </summary>
        public void BreakMe()
        {
            boxCollider.isTrigger = false;

            rigidbody.AddForce(GetRandomForce(), GetRandomForce(), GetRandomForce());
            rigidbody.useGravity = true;
        }

        /// <summary>
        ///     Called by the PlayField after being broken.
        ///     Returns whether to remove it.
        /// </summary>
        /// <returns>Whether to remove it.</returns>
        public bool UpdateWhileBroken()
        {
            timeUntilRemoved -= Time.deltaTime;
            return timeUntilRemoved < 0f;
        }

        /// <summary>
        ///     Returns a random value to be used as a force added within <seealso cref="BreakMe"/>
        /// </summary>
        /// <returns>A float in the range of [-<seealso cref="FORCE_VALUE_RANGE"/>, <seealso cref="FORCE_VALUE_RANGE"/>]</returns>
        private float GetRandomForce()
        {
            return (Random.value - 0.5f) * 2 * FORCE_VALUE_RANGE;
        }
    }
}
