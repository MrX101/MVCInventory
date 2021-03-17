using UnityEngine;
using Game.Inventory;
using Game.Inventory.GUI;

namespace Game.Units
{
    public class Player : MonoBehaviour
    {
        [SerializeField] private Inventory.Inventory _inventory = new Inventory.Inventory();


        private void Start()
        {
            //which Inventory do we use the one here or one in InventoryControllerGUI?
            _inventory.ContainerSettings = GlobalInventoryControllerGUI.Instance.PlayerContainersSettings;
            _inventory.Initialize();
            GlobalInventoryControllerGUI.Instance.InitPlayerInventory(_inventory);
        }

    }
}

