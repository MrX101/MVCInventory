using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.Inventory.GUI
{
    public class InventorySlot_GUI : MonoBehaviour, IDropHandler
    {
        protected InventoryItemGUI _item;
        protected SlotIdentifier _slotId;
        protected Action<SlotIdentifier> OnItemDroppedEvent;
        protected Action<SlotIdentifier> OnItemRemovedEvent;

        protected RectTransform _rectTransform;

        protected void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
        }
        
        public SlotIdentifier SlotId
        {
            get => _slotId;
            set => _slotId = value;
        }

        private void OnEnable()
        {
            OnItemDroppedEvent += GlobalInventoryControllerGUI.instance.ItemDroppedIn;
        }

        private void OnDisable()
        {
            OnItemDroppedEvent -= GlobalInventoryControllerGUI.instance.ItemDroppedIn;
        }

        public void OnDrop(PointerEventData eventData)
        {
            OnItemDroppedEvent?.Invoke(_slotId);
        }

        /// <summary>
        /// Creates an Item if it currently does not have an item
        /// </summary>
        /// <param name="slotId"></param>
        /// <param name="itemInfo"></param>
        public void UpdateItemInfoAndLocation(SlotIdentifier slotId,  ItemInfo itemInfo)
        {
            if (_rectTransform == null)
            {
                Awake();
            }
            Internal_UpdateItemInfoAndLocation( slotId, itemInfo);
        }

        protected void CreateItem()
        {
            _item = ItemsManager.instance.CreateGUIItem();
            _item.transform.parent = _rectTransform.parent;
            _item.OnBeginDragEvent += GlobalInventoryControllerGUI.instance.SetAsDragged;
        }

        protected void Internal_UpdateItemInfoAndLocation(SlotIdentifier slotId,  ItemInfo itemInfo)
        {
            if (itemInfo == ItemInfo.NULL)
            {
                if (_item != null) { DestroyItem(); }
                _item = null;
                OnItemRemovedEvent?.Invoke(_slotId);
                return;
            }
            if (_item == null) { CreateItem(); }
            _item.SetItemInfo(itemInfo);
            _item.PlaceInSlot(_rectTransform, slotId);
        }

        protected void DestroyItem()
        {
            _item.DestroySelf();
            _item = null;
        }

        public void ReturnItemToSlot()
        {
            _item.ReturnToSlot();
        }
        
        public InventoryItemGUI TakeItem()
        {
            var temp = _item;
            _item = null;
            OnItemRemovedEvent?.Invoke(_slotId);
            return temp;
        }
    }
}