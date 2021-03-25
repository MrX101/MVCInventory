using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Inventory
{
    [Serializable]
    public class Inventory 
    {
        private List<ContainerSettings> containerSettings = new List<ContainerSettings>();
        private Dictionary<string, DataInventoryContainer> _containers = new Dictionary<string, DataInventoryContainer>();
        private ItemsManager _itemsManager;
        
        public List<ContainerSettings> ContainerSettings
        {
            get => containerSettings;
            set => containerSettings = value;
        }

        public ItemsManager ItemManager
        {
            get => _itemsManager;
            set => _itemsManager = value;
        }

        public void DebugShowAllItems()
        {
            Debug.Log("Start Of Inventory Log");
            foreach (var containerInfo in GetAllContainersInfo())
            {
                Debug.Log("Container: "+ containerInfo.Id + " has "+containerInfo.InventorySlots.Length + " slots");
                foreach (SlotInfo slot in containerInfo.InventorySlots)
                {
                    if (slot.HasItem)
                    {
                        Debug.Log( "Slot Index: " + slot.SlotId.SlotIndex + " has Item: " +
                                   "Name: " + slot.Item.Name +" Id: "+slot.Item.UniqueId);
                    }
                }
            }
            Debug.Log("End Of Inventory Log");
        }

        public void Initialize()
        {
            //todo add items
            foreach (var settings in ContainerSettings)
            {
                if (settings.CreateContainerInstance(out var container))
                {
                    _containers.Add(settings.Identifier, container);
                }

                foreach (ItemSettings itemToCreate in settings.ItemsToCreate)
                {
                    if (!itemToCreate.IsValid())
                    {
                        Debug.Log(itemToCreate.Item.Name + "'s Settings in Container "+ settings.Identifier+" are not valid.");
                    }
                    
                    if (!itemToCreate.RollSpawnChance()) { continue; }
                    var itemsToCreate = ItemSettings.RollAmountToCreate(itemToCreate);

                    for (int i = 0; i < itemsToCreate.Count; i++)
                    {
                        var item = itemsToCreate[i];
                        if (!StoreItem(ref item, settings.Identifier ,out var slotIds, false))
                        {
                            Debug.Log("Unable to add item '" + itemToCreate.Item.Name + 
                                      "' in container: " + settings.Identifier);
                        }
                    }
                    
                }
            }
        }

        //Returns the amount left from the amount.
        public int ConsumeAmmo(string ammoId, int amount, string containerId = "Any")
        {
            if (containerId != "Any" && _containers.ContainsKey(containerId))
            {
                amount = ConsumeAmmoInternal(ammoId, amount, _containers[containerId]);
            }
            else
            {
                foreach (var KVP in _containers)
                {
                    amount = ConsumeAmmoInternal(ammoId, amount, KVP.Value);
                }
            }
            return amount;
        }

        private int ConsumeAmmoInternal(string ammoId, int amount, DataInventoryContainer container)
        {
            if (!container.CanStoreItemType(ItemType.Ammo))
            {
                return amount;
            }
            if (container.HasItemsOfUniqueId(ammoId))
            {
                amount = container.GetSpecificAmountOfItem(ammoId, amount, out var item);
                item.DeleteSelf();
            }
            return amount;
        }

        public bool UseConsumable(SlotIdentifier request,int amount = 1)
        {
            if (IsRequestValid(request) && !IsRequestSlotEmpty(request) 
                 && GetRequestItemType(request) == ItemType.Consumable && GetRequestStackSize(request) >= amount )
            {
                _containers[request.ContainerId].ConsumeStackAmount(request.SlotIndex, amount);
                return true;
            }    
            return false;
        }

        public bool DropItem(SlotIdentifier request, Vector3 position, Quaternion rotation)
        {
            if (TakeItem(out var item, request))
            {
                _itemsManager.CreateWorldItem(item, position, rotation);
                return true;
            }
            return false;
        }

        public ContainerInfo GetContainerInfo(string containerId)
        {
            return new ContainerInfo(_containers[containerId]);
        }

        public bool SetSelectedItem(SlotIdentifier request)
        {
            if (IsRequestValid(request) && !IsRequestSlotEmpty(request))
            {
                if (_containers[request.ContainerId] is DataWeaponWheelContainer container)
                {
                    container.SetSelectedWeapon(request.SlotIndex);
                    return true;
                }
            }
            return false;
        }

        public bool DeleteItem(SlotIdentifier request)
        {
            if (!IsRequestValid(request) || IsRequestSlotEmpty(request))
            {
                return false;
            }
            _containers[request.ContainerId].DeleteItem(request.SlotIndex);
            return true;
        }

        public bool TakeItem(out IItem outItem, SlotIdentifier request)
        {
            if (!IsRequestValid(request) || IsRequestSlotEmpty(request))
            {
                outItem = null;
                return false;
            }
            _containers[request.ContainerId].TakeItem(request.SlotIndex,out outItem);
            return true;

        }

        public bool UnEquipItem(SlotIdentifier fromRequest, out List<SlotInfo> outInfo)
        {
            outInfo = new List<SlotInfo>();
            if (!IsRequestValid(fromRequest))
            {
                return false;
            }

            var fromContainer = GetContainer(fromRequest.ContainerId);
            if (!IsEquipmentContainer(fromContainer)
                || !HasItem(fromRequest))
            {
                return false;
            }

            fromContainer.GetItemSlotInfo(fromRequest.SlotIndex, out var itemInfo);
            if (!CanStoreItem(itemInfo.Item, out var availableSlotId, false))
            {
                return false;
            }
            
            if (TakeItem(out var item, fromRequest))
            {
                GetSlotInfo(fromRequest, out var fromSlotInfo);
                outInfo.Add(fromSlotInfo);
                if (StoreItem(ref item, availableSlotId, out var infoList))
                {
                    outInfo.AddRange(GetSlotInfoList(infoList));
                    return true;
                }
                else
                {
                    Debug.Log("Failed To Store Item in specified Slot");
                }
            }
            return false;
        }

        private bool HasItem(SlotIdentifier fromRequest)
        {
            return GetContainer(fromRequest.ContainerId).HasItem(fromRequest.SlotIndex);
        }

        private bool GetSlotInfo(SlotIdentifier slotID, out SlotInfo SlotInfo)
        {
            if (GetContainer(slotID.ContainerId).GetItemSlotInfo(slotID.SlotIndex, out var slotInfo))
            {
                SlotInfo = slotInfo;
                return true;
            }
            SlotInfo = default;
            return false;
        }

        private List<SlotInfo> GetSlotInfoList(List<SlotIdentifier> slotIdInfos)
        {
            var outInfo = new List<SlotInfo>();
            foreach (var slotId in slotIdInfos)
            {
                GetContainer(slotId.ContainerId).GetItemSlotInfo(slotId.SlotIndex, out var slotInfo);
                outInfo.Add(slotInfo);
            }
            return outInfo;
        }

        ///Stores Item in first available slot in any container
        public bool StoreItemAnywhere(ref IItem item, out List<SlotIdentifier> resultSlotIds)
        {
            resultSlotIds = new List<SlotIdentifier>();
            if (item == null) {  return false; }
            
            foreach (var KVP in _containers)
            {
                var container = KVP.Value;
                if (!container.CanStoreItemType(item.GetItemType())) { continue; }
                if (container.HasItemsOfUniqueId(item.UniqueId))
                {
                    if (container.StackItemInExistingStacks(ref item, out var info))
                    {
                        resultSlotIds.AddRange(info);
                        return true;
                    }
                }
                if (item != null) //In case it was partially stacked, but still has stacks left.
                {
                    if (container.StoreInFirstEmptySlot(ref item, out var info))
                    {
                        resultSlotIds.Add(info);
                        return true;
                    }
                }
            }
            return false;
        }
        
        ///Equips Item in first available Equip Container, that accepts that type of item.
        /// If the first viable container has no available slots, it will equip them in the first slot and
        /// put the other item in Storage.
        public bool EquipItemAnywhere(SlotIdentifier fromSlotId, out List<SlotInfo> resultSlotIds)
        {
            resultSlotIds = new List<SlotInfo>();
            if (!IsRequestValid(fromSlotId)) { return false; }
            if (IsRequestSlotEmpty(fromSlotId)) { return false; }
            
            var fromContainer = _containers[fromSlotId.ContainerId];
            if (IsEquipmentContainer(fromContainer))
            {
                //This stops trying to equip items that are already Equipped.
                return false;
            }
            var fromItemType = fromContainer.GetItemTypeOfItemInSlot(fromSlotId.SlotIndex);
            
            foreach (var KVP in _containers)
            {
                var toContainer = KVP.Value;
                if (IsEquipmentContainer(toContainer))
                {
                    if (!toContainer.CanStoreItemType(fromItemType)) { continue; }

                    if (toContainer.HasEmptySlots())
                    {
                        fromContainer.TakeItem(fromSlotId.SlotIndex, out var fromItem);
                        toContainer.StoreInFirstEmptySlot(ref fromItem, out var toSlotId);
                        fromContainer.GetItemSlotInfo(fromSlotId.SlotIndex, out var fromSlotInfo);
                        toContainer.GetItemSlotInfo(toSlotId.SlotIndex, out var toSlotInfo);
                        resultSlotIds.Add(fromSlotInfo);
                        resultSlotIds.Add(toSlotInfo);
                        return true;
                    }
                    else if (toContainer.CanStoreItemType(fromItemType))
                    {
                        InternalSwap(fromSlotId, new SlotIdentifier(toContainer.ContainerId, 0),
                            out var fromSlotInfo, out var toSlotInfo );
                        resultSlotIds.Add(fromSlotInfo);
                        resultSlotIds.Add(toSlotInfo);
                        return true;
                    }
                    else
                    {
                        //for when you can't swap, because the fromContainer can't store the toContainer's  item.
                        //So we store that item in first available container.
                        fromContainer.TakeItem(fromSlotId.SlotIndex, out var fromItem);
                        toContainer.TakeItem(0, out var toItem);
                        
                        toContainer.StoreItem(ref fromItem, 0);
                        toContainer.GetItemSlotInfo(fromSlotId.SlotIndex, out var toSlotInfo);
                        resultSlotIds.Add(toSlotInfo);
                        StoreItemAnywhere(ref toItem, out var outList);
                        foreach (var slotId in outList)
                        {
                            _containers[slotId.ContainerId].GetItemSlotInfo(slotId.SlotIndex, out var slotInfo);
                            resultSlotIds.Add(slotInfo);
                        }

                        fromContainer.GetItemSlotInfo(fromSlotId.SlotIndex, out var fromSlotInfo);
                        resultSlotIds.Add(fromSlotInfo);
                        return true;
                    }
                }
            }
            return false;
        }

        private static bool IsEquipmentContainer(DataInventoryContainer fromContainer)
        {
            return fromContainer is DataEquiptmentContainer || fromContainer is DataWeaponWheelContainer;
        }

        ///Stores Item in first available slot of specified container
        public bool StoreItem(ref IItem item, string containerId , 
            out List<SlotIdentifier> resultSlotIds, bool allowStacking = true)
        {
            resultSlotIds = new List<SlotIdentifier>();
            if (item == null) {  return false; }
            if (!_containers.ContainsKey(containerId)) { return false; }
            
            var container = _containers[containerId];

            if (!container.CanStoreItemType(item.GetItemType())) { return false; }
            
            if (allowStacking && container.HasItemsOfUniqueId(item.UniqueId))
            {
                if (container.StackItemInExistingStacks(ref item, out var info)) 
                    //Returns false if item still has stacks remaining.
                {
                    resultSlotIds.AddRange(info);
                    return true;
                }
            }
            if (item != null)
            {
                if (container.StoreInFirstEmptySlot(ref item, out var info))
                {
                    resultSlotIds.Add(info);
                    return true;
                }
            }
            
            return false;
        }
        
        

        /// Store Item in Specific container/Slot.
        public bool StoreItem(ref IItem item, SlotIdentifier request, out List<SlotIdentifier> resultSlotIds)
        {
            resultSlotIds = new List<SlotIdentifier>();
            if (item == null || request == null) { return false; }

            if (!IsRequestValid(request)) { return false; }

            var container = _containers[request.ContainerId];
            if (!container.CanStoreItemType(item.GetItemType())) { return false; }
            if (container.HasItemsOfUniqueId(item.UniqueId))
            {
                var wasSuccessfullyStacked = container.StackItemInExistingStacks(ref item, out var info);
                
                if (info != null)
                {
                    resultSlotIds.AddRange(info);
                }
                
                if (wasSuccessfullyStacked)
                {
                    return true;
                }

            }
            if (item != null) //In case it was partially stacked, but still has stacks left.
            {
                if (container.StoreInFirstEmptySlot(ref item, out var info))
                {
                    resultSlotIds.Add(info);
                    return true;
                }
            }
            
            return false;
        }

        public bool SplitStack(SlotIdentifier fromRequest, SlotIdentifier toRequest)
        {
            if (IsRequestValid(fromRequest) && IsRequestValid(toRequest) && !AreSameSlotId(toRequest, fromRequest)
                && !IsRequestSlotEmpty(fromRequest) && IsRequestSlotEmpty(toRequest)
                && GetRequestStackSize(fromRequest) > 1)
            {
                var item = _containers[fromRequest.ContainerId].SplitStack(fromRequest.SlotIndex, 0.5f);
                _containers[toRequest.ContainerId].StoreItem(ref item, toRequest.SlotIndex);
                return true;
            }
            return false;
        }


        ///Swaps items in slots, still works if toSlot is empty
        public bool SwapItem(SlotIdentifier fromRequest, SlotIdentifier toRequest, out List<SlotInfo> responseSlotsInfo)
        {
            responseSlotsInfo = new List<SlotInfo>();
            if (IsRequestValid(fromRequest) && IsRequestValid(toRequest) && !AreSameSlotId(toRequest, fromRequest))
            {
                var fromContainer = _containers[fromRequest.ContainerId];
                var toContainer = _containers[toRequest.ContainerId];
                
                if (IsRequestSlotEmpty(fromRequest))
                {
                    //don't need to check for empty slot in toContainer, since we can still swap slots in that situation.
                    return false;
                }
                
                var fromItemType = fromContainer.GetItemTypeOfItemInSlot(fromRequest.SlotIndex);
                if (!toContainer.CanStoreItemType(fromItemType))
                {
                    return false;
                }

                if (IsRequestSlotEmpty(toRequest))
                {
                    InternalMoveItem(fromRequest, toRequest,  out var fromItemInfo, out var toItemInfo);
                    responseSlotsInfo.Add(fromItemInfo);
                    responseSlotsInfo.Add(toItemInfo);
                    return true;
                }
                else if (!IsRequestSlotEmpty(toRequest) && CanItemsStack(fromRequest, toRequest))
                {
                    InternalStackInSpecificSlot(fromRequest, toRequest, out var fromItemInfo, out var toItemInfo);
                    responseSlotsInfo.Add(fromItemInfo);
                    responseSlotsInfo.Add(toItemInfo);
                    return true;
                }
                else if(!fromContainer.CanStoreItemType(
                    toContainer.GetItemTypeOfItemInSlot(toRequest.SlotIndex)))
                {
                    //Can place fromItem in toSlot, but cannot place toItem in fromSlot
                    //As such place it in inventory if possible, else cancel.
                    toContainer.GetItemSlotInfo(toRequest.SlotIndex, out var itemInfo);
                    foreach (var KVP in _containers)
                    {
                        if (KVP.Value.CanStoreItem(itemInfo.Item, out var slotIndex))
                        {
                            InternalMoveItem(toRequest, new SlotIdentifier(KVP.Value.ContainerId, slotIndex), 
                                out var fromItemInfo, out var toItemInfo);
                            InternalMoveItem(fromRequest, toRequest, 
                                out var fromItemInfo2, out var toItemInfo2);
                            responseSlotsInfo.Add(toItemInfo);
                            responseSlotsInfo.Add(fromItemInfo2);
                            responseSlotsInfo.Add(toItemInfo2);
                            return true;
                        }
                    }
                }
                else
                {
                    
                    InternalSwap(fromRequest, toRequest, out var fromItemInfo, out var toItemInfo);
                    responseSlotsInfo.Add(fromItemInfo);
                    responseSlotsInfo.Add(toItemInfo);
                    return true;
                }
            }
            return false;
        }

        public bool EquipItem(SlotIdentifier fromRequest, SlotIdentifier toRequest, out List<SlotInfo> responseSlotsInfo)
        {
            responseSlotsInfo = new List<SlotInfo>();
            if (IsRequestValid(fromRequest) && IsRequestValid(toRequest) && !IsRequestSlotEmpty(fromRequest) && 
                !AreSameSlotId(fromRequest, toRequest))
            {
                var fromContainer = _containers[fromRequest.ContainerId];
                var toContainer = _containers[toRequest.ContainerId];
                var fromItemType = fromContainer.GetItemTypeOfItemInSlot(fromRequest.SlotIndex);
                
                if (toContainer.SlotIsEmpty(toRequest.SlotIndex) 
                    && (toContainer is DataEquiptmentContainer || toContainer is DataWeaponWheelContainer)
                    && toContainer.CanStoreItemType(fromItemType))
                {
                    fromContainer.TakeItem(fromRequest.SlotIndex, out var item);
                    toContainer.StoreItem(ref item, toRequest.SlotIndex);
                    fromContainer.GetItemSlotInfo(fromRequest.SlotIndex, out var fromItemInfo);
                    toContainer.GetItemSlotInfo(toRequest.SlotIndex, out var toItemInfo);
                    responseSlotsInfo.Add(fromItemInfo);
                    responseSlotsInfo.Add(toItemInfo);
                    return true;
                }
            }
            responseSlotsInfo = null;
            return false;
        }

        private void InternalStackInSpecificSlot(SlotIdentifier fromRequest, SlotIdentifier toRequest, 
            out SlotInfo FromItemInfo, out SlotInfo ToItemInfo)
        {
            var fromContainer = GetContainer(fromRequest.ContainerId);
            var toContainer = GetContainer(toRequest.ContainerId);
            fromContainer.TakeItem(fromRequest.SlotIndex, out var fromItem2);
            if (!toContainer.StackItemInSlot(ref fromItem2, toRequest.SlotIndex))
            {
                fromContainer.StoreItem(ref fromItem2, fromRequest.SlotIndex);
            }
            fromContainer.GetItemSlotInfo(fromRequest.SlotIndex, out SlotInfo fromItemInfo );
            toContainer.GetItemSlotInfo(toRequest.SlotIndex, out SlotInfo toItemInfo );
            FromItemInfo = fromItemInfo;
            ToItemInfo = toItemInfo;
            //todo this might cause item in a 3rd slot?
        }

        private void InternalSwap(SlotIdentifier fromRequest, SlotIdentifier toRequest,
            out SlotInfo FromSlotInfo, out SlotInfo ToSlotInfo)
        {
            var fromContainer = GetContainer(fromRequest.ContainerId);
            var toContainer = GetContainer(toRequest.ContainerId);
            fromContainer.TakeItem(fromRequest.SlotIndex, out var fromItem);
            toContainer.TakeItem(toRequest.SlotIndex, out var toItem);
            fromContainer.StoreItem(ref toItem, fromRequest.SlotIndex);
            toContainer.StoreItem(ref fromItem, toRequest.SlotIndex);
            fromContainer.GetItemSlotInfo(fromRequest.SlotIndex, out SlotInfo fromSlotInfo );
            toContainer.GetItemSlotInfo(toRequest.SlotIndex, out SlotInfo toSlotInfo );
            FromSlotInfo = fromSlotInfo;
            ToSlotInfo = toSlotInfo;
        }

        public bool CanStoreItem(ItemInfo itemInfo, out SlotIdentifier availableSlotId, bool allowEquipSlot)
        {
            availableSlotId = null;
            if (itemInfo == ItemInfo.NULL) { return false; }
            
            foreach (var KVP in _containers)
            {
                if (allowEquipSlot && IsEquipmentContainer(KVP.Value))
                {
                    continue;
                }
                if (KVP.Value.CanStoreItem(itemInfo, out var slotIndex))
                {
                    availableSlotId = new SlotIdentifier(KVP.Value.ContainerId, slotIndex);
                    return true;
                }
            }
            return false;
        }
        
        private void InternalMoveItem(SlotIdentifier fromRequest, SlotIdentifier toRequest,
            out SlotInfo FromItemInfo, out SlotInfo ToItemInfo)
        {
            var fromContainer = GetContainer(fromRequest.ContainerId);
            var toContainer = GetContainer(toRequest.ContainerId);
            fromContainer.TakeItem(fromRequest.SlotIndex, out var fromItem);
            toContainer.StoreItem(ref fromItem, toRequest.SlotIndex);
            fromContainer.GetItemSlotInfo(fromRequest.SlotIndex, out SlotInfo fromItemInfo );
            toContainer.GetItemSlotInfo(toRequest.SlotIndex, out SlotInfo toItemInfo );
            FromItemInfo = fromItemInfo;
            ToItemInfo = toItemInfo;
        }

        private DataInventoryContainer GetContainer(string containerId)
        {
            return _containers[containerId];
        }

        private bool CanItemsStack(SlotIdentifier fromRequest, SlotIdentifier toRequest)
        {
            return _containers[fromRequest.ContainerId].GetItemIdOfSlot(fromRequest.SlotIndex) 
                   == _containers[toRequest.ContainerId].GetItemIdOfSlot(toRequest.SlotIndex);
        }

        private ContainerInfo[] GetAllContainersInfo()
        {
            var containersInfo = new ContainerInfo[_containers.Count];
            var i = 0;
            foreach (var KVP in _containers)
            {
                containersInfo[i] = GetContainerInfo(KVP.Key);
                i++;
            }
            return containersInfo;
        }

        private ItemType GetRequestItemType(SlotIdentifier request)
        {
            return _containers[request.ContainerId].GetItemTypeOfItemInSlot(request.SlotIndex);
        }

        private bool IsRequestSlotEmpty(SlotIdentifier request)
        {
            return _containers[request.ContainerId].SlotIsEmpty(request.SlotIndex);
        }

        // Confirms the ContainerId and slotIndex exist, nothing more.

        private bool AreSameSlotId(SlotIdentifier toSlotId, SlotIdentifier fromSlotId)
        {
            return toSlotId.ContainerId == fromSlotId.ContainerId 
                   && toSlotId.SlotIndex == fromSlotId.SlotIndex;
        }

        private bool IsRequestValid(SlotIdentifier slotIdentifier)
        {
            if (slotIdentifier == null)
            {
                return false;
            }
            return IsValidContainerId(slotIdentifier.ContainerId) && IsValidSlotIndex(slotIdentifier.ContainerId, slotIdentifier.SlotIndex);
        }

        private bool IsValidContainerId(string containerId)
        {
            if (containerId != string.Empty && _containers.ContainsKey(containerId))
            {
                return true;
            }
            return false;
        }

        ///Check containerId is Valid before using.
        private bool IsValidSlotIndex(string containerId, int slotIndex)
        {
            return _containers[containerId].isValidSlotIndex(slotIndex);
        }

        private int GetRequestStackSize(SlotIdentifier fromRequest)
        {
            return _containers[fromRequest.ContainerId].GetCurrentStackSize(fromRequest.SlotIndex);
        }

        public static ContainerType GetContainerTypeEnum(DataInventoryContainer container)
        {
            ContainerType temp; 
            if (container is DataEquiptmentContainer)
            {
                temp = ContainerType.Equiptment;
            }
            else if (container is DataWeaponWheelContainer)
            {
                temp = ContainerType.WeaponWheel;
            }
            else
            {
                temp = ContainerType.Storage;
            }
            return temp;
        }
    }
    
}