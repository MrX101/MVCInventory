using System;
using UnityEngine;

namespace Game.Inventory
{
    //This class represents an inventory slot in any container, it simply gives control/info of itself to the containerClasses
    //,since it does not know enough information to make decisions itself.
    
    //Everything is private/protected and then changed with functions/methods
    //to ensure that the appropriate actions can be done when a change occurs. Such as event triggers and possible logging etc.
    [Serializable]
    public class DataInventorySlot
    {
        protected IItem _item;
        protected int _containerIndex;
        protected int _slotIndex;
        
        protected bool _isEquipped = false;

        public int ContainerIndex => _containerIndex;
        public int SlotIndex => _slotIndex;

        ///Will be null if nothing is stored/equipped
        public Action<IItem> OnItemStoredChanged;
        //Not sure if useful
        public Action<int> OnContainerIndexChanged;
        public Action<bool, IItem> OnItemEquippedStatus;

        public DataInventorySlot(int slotIndex)
        {
            this._slotIndex = slotIndex;
        }
        
        
        public bool HasItem()
        {
            return (_item != null);
        }

        public void SetEquipped()
        {
            if (_isEquipped) { return; }
            _isEquipped = false;
            OnItemEquippedStatus?.Invoke(_isEquipped, _item);
        }
        

        public IItem TakeItem()
        {
            var item = _item;
            _item = null;
            OnItemStoredChanged?.Invoke(_item);
            return item;
        }

        public void SetUnEquipped()
        {
            if (!_isEquipped) { return; }
            _isEquipped = true;
            OnItemEquippedStatus?.Invoke(_isEquipped, _item);
        }
        
        ///Make Duplicate First if needed for storing the stacks.
        public void TakePartial(ref IItem itemToStack, int stackAmount)
        {
            _item.CurrentStackSize -= stackAmount;
            itemToStack.CurrentStackSize = stackAmount;
            OnItemStoredChanged?.Invoke(_item);
        }

        public void ConsumeStack(int slotIndex, int amount)
        {
            _item.CurrentStackSize -= amount;
            if (_item.CurrentStackSize <= 0)
            {
                DeleteItem();
            }
        }

        public IItem MakeDuplicate()
        {
            return _item.CreateEmptyDuplicate();
        }

        public void StoreItem( ref IItem itemRef)
        {
            _item = itemRef;
            OnItemStoredChanged?.Invoke(_item);
        }
        

        ///True is none left. Otherwise currentstacksize of itemRef will be reduced.
        public bool StackItem( ref IItem itemRef)
        {
            _item.CurrentStackSize += itemRef.CurrentStackSize;
            if (_item.CurrentStackSize < _item.MaxStackSize)
            {
                itemRef.CurrentStackSize = _item.CurrentStackSize - _item.MaxStackSize;
                _item.CurrentStackSize = itemRef.MaxStackSize;
                OnItemStoredChanged?.Invoke(_item);
                return false;
            }
            itemRef = null;
            OnItemStoredChanged?.Invoke(_item);
            return true;
        }
        
        public void DeleteItem()
        {
            _item.DeleteSelf();
            _item = null;
            OnItemStoredChanged?.Invoke(_item);
        }

        public int GetCurrentStackSize()
        {
            return _item.CurrentStackSize;
        }
        
        public int GetMaxStackSize()
        {
            return _item.MaxStackSize;
        }

        public int GetRemainingStackSize()
        {
            return (_item.MaxStackSize -_item.CurrentStackSize);
        }
        
        public string GetUniqueId()
        {
            return _item.UniqueId;
        }
        
        public string GetName()
        {
            return _item.Name;
        }
        
        public string GetDescription()
        {
            return _item.UniqueId;
        }
        
        public Sprite GetSprite()
        {
            return _item.Sprite;
        }
        
        public ItemType GetItemType()
        {
            return _item.GetItemType();
        }
        
        public void SetContainerIndex(int index)
        {
            _containerIndex = index;
            OnContainerIndexChanged?.Invoke(_containerIndex);
        }
    }
}