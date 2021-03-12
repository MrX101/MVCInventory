using System;
using System.Collections.Generic;
using UnityEngine;
using Game.Inventory;
using Game.Inventory.GUI;
using NaughtyAttributes;

namespace Game.Units
{
    public class Player : MonoBehaviour
    {
        [SerializeField] private Inventory.Inventory _inventory = new Inventory.Inventory();


        private void Start()
        {
            //which Inventory do we use the one here or one in InventoryControllerGUI?
            _inventory.ContainerSettings = GlobalInventoryControllerGUI.instance.PlayerContainersSettings;
            _inventory.Initialize();
            GlobalInventoryControllerGUI.instance.InitPlayerInventory(_inventory);
        }

    }
}

