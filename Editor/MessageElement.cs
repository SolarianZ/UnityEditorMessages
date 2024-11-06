using System;
using System.Globalization;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;

namespace GBG.EditorMessages.Editor
{
    public class MessageElement : VisualElement
    {
        public Label LineNumberLabel { get; }
        public Image TypeImage { get; }
        public Label TimestampLabel { get; }
        public Label TimestampSeparatorLabel { get; }
        public Label MessageLabel { get; }
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
            style.minWidth = 100;

            LineNumberLabel = new Label
            {
                style =
                {
                    flexShrink = 0,
                    marginRight = 2,
                    overflow = Overflow.Hidden,
                    unityTextAlign = TextAnchor.MiddleRight,
                    unityFontDefinition = new StyleFontDefinition(EditorMessageUtility.GetMonospaceFontAsset()),
                }
            };
            Add(LineNumberLabel);

            TypeImage = EditorMessageUtility.NewImage();
            Add(TypeImage);

            TimestampLabel = new Label
            {
                style =
                {
                    paddingRight = 0,
                    unityTextAlign = TextAnchor.MiddleCenter,
                    unityFontDefinition = new StyleFontDefinition(EditorMessageUtility.GetMonospaceFontAsset()),
                }
            };
            Add(TimestampLabel);

            TimestampSeparatorLabel = new Label
            {
                text = "|",
                style =
                {
                    paddingLeft = 0,
                    paddingRight = 4,
                    unityTextAlign = TextAnchor.MiddleLeft,
                    //unityFontDefinition = new StyleFontDefinition(ResCache.GetMonospaceFontAsset()),
                }
            };
            Add(TimestampSeparatorLabel);

            MessageLabel = new Label
            {
                style =
                {
                    flexGrow = 1,
                    flexShrink = 1,
                    overflow = Overflow.Hidden,
                    textOverflow = TextOverflow.Ellipsis,
                    unityTextAlign = TextAnchor.MiddleLeft,
                    unityFontDefinition = new StyleFontDefinition(EditorMessageUtility.GetMonospaceFontAsset()),
                }
            };
            Add(MessageLabel);

            ContextImage = EditorMessageUtility.NewImage(tooltip: "This message has context.");
            Add(ContextImage);

            CustomDataImage = EditorMessageUtility.NewImage(tooltip: "This message has custom data.");
            Add(CustomDataImage);

            RegisterCallback<ClickEvent>(OnClick);
            RegisterCallback<ContextClickEvent>(OnContextClick);
        }

        public void SetMessage(Message message, int lineNumber, int lineNumberLabelWidth = -1)
        {
            Assert.IsTrue(message != null);

            Message = message;
            LineNumber = lineNumber;
            LineNumberLabelWidth = lineNumberLabelWidth;
            TypeImage.image = EditorMessageUtility.GetMessageTypeIcon(message.type);
            MessageLabel.text = message.message;

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

            TimestampLabel.text = new DateTime(Message.timestamp).ToString(CultureInfo.CurrentCulture);
            TimestampLabel.style.display = DisplayStyle.Flex;
            TimestampSeparatorLabel.style.display = DisplayStyle.Flex;
        }

        private void UpdateContextImage()
        {
            if (string.IsNullOrEmpty(Message.context))
            {
                ContextImage.style.display = DisplayStyle.None;
                return;
            }

            ContextImage.image = EditorMessageUtility.GetContextIcon();
            ContextImage.style.display = DisplayStyle.Flex;
        }

        private void UpdateCustomDataImage()
        {
            if (string.IsNullOrEmpty(Message.customData))
            {
                CustomDataImage.style.display = DisplayStyle.None;
                return;
            }

            CustomDataImage.image = EditorMessageUtility.GetCustomDataIcon();
            CustomDataImage.style.display = DisplayStyle.Flex;
        }

        private void OnClick(ClickEvent evt)
        {
            if (evt.clickCount == 1)
            {
                Message.TryPingContextObject(false);
            }
            else if (evt.clickCount == 2 && !string.IsNullOrEmpty(Message?.customData))
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

        private void OnContextClick(ContextClickEvent evt)
        {
            GenericMenu menu = new GenericMenu();

            // Copy Message
            menu.AddItem(new GUIContent("Copy Message"), false, () => EditorGUIUtility.systemCopyBuffer = Message.message);

            // Copy Context
            if (!string.IsNullOrEmpty(Message.context))
            {
                menu.AddItem(new GUIContent("Copy Context"), false, () => EditorGUIUtility.systemCopyBuffer = Message.context);
            }

            // Show Context Object In Scene
            if (!Message.GetUnityContextObject() && Message.TryGetContextOwnerSceneAsset(out SceneAsset sceneAsset))
            {
                menu.AddItem(new GUIContent("Reveal Context Object in Scene"), false, () => Message.TryPingContextObject(true));
            }

            // Copy Custom Data
            if (!string.IsNullOrEmpty(Message.customData))
            {
                menu.AddItem(new GUIContent("Copy Custom Data"), false, () => EditorGUIUtility.systemCopyBuffer = Message.customData);
            }

            menu.ShowAsContext();
        }
    }
}
