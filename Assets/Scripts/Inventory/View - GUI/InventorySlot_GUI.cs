using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.Inventory.GUI
{
    public class InventorySlot_GUI : MonoBehaviour, IDropHandler
    {
        protected InventoryItemGUI _item;
        protected SlotIdentifier _slotId;
        protected Action<SlotIdentifier> OnItemDroppedEvent;
        protected Action<SlotIdentifier> OnItemRemovedEvent;

        protected RectTransform _rectTransform;
        
        protected RawImage _backgroundImage;
        protected RawImage _foregroundImage;
        
        protected readonly Color _transparent = new Color(1f, 1f, 1f, 0f);
        [Header("Icon Settings - while no Item")]
        [SerializeField]protected Color _iconColor = Color.white;
        [SerializeField]protected Texture _icon;

        protected void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _backgroundImage = GetComponent<RawImage>();
            _foregroundImage = transform.GetChild(0).GetComponent<RawImage>();
            UpdateIcon();
        }

        private void Start()
        {
            if (_foregroundImage.texture == null)
            {
                _foregroundImage.color = _transparent;
            }
        }

        public SlotIdentifier SlotId
        {
            get => _slotId;
            set => _slotId = value;
        }

        private void OnEnable()
        {
            OnItemDroppedEvent += GlobalInventoryControllerGUI.Instance.ItemDroppedIn;
        }

        private void OnDisable()
        {
            OnItemDroppedEvent -= GlobalInventoryControllerGUI.Instance.ItemDroppedIn;
        }

        public void OnDrop(PointerEventData eventData)
        {
            if (_slotId == null)
            {
                Debug.Log("Error this slot "+ gameObject.name +" does not have it's slotId set correctly");
            }
            OnItemDroppedEvent?.Invoke(_slotId);
        }

        /// <summary>
        /// Creates an Item if it currently does not have an item
        /// </summary>
        /// <param name="slotId"></param>
        /// <param name="itemInfo"></param>
        public void UpdateItemInfoAndLocation(SlotIdentifier slotId,  ItemInfo itemInfo)
        {
            if (_rectTransform == null)
            {
                Awake();
            }
            Internal_UpdateItemInfoAndLocation( slotId, itemInfo);
        }

        protected void CreateItem()
        {
            _item = GlobalInventoryControllerGUI.Instance.GetGUIItem();
            _item.transform.root.SetParent(_rectTransform.parent); 
            _item.OnBeginDragEvent += GlobalInventoryControllerGUI.Instance.SetAsDragged;
        }

        protected void Internal_UpdateItemInfoAndLocation(SlotIdentifier slotId,  ItemInfo itemInfo)
        {
            if (itemInfo == ItemInfo.NULL)
            {
                if (_item != null) { DestroyItem(); }
                _item = null;
                OnItemRemovedEvent?.Invoke(_slotId);
                UpdateIcon();
                return;
            }
            if (_item == null) { CreateItem(); }
            _item.SetItemInfo(itemInfo);
            _item.PlaceInSlot(_rectTransform, slotId);
            SetIcon(null, _transparent);
        }

        private void UpdateIcon()
        {
            if (_icon == null)
            {
                SetIcon(null, _transparent);
            }
            else
            {
                SetIcon(_icon, _iconColor);
            }
        }

        protected void DestroyItem()
        {
            _item.DestroySelf();
            _item = null;
        }

        public void SetIcon(Texture texture, Color color)
        {
            _foregroundImage.texture = texture;
            _foregroundImage.color = color;
        }
        
        public void ReturnItemToSlot()
        {
            _item.ReturnToSlot();
        }
        
        public InventoryItemGUI TakeItem()
        {
            var temp = _item;
            _item = null;
            OnItemRemovedEvent?.Invoke(_slotId);
            return temp;
        }

        private void OnValidate()
        {
            if (_foregroundImage == null)
            {
                _foregroundImage = transform.GetChild(0).GetComponent<RawImage>();
            }
            UpdateIcon();
        }
    }
}