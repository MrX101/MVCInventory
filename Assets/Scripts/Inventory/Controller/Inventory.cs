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

        public List<ContainerSettings> ContainerSettings
        {
            get => containerSettings;
            set => containerSettings = value;
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
                if (settings.CreateInstance(out var container))
                {
                    _containers.Add(settings.Identifier, container);
                }

                foreach (ItemSettings itemToCreate in settings.ItemsToCreate)
                {
                    if (itemToCreate.IsValid())
                    {
                        if (!StoreItem(ref itemToCreate.Item, settings.Identifier ,out var slotIds))
                        {
                            Debug.Log("Unable to add item");
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
                amount = container.GetSpecificAmount_SpecificItem_FromAllSlots(ammoId, amount, out var item);
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
                ItemsManager.instance.CreateWorldItem(item, position, rotation);
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

        public bool UnEquipItem(SlotIdentifier request, out List<SlotIdentifier> outInfo)
        {
            if (TakeItem(out var item, request))
            {
                bool result = StoreItemAnywhere(ref item, out var info);
                if (result)
                {
                    outInfo = info;
                    return true;
                }
            }
            outInfo = null;
            return false;
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
        
        ///Stores Item in first available slot of specified container
        public bool StoreItem(ref IItem item, string containerId , out List<SlotIdentifier> resultSlotIds)
        {
            resultSlotIds = new List<SlotIdentifier>();
            if (item == null) {  return false; }
            if (!_containers.ContainsKey(containerId)) { return false; }
            
            var container = _containers[containerId];

            if (!container.CanStoreItemType(item.GetItemType())) { return false; }
            
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
            
            return false;
        }
        
        /// <summary>
        /// Store Item in Specific container/Slot.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="request"></param>
        /// <returns></returns>
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
            if (IsRequestValid(fromRequest) && IsRequestValid(toRequest) 
                && !IsRequestSlotEmpty(fromRequest) && IsRequestSlotEmpty(toRequest)
                && GetRequestStackSize(fromRequest) > 1)
            {
                var item = _containers[fromRequest.ContainerId].SplitStack(fromRequest.SlotIndex, 0.5f);
                _containers[toRequest.ContainerId].StoreItem(ref item, toRequest.SlotIndex);
                return true;
            }
            return false;
        }


        ///Swaps slots of items, still works if 1 is empty
        public bool SwapItem(SlotIdentifier fromRequest, SlotIdentifier toRequest, out List<SlotInfo> responseSlotsInfo)
        {
            responseSlotsInfo = new List<SlotInfo>();
            if (IsRequestValid(fromRequest) && IsRequestValid(toRequest))
            {
                var fromContainer = _containers[fromRequest.ContainerId];
                var toContainer = _containers[toRequest.ContainerId];
                if (IsRequestSlotEmpty(fromRequest))
                {
                    //don't need to check for empty slot in toContainer, since we can still swap slots in that situation.
                    responseSlotsInfo = null;
                    return false;
                }

                if (IsRequestSlotEmpty(toRequest))
                {
                    InternalMoveItem(fromRequest, toRequest, ref fromContainer, ref toContainer, out var fromItemInfo, out var toItemInfo);
                    responseSlotsInfo.Add(fromItemInfo);
                    responseSlotsInfo.Add(toItemInfo);
                    return true;
                }
                else if (!IsRequestSlotEmpty(toRequest) && CanSlotsStack(fromRequest, toRequest))
                {
                    InternalStackInSpecificSlot(fromRequest, toRequest, ref fromContainer, ref toContainer, out var fromItemInfo, out var toItemInfo);
                    responseSlotsInfo.Add(fromItemInfo);
                    responseSlotsInfo.Add(toItemInfo);
                    return true;
                }
                else
                {
                    InternalSwap(fromRequest, toRequest, ref fromContainer, ref toContainer, out var fromItemInfo, out var toItemInfo);
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
            if (IsRequestValid(fromRequest) && IsRequestValid(toRequest) && !IsRequestSlotEmpty(fromRequest))
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
                    fromContainer.GetItemSlotInfo(toRequest.SlotIndex, out var toItemInfo);
                    responseSlotsInfo.Add(fromItemInfo);
                    responseSlotsInfo.Add(toItemInfo);
                    return true;
                }
            }
            responseSlotsInfo = null;
            return false;
        }

        private void InternalStackInSpecificSlot(SlotIdentifier fromRequest, SlotIdentifier toRequest,
            ref DataInventoryContainer fromContainer, ref DataInventoryContainer toContainer,
            out SlotInfo FromItemInfo, out SlotInfo ToItemInfo)
        {
            fromContainer.TakeItem(fromRequest.SlotIndex, out var fromItem2);
            toContainer.StackItemInSlot(ref fromItem2, toRequest.SlotIndex);
            fromContainer.StoreItem(ref fromItem2, fromRequest.SlotIndex);
            fromContainer.GetItemSlotInfo(fromRequest.SlotIndex, out SlotInfo fromItemInfo );
            toContainer.GetItemSlotInfo(toRequest.SlotIndex, out SlotInfo toItemInfo );
            FromItemInfo = fromItemInfo;
            ToItemInfo = toItemInfo;
            //todo this might cause item in a 3rd slot?
        }

        private void InternalSwap(SlotIdentifier fromRequest, SlotIdentifier toRequest,
            ref DataInventoryContainer fromContainer, ref DataInventoryContainer toContainer,
            out SlotInfo FromItemInfo, out SlotInfo ToItemInfo)
        {
            fromContainer.TakeItem(fromRequest.SlotIndex, out var fromItem);
            toContainer.TakeItem(toRequest.SlotIndex, out var toItem);
            fromContainer.StoreItem(ref toItem, fromRequest.SlotIndex);
            toContainer.StoreItem(ref fromItem, toRequest.SlotIndex);
            fromContainer.GetItemSlotInfo(fromRequest.SlotIndex, out SlotInfo fromItemInfo );
            toContainer.GetItemSlotInfo(toRequest.SlotIndex, out SlotInfo toItemInfo );
            FromItemInfo = fromItemInfo;
            ToItemInfo = toItemInfo;
        }
        
        private void InternalMoveItem(SlotIdentifier fromRequest, SlotIdentifier toRequest,
            ref DataInventoryContainer fromContainer, ref DataInventoryContainer toContainer,
            out SlotInfo FromItemInfo, out SlotInfo ToItemInfo)
        {
            fromContainer.TakeItem(fromRequest.SlotIndex, out var fromItem);
            toContainer.StoreItem(ref fromItem, toRequest.SlotIndex);
            fromContainer.GetItemSlotInfo(fromRequest.SlotIndex, out SlotInfo fromItemInfo );
            toContainer.GetItemSlotInfo(toRequest.SlotIndex, out SlotInfo toItemInfo );
            FromItemInfo = fromItemInfo;
            ToItemInfo = toItemInfo;
        }

        private bool CanSlotsStack(SlotIdentifier fromRequest, SlotIdentifier toRequest)
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

        private bool IsRequestValid(SlotIdentifier slotIdentifier)
        {
            return IsValidContainerId(slotIdentifier.ContainerId) && IsValidSlotIndex(slotIdentifier.ContainerId, slotIdentifier.SlotIndex);
        }

        private bool IsValidContainerId(string containerId)
        {
            if (_containers.ContainsKey(containerId))
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