using UnityEngine.EventSystems;

namespace Game.Inventory.GUI
{
    public class InventoryEquipSlot_GUI : InventorySlot_GUI
    {
        private void OnEnable()
        {
            OnItemDroppedEvent += GlobalInventoryControllerGUI.Instance.EquipItemDroppedIn;
        }

        private void OnDisable()
        {
            OnItemDroppedEvent -= GlobalInventoryControllerGUI.Instance.EquipItemDroppedIn;
        }
    }
}