using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Rails.UI
{
    public class TrackSelectDeleteItem : MonoBehaviour
    {
        [Header("References")]
        public TMPro.TMP_Text NameText;
        public Button SelectButton;
        public Button DeleteButton;
        public bool ShowBoth;

        public string Name { get => NameText.text; set => NameText.text = value; }
        public event Action<TrackSelectDeleteItem> OnTrackSelected;
        public event Action<TrackSelectDeleteItem> OnTrackDeleted;
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

        private void Start()
        {
            // subscribe to button events
            SelectButton.onClick.AddListener(OnSelectClicked);
            DeleteButton.onClick.AddListener(OnDeleteClicked);
            UpdateState();
        }

        private void OnSelectClicked()
        {
            // pass event downstream
            OnTrackSelected?.Invoke(this);
        }

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
