using System;
using UnityEngine;

namespace Game.Inventory
{
    [Serializable]
    public class BaseItem : IItem
    {
        [SerializeField]private string _name;
        [SerializeField]private Sprite _sprite;
        [SerializeField]private string _description;
        [SerializeField]private string _uniqueId = Guid.NewGuid().ToString().Substring(0,5);
        [SerializeField]private ItemType _itemType = ItemType.Gun;
        [SerializeField]private int _maxStackSize = 1;
        [SerializeField]private int _currentStackSize = 1;

        #region props
        public string Name
        {
            get => _name;
            set => _name = value;
        }

        public string Description
        {
            get => _description;
            set => _description = value;
        }

        public Sprite Sprite
        {
            get => _sprite;
            set => _sprite = value;
        }

        public string UniqueId
        {
            get => _uniqueId;
            set => _uniqueId = value;
        }

        public int MaxStackSize
        {
            get => _maxStackSize;
            set => _maxStackSize = value;
        }

        public int CurrentStackSize
        {
            get => _currentStackSize;
            set => _currentStackSize = value;
        }

        public ItemType ItemType
        {
            get => _itemType;
            set => _itemType = value;
        }

        #endregion
        
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
            copy.ItemType = this.ItemType;
            copy.Sprite = this.Sprite;
            copy.MaxStackSize = this.MaxStackSize;
            copy.CurrentStackSize = 0;
            return copy;
        }
        
    }
}