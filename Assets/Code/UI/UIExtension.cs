using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Code.UI
{
    public static class UIExtension
    {
        public static async UniTask RebuildChildren(RectTransform root)
        {
            await UniTask.DelayFrame(1);
            RectTransform[] layouts = root.GetComponentsInChildren<RectTransform>();
            Array.Reverse(layouts);
            foreach (RectTransform rect in layouts)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(root);
        }
    }
}