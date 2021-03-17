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
        public static ItemsManager _instance;
        
        public static ItemsManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    var itemsFound = Resources.FindObjectsOfTypeAll<ItemsManager>();
                    if (itemsFound.Length == 0)
                    {
                        Debug.Log(nameof(ItemsManager)+" Not Found");
                    }
                    else if (itemsFound.Length > 1)
                    {
                        Debug.Log("Too many "+ nameof(ItemsManager)+" Found");
                    }
                    else
                    {
                        _instance = itemsFound[0];
                    }
                }
                return _instance;
            }
        }
        
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