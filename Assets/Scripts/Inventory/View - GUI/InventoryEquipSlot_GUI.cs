using UnityEngine.EventSystems;

namespace Game.Inventory.GUI
{
    public class InventoryEquipSlot_GUI : InventorySlot_GUI
    {
        private void OnEnable()
        {
            OnItemDroppedEvent += GlobalInventoryControllerGUI.instance.EquipItemDroppedIn;
        }

        private void OnDisable()
        {
            OnItemDroppedEvent -= GlobalInventoryControllerGUI.instance.EquipItemDroppedIn;
        }
    }
}