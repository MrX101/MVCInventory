using System;

namespace Game.Inventory
{
    [Serializable]
    //Being equipped in this container indicates you are now useable and gives stats.
    public class DataEquiptmentContainer : DataInventoryContainer
    {

        public DataEquiptmentContainer(ContainerSettings containerSettings) : base(containerSettings)
        {
            
        }

        public override void StoreItem(ref IItem itemRef, int slotIndex)
        {
            base.StoreItem(ref itemRef, slotIndex);
            _inventorySlots[slotIndex].SetEquipped();
        }
    }

}