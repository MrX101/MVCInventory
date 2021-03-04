using System;
using UnityEngine;

namespace Game.Inventory
{
    [Serializable]
    public struct ContainerInfo
    {
        public string Id;
        public ItemType[] _allowedItemTypes;
        public ContainerType ContainerType;
        public SlotInfo[] InventorySlots;

        public ContainerInfo(DataInventoryContainer container)
        {
            InventorySlots = new SlotInfo[] { };
            Id = container.ContainerId;
            _allowedItemTypes = container.AllowedItemTypes;
            ContainerType = Inventory.GetContainerTypeEnum(container);
            if (container.GetItemSlotsInfo(out SlotInfo[] inventorySlots))
            {
                InventorySlots = inventorySlots;
            }
        }
    }

    [Serializable]
    public struct SlotInfo
    {
        public SlotIdentifier SlotId;
        public bool HasItem;
        public ItemInfo Item;
        public bool IsEquipped;
        
        public SlotInfo(DataInventorySlot dataSlot)
        {
            if (dataSlot == null)
            {
                Item = ItemInfo.NULL;
                HasItem = false;
                SlotId = null;
                IsEquipped = false;
                return;
            }
            SlotId = new SlotIdentifier(dataSlot.ContainerId, dataSlot.SlotIndex);
            this.IsEquipped = dataSlot.IsEquipped;
            Item = dataSlot.GetItemInfo();
            HasItem = dataSlot.HasItem();
        }
    }

    [Serializable]
    public struct ItemInfo
    {
        public static readonly ItemInfo NULL = new ItemInfo{UniqueId = "NULL", Name = "NULL"};
        public string Name;
        public string Description;
        public Sprite Sprite;
        public string UniqueId;
        public int MaxStackSize;
        public int CurrentStackSize;
        public ItemType ItemType;

        public ItemInfo(IItem item)
        {
            this.Name = item.Name;
            this.Description = item.Description;
            this.Sprite = item.Sprite;
            this.UniqueId = item.UniqueId;
            this.MaxStackSize = item.MaxStackSize;
            this.CurrentStackSize = item.CurrentStackSize;
            this.ItemType = item.GetItemType();
        }
        
        public static bool operator ==(ItemInfo one, ItemInfo two)
        {
            return (one.UniqueId == two.UniqueId && one.Name == two.Name && one.Description == two.Description &&
                    one.CurrentStackSize == two.CurrentStackSize && one.MaxStackSize == two.MaxStackSize);
        }

        public static bool operator !=(ItemInfo one, ItemInfo two)
        {
            return (one.UniqueId != two.UniqueId || one.Name != two.Name || one.Description != two.Description ||
                    one.CurrentStackSize != two.CurrentStackSize || one.MaxStackSize != two.MaxStackSize);;
        }

        public override bool Equals(object obj)
        {
            if (obj ==null)
            {
                return false;
            }
            return this == (ItemInfo)obj;
        }
    }
}