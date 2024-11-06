using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace GBG.EditorMessages.Editor
{
    public class MessageDetailsTabElement : VisualElement
    {
        public Image Icon { get; }


        public MessageDetailsTabElement(Texture texture, string tooltip, Action onClick)
        {
            float iconSize = EditorMessageUtility.GlobalIconSize;
            style.height = iconSize;
            this.tooltip = tooltip;

            Icon = EditorMessageUtility.NewImage(texture);
            Add(Icon);

            RegisterCallback<ClickEvent>(evt => onClick?.Invoke());
        }
    }
}
