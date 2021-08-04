using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rails.UI
{
  public class NameValueItem : MonoBehaviour
  {
    public TMPro.TMP_Text NameText;
    public TMPro.TMP_Text ValueText;

    public string Name { get => NameText.text; set => NameText.text = value; }
    public string Value { get => ValueText.text; set => ValueText.text = value; }
  }
}
