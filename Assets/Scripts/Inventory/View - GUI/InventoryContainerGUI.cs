﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Inventory.GUI
{
    public class InventoryContainerGUI : MonoBehaviour
    {
        // [Header("Select what type of Container this will be")]
        // [SerializeField] public GUIContainerSlot _containerSelection;
        
        private RectTransform _rectTransform;
        [SerializeField]private string _containerId;
        private GridLayoutGroup _layoutGroup;
        
        private Vector2 TargetSlotSize = new Vector2(45f,45f);
        private List<InventorySlot_GUI> _inventorySlots = new List<InventorySlot_GUI>();
        private ItemType[] _allowedItemTypes;
        
        public string ContainerId
        {
            get => _containerId;
            set => _containerId = value;
        }

        public ItemType[] AllowedItemTypes => _allowedItemTypes;

        private void Awake()
        {
            _layoutGroup = GetComponent<GridLayoutGroup>();
            _rectTransform = GetComponent<RectTransform>();
        }

        public void Init(ContainerSettings settings, ContainerInfo containerInfo)
        {
            _containerId = settings.Identifier;
            _allowedItemTypes = settings.AllowedItemTypes;
            if (_rectTransform == null)
            {
                Awake();
            }
            //todo make slots using containerInfo Instead, so it accurately matches inventory?
            CreateSlots(settings);
            AddItems(containerInfo);
            GlobalInventoryControllerGUI.instance.RegisterContainer(this);
        }

        private void CreateSlots(ContainerSettings settings)
        {
            List<InventorySlot_GUI> existingInvSlots = GetComponentsInChildren<InventorySlot_GUI>().ToList();
            List<InventoryEquipSlot_GUI> existingInvEquipSlots = new List<InventoryEquipSlot_GUI>();
            for (int i = 0; i < existingInvSlots.Count; i++)
            {
                if (existingInvSlots[i] is InventoryEquipSlot_GUI )
                {
                    existingInvEquipSlots.Add(existingInvSlots[i] as InventoryEquipSlot_GUI);
                    existingInvSlots.RemoveAt(i);
                }
            }
            InventorySlot_GUI[] slots;
            
            RemoveWrongSlotType();

            RemoveExtraSlots();

            var numOfSlotToCreate = settings.NumberOfSlots - slots.Length;

            AddCurrentSlotsToList();
            
            SpawnNeededSlots();

            _layoutGroup.enabled = true; 
            //WARNING GridLayoutGroup Must be disabled in inspector,
            //since it causes all items to have positon/size of 0,0,0 in first frame(for unknown reasons).
            //Which will cause the items to be set to the wrong place when initially
            //created and placed to ItemSlot Locations.
            
            void RemoveWrongSlotType()
            {
                if (settings.Type == ContainerType.Storage)
                {
                    slots = existingInvSlots.ToArray();
                    foreach (var equipSlot in existingInvEquipSlots)
                    {
                        Destroy(equipSlot.gameObject);
                    }
                }
                else
                {
                    slots = existingInvEquipSlots.ToArray();
                    foreach (var invSlots in existingInvSlots)
                    {
                        Destroy(invSlots.gameObject);
                    }
                }
            }

            void RemoveExtraSlots()
            {
                if (slots.Length > settings.NumberOfSlots)
                {
                    for (int i = settings.NumberOfSlots; i < slots.Length; i++)
                    {
                        Destroy(slots[i].gameObject);
                    }
                    Array.Resize(ref slots, settings.NumberOfSlots);
                }
            }

            void AddCurrentSlotsToList()
            {
                for (int i = 0; i < slots.Length; i++)
                {
                    _inventorySlots.Add(slots[i]);
                    slots[i].SlotId = new SlotIdentifier(_containerId, i);
                }
            }

            void SpawnNeededSlots()
            {
                if (numOfSlotToCreate > 0)
                {
                    for (int i = 0; i < numOfSlotToCreate; i++)
                    {
                        var inventorySlot = settings.Type == ContainerType.Storage ? ItemsManager.instance.GetInventorySlot() : ItemsManager.instance.GetEquipSlot();
                        inventorySlot.transform.SetParent(transform);
                        inventorySlot.SlotId = new SlotIdentifier(_containerId, _inventorySlots.Count);
                        _inventorySlots.Add(inventorySlot);
                    }
                }
            }
        }

        private void AddItems( ContainerInfo containerInfo)
        {
            foreach (var slotInfo in containerInfo.InventorySlots)
            {
                if (slotInfo.HasItem)
                {
                    UpdateItemInfo(slotInfo.SlotId, slotInfo.Item);
                }
            }
        }

        public bool IsAllowedItemType(ItemType itemType)
        {
            for (int i = 0; i < _allowedItemTypes.Length; i++)
            {
                if (_allowedItemTypes[i] == itemType)
                {
                    return true;
                }
            }
            return false;
        }
        
        //What were we going to use this for again??
        public static float CalculatePercentChange(float current, float previous)
        {
            return ((current - previous) / previous)*100f;
        }

        /// <summary>
        /// Will Create GUI Items if the slot being updated doesn't have one.
        /// </summary>
        /// <param name="slotId"></param>
        /// <param name="itemInfo"></param>
        public void UpdateItemInfo(SlotIdentifier slotId, ItemInfo itemInfo)
        {
            GetSlot(slotId.SlotIndex).UpdateItemInfoAndLocation(slotId, itemInfo); 
        }

        public void ReturnItemToSlot(int slotIndex)
        {
            GetSlot(slotIndex).ReturnItemToSlot();
        }

        public InventoryItemGUI TakeItem(int slotIndex)
        {
            return GetSlot(slotIndex).TakeItem();
        }

        private InventorySlot_GUI GetSlot(int slotIndex)
        {
            return _inventorySlots[slotIndex];
        }
    }
    
}