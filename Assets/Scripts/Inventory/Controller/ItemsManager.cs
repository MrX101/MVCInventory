using System.Collections.Generic;
using Game.Inventory.GUI;
using UnityEngine;
using UnityEngine.Serialization;

namespace Game.Inventory
{
    [CreateAssetMenu(fileName = "WorldItemsManager", menuName = "Create ItemsManager", order = 0)]
    public class ItemsManager : ScriptableObject
    {
        public static ItemsManager Singleton;
        [SerializeField]private WorldItem _worldItemPrefab;
        [SerializeField]private InventoryItemGUI _inventoryItemPrefab;

        private List<WorldItem> _worldItems = new List<WorldItem>();
        
        private void OnEnable()
        {
            Singleton = this;
            
        }

        public void TakeItem( WorldItem worldItem, Inventory userInventory)
        {
            var item = worldItem.TakeItem();
            userInventory.StoreItem(ref item, out var info);
            ReturnPrefabInstance(worldItem);
        }
    
        public void CreateWorldItem(IItem item, Vector3 position, Quaternion rotation)
        {
            var worldItem = GetPrefabInstance(position, rotation);
            _worldItems.Add(worldItem);
        }


        public InventoryItemGUI CreateGUIItem()
        {
            return Instantiate(_inventoryItemPrefab);
        }
        
        private WorldItem GetPrefabInstance(Vector3 position, Quaternion rotation)
        {
            return Instantiate(_worldItemPrefab, position, rotation);
        } 
    
        private void ReturnPrefabInstance(WorldItem worldItem)
        {
            //todo return to pool
        }
    }
}