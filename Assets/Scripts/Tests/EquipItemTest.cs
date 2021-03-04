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
            inventory.ContainerSettings = settingsList;
            inventory.Initialize();
            //inventory.DebugShowAllItems();
            
            IItem item1 = new BaseItem{Name = "item1", UniqueId = "item1_Id", MaxStackSize = 1, CurrentStackSize = 1};
            var isItemStored = inventory.StoreItem(ref item1, out var info1);
            var isItemEquipped = inventory.EquipItem(new SlotIdentifier("1",0), new SlotIdentifier("2", 0));
            //inventory.DebugShowAllItems();

            var info = inventory.GetContainerInfo("2");
            var invSlot = info.InventorySlots[0];
            if (invSlot.HasItem && invSlot.Item.UniqueId == item1.UniqueId)
            {
                correctEquipSlot = true;
            }
            
            info = inventory.GetContainerInfo("1");
            invSlot = info.InventorySlots[0];
            if (!invSlot.HasItem)
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
            bool isFilledInvSlot = false;
            bool isEmptyEquipSlot = false;

            List<ContainerSettings> settingsList = new List<ContainerSettings>
            {
                new ContainerSettings{Identifier = "1", Type = ContainerType.Storage, NumberOfSlots = 5},
                new ContainerSettings{Identifier = "2", Type = ContainerType.Equiptment, NumberOfSlots = 5}
            };
            inventory.ContainerSettings = settingsList;
            inventory.Initialize();
            //inventory.DebugShowAllItems();
            
            IItem item1 = new BaseItem{Name = "item1", UniqueId = "item1_Id", MaxStackSize = 1, CurrentStackSize = 1};
            var isItemStored = inventory.StoreItem(ref item1, out var outInfo1);
            var isItemEquipped = inventory.EquipItem(new SlotIdentifier("1",0), new SlotIdentifier("2", 0));
            inventory.DebugShowAllItems();
            var isItemUnequipped = inventory.UnEquipItem(new SlotIdentifier("2", 0), out var outInfo2);
            //inventory.DebugShowAllItems();

            var info = inventory.GetContainerInfo("1");
            var invSlot = info.InventorySlots[0];
            if (invSlot.HasItem && invSlot.Item.UniqueId == item1.UniqueId)
            {
                isFilledInvSlot = true;
            }
            
            info = inventory.GetContainerInfo("2");
            invSlot = info.InventorySlots[0];
            if (!invSlot.HasItem)
            {
                isEmptyEquipSlot = true;
            }

            Assert.IsTrue(isEmptyEquipSlot && isFilledInvSlot && isItemEquipped && isItemStored && isItemUnequipped);
        }
        
        [Test]
        public void Equip_UnEquipItem_MultipleTimes_True()
        {
            // Use the Assert class to test conditions.
            var inventory = new Game.Inventory.Inventory();
            bool isFilledInvSlot = false;
            bool isEmptyEquipSlot = false;

            List<ContainerSettings> settingsList = new List<ContainerSettings>
            {
                new ContainerSettings{Identifier = "1", Type = ContainerType.Storage, NumberOfSlots = 5},
                new ContainerSettings{Identifier = "2", Type = ContainerType.Equiptment, NumberOfSlots = 5}
            };
            inventory.ContainerSettings = settingsList;
            inventory.Initialize();
            //inventory.DebugShowAllItems();
            
            IItem item1 = new BaseItem{Name = "item1", UniqueId = "item1_Id", MaxStackSize = 1, CurrentStackSize = 1};
            var isItemStored = inventory.StoreItem(ref item1, out var outContainer);
            
            //inventory.DebugShowAllItems();
            
            int numOfTimes = 5;
            bool[] isItemEquippedArray = new bool[numOfTimes];
            bool[] isItemUnequippedArray = new bool[numOfTimes];
            for (int i = 0; i < numOfTimes; i++)
            {
                isItemEquippedArray[i] = inventory.EquipItem(new SlotIdentifier("1",0), new SlotIdentifier("2", 0));
                isItemUnequippedArray[i] = inventory.UnEquipItem(new SlotIdentifier("2", 0), out var outContainer2);
            }
            
            //inventory.DebugShowAllItems();

            bool isItemEquipped = true;
            foreach (var boolToCheck in isItemEquippedArray)
            {
                if (!boolToCheck)
                {
                    isItemEquipped = false;
                }
            }
            
            bool isItemUnequipped = true;
            foreach (var boolToCheck in isItemUnequippedArray)
            {
                if (!boolToCheck)
                {
                    isItemUnequipped = false;
                }
            }
            
            
            var containerInfo = inventory.GetContainerInfo("1");
            SlotInfo slotinfo = containerInfo.InventorySlots[0];
            if (slotinfo.HasItem && slotinfo.Item.UniqueId == item1.UniqueId)
            {
                isFilledInvSlot = true;
            }
            
            containerInfo = inventory.GetContainerInfo("2");
            slotinfo = containerInfo.InventorySlots[0];
            if (!slotinfo.HasItem)
            {
                isEmptyEquipSlot = true;
            }

            Assert.IsTrue(isEmptyEquipSlot && isFilledInvSlot && isItemUnequipped && isItemStored && isItemEquipped);
        }
    }
}