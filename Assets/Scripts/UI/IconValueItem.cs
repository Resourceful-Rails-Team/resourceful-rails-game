using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Rails.UI
{
    public class IconValueItem : MonoBehaviour
    {
        public Image IconImage;
        public TMPro.TMP_Text ValueText;

        public Sprite Sprite { get => IconImage.overrideSprite; set => IconImage.overrideSprite = value; }
        public string Value { get => ValueText.text; set => ValueText.text = value; }
        public bool Disabled { get => IconImage.enabled; set { IconImage.enabled = value; ValueText.color = value ? Color.grey : _startColor; } }

        private Color _startColor;

        public void Start()
        {
            _startColor = ValueText.color;
        }
    }
}
