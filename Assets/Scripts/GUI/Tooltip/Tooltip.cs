using System;
using NaughtyAttributes;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace Game.GUI.Tooltip
{
    public class Tooltip : MonoBehaviour
    {
        [SerializeField]private TooltipController _tooltipController;
        [SerializeField]private TMP_Text _tmpText;
        [SerializeField]protected Image _background;
        protected RectTransform _backgroundTrans;

        [SerializeField]protected int _paddingX = 4;
        [SerializeField]protected int _paddingY = 4;

        protected bool _isEnabled = false;
        
        public virtual void Awake()
        {
            _tooltipController._tooltip = this;
            _backgroundTrans = _background.GetComponent<RectTransform>();
            DisableToolTip();
        }


        public void Update()
        {
            if (!_isEnabled) { return; }
            
            OnUpdate();
        }

        protected virtual void OnUpdate()
        {
            SetPosition();
        }

        protected void SetPosition()
        {
            Vector2 localPoint = Input.mousePosition;

            if (Screen.width - Input.mousePosition.x < _backgroundTrans.rect.width)
            {
                localPoint -= new Vector2(_backgroundTrans.rect.width, 0);
            }

            if (Screen.height - Input.mousePosition.y < _backgroundTrans.rect.height)
            {
                localPoint -= new Vector2(0, _backgroundTrans.rect.height);
            }

            transform.position = localPoint;
        }


        public virtual void EnableToolTip()
        {
            _tmpText.gameObject.SetActive(true);
            _background.gameObject.SetActive(true);
            _isEnabled = true;
        }
        
        public virtual void DisableToolTip()
        {
            _tmpText.gameObject.SetActive(false);
            _background.gameObject.SetActive(false);
            _isEnabled = false;
        }
        
        public void UpdateTooltip(string text)
        {
            _tmpText.SetText(text);
            _background.rectTransform.sizeDelta = CalculateBackgroundSize();
        }

        protected virtual Vector2 CalculateBackgroundSize()
        {
            //Times 2 since needs to be padding on both sides of each axis.
            return new Vector2(_tmpText.preferredWidth + _paddingX * 2, _tmpText.preferredHeight + _paddingY * 2);
        }
    }
}