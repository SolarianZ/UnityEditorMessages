using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace GBG.EditorMessages.Editor
{
    public class MessageBanner : VisualElement
    {
        public Image TypeImage { get; }
        public Label ContentLabel { get; }
        public Image InfoTypeImage { get; }
        public Label InfoCountLabel { get; }
        public Image WarningTypeImage { get; }
        public Label WarningCountLabel { get; }
        public Image ErrorTypeImage { get; }
        public Label ErrorCountLabel { get; }

        private bool _showMessageTypeCount;
        public bool ShowMessageTypeCount
        {
            get => _showMessageTypeCount;
            set
            {
                if (_showMessageTypeCount == value)
                    return;

                _showMessageTypeCount = value;
                RefreshMessageTypeCountDisplay();
            }
        }

        public object Source { get; private set; }
        public string SourceName { get; private set; }
        public IList<Message> Messages { get; private set; }


        public MessageBanner(object source, string sourceName, bool showMessageTypeCount = true)
            : this(null, source, sourceName, showMessageTypeCount) { }

        public MessageBanner(IList<Message> messages, object source, string sourceName,
            bool showMessageTypeCount = true)
        {
            _showMessageTypeCount = showMessageTypeCount;
            Messages = messages;
            Source = source;
            SourceName = sourceName;

            style.flexDirection = FlexDirection.Row;
            style.paddingLeft = 4;
            style.paddingRight = 4;
            style.height = 20;


            float iconSize = 16;

            TypeImage = new Image
            {
                style =
                {
                    alignSelf = Align.Center,
                    minWidth = iconSize,
                    maxWidth = iconSize,
                    minHeight = iconSize,
                    maxHeight = iconSize,
                }
            };
            Add(TypeImage);

            ContentLabel = new Label
            {
                style =
                {
                    flexGrow = 1,
                    flexShrink = 1,
                    marginRight = 2,
                    minWidth = 100,
                    overflow = Overflow.Hidden,
                    unityTextAlign = TextAnchor.MiddleLeft,
                    unityFontDefinition = new StyleFontDefinition(ResCache.GetMonospaceFontAsset()),
                }
            };
            Add(ContentLabel);

            InfoTypeImage = CreateMessageTypeImage(ResCache.GetInfoIcon(true), iconSize);
            Add(InfoTypeImage);

            InfoCountLabel = CreateMessageTypeCountLabel();
            Add(InfoCountLabel);

            WarningTypeImage = CreateMessageTypeImage(ResCache.GetWarningIcon(true), iconSize);
            Add(WarningTypeImage);

            WarningCountLabel = CreateMessageTypeCountLabel();
            Add(WarningCountLabel);

            ErrorTypeImage = CreateMessageTypeImage(ResCache.GetErrorIcon(true), iconSize);
            Add(ErrorTypeImage);

            ErrorCountLabel = CreateMessageTypeCountLabel();
            Add(ErrorCountLabel);

            RegisterCallback<ClickEvent>(OnClick);
            RegisterCallback<ContextClickEvent>(OnContextClick);

        }

        private Image CreateMessageTypeImage(Texture defaultIcon, float iconSize)
        {
            Image image = new Image
            {
                image = defaultIcon,
                style =
                {
                    display = ShowMessageTypeCount ? DisplayStyle.Flex : DisplayStyle.None,
                    alignSelf = Align.Center,
                    minWidth = iconSize,
                    maxWidth = iconSize,
                    minHeight = iconSize,
                    maxHeight = iconSize,
                }
            };
            return image;
        }

        private Label CreateMessageTypeCountLabel()
        {
            Label label = new Label
            {
                text = "0",
                style =
                {
                    display = ShowMessageTypeCount ? DisplayStyle.Flex : DisplayStyle.None,
                    //flexShrink = 0,
                    marginLeft = -3,
                    marginRight = -3,
                    paddingLeft = 0,
                    paddingRight = 0,
                    overflow = Overflow.Hidden,
                    unityTextAlign = TextAnchor.MiddleCenter,
                    unityFontDefinition = new StyleFontDefinition(ResCache.GetMonospaceFontAsset()),
                }
            };
            return label;
        }


        public void SetMessages(IList<Message> messages)
        {
            Messages = messages;
            Refresh();
        }

        public void Refresh()
        {
            Message message = (Messages?.Count ?? 0) > 0 ? Messages[Messages.Count - 1] : null;
            SetMessage(message);

            Messages.CountByType(out int infoCount, out int warningCount, out int errorCount);
            SetMessageCount(MessageType.Info, infoCount);
            SetMessageCount(MessageType.Warning, warningCount);
            SetMessageCount(MessageType.Error, errorCount);
            RefreshMessageTypeCountDisplay();
        }

        private void SetMessage(Message message)
        {
            TypeImage.image = message != null ? ResCache.GetMessageTypeIcon(message.Type) : null;
            ContentLabel.text = message?.Content;
            ContentLabel.tooltip = message?.Content;
        }

        private void SetMessageCount(MessageType messageType, int count)
        {
            string text = count > 999 ? "999+" : count.ToString();
            bool inactive = count < 1;
            switch (messageType)
            {
                case MessageType.Info:
                    InfoCountLabel.text = text;
                    InfoTypeImage.image = ResCache.GetInfoIcon(inactive);
                    break;
                case MessageType.Warning:
                    WarningCountLabel.text = text;
                    WarningTypeImage.image = ResCache.GetWarningIcon(inactive);
                    break;
                case MessageType.Error:
                    ErrorCountLabel.text = text;
                    ErrorTypeImage.image = ResCache.GetErrorIcon(inactive);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(messageType), messageType, null);
            }
        }

        private void RefreshMessageTypeCountDisplay()
        {
            DisplayStyle display = ShowMessageTypeCount ? DisplayStyle.Flex : DisplayStyle.None;
            InfoTypeImage.style.display = display;
            InfoCountLabel.style.display = display;
            WarningTypeImage.style.display = display;
            WarningCountLabel.style.display = display;
            ErrorTypeImage.style.display = display;
            ErrorCountLabel.style.display = display;
        }

        private void OnClick(ClickEvent evt)
        {
            if (evt.clickCount == 2)
            {
                MessageViewer.Open(Messages, Source, SourceName);
            }
        }

        private void OnContextClick(ContextClickEvent evt)
        {
            GenericMenu menu = new GenericMenu();

            // Open Message Viewer
            menu.AddItem(new GUIContent("Open Message Viewer"), false, () => MessageViewer.Open(Messages, Source, SourceName));

            menu.ShowAsContext();
        }
    }
}
