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
                // save parent before we move it
                _parentBeforeShow = TooltipRoot.transform.parent;

                // move to root of canvas
                // this ensures that the tooltip will render on top of everything else
                TooltipRoot.transform.SetParent(GameHUDManager.Singleton.transform, true);

                // show
                TooltipRoot.SetActive(true);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            // hide
            if (TooltipRoot)
            {
                // return gameobject to normal parent
                if (_parentBeforeShow)
                    TooltipRoot.transform.SetParent(_parentBeforeShow);

                // hide
                TooltipRoot.SetActive(false);
            }
        }
    }
}
