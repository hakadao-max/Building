using System.Collections.Generic;
using UnityEngine;

namespace Test
{
    public class MTWorldTipPanel : MTPanelBase
    {
        public WorldTipItem template;

        private List<WorldTipItem> unactiveItemList = new List<WorldTipItem>();

        public Dictionary<int, WorldTipItem> aliveItemMap = new Dictionary<int, WorldTipItem>();

        protected override void OnHide()
        {
            template.gameObject.SetActive(false);
        }

        public void ShowWorldTip(int hash, string title, string content, Vector3 pos, Quaternion rot, Transform target = null)
        {
            var item = GetAliveItem();
            item.transform.SetParent(transform);
            item.Show(title, content, pos, rot, target);
            aliveItemMap[hash] = item;
        }

        public void HideWorldItem(int hash)
        {
            if (aliveItemMap.TryGetValue(hash, out var item))
            {
                item.Hide();
                unactiveItemList.Add(item);
            }
        }

        private WorldTipItem GetAliveItem()
        {
            if (unactiveItemList.Count > 0)
            {
                var target = unactiveItemList[^1];
                unactiveItemList.RemoveAt(unactiveItemList.Count - 1);
                return target;
            }

            return Instantiate(template);
        }
    }
}