using System;
using UnityEngine;
using Game.Inventory;
using Game.Inventory.GUI;
using NaughtyAttributes;

namespace Game.Units
{
    public class Unit : MonoBehaviour
    {
        [SerializeField] private Inventory.Inventory _inventory = new Inventory.Inventory();
        [SerializeField]private InventoryContainerGUI _containerGUI;


        private void Awake()
        {
            //which Inventory do we use the one here or one in InventoryControllerGUI?
            _inventory.Initialize();
            InventoryControllerGUI.Singleton.SetInventory(_inventory);
            _containerGUI.Init(_inventory._containersToCreate[0]);
        }

        private void OnValidate()
        {
            foreach (var containerSetting in _inventory._containersToCreate)
            {
                containerSetting.OnValidate();
            }
        }

        [Button("SetItems")]
        private void AddItems()
        {
            foreach (var containerSetting in _inventory._containersToCreate)
            {
                containerSetting.SetItems();
            }
        }
        
    }
}

