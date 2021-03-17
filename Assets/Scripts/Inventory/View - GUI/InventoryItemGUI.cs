using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.Inventory.GUI
{
    public class InventoryItemGUI : MonoBehaviour,IDropHandler ,IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerClickHandler
    {
        private SlotIdentifier _slotId;

        private RectTransform _rectTransform;
        private CanvasScaler _canvasScaler;
        private Canvas _canvas;
        private Image _image;
        private TextMeshProUGUI _stackAmount;

        private ItemInfo _item;

        private bool _isBeingDragged;
        private Vector2 _originalPosition;

        public Action<InventoryItemGUI> OnBeginDragEvent;
        public Action<InventoryItemGUI> OnEndDragEvent;
        public Action<InventoryItemGUI> OnClickedEvent;
        public Action<InventoryItemGUI> OnDropEvent;


        public SlotIdentifier SlotId
        {
            get => _slotId;
            set => _slotId = value;
        }

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _stackAmount = GetComponentInChildren<TextMeshProUGUI>();
            _image = GetComponent<Image>();
            _canvas = GetComponent<Canvas>();
        }

        public void OnDrag(PointerEventData eventData)
        {
            _rectTransform.anchoredPosition += eventData.delta / _canvasScaler.scaleFactor;   
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (_canvasScaler == null)
            {
                _canvasScaler = GetComponentInParent<CanvasScaler>();
            }
            _isBeingDragged = true;
            _image.raycastTarget = false;
            SetToFrontLayer();
            OnBeginDragEvent?.Invoke(this);
        }

        private void SetToFrontLayer()
        {
            _canvas.overrideSorting = true;
            _canvas.sortingOrder = 100;
        }
        
        private void SetToNormalLayer()
        {
            _canvas.overrideSorting = false;
            _canvas.sortingOrder = 0;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (_isBeingDragged)
            {
                _rectTransform.anchoredPosition = _originalPosition;
            }
            _image.raycastTarget = true;
            SetToNormalLayer();
            OnEndDragEvent?.Invoke(this);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            OnClickedEvent?.Invoke(this);
        }

        public void SetItemInfo(ItemInfo itemInfo )
        {
            _item = itemInfo;
            UpdateSpriteAndStackSize();
        }

        public void PlaceInSlot(RectTransform rect, SlotIdentifier slotId)
        {
            _slotId = slotId;
            _isBeingDragged = false;
            if (_rectTransform == null)
            {
                Awake();
            }

            _rectTransform.sizeDelta = rect.sizeDelta;
            _rectTransform.anchorMin = rect.anchorMin;
            _rectTransform.anchorMax = rect.anchorMax;
            _rectTransform.anchoredPosition = rect.anchoredPosition;
            _originalPosition = rect.anchoredPosition;
        }

        public void ReturnToSlot()
        {
            _isBeingDragged = false;
            _rectTransform.anchoredPosition = _originalPosition;
        }

        private void UpdateSpriteAndStackSize()
        {
            _image.sprite = _item.Sprite;
            _stackAmount.SetText(_item.CurrentStackSize.ToString());
        }


        public void DestroySelf()
        {
            Destroy(this.gameObject);
        }
        
        public void OnDrop(PointerEventData eventData)
        {
            OnDropEvent?.Invoke(this);
            GlobalInventoryControllerGUI.Instance.ItemDroppedIn(_slotId);
        }
    }
}