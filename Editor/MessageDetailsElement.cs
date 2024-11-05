using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace GBG.EditorMessages.Editor
{
    public class MessageDetailsElement : VisualElement
    {
        private readonly VisualElement _typeToggleContainer;
        private readonly Image _messageToggle;
        private readonly Label _detailsLabel;


        public MessageDetailsElement()
        {
            float iconSize = EditorMessageUtility.GlobalIconSize;

            style.flexDirection = FlexDirection.Row;
            style.minHeight = iconSize * 3 + 2;


            #region Details Type Toggle

            // Type Toggle Container
            _typeToggleContainer = new VisualElement
            {
                style =
                {
                    width = iconSize + 2,
                    //paddingLeft = 1,
                    //paddingRight = 1,
                    //paddingTop = 1,
                    //paddingBottom = 1,
                }
            };
            Add(_typeToggleContainer);

            Color inactiveColor = EditorGUIUtility.isProSkin ? new Color(1f, 1f, 1f, 0.15f) : new Color(1f, 1f, 1f, 0.26f);

            // Message Toggle
            _messageToggle = new Image
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
            _typeToggleContainer.Add(_messageToggle);

            // Context Toggle
            Image contextToggle = new Image
            {
                image = EditorMessageUtility.GetContextIcon(),
                style =
                {
                    alignSelf = Align.Center,
                    minWidth = iconSize,
                    maxWidth = iconSize,
                    minHeight = iconSize,
                    maxHeight = iconSize,
                    backgroundColor = inactiveColor,
                }
            };
            _typeToggleContainer.Add(contextToggle);

            // Custom Data Toggle
            Image customDataToggle = new Image
            {
                image = EditorMessageUtility.GetCustomDataIcon(),
                style =
                {
                    alignSelf = Align.Center,
                    minWidth = iconSize,
                    maxWidth = iconSize,
                    minHeight = iconSize,
                    maxHeight = iconSize,
                    backgroundColor = inactiveColor,
                }
            };
            _typeToggleContainer.Add(customDataToggle);

            #endregion


            #region Details Content

            // Details Container
            VisualElement detailsContainer = new VisualElement();
            Add(detailsContainer);

            // Message Details Scroll
            ScrollView detailsScrollView = new ScrollView(ScrollViewMode.Vertical)
            {
                style =
                {
                    flexGrow = 1,
                }
            };
            detailsContainer.Add(detailsScrollView);

            // Message Details Label
            _detailsLabel = new Label
            {
                enableRichText = true,
                selection = { isSelectable = true, },
                style =
                {
                    flexGrow = 1,
                    whiteSpace = WhiteSpace.Normal,
                }
            };
            detailsScrollView.Add(_detailsLabel);

            #endregion
        }

        public void SetMessage(Message message)
        {
            _detailsLabel.text = message?.Content;
        }
    }
}
