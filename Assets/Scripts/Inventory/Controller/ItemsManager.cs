using System.Collections.Generic;
using Game.Inventory.GUI;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace Game.Inventory
{
    [CreateAssetMenu(fileName = "WorldItemsManager", menuName = "Create ItemsManager", order = 0)]
    public class ItemsManager : ScriptableSingleton<ItemsManager>
    {
        [SerializeField]private WorldItem _worldItemPrefab;
        
        [SerializeField]private InventoryItemGUI _inventoryItemPrefab;
        [SerializeField]private InventorySlot_GUI _inventorySlotPrefab;
        [SerializeField]private InventoryEquipSlot_GUI _inventoryEquipSlotPrefab;

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

        public InventoryItemGUI GetGUIItem()
        {
            return Instantiate(_inventoryItemPrefab);
        }

        public InventorySlot_GUI GetInventorySlot()
        {
            return Instantiate(_inventorySlotPrefab);
        }

        public InventoryEquipSlot_GUI GetEquipSlot()
        {
            return Instantiate(_inventoryEquipSlotPrefab);
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