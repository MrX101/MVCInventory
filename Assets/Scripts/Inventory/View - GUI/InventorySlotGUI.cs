using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.Inventory.GUI
{
    public class InventorySlotGUI : MonoBehaviour, IDropHandler
    {
        private InventoryItemGUI _item;
        private SlotIdentifier _slotId;
        private Action<SlotIdentifier> OnItemDroppedEvent;
        private Action<SlotIdentifier> OnItemRemovedEvent;

        private RectTransform _rectTransform;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
        }
        
        public SlotIdentifier SlotId
        {
            get => _slotId;
            set => _slotId = value;
        }

        public void OnDrop(PointerEventData eventData)
        {
            Debug.Log("Item Dropped");
            OnItemDroppedEvent?.Invoke(_slotId);
            GlobalInventoryControllerGUI.instance.ItemDroppedIn(_slotId);
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

        private void CreateItem()
        {
            Debug.Log("Create Item");
            _item = ItemsManager.instance.CreateGUIItem();
            _item.transform.parent = _rectTransform.parent;
            _item.OnBeginDragEvent += GlobalInventoryControllerGUI.instance.SetAsDragged;
        }

        private void Internal_UpdateItemInfoAndLocation(SlotIdentifier slotId,  ItemInfo itemInfo)
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

        private void DestroyItem()
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