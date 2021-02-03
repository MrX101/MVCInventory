using System;

namespace Game.Inventory
{
    [Serializable]
    public class SlotIdentifier
    {
        public string ContainerId;
        public int SlotIndex;

        public SlotIdentifier()
        {
            
        }

        public SlotIdentifier(string containerId, int slotIndex)
        {
            ContainerId = containerId;
            SlotIndex = slotIndex;
        }
    }
}