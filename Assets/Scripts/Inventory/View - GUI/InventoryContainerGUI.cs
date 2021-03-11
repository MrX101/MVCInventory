using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Inventory.GUI
{
    public class InventoryContainerGUI : MonoBehaviour
    {
        // [Header("Select what type of Container this will be")]
        // [SerializeField] public GUIContainerSlot _containerSelection;
        
        private RectTransform _rectTransform;
        [SerializeField]private string _containerId;
        private GridLayoutGroup _layoutGroup;
        
        private Vector2 TargetSlotSize = new Vector2(45f,45f);
        private List<InventorySlot_GUI> _inventorySlots = new List<InventorySlot_GUI>();

        public string ContainerId
        {
            get => _containerId;
            set => _containerId = value;
        }

        private void Awake()
        {
            _layoutGroup = GetComponent<GridLayoutGroup>();
            _rectTransform = GetComponent<RectTransform>();
        }

        public void Init(ContainerSettings settings)
        {
            _containerId = settings.Identifier;
            if (_rectTransform == null)
            {
                Awake();
            }
            CreateSlots(settings);
            GlobalInventoryControllerGUI.instance.RegisterContainer(this);
        }

        private void CreateSlots(ContainerSettings settings)
        {
            var existingInvSlots = GetComponentsInChildren<InventorySlot_GUI>();
            
            if (existingInvSlots.Length > settings.NumberOfSlots)
            {
                for (int i = settings.NumberOfSlots; i < existingInvSlots.Length; i++)
                {
                    Destroy(existingInvSlots[i].gameObject);
                }
            }
            var numOfSlotToCreate = settings.NumberOfSlots - existingInvSlots.Length;
            AddExistingSlots();
            SpawnNeededSlots();
            _layoutGroup.enabled = true; //WARNING GridLayoutGroup Must be disabled in inspector, since it causes all items to have positon/size of 0,0,0 in first frame(for unknown reasons).
                                         //Which will cause the items to be set to the wrong place when intially created and placed to ItemSlot Locations.
            AddItems();
            
            void AddExistingSlots()
            {
                for (int i = 0; i < existingInvSlots.Length; i++)
                {
                    _inventorySlots.Add(existingInvSlots[i]);
                    existingInvSlots[i].SlotId = new SlotIdentifier(_containerId, i);
                }
            }

            void SpawnNeededSlots()
            {
                if (numOfSlotToCreate > 0)
                {
                    for (int i = 0; i < numOfSlotToCreate; i++)
                    {
                        InventorySlot_GUI inventorySlot =
                            Instantiate(GlobalInventoryControllerGUI.instance.SlotPrefab, parent: this.transform);
                        inventorySlot.SlotId = new SlotIdentifier(_containerId, i);
                        _inventorySlots.Add(inventorySlot);
                    }
                }
            }

            void AddItems()
            {
                var slotIndex = 0;
                foreach (var itemSettings in settings.ItemsToCreate)
                {
                    UpdateItemInfo(new SlotIdentifier(settings.Identifier, slotIndex), new ItemInfo(itemSettings.Item));
                    slotIndex++;
                }
            }
        }
        
        //What were we going to use this for again??
        public static float CalculatePercentChange(float current, float previous)
        {
            return ((current - previous) / previous)*100f;
        }

        /// <summary>
        /// Will Create GUI Items if the slot being updated doesn't have one.
        /// </summary>
        /// <param name="slotId"></param>
        /// <param name="itemInfo"></param>
        public void UpdateItemInfo(SlotIdentifier slotId, ItemInfo itemInfo)
        {
            GetSlot(slotId.SlotIndex).UpdateItemInfoAndLocation(slotId, itemInfo); 
        }

        public void ReturnItemToSlot(int slotIndex)
        {
            GetSlot(slotIndex).ReturnItemToSlot();
        }

        public InventoryItemGUI TakeItem(int slotIndex)
        {
            return GetSlot(slotIndex).TakeItem();
        }

        private InventorySlot_GUI GetSlot(int slotIndex)
        {
            return _inventorySlots[slotIndex];
        }
    }
    
}