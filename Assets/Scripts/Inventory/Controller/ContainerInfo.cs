using System;

namespace Game.Inventory
{
    [Serializable]
    public class ContainerInfo
    {
        public string Id;
        public ItemType[] _allowedItemTypes;
        public ContainerType ContainerType;
        public DataInventorySlot[] InventorySlots;

        public ContainerInfo(DataInventoryContainer container)
        {
            Id = container.ContainerId;
            _allowedItemTypes = container.AllowedItemTypes;
            ContainerType = Inventory.GetContainerTypeEnum(container);
            if (container.GetItemSlots(out var inventorySlots))
            {
                InventorySlots = inventorySlots;
            }
        }
    }
    
    //todo add equivalent SlotInfo and ItemInfo classes;
}