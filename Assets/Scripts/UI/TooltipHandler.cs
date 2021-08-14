using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Rails.UI
{
    /// <summary>
    /// Hides and shows a gameobject when the cursor hovers over the attached UI gameobject.
    /// Adapted from http://answers.unity.com/answers/1820422/view.html
    /// by 'unity_Hc_fAdX4wuuitA'
    /// </summary>
    public class TooltipHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public GameObject TooltipRoot;
        private Transform _parentBeforeShow;

        /// <summary>
        /// Triggered on start.
        /// </summary>
        void Start()
        {
            // ensure hidden at start
            if (TooltipRoot)
            {
                TooltipRoot.SetActive(false);
            }
        }

        /// <summary>
        /// Triggered whenever the cursor enters the UI gameobject's bounds.
        /// </summary>
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

        /// <summary>
        /// Triggered whenever the cursor leaves the UI gameobject's bounds.
        /// </summary>
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
