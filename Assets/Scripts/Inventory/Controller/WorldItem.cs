using System.Collections;
using UnityEngine;

namespace Game.Inventory
{
    public class WorldItem : MonoBehaviour
    {
        private IItem _item;

        public IItem Item
        {
            get => _item;
            set => StoreItem(value);
        }

        public void StoreItem(IItem item)
        {
            _item = item;
        }
        
        public IItem TakeItem()
        {
            StartCoroutine(DeleteSelfNextFrame());
            return Item;
        }

        IEnumerator DeleteSelfNextFrame()
        {
            yield return null;
            //todo replace with return to pool
            Destroy(this.gameObject);
        }
    }
}