using System;

namespace Game.Inventory
{
    //This class is an container that can have many stored weapons at once, but only a set number(currently only 1) of active ones(called primaries)
    //Currently being used to set a primary active weapon in a weapon wheel, where you have 6 currently slotted weapons, but can only use 1 at a time.
    //The .SetEquipped is being called to indicate it's set to primary.
    [Serializable]
    public class DataWeaponWheelContainer : DataInventoryContainer
    {
        private Action<int> OnSelectedWeaponChanged;

        private int _selectedWeaponIndex = 0;
        
        //protected int MaxNumOfPrimaryWeapons = 1;
        public DataWeaponWheelContainer(ContainerSettings containerSettings) 
            : base(containerSettings)
        {
            
        }
        
        public override void StoreItem(ref IItem itemRef, int slotIndex)
        {
            base.StoreItem(ref itemRef, slotIndex);
            if (slotIndex == _selectedWeaponIndex)
            {
                _inventorySlots[slotIndex].SetEquipped();
            }
        }

        //Used to set which weapon of the equipped slot the character is currently using.
        public void SetSelectedWeapon(int itemIndex)
        {
            if (itemIndex == _selectedWeaponIndex) { return; }
            _inventorySlots[itemIndex].SetEquipped();
            _inventorySlots[_selectedWeaponIndex].SetUnEquipped();
             OnSelectedWeaponChanged.Invoke(itemIndex);
        }
    }
}