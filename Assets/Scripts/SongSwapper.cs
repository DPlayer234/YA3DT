//-----------------------------------------------------------------------
// <copyright file="SongSwapper.cs" company="SAE">
//     Copyright (c) Darius Kinstler, SAE. All rights reserved.
// </copyright>
// <author>Darius Kinstler</author>
//-----------------------------------------------------------------------
namespace SAE.YA3DT
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    ///     Changes the song upon a game over
    /// </summary>
    public class SongSwapper : MonoBehaviour
    {
        /// <summary> The active game state handler </summary>
        public GameStateHandler gameStateHandler;

        /// <summary> The song to swap to </summary>
        public AudioClip song;

        /// <summary>
        ///     Called by Unity to initialize the SongSwapper
        /// </summary>
        private void Start()
        {
            gameStateHandler.GameOverEvents.Add(OnGameOver);
        }

        /// <summary>
        ///     Event to be called upon a game over.
        ///     Changes the current song.
        /// </summary>
        private void OnGameOver()
        {
            AudioSource audioSource = GetComponent<AudioSource>();

            if (audioSource != null)
            {
                audioSource.Stop();
                audioSource.clip = song;
                audioSource.Play();
            }
        }
    }
}