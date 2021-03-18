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
    
    public static class ItemHelper
    {
        public static string GenerateID()
        {
            return Guid.NewGuid().ToString().Substring(0, 5);
        }
    }
    
    //Warning Will need to update this as new classes that subscribe to IItem get added.
    public enum ItemClasses
    {
        Baseclass,
    }
    
    
    public enum ItemType
    {
        Gun,
        Melee,
        Grenade,
        Chest,
        Arms,
        Leg,
        Head,
        Consumable,
        Ammo,
        Currency,
        Other,
        NotSet
    }
}