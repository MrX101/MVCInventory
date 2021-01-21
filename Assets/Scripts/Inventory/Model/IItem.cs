using System;
using UnityEngine;

namespace Game.Inventory
{
    public interface IItem
    {
        string Name { get; set; }
        string Description { get; set; }
        Sprite Sprite { get; set; }
        string UniqueId { set; get; }
        int MaxStackSize { get; set; }
        int CurrentStackSize { get; set;}
        ItemType GetItemType();
        void DeleteSelf();
        // Returns a copy with same values, but CurrentStackSize set to 0.
        IItem CreateEmptyDuplicate();
    }
    
    [Serializable]
    public enum ItemType
    {
        Gun,
        Melee,
        Grenade,
        Consumable,
        Ammo,
        Currency,
        Other,
        NotSet
    }
}