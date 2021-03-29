using System;
using UnityEngine;

namespace Game.GUI.Tooltip
{
    [CreateAssetMenu(fileName = "TooltipController", menuName = "SO/Create TooltipController", order = 0)]
    public class TooltipController : ScriptableObject
    {
       
        [HideInInspector]public Tooltip _tooltip;
        [HideInInspector]public ItemTooltip _itemTooltip;
        

        public void ShowItemTooltip(string itemName,int amount, string description, Sprite icon )
        {
            _itemTooltip.EnableToolTip();
            _itemTooltip.UpdateTooltip(itemName,amount, description, icon);
        }
        
        public void HideItemTooltip()
        {
            _itemTooltip.DisableToolTip();
        }

    }
}