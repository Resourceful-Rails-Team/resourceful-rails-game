using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Rails.UI
{
  public class NameCheckItem : MonoBehaviour
  {
    public TMPro.TMP_Text NameText;
    public Image CheckImage;

    public string Name { get => NameText.text; set => NameText.text = value; }
    public bool Checked { get => CheckImage.enabled; set => CheckImage.enabled = value; }
  }
}
