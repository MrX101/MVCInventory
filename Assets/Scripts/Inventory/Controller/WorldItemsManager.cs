using System.Collections.Generic;
using UnityEngine;

namespace Game.Inventory
{
    [CreateAssetMenu(fileName = "WorldItemsManager", menuName = "Create WorldItemsManager", order = 0)]
    public class WorldItemsManager : ScriptableObject
    {
        public static WorldItemsManager Singleton;
        [SerializeField]private WorldItem _itemPrefab;

        private List<WorldItem> _worldItems = new List<WorldItem>();
        private void Awake()
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
    
    
        private WorldItem GetPrefabInstance(Vector3 position, Quaternion rotation)
        {
            return Instantiate(_itemPrefab, position, rotation);
        } 
    
        private void ReturnPrefabInstance(WorldItem worldItem)
        {
            //todo return to pool
        }
    }
}