using System;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

namespace Game.Inventory
{
    [Serializable]
    public class ContainerSettings
    {
        public string Identifier = "";
        public int NumberOfSlots = 0;
        public ContainerType Type;
        public List<ItemSettings> ItemsToCreate = new List<ItemSettings>();
        
        [SerializeField]
        public ItemType[] AllowedItemTypes = 
        {
            ItemType.Gun, ItemType.Melee, ItemType.Grenade, ItemType.Consumable, ItemType.Ammo, ItemType.Currency,
        };

        public void OnValidate()
        {
            foreach (ItemSettings itemSetting in ItemsToCreate)
            {
                itemSetting.Validate();
            }
        }

        public bool CreateContainerInstance(out DataInventoryContainer container)
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

    //Settings for creating items from within the inspector.
    //This will enable both testing of functionality and allow containers to be created with preset lists of items.
    
    [Serializable]
    public class ItemSettings 
    {
        [SerializeField]public ItemClasses ItemClass = ItemClasses.Baseclass;
        [SerializeReference]public IItem Item;
        [SerializeField]public float ChanceToSpawn = 100f;
        [SerializeField]public int minStackSize = 1;
        [SerializeField]public int maxStackSize = 1;

        public void Validate()
        {
            if (Item == null)
            {
                Item = CreateItem(ItemClass);
            }
            
            if (string.IsNullOrWhiteSpace(Item.UniqueId))
            {
                Item.UniqueId = ItemHelper.GenerateID();
            }
            
            if (!IsChanceToSpawnValid())
            {
                ChanceToSpawn = 0f;
            }

            if (!IsCurrentItemStackSizeValid())
            {
                Item.CurrentStackSize = 1;
            }

            if (!IsMaxItemStackSizeValid())
            {
                Item.MaxStackSize = Item.CurrentStackSize;
            }

            if (!IsMinStackSizeValid())
            {
                minStackSize = 1;
            }

            if (!IsMaxStackSizeValid())
            {
                maxStackSize = Item.MaxStackSize;
            }
        }

        private bool IsMaxStackSizeValid()
        {
            return maxStackSize > 0 && maxStackSize <= Item.MaxStackSize;
        }

        private bool IsMinStackSizeValid()
        {
            return minStackSize > 0 && minStackSize <= Item.MaxStackSize;
        }

        public bool IsValid()
        {    
            return (Item != null &&
                    ChanceToSpawn > 0f && ChanceToSpawn <= 100f &&
                    Item.MaxStackSize > 0 && Item.CurrentStackSize > 0 &&
                    Item.MaxStackSize >= Item.CurrentStackSize &&
                    minStackSize > 0 && minStackSize <= Item.MaxStackSize &&
                    maxStackSize > 0 && maxStackSize <= Item.MaxStackSize);
        }

        private bool IsChanceToSpawnValid()
        {
            return ChanceToSpawn > 0f && ChanceToSpawn <= 100f;
        }
        
        private bool IsCurrentItemStackSizeValid()
        {
            return Item.CurrentStackSize > 0;
        }
        
        private bool IsMaxItemStackSizeValid()
        {
            return Item.MaxStackSize > 0 && Item.MaxStackSize >= Item.CurrentStackSize;
        }
        

        public bool RollSpawnChance()
        {
            return ChanceToSpawn >= UnityEngine.Random.Range(0f, 100f);
        }

        public void RollStackSize()
        {
            if (Item.CurrentStackSize < Item.MaxStackSize)
            {
                Item.CurrentStackSize = UnityEngine.Random.Range(minStackSize, maxStackSize+1);
            }
            Item.CurrentStackSize = Mathf.Clamp(Item.CurrentStackSize,minStackSize,Item.MaxStackSize);
        }

        //Warning Will need to update this as new classes that subscribe to IItem get added.
        public static IItem CreateItem(ItemClasses itemClass)
        {
            if (itemClass == ItemClasses.Baseclass)
            {
                return new BaseItem();
            }
            return default;
        }
    }
}