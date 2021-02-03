using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Inventory.GUI
{
    public class InventoryContainerGUI : MonoBehaviour
    {
        private RectTransform _rectTransform;
        private string _containerId;
        private GridLayoutGroup _layoutGroup;
        
        private Vector2 TargetSlotSize = new Vector2(45f,45f);
        private List<InventorySlotGUI> _inventorySlots = new List<InventorySlotGUI>();

        public string ContainerId
        {
            get => _containerId;
            set => _containerId = value;
        }


        private void Awake()
        {
            _layoutGroup = GetComponent<GridLayoutGroup>();
            _rectTransform = GetComponent<RectTransform>();
            
            //todo remove this at a later point
            CreateSlots(new ContainerSettings{ Identifier = "container1",NumberOfSlots = 24});
        }

        public void CreateSlots(ContainerSettings settings)
        {
            var existingInvSlots = GetComponentsInChildren<InventorySlotGUI>();
            
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

            //code about resizing GUI...probably delete it all
            // var width = _rectTransform.rect.size.x - _layoutGroup.padding.left - _layoutGroup.padding.right;
            // var height = _rectTransform.rect.size.y -_layoutGroup.padding.top - _layoutGroup.padding.bottom;
            // // var totalxSpacing = _layoutGroup.spacing.x * _inventorySlots.Count;
            // // var totalySpacing = _layoutGroup.spacing.y * _inventorySlots.Count;
            // width -= totalxSpacing;
            // height -= totalySpacing;
            // if (width < 0 || height < 0)
            // {
            //     Debug.Log("ERROR: Container:" + _containerId+", is smaller than total padding. Please Fix");
            // }
            //
            // var columnCount = Mathf.Abs(width / TargetSlotSize.x);
            // var rowCount = Mathf.Abs(height / TargetSlotSize.y);
            // if((columnCount * rowCount) > _inventorySlots.Count)
            // {
            //     //desired slots fits into desired cellSize awesome!
            //     _layoutGroup.cellSize = TargetSlotSize;
            // }
            // else
            // {
            //     //todo calculate size that fits.
            //     // Vector2 cellsize = new Vector2(width / settings.NumberOfSlots, height / settings.NumberOfSlots);
            //     // if (CalculatePercentChange(cellsize.x, _layoutGroup.cellSize.x) > 20f)
            //     // {
            //     //     Debug.Log("Cell Size  for: " + _containerId +" ,was changed considrably, please check if it looks awful");
            //     // }
            //     // _layoutGroup.cellSize = cellsize;
            // }

            void AddExistingSlots()
            {
                for (int i = 0; i < existingInvSlots.Length; i++)
                {
                    _inventorySlots.Add(existingInvSlots[i]);
                    existingInvSlots[i].SlotId = new SlotIdentifier(_containerId, 1);
                }
            }

            void SpawnNeededSlots()
            {
                if (numOfSlotToCreate > 0)
                {
                    for (int i = 0; i < numOfSlotToCreate; i++)
                    {
                        InventorySlotGUI inventorySlot =
                            Instantiate(InventoryControllerGUI.Singleton.SlotPrefab, parent: this.transform);
                        inventorySlot.SlotId = new SlotIdentifier(_containerId, i);
                        _inventorySlots.Add(inventorySlot);
                    }
                }
            }
        }
        
        public static float CalculatePercentChange(float current, float previous)
        {
            return ((current - previous) / previous)*100f;
        }

        public void UpdateSlot(int slotIndex, ItemInfo itemInfo)
        {
            _inventorySlots[slotIndex].UpdateItem(itemInfo); 
        }
    }
    
}