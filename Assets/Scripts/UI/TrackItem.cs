using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Rails.UI
{
    public class TrackItem : MonoBehaviour
    {
        [Header("References")]
        public TMPro.TMP_Text TrackNameText;
        public TMPro.TMP_Text TrackCostText;
        public Button SelectButton;
        public Button DeleteButton;

        public string Name { get => TrackNameText.text; set => TrackNameText.text = value; }
        public string Cost { get => TrackCostText.text; set => TrackCostText.text = value; }
        public event Action<TrackItem> OnTrackSelected;
        public event Action<TrackItem> OnTrackDeleted;

        private void Start()
        {
            // subscribe to button events
            SelectButton.onClick.AddListener(OnSelectClicked);
            DeleteButton.onClick.AddListener(OnDeleteClicked);
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
    }
}
