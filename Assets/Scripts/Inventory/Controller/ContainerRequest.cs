using System;

namespace Game.Inventory
{
    [Serializable]
    public class ContainerRequest
    {
        public string ContainerId;
        public int SlotIndex;

        public ContainerRequest()
        {
            
        }

        public ContainerRequest(string containerId, int slotIndex)
        {
            ContainerId = containerId;
            SlotIndex = slotIndex;
        }
    }
}