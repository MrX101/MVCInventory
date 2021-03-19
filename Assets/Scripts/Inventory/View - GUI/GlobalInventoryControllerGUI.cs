using System;
using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using UnityEngine;

namespace Game.Inventory.GUI
{
    
    [CreateAssetMenu(fileName = "GlobalInventoryControllerGUI", menuName = "Create GlobalInventoryControllerGUI", order = 0)]
    public class GlobalInventoryControllerGUI : ScriptableObject
    {
        private static GlobalInventoryControllerGUI _instance;

        public static GlobalInventoryControllerGUI Instance
        {
            get
            {
                if (_instance == null)
                {
                    var resourcesFound = Resources.FindObjectsOfTypeAll<GlobalInventoryControllerGUI>();
                    if (resourcesFound.Length == 0)
                    {
                        Debug.Log(nameof(GlobalInventoryControllerGUI)+" Not Found");
                    }
                    else if (resourcesFound.Length > 1)
                    {
                        Debug.Log("Too many "+ nameof(GlobalInventoryControllerGUI)+" Found");
                    }
                    else
                    {
                        _instance = resourcesFound[0];
                    }
                }
                return _instance;
            }
        }

        private Inventory _playerInventory;

        private List<InventoryContainerGUI> _containerGUIs = new List<InventoryContainerGUI>();
        [SerializeField] private List<ContainerSettings> _playerContainersSettings;
        private Dictionary<string,InventoryContainerGUI> _containers = new Dictionary<string, InventoryContainerGUI>(); //containerId is key.
        private SlotIdentifier _slotItemBeingDragged;
        
        [SerializeField]private InventoryItemGUI _inventoryItemPrefab;
        [SerializeField]private GameObject _inventorySlotPrefab;
        [SerializeField]private GameObject _inventoryEquipSlotPrefab;

        public InventoryItemGUI GetGUIItem()
        {
            return Instantiate(_inventoryItemPrefab);
        }

        public InventorySlot_GUI GetInventorySlot()
        {
            return Instantiate(_inventorySlotPrefab).GetComponentInChildren<InventorySlot_GUI>();
        }

        public InventoryEquipSlot_GUI GetEquipSlot()
        {
            return Instantiate(_inventoryEquipSlotPrefab).GetComponentInChildren<InventoryEquipSlot_GUI>();
        }

        public List<ContainerSettings> PlayerContainersSettings
        {
            get => _playerContainersSettings;
            set => _playerContainersSettings = value;
        }

        public void InitPlayerInventory(Inventory inventory)
        {
            _playerInventory = inventory;
            _containerGUIs = FindObjectsOfType<InventoryContainerGUI>().ToList();

            foreach (InventoryContainerGUI inventoryContainerGUI in _containerGUIs)
            {
                foreach (var containerSettings in _playerContainersSettings)
                {
                    if (inventoryContainerGUI.ContainerId == containerSettings.Identifier)
                    {
                        inventoryContainerGUI.Init(containerSettings,
                            _playerInventory.GetContainerInfo(containerSettings.Identifier));
                    }
                }
            }
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
            //_playerInventory.DebugShowAllItems();
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

        public void EquipItemInAnyAvailableSlot(SlotIdentifier slotId)
        {
            if (_playerInventory.EquipItemAnywhere(slotId, out var responseSlotsInfo) )
            {
                foreach (var slotInfo in responseSlotsInfo)
                {
                    UpdateItemInfo(slotInfo.SlotId, slotInfo.Item);
                }
            }
        }
        
        public void EquipItemDroppedIn(SlotIdentifier toSlotId)
        {
            if (_slotItemBeingDragged == null)
            {
                return;
            }
            //_playerInventory.DebugShowAllItems();
            if (_playerInventory.EquipItem(_slotItemBeingDragged, toSlotId, out var responseSlotsInfo))
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
            FixItemDuplicates();
        }

        //This is needed because Unity automatically duplicates the previous array entry in the inspector, when a new one is added.
        //Since the items are shown by reference(ie the [SerializeReference] attribute), it will share the same item between multiple array entries.
        //This will remove the duplicate references.
        public void FixItemDuplicates()
        {
            List<IItem> itemsList = new List<IItem>();
            foreach (var containersSetting in _playerContainersSettings)
            {
                foreach (var itemSettings in containersSetting.ItemsToCreate)
                {
                    var item = itemSettings.Item;
                    if (itemsList.Contains(item))
                    {
                        itemSettings.Item = ItemSettings.CreateItem(itemSettings.ItemClass);
                        itemsList.Add(itemSettings.Item);
                        continue;
                    }
                    itemsList.Add(item);
                }
            }
        }
    }
}