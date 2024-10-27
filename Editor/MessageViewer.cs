using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using UObject = UnityEngine.Object;

namespace GBG.EditorMessages.Editor
{
    public class MessageViewer : EditorWindow
    {
        private static MessageViewer _sourcelessInstance;
        private static Dictionary<object, MessageViewer> _sourcedInstanceDict;

        public static MessageViewer Open(IList<Message> messages, object source, string sourceName)
        {
            if (source == null)
            {
                if (!_sourcelessInstance)
                {
                    _sourcelessInstance = CreateInstance<MessageViewer>();
                    _sourcelessInstance.titleContent = new GUIContent("Message Viewer");
                    _sourcelessInstance._sourceless = true;
                }

                _sourcelessInstance.SetMessages(messages);
                _sourcelessInstance.Show();
                _sourcelessInstance.Focus();
                return _sourcelessInstance;
            }

            _sourcedInstanceDict ??= new Dictionary<object, MessageViewer>();
            if (!_sourcedInstanceDict.TryGetValue(source, out MessageViewer viewer) || !viewer)
            {
                viewer = CreateInstance<MessageViewer>();
                _sourcedInstanceDict[source] = viewer;
            }

            viewer.titleContent = new GUIContent($"Message Viewer({sourceName ?? source})");
            viewer.Source = source;
            viewer.SetMessages(messages);
            viewer.Show();
            viewer.Focus();
            return viewer;
        }

        public static MessageViewer Open(object source, string sourceName)
        {
            return Open(null, source, sourceName);
        }


        private bool _createGuiEnd;
        private ToolbarToggle _lineNumberToggle;
        private ToolbarToggle _timestampToggle;
        private ToolbarSearchField _searchField;
        private ToolbarToggle _regexToggle;
        private MessageTypeToggle _infoMessageToggle;
        private MessageTypeToggle _warningMessageToggle;
        private MessageTypeToggle _errorMessageToggle;
        private ListView _messageListView;
        private Label _messageDetailsLabel;

        public object Source { get; private set; }
        public IList<Message> Messages { get; private set; }

        private bool _sourceless;
        private readonly List<Message> _filteredMessageList = new List<Message>();
        private Action<Message> _customDataHandler;


        #region Serialized Fields

        [SerializeField]
        [HideInInspector]
        private bool _showLineNumber;
        [SerializeField]
        [HideInInspector]
        private bool _showTimestamp;
        [SerializeField]
        [HideInInspector]
        private string _searchPattern;
        [SerializeField]
        [HideInInspector]
        private bool _useRegex;
        [SerializeField]
        [HideInInspector]
        private bool _showInfoMessages = true;
        [SerializeField]
        [HideInInspector]
        private bool _showWarningMessages = true;
        [SerializeField]
        [HideInInspector]
        private bool _showErrorMessage = true;

        #endregion


        #region Unity Messages

        private void OnEnable()
        {
            minSize = new Vector2(250, 150);

            _createGuiEnd = false;
            if (_sourceless) // Used for restore status after reload assemeblies
            {
                if (!_sourcelessInstance)
                {
                    _sourcelessInstance = this;
                }
                else if (_sourcelessInstance != this)
                {
                    Debug.LogError("_sourcelessInstance != this", this);
                }
            }
        }

        private void CreateGUI()
        {
            // Toolbar
            Toolbar toolbar = new Toolbar();
            rootVisualElement.Add(toolbar);

            // TODO TAG

            // Line Number Toggle
            _lineNumberToggle = new ToolbarToggle
            {
                value = _showLineNumber,
                text = "#",
                tooltip = "Show Line Number",
                style =
                {
                    flexShrink = 0,
                }
            };
            _lineNumberToggle.RegisterValueChangedCallback(OnLineNumberToggleChanged);
            toolbar.Add(_lineNumberToggle);

            // Timestamp Toggle
            _timestampToggle = new ToolbarToggle
            {
                value = _showTimestamp,
                text = "T",
                tooltip = "Show Timestamp",
                style =
                {
                    flexShrink = 0,
                }
            };
            _timestampToggle.RegisterValueChangedCallback(OnTimestampToggleChanged);
            toolbar.Add(_timestampToggle);


            // Search Field
            _searchField = new ToolbarSearchField
            {
                value = _searchPattern,
                style =
                {
                    flexGrow = 1,
                    flexShrink = 1,
                    marginRight = 4,
                }
            };
            _searchField.RegisterValueChangedCallback(OnSearchPatternChanged);
            toolbar.Add(_searchField);

            // Regex Toggle
            _regexToggle = new ToolbarToggle
            {
                value = _useRegex,
                text = ".*",
                tooltip = "Use Regular Expression",
            };
            _regexToggle.RegisterValueChangedCallback(OnRegexToggleChanged);
            toolbar.Add(_regexToggle);

            // Message Toggle Container
            VisualElement typeToggleContainer = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    flexShrink = 0,
                }
            };
            toolbar.Add(typeToggleContainer);

            // Info Message Toggle
            _infoMessageToggle = new MessageTypeToggle(_showInfoMessages);
            _infoMessageToggle.SetMessageType(MessageType.Info, 0);
            _infoMessageToggle.RegisterValueChangedCallback(OnMessageTypeToggleChanged);
            typeToggleContainer.Add(_infoMessageToggle);

            // Warning Message Toggle
            _warningMessageToggle = new MessageTypeToggle(_showWarningMessages);
            _warningMessageToggle.SetMessageType(MessageType.Warning, 0);
            _warningMessageToggle.RegisterValueChangedCallback(OnMessageTypeToggleChanged);
            typeToggleContainer.Add(_warningMessageToggle);

            // Error Message Toggle
            _errorMessageToggle = new MessageTypeToggle(_showErrorMessage);
            _errorMessageToggle.SetMessageType(MessageType.Error, 0);
            _errorMessageToggle.RegisterValueChangedCallback(OnMessageTypeToggleChanged);
            typeToggleContainer.Add(_errorMessageToggle);


            // Vertical Split View
            TwoPaneSplitView splitView = new TwoPaneSplitView(0, 200, TwoPaneSplitViewOrientation.Vertical);
            rootVisualElement.Add(splitView);

            // Message List Container
            VisualElement messageListContainer = new VisualElement
            {
                name = "message-list-container",
                style =
                {
                    minHeight = 50,
                }
            };
            splitView.Add(messageListContainer);

            // Message Details Container
            VisualElement messageDetailsContainer = new VisualElement
            {
                name = "message-details-container",
                style =
                {
                    minHeight = 50,
                }
            };
            splitView.Add(messageDetailsContainer);

            // Message List View
            _messageListView = new ListView(_filteredMessageList)
            {
                makeItem = MakeMessageElement,
                bindItem = BindMessageElement,
                unbindItem = UnbindMessageElement,
                showAlternatingRowBackgrounds = AlternatingRowBackground.ContentOnly,
                style =
                {
                    flexGrow = 1,
                }
            };
            _messageListView.selectionChanged += OnSelectedMessageChanged;
            messageListContainer.Add(_messageListView);

            // Message Details Label
            _messageDetailsLabel = new Label
            {
                enableRichText = true,
                selection = { isSelectable = true, },
                style =
                {
                    flexGrow = 1,
                }
            };
            messageDetailsContainer.Add(_messageDetailsLabel);

            _createGuiEnd = true;
            Refresh();
        }

        private void Update()
        {
            TryClose();
        }

        private void OnDisable()
        {
            if (Source != null)
            {
                _sourcedInstanceDict.Remove(Source);
            }
        }

        #endregion


        public void SetMessages(IList<Message> messages)
        {
            Messages = messages;
            Refresh();
        }

        public void SetCustomDataHandler(Action<Message> handler)
        {
            _customDataHandler = handler;
        }

        public void Refresh()
        {
            if (!_createGuiEnd)
            {
                return;
            }

            Messages.CountByType(out int infoCount, out int warningCount, out int errorCount);
            _infoMessageToggle.SetMessageCount(infoCount);
            _warningMessageToggle.SetMessageCount(warningCount);
            _errorMessageToggle.SetMessageCount(errorCount);

            CalcMaxLineNumberLabelWidth();
            FilterMessages();
        }

        private void FilterMessages()
        {
            _messageListView.ClearSelection();
            _filteredMessageList.Clear();
            if (Messages == null)
            {
                RebuildMessageListView();
                return;
            }

            bool noSearchPattern = string.IsNullOrEmpty(_searchPattern);
            for (int i = 0; i < Messages.Count; i++)
            {
                Message message = Messages[i];
                switch (message.Type)
                {
                    case MessageType.Info:
                        if (!_infoMessageToggle.value)
                        {
                            continue;
                        }

                        break;
                    case MessageType.Warning:
                        if (!_warningMessageToggle.value)
                        {
                            continue;
                        }

                        break;
                    case MessageType.Error:
                        if (!_errorMessageToggle.value)
                        {
                            continue;
                        }

                        break;
                    default: throw new ArgumentOutOfRangeException(nameof(message.Type), message.Type, null);
                }

                if (noSearchPattern)
                {
                    _filteredMessageList.Add(message);
                }
                else if (string.IsNullOrEmpty(message.Context))
                {
                    continue;
                }
                else if (_useRegex)
                {
                    try
                    {
                        if (Regex.IsMatch(message.Content, _searchPattern, RegexOptions.IgnoreCase))
                        {
                            _filteredMessageList.Add(message);
                        }
                    }
                    catch
                    {
                        // ignored
                    }
                }
                else if (message.Content.Contains(_searchPattern, StringComparison.OrdinalIgnoreCase))
                {
                    _filteredMessageList.Add(message);
                }
            }

            RebuildMessageListView();
        }

        private void TryClose()
        {
            if (_sourcelessInstance == this)
            {
                return;
            }

            if (Source == null)
            {
                Close();
                return;
            }

            if (Source is UObject unitySource && !unitySource)
            {
                Close();
            }
        }


        #region List View

        private int _maxLineNumberWidth;


        private void RebuildMessageListView()
        {
            _messageListView.Rebuild();
            if (_messageListView.itemsSource.Count == 0)
            {
                _messageListView.Q<Label>(className: BaseListView.emptyLabelUssClassName).style.display = DisplayStyle.None;
            }
        }

        private void CalcMaxLineNumberLabelWidth()
        {
            _maxLineNumberWidth = 0;
            if (Messages != null)
            {
                int digit = Messages.Count;
                while (digit > 0)
                {
                    digit /= 10;
                    _maxLineNumberWidth++;
                }

                _maxLineNumberWidth *= 8;
            }
        }

        private VisualElement MakeMessageElement()
        {
            MessageElement messageElement = new MessageElement();
            messageElement.WantsToProcessCustomData += OnWantsToProcessCustomData;

            return messageElement;
        }

        private void BindMessageElement(VisualElement element, int index)
        {
            MessageElement messageElement = (MessageElement)element;
            messageElement.ShowTimestamp = _showTimestamp;
            Message message = _filteredMessageList[index];
            int lineNumber = _showLineNumber ? index + 1 : -1;
            messageElement.SetMessage(message, lineNumber, _maxLineNumberWidth);
        }

        private void UnbindMessageElement(VisualElement element, int index)
        {
        }

        private void OnSelectedMessageChanged(IEnumerable<object> _)
        {
            Message selectedMessage = _messageListView.selectedItem as Message;
            _messageDetailsLabel.text = selectedMessage?.Content;
        }

        private void OnWantsToProcessCustomData(Message message)
        {
            if (_customDataHandler != null)
            {
                _customDataHandler(message);
                return;
            }

            Debug.LogError($"Custom data handler is not registered: {message}", message.GetUnityContextObject());
        }

        #endregion


        #region UI Callback

        private void OnLineNumberToggleChanged(ChangeEvent<bool> evt)
        {
            _showLineNumber = _lineNumberToggle.value;
            if (_showLineNumber)
            {
                CalcMaxLineNumberLabelWidth();
            }

            RebuildMessageListView();
        }

        private void OnTimestampToggleChanged(ChangeEvent<bool> evt)
        {
            _showTimestamp = _timestampToggle.value;
            RebuildMessageListView();
        }

        private void OnSearchPatternChanged(ChangeEvent<string> evt)
        {
            _searchPattern = _searchField.value;
            FilterMessages();
        }

        private void OnRegexToggleChanged(ChangeEvent<bool> evt)
        {
            _useRegex = _regexToggle.value;
            FilterMessages();
        }

        private void OnMessageTypeToggleChanged(ChangeEvent<bool> evt)
        {
            _showInfoMessages = _infoMessageToggle.value;
            _showWarningMessages = _warningMessageToggle.value;
            _showErrorMessage = _errorMessageToggle.value;

            FilterMessages();
        }

        #endregion
    }
}