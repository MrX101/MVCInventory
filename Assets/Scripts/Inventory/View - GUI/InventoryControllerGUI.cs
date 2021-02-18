using System.Collections.Generic;
using UnityEngine;

namespace Game.Inventory.GUI
{
    [CreateAssetMenu(fileName = "InventoryControllerGUI", menuName = "Create InventoryControllerGUI", order = 0)]
    public class InventoryControllerGUI : ScriptableObject
    {
        public static InventoryControllerGUI Singleton;
        private Inventory _inventory;
        [SerializeField]public InventorySlotGUI SlotPrefab;


        private Dictionary<string,InventoryContainerGUI> _containers = new Dictionary<string, InventoryContainerGUI>(); //containerId is key.
        
        private SlotIdentifier _slotItemBeingDragged;

        public void SetInventory(Inventory inventory)
        {
            _inventory = inventory;
        }
        
        private void OnEnable()
        {
            Singleton = this;
        }

        public void SetAsDragged(InventoryItemGUI itemGUI)
        {
            _slotItemBeingDragged = itemGUI.SlotId;
        }

        public void ItemDroppedIn(SlotIdentifier toSlotId)
        {
            if (_slotItemBeingDragged == null)
            {
                return;
            }
            
            if (_inventory.SwapItem(_slotItemBeingDragged, toSlotId))
            {
                //No other containers besides the item being dragged and the target drop should be changed right?
                ContainerInfo containerInfo1 = _inventory.GetContainerInfo(toSlotId.ContainerId);
                MoveItemToSlot(toSlotId, TakeItem(_slotItemBeingDragged));
                UpdateItemInfo(toSlotId, containerInfo1.InventorySlots[toSlotId.SlotIndex].Item);
                if (_slotItemBeingDragged.ContainerId != toSlotId.ContainerId)
                {
                    ContainerInfo containerInfo2 = _inventory.GetContainerInfo(_slotItemBeingDragged.ContainerId);
                    UpdateItemInfo(toSlotId, containerInfo2.InventorySlots[toSlotId.SlotIndex].Item);
                }
            }
            else
            {
                ReturnItemToSlotPosition(_slotItemBeingDragged);
            }
           
        }

        private void UpdateItemInfo(SlotIdentifier slotId, ItemInfo itemInfo)
        {
            GetContainer(slotId).UpdateItemInfo(slotId, itemInfo);
        }

        private InventoryItemGUI TakeItem(SlotIdentifier slotId )
        {
            return GetContainer(slotId).TakeItem(slotId.SlotIndex);
        }

        private void ReturnItemToSlotPosition(SlotIdentifier slotId)
        {
            GetContainer(slotId).ReturnItemToSlot(slotId.SlotIndex);
        }

        private void MoveItemToSlot(SlotIdentifier slotId, InventoryItemGUI item)
        {
            GetContainer(slotId).MoveItemToSlot(slotId.SlotIndex, item);
        }

        private InventoryContainerGUI GetContainer(SlotIdentifier slotId)
        {
            return _containers[slotId.ContainerId];
        }

        public void RegisterContainer(InventoryContainerGUI container)
        {
            if (!_containers.ContainsKey(container.ContainerId))
            {
                _containers.Add(container.ContainerId ,container);
            }
            else
            {
                Debug.Log("This container was already registered " + container.ContainerId);
            }
        }
    }
}