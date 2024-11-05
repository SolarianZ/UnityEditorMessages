using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace GBG.EditorMessages.Editor
{
    public class MessageDetailsTabElement : VisualElement
    {
        public Image Icon { get; }

        public MessageDetailsTabElement()
        {
            float iconSize = EditorMessageUtility.GlobalIconSize;
            //Color inactiveColor = EditorGUIUtility.isProSkin
            //    ? new Color(1f, 1f, 1f, 0.15f)
            //    : new Color(1f, 1f, 1f, 0.26f);

            Icon = new Image
            {
                image = EditorMessageUtility.GetInfoIcon(), // todo : test
                style =
                {
                    alignSelf = Align.Center,
                    //minWidth = iconSize,
                    //maxWidth = iconSize,
                    minHeight = iconSize,
                    maxHeight = iconSize,
                    //backgroundColor = inactiveColor,
                }
            };
            Add(Icon);
        }
    }
}
