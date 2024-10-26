using System;
using System.Globalization;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.TextCore.Text;
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
        //private static Font _monospaceFont;
        private static FontAsset _monospaceFontAsset;

        public Label LineNumberLabel { get; }
        public Image TypeImage { get; }
        public Label TimestampLabel { get; }
        public Label TimestampSeparatorLabel { get; }
        public Label ContentLabel { get; }
        public Image ContextImage { get; }
        public Image CustomDataImage { get; }

        public Message Message { get; private set; }
        public int LineNumber { get; private set; } = -1;
        public int LineNumberLabelWidth { get; set; } = -1;
        public bool ShowTimestamp { get; set; } = true;

        public event Action<Message> WantsToProcessCustomData;


        public MessageElement()
        {
            style.flexDirection = FlexDirection.Row;
            style.paddingLeft = 4;
            style.paddingRight = 4;

            if (!_monospaceFontAsset)
            {
                //_monospaceFont = (Font)EditorGUIUtility.LoadRequired("fonts/robotomono/robotomono-regular.ttf");
                _monospaceFontAsset = (FontAsset)EditorGUIUtility.LoadRequired("fonts/robotomono/robotomono-regular sdf.asset");
            }

            LineNumberLabel = new Label
            {
                style =
                {
                    flexShrink = 0,
                    marginRight = 2,
                    overflow = Overflow.Hidden,
                    unityTextAlign = TextAnchor.MiddleRight,
                    unityFontDefinition = new StyleFontDefinition(_monospaceFontAsset),
                }
            };
            Add(LineNumberLabel);

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

            TimestampLabel = new Label
            {
                style =
                {
                    unityTextAlign = TextAnchor.MiddleCenter,
                    unityFontDefinition = new StyleFontDefinition(_monospaceFontAsset),
                }
            };
            Add(TimestampLabel);

            TimestampSeparatorLabel = new Label
            {
                text = "|",
                style =
                {
                    unityTextAlign = TextAnchor.MiddleLeft,
                    unityFontDefinition = new StyleFontDefinition(_monospaceFontAsset),
                }
            };
            Add(TimestampSeparatorLabel);

            ContentLabel = new Label
            {
                style =
                {
                    flexGrow = 1,
                    flexShrink = 1,
                    overflow = Overflow.Hidden,
                    unityTextAlign = TextAnchor.MiddleLeft,
                    unityFontDefinition = new StyleFontDefinition(_monospaceFontAsset),
                }
            };
            Add(ContentLabel);

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
            Add(ContextImage);

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
            Add(CustomDataImage);

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
            UpdateTimestampLabel();
            UpdateContextImage();
            UpdateCustomDataImage();
        }

        private void UpdateLineNumberLabel()
        {
            if (LineNumber < 0)
            {
                LineNumberLabel.style.display = DisplayStyle.None;
                return;
            }

            LineNumberLabel.text = LineNumber.ToString();
            LineNumberLabel.style.width = LineNumberLabelWidth > 0 ? LineNumberLabelWidth : StyleKeyword.Auto;
            LineNumberLabel.style.display = DisplayStyle.Flex;
        }

        private void UpdateTimestampLabel()
        {
            if (!ShowTimestamp)
            {
                TimestampLabel.style.display = DisplayStyle.None;
                TimestampSeparatorLabel.style.display = DisplayStyle.None;
                return;
            }

            TimestampLabel.text = new DateTime(Message.Timestamp).ToString(CultureInfo.CurrentCulture);
            TimestampLabel.style.display = DisplayStyle.Flex;
            TimestampSeparatorLabel.style.display = DisplayStyle.Flex;
        }

        private void UpdateContextImage()
        {
            if (string.IsNullOrEmpty(Message.Context))
            {
                ContextImage.style.display = DisplayStyle.None;
                return;
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
                CustomDataImage.style.display = DisplayStyle.None;
                return;
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
