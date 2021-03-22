using System;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace Game.Inventory
{
    //This Class is a container that contains inventorySlots, and directly controls what the inventoryslots do.
    //This class does do some decisions on it's own, but the majority should be done by other higher level classes controlling it,
    //which have access to more information about the change requested.
    
    //Currently it does not do many checks, and instead expects the higher up class, to do the checks first,
    //this is purely for efficiency reasons, and may change if it causes issues
    [Serializable]
    public class DataInventoryContainer
    {
        protected List<DataInventorySlot> _inventorySlots = new List<DataInventorySlot>();
        protected ItemType[] _allowedItemTypes;
        protected string _containerId;

        protected Dictionary<string, List<int>> _itemIdIndex = new Dictionary<string, List<int>> (); 
        //key is item uniqueID, stores all slotsIndexes with that itemId.

        public ItemType[] AllowedItemTypes => _allowedItemTypes;
        public string ContainerId => _containerId;
        public Action<int> OnItemStoredChanged; //slotIndex 
        
        // public DataInventoryContainer(int numOfSlots, ItemType[] allowedItemTypes, string containerId)
        // {
        //     this._containerId = containerId;
        //     CreateSlots(numOfSlots);
        //     _allowedItemTypes = allowedItemTypes;
        // }

        
        //todo keep container index updated accordingly.
        public DataInventoryContainer(ContainerSettings containerSettings)
        {
            _containerId = containerSettings.Identifier;
            CreateSlots(containerSettings.NumberOfSlots);
            _allowedItemTypes = containerSettings.AllowedItemTypes;
        }

        public bool GetItemSlots(out DataInventorySlot[] OutSlotsData)
        {
            if (_inventorySlots == null || _inventorySlots.Count == 0)
            {
                OutSlotsData = null;
                return false;
            }
            OutSlotsData = _inventorySlots.ToArray();
            return true;
        }
        
        public bool GetItemSlotsInfo(out SlotInfo[] OutSlotsData)
        {
            if (_inventorySlots == null || _inventorySlots.Count == 0)
            {
                OutSlotsData = null;
                return false;
            }

            SlotInfo[] infos = new SlotInfo[_inventorySlots.Count];
            for (int i = 0; i < _inventorySlots.Count; i++)
            {
                infos[i] = _inventorySlots[i].GetSlotInfo();
            }
            OutSlotsData = infos;
            return true;
        }
        
        public bool GetItemSlotInfo(int slotIndex,out SlotInfo OutslotInfo)
        {
            if (_inventorySlots == null || _inventorySlots.Count == 0)
            {
                OutslotInfo = new SlotInfo(null);
                return false;
            }
            OutslotInfo = _inventorySlots[slotIndex].GetSlotInfo();
            return true;
        }
        
        protected void CreateSlots(int numOfSlots)
        {
            for (int i = 0; i < numOfSlots; i++)
            {
                _inventorySlots.Add(new DataInventorySlot(i));
                _inventorySlots[i].SetContainerIndex(_containerId);
            }
        }

        public void StackItemInSlot(ref IItem itemRef, int slotIndex)
        {
            _inventorySlots[slotIndex].StackItem(ref itemRef);
        }
        
        /// Check that Slot Exists First
        public ItemType GetItemTypeOfItemInSlot(int slotIndex)
        {
            return _inventorySlots[slotIndex].GetItemType();
        }

        ///Check that slot Exists first.
        public bool SlotIsEmpty(int slotIndex)
        {
            return !_inventorySlots[slotIndex].HasItem();
        }
        
        ///Check that slot Exists first.
        public bool HasItem(int slotIndex)
        {
            return _inventorySlots[slotIndex].HasItem();
        }

        
        public bool CanStoreItem(ItemInfo itemInfo, out int slotIndex)
        {
            slotIndex = -1;
            if (!CanStoreItemType(itemInfo.ItemType))
            {
                return false;
            }
            
            for (int i = 0; i < _inventorySlots.Count; i++)
            {
                if (!_inventorySlots[i].HasItem())
                {
                    slotIndex = i;
                    return true;
                }
                else if(itemInfo.UniqueId == _inventorySlots[i].GetUniqueId()
                && (itemInfo.CurrentStackSize + _inventorySlots[i].GetCurrentStackSize()) <= itemInfo.MaxStackSize)
                {
                    //todo implement check, if it can be stored into multiple slots.
                    slotIndex = i;
                    return true;
                }
            }
            return false;
        }
        
        public void AddMoreSlots(int numOfSlots)
        {
            CreateSlots(numOfSlots);
        }
        
        /// Returns false if not enough empty slots.
        public bool RemoveSlots(int numOfSlots)
        {
            if (GetEmptySlotsIndex(numOfSlots, out int[] indexes))
            {
                foreach (var index in indexes)
                {
                    _inventorySlots.RemoveAt(index);
                }
                return true;
            }
            Debug.Log("Not enough Empty Slots to Delete slots");
            return false;
        }

        public void TakeItem(int index, out IItem item)
        {
            item =_inventorySlots[index].TakeItem();
            if (item == null) { return; }
            var count = _itemIdIndex[item.UniqueId].Count;
            _itemIdIndex[item.UniqueId].Remove(index);
            if (_itemIdIndex[item.UniqueId].Count < 1)
            {
                _itemIdIndex.Remove(item.UniqueId);
            }
        }


        public IItem SplitStack(int slotIndex, float percent)
        {
            var item = GetEmptyDuplicate(slotIndex);
            var halfAmount = Mathf.RoundToInt((_inventorySlots[slotIndex].GetCurrentStackSize() * percent));
            _inventorySlots[slotIndex].TakePartial(ref item, halfAmount);
            return item;
        }
        
        public IItem SplitStack(int slotIndex, int amount)
        {
            var item = GetEmptyDuplicate(slotIndex);
            var newItemAmount = Mathf.RoundToInt((_inventorySlots[slotIndex].GetCurrentStackSize() - amount));
            _inventorySlots[slotIndex].TakePartial(ref item, newItemAmount);
            return item;
        }
        
        /// Returns amount left, if there wasn't enough. Checks all slots.
        public int GetSpecificAmountOfItem(string itemId, int amount, out IItem outItem)
        {
            int i = _itemIdIndex[itemId][0]; //todo fix this line one we can more easily duplicate Items, since it will make zero sense to anyone else.
            var tempItem = GetEmptyDuplicate(i);

            foreach (var slotIndex in _itemIdIndex[itemId])
            {
                if (_inventorySlots[slotIndex].GetCurrentStackSize() < amount)
                {
                    amount -= _inventorySlots[slotIndex].GetCurrentStackSize();
                    TakeItem(slotIndex, out var item);
                    StackItems(ref tempItem, ref item);
                }
                else
                {
                    amount = 0;
                    _inventorySlots[slotIndex].TakePartial(ref tempItem, amount);
                }
            }
            outItem = tempItem;
            return amount;
        }

        private IItem GetEmptyDuplicate(int slotIndex)
        {
            IItem tempItem = _inventorySlots[slotIndex].MakeDuplicate();
            tempItem.CurrentStackSize = 0;
            return tempItem;
        }

        public void ConsumeStackAmount(int slotIndex, int amount)
        {
            var id = _inventorySlots[slotIndex].GetUniqueId();
            _inventorySlots[slotIndex].ConsumeStack(slotIndex, amount);
            if (!_inventorySlots[slotIndex].HasItem())
            {
                _itemIdIndex[id].Remove(slotIndex);
            }
        }
        
        public int GetCurrentStackSize(int slotIndex)
        {
            return _inventorySlots[slotIndex].GetCurrentStackSize();
        }
        
        private void StackItems(ref IItem itemToStackTo, ref IItem itemToRemoveStacks)
        {
            itemToStackTo.CurrentStackSize += itemToRemoveStacks.CurrentStackSize;
            itemToRemoveStacks.DeleteSelf();
        }

        ///Store Item Into EmptySlot
        public virtual void StoreItem(ref IItem itemRef, int slotIndex)
        {
            _inventorySlots[slotIndex].StoreItem(ref itemRef);
            AddToIndex(itemRef.UniqueId, slotIndex);
        }

        private void AddToIndex(string itemId, int slotIndex)
        {
            if (!_itemIdIndex.ContainsKey(itemId))
            {
                _itemIdIndex.Add(itemId, new List<int> {slotIndex});
            }
            _itemIdIndex[itemId].Add(slotIndex);
        }
        
        private void RemoveFromIndex(string itemId, int slotIndex)
        {
            if (_itemIdIndex.ContainsKey(itemId))
            {
                _itemIdIndex[itemId].Remove(slotIndex);
                if (_itemIdIndex[itemId].Count < 1)
                {
                    _itemIdIndex.Remove(itemId);
                }
            }
            else
            {
                Debug.Log("Error: ItemId: "+ itemId.ToString() + " Not Found in IndexDictionary");
            }
        }
        
        
        ///Returns False if stack wasn't reduced to zero in existing stacks.
        ///Out ContainerInfo will be null if stack wasn't stored anywhere.
        public bool StackItemInExistingStacks(ref IItem itemref, out List<SlotIdentifier> outInfo)
        {
            outInfo = null;
            if (_itemIdIndex.ContainsKey(itemref.UniqueId))
            {
                List<SlotIdentifier> outList = new List<SlotIdentifier>();
                var indexList = _itemIdIndex[itemref.UniqueId];
                foreach (var i in indexList)
                {
                    if (_inventorySlots[i].HasItem() && _inventorySlots[i].GetRemainingStackSize() > 0)
                    {
                        outList.Add(new SlotIdentifier(_containerId, i));
                        if (_inventorySlots[i].StackItem(ref itemref))
                        {
                            outInfo = outList;
                            return true;
                        }
                    }
                }
                outInfo = outList;
            }
            return false;
        }

        public bool CanStoreItemType(ItemType itemType)
        {
            foreach (var itemtype in AllowedItemTypes)
            {
                if (itemType == itemtype)
                {
                    return true;
                }
            }
            return false;
        }


        public void DeleteItem(int slotIndex)
        {
            var id = _inventorySlots[slotIndex].GetUniqueId();
            _inventorySlots[slotIndex].DeleteItem();
            RemoveFromIndex(id, slotIndex);
        }
        
        public bool StoreInFirstEmptySlot(ref IItem item, out SlotIdentifier info)
        {
            info = null;
            if (FindFirstEmptySlotIndex(out var slotIndex))
            {
                StoreItem(ref item, slotIndex);
                info = new SlotIdentifier(_containerId, slotIndex);
                return true;
            }
            return false;
        }

        public string GetItemIdOfSlot(int slotIndex)
        {
            return _inventorySlots[slotIndex].GetUniqueId();
        }

        /// False if no empty slots
        public bool FindFirstEmptySlotIndex(out int slotIndex)
        {
            for (int i = 0; i < _inventorySlots.Count; i++)
            {
                if (!_inventorySlots[i].HasItem())
                {
                    slotIndex = i;
                    return true;
                }
            }
            slotIndex = -1;
            return false;
        }

        public bool HasEmptySlots()
        {
            bool hasEmpty = false;
            foreach (var invSlot in _inventorySlots)
            {
                if (!invSlot.HasItem())
                {
                    hasEmpty = true;
                    break;
                }
            }
            return hasEmpty;
        }
        
        protected bool GetEmptySlotsIndex(int numOfEmptySlotsNeeded, out int[] indexOfEmptySlots)
        {
            int numOfEmptySlots = 0;
            indexOfEmptySlots = new int[numOfEmptySlotsNeeded];
            for (int i = 0; i < _inventorySlots.Count; i++)
            {
                if (!_inventorySlots[i].HasItem())
                {
                    indexOfEmptySlots[numOfEmptySlots] = i;
                    numOfEmptySlots++;
                    if (numOfEmptySlotsNeeded == numOfEmptySlots) { break; }
                }
            }
            return (numOfEmptySlots == numOfEmptySlotsNeeded);
        }

        protected bool IsIndexValid(int index)
        {
            if (index > -1 && _inventorySlots.Count > index)
            {
                return true;
            }
            return false;
        }

        public bool isValidSlotIndex(int slotIndex)
        {
            if (0 <= slotIndex && _inventorySlots.Count > slotIndex)
            {
                return true;
            }
            return false;
        }

        public bool HasItemsOfUniqueId(string uniqueId)
        {
            return _itemIdIndex.ContainsKey(uniqueId);
        }
    }
    
    public enum ContainerType
    {
        Storage,
        Equiptment,
        WeaponWheel
    }
}