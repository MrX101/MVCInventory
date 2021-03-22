using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace Game.Inventory
{
    [CreateAssetMenu(fileName = "WorldItemsManager", menuName = "Create ItemsManager", order = 0)]
    public class ItemsManager : ScriptableObject
    {
        
        [SerializeField]private WorldItem _worldItemPrefab;

        private List<WorldItem> _worldItems = new List<WorldItem>();
        


        public void TakeItem( WorldItem worldItem, Inventory userInventory)
        {
            var item = worldItem.TakeItem();
            userInventory.StoreItemAnywhere(ref item, out var info);
            ReturnWorldItem(worldItem);
        }
    
        public void CreateWorldItem(IItem item, Vector3 position, Quaternion rotation)
        {
            var worldItem = GetWorldItem(position, rotation);
            _worldItems.Add(worldItem);
        }

        private WorldItem GetWorldItem(Vector3 position, Quaternion rotation)
        {
            return Instantiate(_worldItemPrefab, position, rotation);
        } 
    
        private void ReturnWorldItem(WorldItem worldItem)
        {
            Destroy(worldItem.gameObject);
            //todo return to pool
        }
    }
}