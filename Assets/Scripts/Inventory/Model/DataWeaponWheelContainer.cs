using System;

namespace Game.Inventory
{
    //This class is an container that can have many stored weapons at once, but only a set number(currently only 1) of active ones(called primaries)
    //Currently being used to set a primary active weapon in a weapon wheel, where you have 6 currently slotted weapons, but can only use 1 at a time.
    //The .SetEquipped is being called to indicate it's set to primary.
    [Serializable]
    public class DataWeaponWheelContainer : DataInventoryContainer
    {
        private Action<int> OnPrimaryWeaponChanged;
        
        
        //protected int MaxNumOfPrimaryWeapons = 1;
        public DataWeaponWheelContainer(ContainerSettings containerSettings) 
            : base(containerSettings)
        {
            
        }

        //Used to set which weapon of the equipped slot the character is currently using.
        public bool SetPrimaryWeapon(int itemIndex)
        {
            for (int i = 0; i < _inventorySlots.Count; i++)
            {
                if (i == itemIndex)
                {
                    _inventorySlots[itemIndex].SetEquipped();
                    return true;
                }
                _inventorySlots[itemIndex].SetUnEquipped();
            }
            return false;
        }
    }
}