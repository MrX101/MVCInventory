using System.Collections;
using System.Collections.Generic;
using Game.Inventory;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Game.Test
{
    public class InventorySwapItemsTests
    {
        [Test]
        public void InventorySwapItems_SameContainer_True()
        {
            var inventory = new Game.Inventory.Inventory();
            bool correctItemInSlot1 = false;
            bool correctItemInSlot2 = false;

            List<ContainerSettings> settingsList = new List<ContainerSettings>
            {
                new ContainerSettings {Identifier = "1", Type = ContainerType.Storage, NumberOfSlots = 5},
            };
            inventory._containersToCreate = settingsList;
            inventory.Initialize();
            IItem item1 = new BaseItem {Name = "item1", UniqueId = "item1_Id", MaxStackSize = 10, CurrentStackSize = 1};
            IItem item2 = new BaseItem {Name = "item2", UniqueId = "item2_Id", MaxStackSize = 10, CurrentStackSize = 1};
            inventory.StoreItem(ref item1, out var info1);
            inventory.StoreItem(ref item2, out var info2);
            //inventory.DebugShowAllItems();

            var result = inventory.SwapItem(new ContainerRequest("1", 0), new ContainerRequest("1", 1));

            var info = inventory.GetContainerInfo("1");
            var invSlot = info.InventorySlots[0];
            if (invSlot.HasItem && invSlot.Item.UniqueId == item2.UniqueId)
            {
                correctItemInSlot1 = true;
            }

            invSlot = info.InventorySlots[1];
            if (invSlot.HasItem && invSlot.Item.UniqueId == item1.UniqueId)
            {
                correctItemInSlot2 = true;
            }

            Assert.IsTrue(result && correctItemInSlot1 && correctItemInSlot2);
        }


        [Test]
        public void InventorySwapItems_DifferentContainer_True()
        {
            var inventory = new Game.Inventory.Inventory();
            bool correctItemInContainer1 = false;
            bool correctItemInContainer2 = false;

            List<ContainerSettings> settingsList = new List<ContainerSettings>
            {
                new ContainerSettings {Identifier = "1", Type = ContainerType.Storage, NumberOfSlots = 5},
                new ContainerSettings {Identifier = "2", Type = ContainerType.Storage, NumberOfSlots = 5},
            };
            inventory._containersToCreate = settingsList;
            inventory.Initialize();
            IItem item1 = new BaseItem {Name = "item1", UniqueId = "item1_Id", MaxStackSize = 10, CurrentStackSize = 1};
            IItem item2 = new BaseItem {Name = "item2", UniqueId = "item2_Id", MaxStackSize = 10, CurrentStackSize = 1};
            inventory.StoreItem(ref item1, out var info1);
            inventory.StoreItem(ref item2, new ContainerRequest("2", 0), out var info2);
            //inventory.DebugShowAllItems();

            var result = inventory.SwapItem(new ContainerRequest("1", 0), new ContainerRequest("2", 0));
            
            //inventory.DebugShowAllItems();
            
            var info = inventory.GetContainerInfo("1");
            var invSlot = info.InventorySlots[0];
            if (invSlot.HasItem && invSlot.Item.UniqueId == item2.UniqueId)
            {
                correctItemInContainer1 = true;
            }
            
            info = inventory.GetContainerInfo("2");
            invSlot = info.InventorySlots[0];
            if (invSlot.HasItem && invSlot.Item.UniqueId == item1.UniqueId)
            {
                correctItemInContainer2 = true;
            }

            Assert.IsTrue(result && correctItemInContainer1 && correctItemInContainer2);
        }
        
        [Test]
        public void InventorySwapItems_DifferentContainer_EmptySlot_True()
        {
            var inventory = new Game.Inventory.Inventory();
            bool correctItemInContainer1 = false;
            bool correctItemInContainer2 = false;

            List<ContainerSettings> settingsList = new List<ContainerSettings>
            {
                new ContainerSettings {Identifier = "1", Type = ContainerType.Storage, NumberOfSlots = 5},
                new ContainerSettings {Identifier = "2", Type = ContainerType.Storage, NumberOfSlots = 5},
            };
            inventory._containersToCreate = settingsList;
            inventory.Initialize();
            IItem item1 = new BaseItem {Name = "item1", UniqueId = "item1_Id", MaxStackSize = 10, CurrentStackSize = 1};
            inventory.StoreItem(ref item1 , out var info1);
            //inventory.DebugShowAllItems();

            var result = inventory.SwapItem(new ContainerRequest("1", 0), new ContainerRequest("2", 0));
            
            //inventory.DebugShowAllItems();
            
            var info = inventory.GetContainerInfo("1");
            var invSlot = info.InventorySlots[0];
            if (!invSlot.HasItem)
            {
                correctItemInContainer1 = true;
            }
            
            info = inventory.GetContainerInfo("2");
            invSlot = info.InventorySlots[0];
            if (invSlot.HasItem && invSlot.Item.UniqueId == item1.UniqueId)
            {
                correctItemInContainer2 = true;
            }

            Assert.IsTrue(result && correctItemInContainer1 && correctItemInContainer2);
        }
        
        [Test]
        public void InventorySwapItems_ToNonExistantSlot_False()
        {
            
            var inventory = new Game.Inventory.Inventory();
            bool correctItemInContainer1 = false;
            bool correctItemInContainer2 = false;

            List<ContainerSettings> settingsList = new List<ContainerSettings>
            {
                new ContainerSettings {Identifier = "1", Type = ContainerType.Storage, NumberOfSlots = 3},
                new ContainerSettings {Identifier = "2", Type = ContainerType.Storage, NumberOfSlots = 3},
            };
            inventory._containersToCreate = settingsList;
            inventory.Initialize();
            IItem item1 = new BaseItem {Name = "item1", UniqueId = "item1_Id", MaxStackSize = 10, CurrentStackSize = 1};
            IItem item2 = new BaseItem {Name = "item2", UniqueId = "item2_Id", MaxStackSize = 10, CurrentStackSize = 1};
            inventory.StoreItem(ref item1, out var info1);
            inventory.StoreItem(ref item2, new ContainerRequest("2", 0), out var info2);
            //inventory.DebugShowAllItems();

            var result = inventory.SwapItem(new ContainerRequest("1", 0), new ContainerRequest("2", 5));
            
            //inventory.DebugShowAllItems();
            
            var info = inventory.GetContainerInfo("1");
            var invSlot = info.InventorySlots[0];
            if (invSlot.HasItem && invSlot.Item.UniqueId == item1.UniqueId)
            {
                correctItemInContainer1 = true;
            }
            
            info = inventory.GetContainerInfo("2");
            invSlot = info.InventorySlots[0];
            if (invSlot.HasItem && invSlot.Item.UniqueId == item2.UniqueId)
            {
                correctItemInContainer2 = true;
            }
            
            Assert.IsTrue(correctItemInContainer1 && correctItemInContainer2);
            Assert.IsFalse(result);

        }
    }
}

