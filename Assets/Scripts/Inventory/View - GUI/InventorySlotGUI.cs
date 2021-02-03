using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.Inventory.GUI
{
    public class InventorySlotGUI : MonoBehaviour, IDropHandler
    {
        private InventoryItemGUI _item;
        private SlotIdentifier _slotId;
        private Action<SlotIdentifier> OnEndDragEvent;
        
        
        public SlotIdentifier SlotId
        {
            get => _slotId;
            set => _slotId = value;
        }

        public void OnDrop(PointerEventData eventData)
        {
            InventoryControllerGUI.Singleton.ItemDroppedIn(_slotId);
        }

        public void UpdateItem(ItemInfo itemInfo)
        {
            _item.UpdateInfo(itemInfo);
        }
    }
}