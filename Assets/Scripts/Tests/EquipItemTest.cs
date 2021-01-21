using System.Collections;
using System.Collections.Generic;
using Game.Inventory;
using NUnit.Framework;
using UnityEngine.TestTools;
using Assert = UnityEngine.Assertions.Assert;

namespace Game.Test
{
    public class EquipItemTest
    {
        [Test]
        public void EquipItem_FromInventory_True()
        {
            // Use the Assert class to test conditions.
            var inventory = new Game.Inventory.Inventory();
            bool correctEquipSlot = false;
            bool correctEmptySlot = false;

            List<ContainerSettings> settingsList = new List<ContainerSettings>
            {
                new ContainerSettings{Identifier = "1", Type = ContainerType.Storage, NumberOfSlots = 5},
                new ContainerSettings{Identifier = "2", Type = ContainerType.Equiptment, NumberOfSlots = 5}
            };
            inventory._containersToCreate = settingsList;
            inventory.Initialize();
            //inventory.DebugShowAllItems();
            
            IItem item1 = new BaseItem{Name = "item1", UniqueId = "item1_Id", MaxStackSize = 1, CurrentStackSize = 1};
            var isItemStored = inventory.StoreItem(ref item1);
            var isItemEquipped = inventory.EquipItem(new ContainerRequest("1",0), new ContainerRequest("2", 0));
            //inventory.DebugShowAllItems();

            var info = inventory.GetContainerInfo("2");
            var invSlot = info.InventorySlots[0];
            if (invSlot.HasItem() && invSlot.GetUniqueId() == item1.UniqueId)
            {
                correctEquipSlot = true;
            }
            
            info = inventory.GetContainerInfo("1");
            invSlot = info.InventorySlots[0];
            if (!invSlot.HasItem())
            {
                correctEmptySlot = true;
            }

            Assert.IsTrue(correctEmptySlot && correctEquipSlot && isItemEquipped && isItemStored);
        }
        
        [Test]
        public void UnEquipItem_FromInventory_True()
        {
            // Use the Assert class to test conditions.
            var inventory = new Game.Inventory.Inventory();
            bool correctEquipSlot = false;
            bool correctEmptySlot = false;

            List<ContainerSettings> settingsList = new List<ContainerSettings>
            {
                new ContainerSettings{Identifier = "1", Type = ContainerType.Storage, NumberOfSlots = 5},
                new ContainerSettings{Identifier = "2", Type = ContainerType.Equiptment, NumberOfSlots = 5}
            };
            inventory._containersToCreate = settingsList;
            inventory.Initialize();
            //inventory.DebugShowAllItems();
            
            IItem item1 = new BaseItem{Name = "item1", UniqueId = "item1_Id", MaxStackSize = 1, CurrentStackSize = 1};
            var isItemStored = inventory.StoreItem(ref item1);
            var isItemEquipped = inventory.EquipItem(new ContainerRequest("1",0), new ContainerRequest("2", 0));
            //inventory.DebugShowAllItems();
            var isItemUnequipped = inventory.UnEquipItem(new ContainerRequest("2", 0));
            //inventory.DebugShowAllItems();

            var info = inventory.GetContainerInfo("2");
            var invSlot = info.InventorySlots[0];
            if (invSlot.HasItem() && invSlot.GetUniqueId() == item1.UniqueId)
            {
                correctEquipSlot = true;
            }
            
            info = inventory.GetContainerInfo("1");
            invSlot = info.InventorySlots[0];
            if (!invSlot.HasItem())
            {
                correctEmptySlot = true;
            }

            Assert.IsTrue(correctEmptySlot && correctEquipSlot && isItemEquipped && isItemStored);
        }
    }
}