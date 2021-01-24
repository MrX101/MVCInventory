﻿using System;
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
    public struct OutContainerInfo
    {
        public static readonly OutContainerInfo NULL = new OutContainerInfo("", -1);
        public string ContainerId;
        public int SlotId;

        public OutContainerInfo(string containerId, int slotId)
        {
            ContainerId = containerId;
            SlotId = slotId;
        }
    }

    [Serializable]
    public struct SlotInfo
    {
        public int SlotIndex;
        public string ContainerId;
        public bool HasItem;
        public ItemInfo Item;
        public bool IsEquipped;

        public SlotInfo(DataInventorySlot dataSlot)
        {
            this.SlotIndex = dataSlot.SlotIndex;
            this.ContainerId = dataSlot.ContainerId;
            this.IsEquipped = dataSlot.IsEquipped;
            Item = dataSlot.GetItemInfo();
            HasItem = dataSlot.HasItem();
        }
    }

    [Serializable]
    public struct ItemInfo
    {
        public static readonly ItemInfo NULL = new ItemInfo{UniqueId = "NULL"};
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
    }
    //todo add equivalent SlotInfo and ItemInfo classes;
}