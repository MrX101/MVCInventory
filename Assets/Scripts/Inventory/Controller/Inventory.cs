using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Inventory
{
    [Serializable]
    public class Inventory 
    {
        [SerializeField]public List<ContainerSettings> _containersToCreate = new List<ContainerSettings>();
        private Dictionary<string, DataInventoryContainer> _containers = new Dictionary<string, DataInventoryContainer>();

        public void DebugShowAllItems()
        {
            Debug.Log("Start Of Log");
            foreach (var containerInfo in GetAllContainersInfo())
            {
                Debug.Log("Container: "+ containerInfo.Id + " has "+containerInfo.InventorySlots.Length + " slots");
                foreach (SlotInfo slot in containerInfo.InventorySlots)
                {
                    if (slot.HasItem)
                    {
                        Debug.Log( "Slot Index: " + slot.SlotIndex + " has Item:\n" +
                                   "Name: " + slot.Item.Name +" Id: "+slot.Item.UniqueId);
                    }
                }
            }
            Debug.Log("End Of Log");
        }

        public void Initialize()
        {
            //todo add items
            foreach (var settings in _containersToCreate)
            {
                if (settings.CreateInstance(out var container))
                {
                    _containers.Add(settings.Identifier, container);
                }

                foreach (ItemSettings itemToCreate in settings.ItemsToCreate)
                {
                    if (itemToCreate.IsValid())
                    {
                        if (!StoreItem(ref itemToCreate.Item, out var thing))
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
                ItemsManager.Singleton.CreateWorldItem(item, position, rotation);
                return true;
            }
            return false;
        }

        public ContainerInfo GetContainerInfo(string containerId)
        {
            return new ContainerInfo(_containers[containerId]);
        }

        public bool SetPrimary(SlotIdentifier request)
        {
            if (IsRequestValid(request) && !IsRequestSlotEmpty(request))
            {
                if (_containers[request.ContainerId] is DataWeaponWheelContainer container)
                {
                    return container.SetPrimaryWeapon(request.SlotIndex);
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
                bool result = StoreItem(ref item, out var info);
                if (result)
                {
                    outInfo = info;
                    return true;
                }
            }
            outInfo = null;
            return false;
        }

        //todo Need to make this tell you where it was stored.
        ///Stores Item in first available slot, Does Not store in Equipment & WeaponWheel Slots.
        public bool StoreItem(ref IItem item, out List<SlotIdentifier> resultSlotIds)
        {
            resultSlotIds = new List<SlotIdentifier>();
            if (item == null) {  return false; }
            
            foreach (var KVP in _containers)
            {
                if (KVP.Value is DataEquiptmentContainer) { continue; }
                if (KVP.Value is DataWeaponWheelContainer) { continue; }
                var container = KVP.Value;
                
                if (container.CanStoreItemType(item.GetItemType()) && container.HasItemsOfUniqueId(item.UniqueId))
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

            if (IsRequestValid(request))
            {
                var container = _containers[request.ContainerId];
                if (container.CanStoreItemType(item.GetItemType()) && container.HasItemsOfUniqueId(item.UniqueId))
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
        public bool SwapItem(SlotIdentifier fromRequest, SlotIdentifier toRequest)
        {
            if (IsRequestValid(fromRequest) && IsRequestValid(toRequest))
            {
                var fromContainer = _containers[fromRequest.ContainerId];
                var toContainer = _containers[toRequest.ContainerId];
                if (IsRequestSlotEmpty(fromRequest))
                {
                    //don't need to check for empty slot in toContainer, since we can still swap slots in that situation.
                    return false;
                }
                if (fromRequest.ContainerId == toRequest.ContainerId && !IsRequestSlotEmpty(toRequest))
                {
                    _containers[fromRequest.ContainerId].SwapSlots(fromRequest.SlotIndex, toRequest.SlotIndex);
                    return true;
                }

                if (IsRequestSlotEmpty(toRequest))
                {
                    InternalMoveItem(fromRequest, toRequest, ref fromContainer, ref toContainer);
                    return true;
                }
                else if (!IsRequestSlotEmpty(toRequest) && CanSlotsStack(fromRequest, toRequest))
                {
                    InternalStackInSpecificSlot(fromRequest, toRequest, ref fromContainer, ref toContainer);
                    return true;
                }
                else
                {
                    InternalSwap(fromRequest, toRequest, ref fromContainer, ref toContainer);
                    return true;
                }
            }
            return false;
        }

        /// Use to equip Item into an empty Equip Slot;
        public bool EquipItem(SlotIdentifier fromRequest, SlotIdentifier toRequest)
        {
            if (IsRequestValid(fromRequest) && IsRequestValid(toRequest) && !IsRequestSlotEmpty(fromRequest))
            {
                var fromContainer = _containers[fromRequest.ContainerId];
                var toContainer = _containers[toRequest.ContainerId];
                var fromItemType = fromContainer.GetItemTypeOfItemInSlot(toRequest.SlotIndex);
                
                if (toContainer.SlotIsEmpty(toRequest.SlotIndex) 
                    && (toContainer is DataEquiptmentContainer || toContainer is DataWeaponWheelContainer)
                    && toContainer.CanStoreItemType(fromItemType))
                {
                    fromContainer.TakeItem(fromRequest.SlotIndex, out var item);
                    toContainer.StoreItem(ref item, toRequest.SlotIndex);
                    return true;
                }
            }
            return false;
        }

        private void InternalStackInSpecificSlot(SlotIdentifier fromRequest, SlotIdentifier toRequest,
            ref DataInventoryContainer fromContainer, ref DataInventoryContainer toContainer)
        {
            fromContainer.TakeItem(fromRequest.SlotIndex, out var fromItem2);
            toContainer.StackItemInSlot(ref fromItem2, toRequest.SlotIndex);
            fromContainer.StoreItem(ref fromItem2, fromRequest.SlotIndex);
        }

        private void InternalSwap(SlotIdentifier fromRequest, SlotIdentifier toRequest,
            ref DataInventoryContainer fromContainer, ref DataInventoryContainer toContainer)
        {
            fromContainer.TakeItem(fromRequest.SlotIndex, out var fromItem);
            toContainer.TakeItem(toRequest.SlotIndex, out var toItem);
            fromContainer.StoreItem(ref toItem, fromRequest.SlotIndex);
            toContainer.StoreItem(ref fromItem, toRequest.SlotIndex);
        }
        
        private void InternalMoveItem(SlotIdentifier fromRequest, SlotIdentifier toRequest,
            ref DataInventoryContainer fromContainer, ref DataInventoryContainer toContainer)
        {
            fromContainer.TakeItem(fromRequest.SlotIndex, out var fromItem);
            toContainer.StoreItem(ref fromItem, toRequest.SlotIndex);
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