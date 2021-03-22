using UnityEngine;
using Game.Inventory;
using Game.Inventory.GUI;

namespace Game.Units
{
    public class Player : MonoBehaviour
    {
        [SerializeField]private Inventory.Inventory _inventory = new Inventory.Inventory();
        [SerializeField]private GlobalInventoryControllerGUI _inventoryControllerGUI;
        [SerializeField]private ItemsManager _itemsManager;

        private void Start()
        {
            //which Inventory do we use the one here or one in InventoryControllerGUI?
            _inventory.ContainerSettings = _inventoryControllerGUI.PlayerContainersSettings;
            _inventory.ItemManager = _itemsManager;
            _inventory.Initialize();
            _inventoryControllerGUI.InitPlayerInventory(_inventory);
        }

    }
}

