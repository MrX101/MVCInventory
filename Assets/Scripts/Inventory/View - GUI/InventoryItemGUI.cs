using System;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;

namespace Game.Inventory.GUI
{
    public class InventoryItemGUI : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerClickHandler
    {
        private SlotIdentifier _slotId;

        private RectTransform _rectTransform;
        private Canvas _canvas;
        private Image _image;
        private TextMeshProUGUI _stackAmount;
         
        
        private bool _isInSlot;
        
        private ItemInfo _item;
        
        private Action<SlotIdentifier> OnBeginDragEvent;
        private Action<SlotIdentifier> OnEndDragEvent;
        private Action<SlotIdentifier> OnClickedEvent;

        public SlotIdentifier SlotId
        {
            get => _slotId;
            set => _slotId = value;
        }

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _canvas = GetComponentInParent<Canvas>();
        }

        public void OnDrag(PointerEventData eventData)
        {
            _rectTransform.anchoredPosition += eventData.delta / _canvas.scaleFactor;   
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            OnBeginDragEvent?.Invoke(_slotId);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            OnEndDragEvent?.Invoke(_slotId);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            OnClickedEvent?.Invoke(_slotId);
        }

        public void UpdateInfo(ItemInfo itemInfo )
        {
            _item = itemInfo;
            UpdateLook();
        }

        public void SetParent(RectTransform rect)
        {
            _rectTransform.parent = rect;
        }

        private void UpdateLook()
        {
            _image.sprite = _item.Sprite;
            _stackAmount.SetText(_item.CurrentStackSize.ToString());
        }
    }
}