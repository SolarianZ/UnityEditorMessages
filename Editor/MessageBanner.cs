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
        public Label MessageLabel { get; }
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
        public int CurrentMessageIndex { get; private set; }


        public MessageBanner(object source, string sourceName, bool showMessageTypeCount = true)
            : this(null, source, sourceName, showMessageTypeCount) { }

        /// <summary>
        /// 消息横幅。
        /// </summary>
        /// <param name="messages">消息列表。</param>
        /// <param name="source">调用源。双击横幅时，作为打开的消息查看器的调用源。</param>
        /// <param name="sourceName">调用源的名字。</param>
        /// <param name="showMessageTypeCount">是否显示各类型消息的计数。</param>
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


            float iconSize = EditorMessageUtility.GlobalIconSize;

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

            MessageLabel = new Label
            {
                style =
                {
                    flexGrow = 1,
                    flexShrink = 1,
                    marginRight = 2,
                    minWidth = 100,
                    overflow = Overflow.Hidden,
                    textOverflow = TextOverflow.Ellipsis,
                    unityTextAlign = TextAnchor.MiddleLeft,
                    unityFontDefinition = new StyleFontDefinition(EditorMessageUtility.GetMonospaceFontAsset()),
                    //transitionDuration = new List<TimeValue>
                    //{
                    //    new TimeValue(_messageContentTransitionDuration, TimeUnit.Millisecond)
                    //},
                    //transitionTimingFunction = new List<EasingFunction>
                    //{
                    //    new EasingFunction(EasingMode.EaseInOut)
                    //},
                    //transitionProperty = new List<StylePropertyName>
                    //{
                    //    new StylePropertyName("scale"),
                    //},
                }
            };
            Add(MessageLabel);

            InfoTypeImage = CreateMessageTypeImage(EditorMessageUtility.GetInfoIcon(true), iconSize);
            Add(InfoTypeImage);

            InfoCountLabel = CreateMessageTypeCountLabel();
            Add(InfoCountLabel);

            WarningTypeImage = CreateMessageTypeImage(EditorMessageUtility.GetWarningIcon(true), iconSize);
            Add(WarningTypeImage);

            WarningCountLabel = CreateMessageTypeCountLabel();
            Add(WarningCountLabel);

            ErrorTypeImage = CreateMessageTypeImage(EditorMessageUtility.GetErrorIcon(true), iconSize);
            Add(ErrorTypeImage);

            ErrorCountLabel = CreateMessageTypeCountLabel();
            Add(ErrorCountLabel);

            RegisterCallback<ClickEvent>(OnClick);
            RegisterCallback<ContextClickEvent>(OnContextClick);

            InitializeMessageSwitch();
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
                    unityFontDefinition = new StyleFontDefinition(EditorMessageUtility.GetMonospaceFontAsset()),
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
            CurrentMessageIndex = (Messages?.Count ?? 0) - 1;

            Message message = CurrentMessageIndex > -1 ? Messages[CurrentMessageIndex] : null;
            SetMessage(message);

            Messages.CountByType(out int infoCount, out int warningCount, out int errorCount);
            SetMessageCount(MessageType.Info, infoCount);
            SetMessageCount(MessageType.Warning, warningCount);
            SetMessageCount(MessageType.Error, errorCount);
            RefreshMessageTypeCountDisplay();
        }

        private void SetMessage(Message message)
        {
            TypeImage.image = message != null ? EditorMessageUtility.GetMessageTypeIcon(message.type) : null;
            MessageLabel.text = message?.message;
            MessageLabel.tooltip = message?.message;
        }

        private void SetMessageCount(MessageType messageType, int count)
        {
            string text = count > 999 ? "999+" : count.ToString();
            bool inactive = count < 1;
            switch (messageType)
            {
                case MessageType.Info:
                    InfoCountLabel.text = text;
                    InfoTypeImage.image = EditorMessageUtility.GetInfoIcon(inactive);
                    break;
                case MessageType.Warning:
                    WarningCountLabel.text = text;
                    WarningTypeImage.image = EditorMessageUtility.GetWarningIcon(inactive);
                    break;
                case MessageType.Error:
                    ErrorCountLabel.text = text;
                    ErrorTypeImage.image = EditorMessageUtility.GetErrorIcon(inactive);
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


        #region Message Transition

        private uint _messageSwitchInterval;
        /// <summary>
        /// 消息轮播时的切换间隔（毫秒）。
        /// 若为0，则不自动切换消息。
        /// </summary>
        public uint MessageSwitchInterval
        {
            get => _messageSwitchInterval;
            set
            {
                if (_messageSwitchInterval == value)
                {
                    return;
                }

                bool prevDisabled = IsMessageSwitchDisabled();
                _messageSwitchInterval = value;
                if (!IsMessageSwitchDisabled() && prevDisabled)
                {
                    InitializeMessageSwitch();
                }
            }
        }

        public bool IsMessageSwitchDisabled()
        {
            return MessageSwitchInterval < 1;
        }


        private void InitializeMessageSwitch()
        {
            if (IsMessageSwitchDisabled())
            {
                return;
            }

            schedule.Execute(SwitchToNextMessage)
                    .Every(MessageSwitchInterval)
                    .StartingIn(MessageSwitchInterval)
                    .Until(IsMessageSwitchDisabled);
        }

        private void SwitchToNextMessage()
        {
            if ((Messages?.Count ?? 0) < 1)
            {
                CurrentMessageIndex = -1;
                return;
            }

            CurrentMessageIndex++;
            if (CurrentMessageIndex == Messages.Count)
            {
                CurrentMessageIndex = 0;
            }

            SetMessage(Messages[CurrentMessageIndex]);
        }

        #endregion
    }
}
