using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Rails.UI
{
    /// <summary>
    /// UI interop class that links the TrackItem prefab's components into one managed object.
    /// </summary>
    public class TrackItem : MonoBehaviour
    {
        [Header("References")]
        public TMPro.TMP_Text TrackNameText;
        public TMPro.TMP_Text TrackCostText;
        public Button SelectButton;
        public Button DeleteButton;

        /// <summary>
        /// Sets the item's name text.
        /// </summary>
        public string Name { get => TrackNameText.text; set => TrackNameText.text = value; }

        /// <summary>
        /// Sets the item's cost text rendered under the name.
        /// </summary>
        public string Cost { get => TrackCostText.text; set => TrackCostText.text = value; }

        /// <summary>
        /// Event raised when the select button is clicked.
        /// </summary>
        public event Action<TrackItem> OnTrackSelected;

        /// <summary>
        /// Event raised when the delete button is clicked.
        /// </summary>
        public event Action<TrackItem> OnTrackDeleted;

        /// <summary>
        /// Triggered on start.
        /// </summary>
        private void Start()
        {
            // subscribe to button events
            SelectButton.onClick.AddListener(OnSelectClicked);
            DeleteButton.onClick.AddListener(OnDeleteClicked);
        }

        /// <summary>
        /// Wrapper function that raised OnTrackSelected event when UI button is clicked.
        /// </summary>
        private void OnSelectClicked()
        {
            // pass event downstream
            OnTrackSelected?.Invoke(this);
        }

        /// <summary>
        /// Wrapper function that raised OnTrackDeleted event when UI button is clicked.
        /// </summary>
        private void OnDeleteClicked()
        {
            // pass event downstream
            OnTrackDeleted?.Invoke(this);
        }
    }
}
