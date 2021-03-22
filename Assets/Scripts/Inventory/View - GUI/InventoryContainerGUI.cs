using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Inventory.GUI
{
    public class InventoryContainerGUI : MonoBehaviour
    {
        [SerializeField]protected GlobalInventoryControllerGUI _inventoryControllerGUI;
        
        private RectTransform _rectTransform;
        [SerializeField]private string _containerId;

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
            StartCoroutine(AddItems_CR(containerInfo));
            
            _inventoryControllerGUI.RegisterContainer(this);
        }
        
        //!!WARNING!!
        //This exists because the layoutGroup component does not move items until next frame, after being enabled.
        //Thus delaying the item spawning by 1 frame, ensures the item have the correct spawn position/original position.
        //Also it causes all items to have positon/size of 0,0,0 in first frame(for unknown reasons).
        //If it was enabled in the editor when the game began.
        IEnumerator AddItems_CR( ContainerInfo containerInfo)
        {
            yield return null;
            AddItems(containerInfo);
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
                        var inventorySlot = settings.Type == ContainerType.Storage ? _inventoryControllerGUI.GetInventorySlot() : _inventoryControllerGUI.GetEquipSlot();
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