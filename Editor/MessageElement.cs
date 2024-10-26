using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;
using UObject = UnityEngine.Object;

namespace GBG.EditorMessages.Editor
{
    public class MessageElement : VisualElement
    {
        private static Texture _infoIcon;
        private static Texture _warningIcon;
        private static Texture _errorIcon;
        private static Texture _contextIcon;
        private static Texture _customDataIcon;

        public Label LineNumberLabel { get; private set; }
        public Image TypeImage { get; }
        public Label ContentLabel { get; }
        public VisualElement SuffixIconContainer { get; }
        public Image ContextImage { get; private set; }
        public Image CustomDataImage { get; private set; }

        public Message Message { get; private set; }
        public int LineNumber { get; private set; } = -1;
        public int LineNumberLabelWidth { get; set; } = -1;

        public event Action<Message> WantsToProcessCustomData;


        public MessageElement()
        {
            style.flexDirection = FlexDirection.Row;
            style.paddingLeft = 4;
            style.paddingRight = 4;

            TypeImage = new Image
            {
                style =
                {
                    alignSelf = Align.Center,
                    minWidth = 16,
                    maxWidth = 16,
                    minHeight = 16,
                    maxHeight = 16,
                }
            };
            Add(TypeImage);

            ContentLabel = new Label
            {
                style =
                {
                    flexGrow = 1,
                    flexShrink = 1,
                    overflow = Overflow.Hidden,
                    unityTextAlign = TextAnchor.MiddleLeft,
                }
            };
            Add(ContentLabel);

            SuffixIconContainer = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    flexShrink = 0,
                }
            };
            Add(SuffixIconContainer);

            RegisterCallback<ClickEvent>(OnClick);
        }

        public void SetMessage(Message message, int lineNumber, int lineNumberLabelWidth = -1)
        {
            Assert.IsTrue(message != null);
            Message = message;
            LineNumber = lineNumber;
            LineNumberLabelWidth = lineNumberLabelWidth;

            Texture typeIcon;
            switch (message.Type)
            {
                case MessageType.Info:
                    if (!_infoIcon)
                    {
                        _infoIcon = EditorGUIUtility.IconContent("console.infoicon").image;
                    }
                    typeIcon = _infoIcon;
                    break;
                case MessageType.Warning:
                    if (!_warningIcon)
                    {
                        _warningIcon = EditorGUIUtility.IconContent("console.warnicon").image;
                    }
                    typeIcon = _warningIcon;
                    break;
                case MessageType.Error:
                    if (!_errorIcon)
                    {
                        _errorIcon = EditorGUIUtility.IconContent("console.erroricon").image;
                    }
                    typeIcon = _errorIcon;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(message.Type), message.Type, null);
            }
            TypeImage.image = typeIcon;
            ContentLabel.text = message.Content;

            UpdateLineNumberLabel();
            UpdateContextImage();
            UpdateCustomDataImage();
        }

        private void UpdateLineNumberLabel()
        {
            if (LineNumber < 0)
            {
                if (LineNumberLabel != null)
                {
                    LineNumberLabel.style.display = DisplayStyle.None;
                }
                return;
            }

            if (LineNumberLabel == null)
            {
                LineNumberLabel = new Label
                {
                    style =
                    {
                        flexShrink = 0,
                        overflow = Overflow.Hidden,
                        unityTextAlign = TextAnchor.MiddleRight,
                    }
                };
                Insert(0, LineNumberLabel);
            }

            LineNumberLabel.text = LineNumber.ToString();
            LineNumberLabel.style.width = LineNumberLabelWidth > 0 ? LineNumberLabelWidth : StyleKeyword.Auto;
            LineNumberLabel.style.display = DisplayStyle.Flex;
        }

        private void UpdateContextImage()
        {
            if (string.IsNullOrEmpty(Message.Context))
            {
                if (ContextImage != null)
                {
                    ContextImage.style.display = DisplayStyle.None;
                }
                return;
            }

            if (ContextImage == null)
            {
                ContextImage = new Image
                {
                    tooltip = "This message has context.",
                    style =
                    {
                        alignSelf = Align.Center,
                        minWidth = 16,
                        maxWidth = 16,
                        minHeight = 16,
                        maxHeight = 16,
                    }
                };
                SuffixIconContainer.Insert(0, ContextImage);
            }

            if (!_contextIcon)
            {
                _contextIcon = EditorGUIUtility.IconContent("gameobject icon").image;
            }

            ContextImage.image = _contextIcon;
            ContextImage.style.display = DisplayStyle.Flex;
        }

        private void UpdateCustomDataImage()
        {
            if (string.IsNullOrEmpty(Message.CustomData))
            {
                if (CustomDataImage != null)
                {
                    CustomDataImage.style.display = DisplayStyle.None;
                }
                return;
            }

            if (CustomDataImage == null)
            {
                CustomDataImage = new Image
                {
                    tooltip = "This message has custom data.",
                    style =
                    {
                        alignSelf = Align.Center,
                        minWidth = 16,
                        maxWidth = 16,
                        minHeight = 16,
                        maxHeight = 16,
                    }
                };
                SuffixIconContainer.Add(CustomDataImage);
            }

            if (!_customDataIcon)
            {
                _customDataIcon = EditorGUIUtility.IconContent("customized").image;
            }

            CustomDataImage.image = _customDataIcon;
            CustomDataImage.style.display = DisplayStyle.Flex;
        }

        private void OnClick(ClickEvent evt)
        {
            if (evt.clickCount == 1)
            {
                UObject context = Message?.GetUnityContextObject();
                if (context)
                {
                    EditorGUIUtility.PingObject(context);
                }
            }

            if (evt.clickCount == 2 && !string.IsNullOrEmpty(Message?.CustomData))
            {
                if (WantsToProcessCustomData != null)
                {
                    WantsToProcessCustomData(Message);
                }
                else
                {
                    Debug.LogError($"Custom data handler is not registered: {Message}", Message.GetUnityContextObject());
                }
            }
        }
    }
}
