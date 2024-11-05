using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace GBG.EditorMessages.Editor
{
    public class DetailsTypeToggle : ToolbarToggle
    {
        private readonly Image _typeImage;
        private Texture _icon;


        public DetailsTypeToggle(bool value)
        {
            base.value = value;

            float iconSize = EditorMessageUtility.GlobalIconSize;

            _typeImage = new Image
            {
                image = EditorMessageUtility.GetInfoIcon(),
                style =
                {
                    alignSelf = Align.Center,
                    minWidth = iconSize,
                    maxWidth = iconSize,
                    minHeight = iconSize,
                    maxHeight = iconSize,
                }
            };
            Insert(0, _typeImage);
        }
    }
}
