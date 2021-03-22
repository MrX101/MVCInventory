using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.Inventory.GUI
{
    public class InventorySlot_GUI : MonoBehaviour, IDropHandler
    {
        [SerializeField]protected GlobalInventoryControllerGUI _inventoryControllerGUI;
        protected InventoryItemGUI _item;
        protected SlotIdentifier _slotId;
        protected Action<SlotIdentifier> OnItemDroppedEvent;
        protected Action<SlotIdentifier> OnItemRemovedEvent;

        protected RectTransform _rectTransform;
        
        protected Image _backgroundImage;

        protected void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _backgroundImage = GetComponent<Image>();
            
        }

        public SlotIdentifier SlotId
        {
            get => _slotId;
            set => _slotId = value;
        }

        private void OnEnable()
        {
            OnItemDroppedEvent += _inventoryControllerGUI.ItemDroppedIn;
        }

        private void OnDisable()
        {
            OnItemDroppedEvent -= _inventoryControllerGUI.ItemDroppedIn;
        }

        public void OnDrop(PointerEventData eventData)
        {
            if (_slotId == null)
            {
                Debug.Log("Error this slot "+ gameObject.name +" does not have it's slotId set correctly");
            }
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
            _item = _inventoryControllerGUI.GetGUIItem();
            _item.transform.root.SetParent(_rectTransform.parent); 
            _item.OnBeginDragEvent += _inventoryControllerGUI.SetAsDragged;
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