using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Rails.UI
{
    /// <summary>
    /// UI interop class that links the TrackSelectDeleteItem prefab's components into one managed object.
    /// </summary>
    public class TrackSelectDeleteItem : MonoBehaviour
    {
        [Header("References")]
        public TMPro.TMP_Text NameText;
        public Button SelectButton;
        public Button DeleteButton;
        public bool ShowBoth;

        /// <summary>
        /// Sets the item's name text.
        /// </summary>
        public string Name { get => NameText.text; set => NameText.text = value; }

        /// <summary>
        /// Event raised when the item's select button is clicked.
        /// </summary>
        public event Action<TrackSelectDeleteItem> OnTrackSelected;
        
        /// <summary>
        /// Event raised when the item's delete button is clicked.
        /// </summary>
        public event Action<TrackSelectDeleteItem> OnTrackDeleted;

        /// <summary>
        /// Whether the current item should render the select or delete button.
        /// If ShowBoth is true, this is irrelevent.
        /// </summary>
        public bool IsSelect
        {
            get => _isSelect;
            set
            {
                _isSelect = value;
                UpdateState();
            }
        }
        private bool _isSelect = true;

        /// <summary>
        /// Triggered on start.
        /// </summary>
        private void Start()
        {
            // subscribe to button events
            SelectButton.onClick.AddListener(OnSelectClicked);
            DeleteButton.onClick.AddListener(OnDeleteClicked);
            UpdateState();
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

        /// <summary>
        /// Hides the select or delete button depending on the state.
        /// </summary>
        private void UpdateState()
        {
            if (!ShowBoth)
            {
                SelectButton.gameObject.SetActive(_isSelect);
                DeleteButton.gameObject.SetActive(!_isSelect);
            }
        }
    }
}
