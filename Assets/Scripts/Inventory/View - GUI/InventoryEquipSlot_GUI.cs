using UnityEngine.EventSystems;

namespace Game.Inventory.GUI
{
    public class InventoryEquipSlot_GUI : InventorySlot_GUI
    {
        private void OnEnable()
        {
            OnItemDroppedEvent += _inventoryControllerGUI.EquipItemDroppedIn;
        }

        private void OnDisable()
        {
            OnItemDroppedEvent -= _inventoryControllerGUI.EquipItemDroppedIn;
        }
    }
}