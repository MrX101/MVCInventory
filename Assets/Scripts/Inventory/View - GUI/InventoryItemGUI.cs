﻿using System;
using Game.GUI.Tooltip;
using Game.Timers;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.Inventory.GUI
{
    public class InventoryItemGUI : MonoBehaviour,IDropHandler ,IDragHandler, IBeginDragHandler, IEndDragHandler,IPointerEnterHandler, IPointerExitHandler ,IPointerClickHandler
    {
        [SerializeField]protected GlobalInventoryControllerGUI _inventoryControllerGUI;

        [SerializeField]private TooltipController _tooltipController;
        
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
        public Action<InventoryItemGUI> OnDropEvent;
        
        public Action<InventoryItemGUI> OnClickedEvent;
        public Action<InventoryItemGUI> OnDoubleClickedEvent;
        public Action<InventoryItemGUI> OnRightClickedEvent;
        public Action<ItemInfo> OnMouseOverEnter;
        public Action<ItemInfo> OnMouseOverExit;

        private Cooldown _doubleClickCooldown = new Cooldown(0.3f);
        
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

        private void OnEnable()
        {
            OnDoubleClickedEvent += HandleDoubleOnDoubleClicked;
            OnMouseOverEnter += (item) => _tooltipController.ShowItemTooltip(item.Name,item.CurrentStackSize, item.Description, item.Sprite);
            OnMouseOverExit += (item) => _tooltipController.HideItemTooltip();
            
        }

        private void OnDisable()
        {
            OnDoubleClickedEvent -= HandleDoubleOnDoubleClicked;
            OnMouseOverEnter -= (item) => _tooltipController.ShowItemTooltip(item.Name,item.CurrentStackSize, item.Description, item.Sprite);
            OnMouseOverExit -= (item) => _tooltipController.HideItemTooltip();
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }
            _rectTransform.anchoredPosition += eventData.delta / _canvasScaler.scaleFactor;   
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }
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

        public virtual void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                if (_doubleClickCooldown.HasStarted() && !_doubleClickCooldown.HasFinished(true))
                {
                    OnDoubleClickedEvent?.Invoke(this);
                    //Debug.Log("Double Click");
                }
                else
                {
                    _doubleClickCooldown.StartCountDown();
                    OnClickedEvent?.Invoke(this);
                    //Debug.Log("Left Click");
                }
            }
            else if (eventData.button == PointerEventData.InputButton.Right)
            {
                OnRightClickedEvent?.Invoke(this);
                //Debug.Log("Right Click");
            }
            OnClickedEvent?.Invoke(this);
        }

        public void SetItemInfo(ItemInfo itemInfo )
        {
            _item = itemInfo;
            UpdateSpriteAndStackSize();
        }

        private void HandleDoubleOnDoubleClicked(InventoryItemGUI inventoryItem)
        {
            if (!_item.IsEquipped)
            {
                EquipInFirstAvailable(_slotId);
            }
            else
            {
                UnEquipItem(_slotId);
            }
        }
        
        private void EquipInFirstAvailable(SlotIdentifier slotId)
        {
            _inventoryControllerGUI.EquipItemInAnyAvailableSlot(slotId);
        }
        
        private void UnEquipItem(SlotIdentifier slotId)
        {
            _inventoryControllerGUI.UnEquipItem(slotId);
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
            _inventoryControllerGUI.ItemDroppedIn(_slotId);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            Debug.Log("OnPointerEnter");
            OnMouseOverEnter?.Invoke(_item);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            Debug.Log("OnPointerExit");
            OnMouseOverExit?.Invoke(_item);
        }
    }
}