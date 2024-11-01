﻿using System;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace GBG.EditorMessages.Editor
{
    public class MessageTypeToggle : ToolbarToggle
    {
        private readonly Image _typeImage;
        private Texture _icon;
        private Texture _iconInactive;


        public MessageTypeToggle(bool value)
        {
            base.value = value;
            _typeImage = new Image
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
            Insert(0, _typeImage);
        }

        public void SetMessageType(MessageType messageType, int messageCount)
        {
            switch (messageType)
            {
                case MessageType.Info:
                    _icon = EditorMessageUtility.GetInfoIcon();
                    _iconInactive = EditorMessageUtility.GetInfoIcon(true);
                    break;

                case MessageType.Warning:
                    _icon = EditorMessageUtility.GetWarningIcon();
                    _iconInactive = EditorMessageUtility.GetWarningIcon(true);
                    break;

                case MessageType.Error:
                    _icon = EditorMessageUtility.GetErrorIcon();
                    _iconInactive = EditorMessageUtility.GetErrorIcon(true);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(messageType), messageType, null);
            }

            SetMessageCount(messageCount);
        }

        public void SetMessageCount(int count)
        {
            text = count > 999 ? "999+" : count.ToString();
            _typeImage.image = count > 0 ? _icon : _iconInactive;
        }
    }
}