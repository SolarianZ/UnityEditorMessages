using UnityEngine.UIElements;

namespace GBG.EditorMessages.Editor
{
    public class MessageDetailsElement : VisualElement
    {
        private readonly VisualElement _typeToggleContainer;
        private readonly MessageDetailsTabElement _messageTab;
        private readonly MessageDetailsTabElement _contextTab;
        private readonly MessageDetailsTabElement _customDataTab;
        private readonly Label _detailsLabel;


        public MessageDetailsElement()
        {
            float iconSize = EditorMessageUtility.GlobalIconSize;

            style.flexDirection = FlexDirection.Row;
            style.flexGrow = 1;
            style.minHeight = iconSize * 3 + 2;


            #region Details Type Toggle

            // Type Toggle Container
            _typeToggleContainer = new VisualElement
            {
                style =
                {
                    backgroundColor = EditorMessageUtility.InactiveColor,
                    width = iconSize + 2,
                    paddingLeft = 1,
                    //paddingRight = 1,
                    paddingTop = 1,
                    paddingBottom = 1,
                }
            };
            Add(_typeToggleContainer);

            _messageTab = new MessageDetailsTabElement(EditorMessageUtility.GetInfoIcon(),
                "Message", OnClickMessageTab);
            _typeToggleContainer.Add(_messageTab);

            _contextTab = new MessageDetailsTabElement(EditorMessageUtility.GetContextIcon(),
                "Context", OnClickContextTab);
            _typeToggleContainer.Add(_contextTab);

            _customDataTab = new MessageDetailsTabElement(EditorMessageUtility.GetCustomDataIcon(),
                "Custom Data", OnClickCustomDataTab);
            _typeToggleContainer.Add(_customDataTab);

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
            ScrollView detailsScrollView = new ScrollView(ScrollViewMode.Vertical);
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


        private void OnClickMessageTab()
        {
            _messageTab.style.backgroundColor = EditorMessageUtility.ActiveColor;
            _contextTab.style.backgroundColor = EditorMessageUtility.InactiveColor;
            _customDataTab.style.backgroundColor = EditorMessageUtility.InactiveColor;
        }

        private void OnClickContextTab()
        {
            _messageTab.style.backgroundColor = EditorMessageUtility.InactiveColor;
            _contextTab.style.backgroundColor = EditorMessageUtility.ActiveColor;
            _customDataTab.style.backgroundColor = EditorMessageUtility.InactiveColor;
        }

        private void OnClickCustomDataTab()
        {
            _messageTab.style.backgroundColor = EditorMessageUtility.InactiveColor;
            _contextTab.style.backgroundColor = EditorMessageUtility.InactiveColor;
            _customDataTab.style.backgroundColor = EditorMessageUtility.ActiveColor;
        }
    }
}
