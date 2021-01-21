using System;
using UnityEngine;

namespace Game.Inventory
{
    public class BaseItem : IItem
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public Sprite Sprite { get; set; }
        public string UniqueId { get; set; }
        public int MaxStackSize { get; set;}
        public int CurrentStackSize { get; set; }
        public ItemType ItemType = ItemType.Gun;
        
        public ItemType GetItemType()
        {
            return ItemType;
        }

        public void DeleteSelf()
        {
            Name = "deleted";
            Description = null;
            Sprite = null;
            UniqueId = null;
            MaxStackSize = 0;
            CurrentStackSize = 0;
            //Don't think we need to do anything
            //since it's not a gameObject and thus null reference alone is good enough?
        }
        
        /// <summary>
        /// Creates a copy with same values, but CurrentStackSize set to 0.
        /// </summary>
        /// <returns></returns>
        public IItem CreateEmptyDuplicate()
        {
            var copy = new BaseItem();
            copy.Name = this.Name;
            copy.Description = this.Description;
            copy.UniqueId = this.UniqueId;
            copy.Sprite = this.Sprite;
            copy.MaxStackSize = this.MaxStackSize;
            copy.CurrentStackSize = 0;
            return copy;
        }
        
    }
}