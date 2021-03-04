using System.Collections.Generic;
using NaughtyAttributes;
using UnityEditor;
using UnityEngine;

namespace Game.Inventory.GUI
{
    //[CreateAssetMenu(fileName = "GlobalInventoryControllerGUI", menuName = "Create GlobalInventoryControllerGUI", order = 0)]
    public class GlobalInventoryControllerGUI : ScriptableSingleton<GlobalInventoryControllerGUI>
    {
        private Inventory _playerInventory;
        [SerializeField]public InventorySlotGUI SlotPrefab;
        [SerializeField]private InventoryContainerGUI _containerGUI;
        [SerializeField]private List<ContainerSettings> _playerContainersSettings = new List<ContainerSettings>();
        
        private Dictionary<string,InventoryContainerGUI> _containers = new Dictionary<string, InventoryContainerGUI>(); //containerId is key.
        
        private SlotIdentifier _slotItemBeingDragged;

        private List<string> _playerContainerIds = new List<string>();

        public List<ContainerSettings> PlayerContainersSettings
        {
            get => _playerContainersSettings;
            set => _playerContainersSettings = value;
        }

        public List<string> PlayerContainerIds => _playerContainerIds;

        public void InitPlayerInventory(Inventory inventory)
        {
            _playerInventory = inventory;
            //todo make this work for all the containers.
            _containerGUI.Init(inventory.ContainerSettings[0]);
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
            
            if (_playerInventory.SwapItem(_slotItemBeingDragged, toSlotId, out var responseSlotsInfo))
            {
                //No other containers besides the item being dragged and the target drop should be changed right?
                foreach (var slotInfo in responseSlotsInfo)
                {
                    UpdateItemInfo(slotInfo.SlotId, slotInfo.Item);
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

        private void OnValidate()
        {
            foreach (var containerSetting in _playerContainersSettings)
            {
                containerSetting.OnValidate();
            }
        }

        [Button("SetItems")]
        private void AddItems()
        {
            foreach (var containerSetting in _playerContainersSettings)
            {
                containerSetting.SetItems();
            }
        }
    }
}