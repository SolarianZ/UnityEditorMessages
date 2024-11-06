using System;
using UnityEngine.UIElements;

namespace GBG.EditorMessages.Editor
{
    public class MessageDetailsElement : VisualElement
    {
        private readonly MessageDetailsTabElement _messageTab;
        private readonly MessageDetailsTabElement _contextTab;
        private readonly MessageDetailsTabElement _customDataTab;
        private readonly Label _contentLabel;
        private Message _message;


        public MessageDetailsElement()
        {
            float iconSize = EditorMessageUtility.GlobalIconSize;

            style.flexDirection = FlexDirection.Row;
            style.flexGrow = 1;
            style.minHeight = iconSize * 3 + 2;


            #region Details Type Toggle

            // Type Toggle Container
            VisualElement tabContainer = new VisualElement
            {
                style =
                {
                    backgroundColor = EditorMessageUtility.TabBackgroundColor,
                    width = iconSize + 2,
                    paddingLeft = 1,
                    //paddingRight = 1,
                    paddingTop = 1,
                    paddingBottom = 1,
                }
            };
            Add(tabContainer);

            _messageTab = new MessageDetailsTabElement(EditorMessageUtility.GetInfoIcon(),
                "Message", OnClickMessageTab);
            tabContainer.Add(_messageTab);

            _contextTab = new MessageDetailsTabElement(EditorMessageUtility.GetContextIcon(),
                "Context", OnClickContextTab);
            tabContainer.Add(_contextTab);

            _customDataTab = new MessageDetailsTabElement(EditorMessageUtility.GetCustomDataIcon(),
                "Custom Data", OnClickCustomDataTab);
            tabContainer.Add(_customDataTab);

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
            _contentLabel = new Label
            {
                enableRichText = true,
                selection = { isSelectable = true, },
                style =
                {
                    flexGrow = 1,
                    whiteSpace = WhiteSpace.Normal,
                }
            };
            detailsScrollView.Add(_contentLabel);

            #endregion
        }

        public void SetMessage(Message message)
        {
            _message = message;

            MessageType messageType = message?.type ?? MessageType.Info;
            switch (messageType)
            {
                case MessageType.Info:
                    _messageTab.Icon.image = EditorMessageUtility.GetInfoIcon();
                    break;
                case MessageType.Warning:
                    _messageTab.Icon.image = EditorMessageUtility.GetWarningIcon();
                    break;
                case MessageType.Error:
                    _messageTab.Icon.image = EditorMessageUtility.GetErrorIcon();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(messageType), messageType, null);
            }

            _contextTab.style.display = string.IsNullOrEmpty(message?.context)
                ? DisplayStyle.None : DisplayStyle.Flex;
            _customDataTab.style.display = string.IsNullOrEmpty(message?.customData)
                ? DisplayStyle.None : DisplayStyle.Flex;

            OnClickMessageTab();
        }


        private void OnClickMessageTab()
        {
            _contentLabel.text = _message?.message;

            _messageTab.style.backgroundColor = EditorMessageUtility.TabActiveColor;
            _contextTab.style.backgroundColor = EditorMessageUtility.TabInactiveColor;
            _customDataTab.style.backgroundColor = EditorMessageUtility.TabInactiveColor;
        }

        private void OnClickContextTab()
        {
            _contentLabel.text = _message?.context;

            _messageTab.style.backgroundColor = EditorMessageUtility.TabInactiveColor;
            _contextTab.style.backgroundColor = EditorMessageUtility.TabActiveColor;
            _customDataTab.style.backgroundColor = EditorMessageUtility.TabInactiveColor;
        }

        private void OnClickCustomDataTab()
        {
            _contentLabel.text = _message?.customData;

            _messageTab.style.backgroundColor = EditorMessageUtility.TabInactiveColor;
            _contextTab.style.backgroundColor = EditorMessageUtility.TabInactiveColor;
            _customDataTab.style.backgroundColor = EditorMessageUtility.TabActiveColor;
        }
    }
}
