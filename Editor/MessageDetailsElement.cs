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
            style.flexGrow = 1;
            style.minHeight = iconSize * 3 + 2;


            #region Details Type Toggle

            Color inactiveColor = EditorGUIUtility.isProSkin ? new Color(1f, 1f, 1f, 0.15f) : new Color(1f, 1f, 1f, 0.26f);

            // Type Toggle Container
            _typeToggleContainer = new VisualElement
            {
                style =
                {
                    backgroundColor = inactiveColor,
                    width = iconSize + 2,
                    paddingLeft = 1,
                    //paddingRight = 1,
                    paddingTop = 1,
                    paddingBottom = 1,
                }
            };
            Add(_typeToggleContainer);

            MessageDetailsTabElement toggle1 = new MessageDetailsTabElement
            {
                style =
                {
                    height = iconSize,
                }
            };
            _typeToggleContainer.Add(toggle1);

            MessageDetailsTabElement toggle2 = new MessageDetailsTabElement
            {
                style =
                {
                    height = iconSize,
                    backgroundColor = EditorGUIUtility.isProSkin ? new Color(0.22f, 0.22f, 0.22f, 1f) : new Color(0.76f, 0.76f, 0.76f, 1f),
                }
            };
            _typeToggleContainer.Add(toggle2);

            MessageDetailsTabElement toggle3 = new MessageDetailsTabElement
            {
                style =
                {
                    height = iconSize,
                }
            };
            _typeToggleContainer.Add(toggle3);

            #endregion


            #region Details Content

            // Details Container
            VisualElement detailsContainer = new VisualElement
            {
                style =
                {
                    flexGrow = 1,
                }
            };
            Add(detailsContainer);

            // Message Details Scroll
            ScrollView detailsScrollView = new ScrollView(ScrollViewMode.Vertical)
            {
                style =
                {
                    //flexGrow = 1,
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
