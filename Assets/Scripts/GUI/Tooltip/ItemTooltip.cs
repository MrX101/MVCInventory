using System;
using System.Globalization;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.GUI.Tooltip
{
    public class ItemTooltip : MonoBehaviour
    {
        [SerializeField]private TooltipController _tooltipController;
        [SerializeField]protected TMP_Text _itemName;
        [SerializeField]protected TMP_Text _descriptionText;
        [SerializeField]protected TMP_Text _amountText;
        [SerializeField]protected Image _ItemImage;
        [SerializeField]protected Image _backgroundImage;
        protected RectTransform _backgroundRect;

        private bool _isEnabled;

        public virtual void Awake()
        {
            _tooltipController._itemTooltip = this;
            _backgroundRect = _backgroundImage.GetComponent<RectTransform>();
            DisableToolTip();
        }

        public void UpdateTooltip(string itemName, int amount, string description,Sprite icon)
        {
            _itemName.SetText(itemName);
            _amountText.SetText(amount.ToString());
            _descriptionText.SetText(description);
            _ItemImage.sprite = icon;
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

            var distanceToRight = Screen.width - Input.mousePosition.x;
            if (distanceToRight < _backgroundRect.rect.width)
            {
                localPoint -= new Vector2(_backgroundRect.rect.width - distanceToRight, 0);
            }
            var distanceToUp = Screen.height - Input.mousePosition.y;
            if (distanceToUp > Screen.height - _backgroundRect.rect.height)
            {
                localPoint = new Vector2(localPoint.x, _backgroundRect.rect.height);
            }
            transform.position = localPoint;
        }


        public virtual void EnableToolTip()
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).gameObject.SetActive(true);
            }
            _isEnabled = true;
        }
        
        public virtual void DisableToolTip()
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).gameObject.SetActive(false);
            }
            _isEnabled = false;
        }
        
    }
}