using System.Collections.Generic;
using UnityEngine;

namespace Game.Inventory.GUI
{
    public class InventoryControllerGUI : ScriptableObject
    {
        public static InventoryControllerGUI Singleton;
        private Inventory _inventory;
        [SerializeField]public InventorySlotGUI SlotPrefab;


        private Dictionary<string,InventoryContainerGUI> _containers; //containerId is key.
        
        private SlotIdentifier _slotItemBeingDragged;
        
        private void OnEnable()
        {
            Singleton = this;
        }

        public void ItemDroppedIn(SlotIdentifier slotId)
        {
            if (_slotItemBeingDragged == null)
            {
                //do nothing?
            }
            else
            {
                if (_inventory.SwapItem(_slotItemBeingDragged, slotId))
                {
                    //UpdateContainer();
                }
                else
                {
                    
                }
            }
        }

        private void UpdateContainer(SlotIdentifier slotid, ItemInfo itemInfo)
        {
            _containers[slotid.ContainerId].UpdateSlot(slotid.SlotIndex, itemInfo);
        }
    }
}