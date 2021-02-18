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
            foreach (var itemSetting in ItemsToCreate)
            {
                //itemSetting.IsValid();
            }
        }
        
        public void SetItems()
        {
            foreach (var itemSetting in ItemsToCreate)
            {
                itemSetting.SetItem();
            }
        }

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

        public bool IsValid()
        {
            return (Item != null &&
                    ChanceToSpawn > 0f && ChanceToSpawn <= 100f &&
                    Item.MaxStackSize > 0 && Item.CurrentStackSize > 0 &&
                    Item.MaxStackSize >= Item.CurrentStackSize &&
                    minStackSize > 0 && minStackSize <= Item.MaxStackSize &&
                    maxStackSize > 0 && maxStackSize <= Item.MaxStackSize);
        }

        public int GenerateStackSize()
        {
            return UnityEngine.Random.Range(minStackSize, maxStackSize+1);
        }
        
        [Button("Create Item", EButtonEnableMode.Editor)]
        public void SetItem()
        {
            Item = CreateItem();
        }
        
        //Warning Will need to update this as new classes that subscribe to IItem get added.
        private IItem CreateItem()
        {
            if (ItemClass == ItemClasses.Baseclass)
            {
                return new BaseItem();
            }
            return default;
        }
    }
    
    //Warning Will need to update this as new classes that subscribe to IItem get added.
    public enum ItemClasses
    {
        Baseclass,
    }
}