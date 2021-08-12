using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Rails.UI
{
    /// <summary>
    /// Adapted from http://answers.unity.com/answers/1820422/view.html
    /// by 'unity_Hc_fAdX4wuuitA'
    /// </summary>
    public class TooltipHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public GameObject TooltipRoot;

        private Transform _parentBeforeShow;

        void Start()
        {
            // ensure hidden at start
            if (TooltipRoot)
            {
                TooltipRoot.SetActive(false);
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            // show
            if (TooltipRoot)
            {
                _parentBeforeShow = TooltipRoot.transform.parent;
                TooltipRoot.transform.SetParent(GameHUDManager.Singleton.transform, true);
                TooltipRoot.SetActive(true);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            // hide
            if (TooltipRoot)
            {
                if (_parentBeforeShow)
                    TooltipRoot.transform.SetParent(_parentBeforeShow);
                TooltipRoot.SetActive(false);
            }
        }
    }
}
