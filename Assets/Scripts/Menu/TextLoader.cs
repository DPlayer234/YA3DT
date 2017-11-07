//-----------------------------------------------------------------------
// <copyright file="TextLoader.cs" company="SAE">
//     Copyright (c) Darius Kinstler, SAE. All rights reserved.
// </copyright>
// <author>Darius Kinstler</author>
//-----------------------------------------------------------------------
namespace SAE.YA3DT.Menu
{
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    ///     Loads and assigns text from a file to a Text component
    /// </summary>
    public class TextLoader : MonoBehaviour
    {
        /// <summary>
        ///     The text asset to load the text from
        /// </summary>
        public TextAsset textAsset;

        /// <summary>
        ///     Called by Unity to initialize the TextLoader.
        /// </summary>
        private void Start()
        {
            Text textComponent = GetComponent<Text>();

            textComponent.text = textAsset.text;

            // Not needed anymore, destroy it.
            Destroy(this);
        }
    }
}
