using System;
using UnityEngine;

namespace Game.Inventory
{
    [Serializable]
    public class ContainerSettings
    {
        public string Identifier = "";
        public int NumberOfSlots = 0;
        public ContainerType Type;
        
        [SerializeField]
        public ItemType[] AllowedItemTypes = 
        {
            ItemType.Gun, ItemType.Melee, ItemType.Grenade, ItemType.Consumable, ItemType.Ammo, ItemType.Currency,
        };

        public bool CreateInstance(out DataInventoryContainer container)
        {
            if (AmINull())
            {
                container = null;
                return false;
            }
            if (Type == ContainerType.Storage)
            {
                container = new DataInventoryContainer(this);
                return true;
            }
            if (Type == ContainerType.Equiptment)
            {
                container = new DataEquiptmentContainer(this);
                return true;
            }
            if (Type == ContainerType.WeaponWheel)
            {
                container = new DataWeaponWheelContainer(this);
                return true;
            }
            container = null;
            return false;
        }

        private bool AmINull()
        {
            if (Identifier == "" || NumberOfSlots == 0 || AllowedItemTypes == null)
            {
                Debug.Log("Incorrect container Settings Set");
                return true;
            }
            return false;
        }
    }
}