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
            OnItemDroppedEvent?.Invoke(_slotId);
            InventoryControllerGUI.Singleton.ItemDroppedIn(_slotId);

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
            if (_item == null)
            {
                CreateItem();
            }
            Internal_UpdateItemInfoAndLocation( slotId, itemInfo);
        }

        private void CreateItem()
        {
            _item = ItemsManager.Singleton.CreateGUIItem();
            _item.transform.parent = _rectTransform.parent;
            _item.OnBeginDragEvent += InventoryControllerGUI.Singleton.SetAsDragged;
        }

        private void Internal_UpdateItemInfoAndLocation(SlotIdentifier slotId,  ItemInfo itemInfo)
        {
            _item.SetItemInfo(itemInfo);
            _item.PlaceInSlot(_rectTransform, slotId);
        }

        public void StoreItem(InventoryItemGUI item)
        {
            _item = item;
            _item.PlaceInSlot(_rectTransform, _slotId);
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