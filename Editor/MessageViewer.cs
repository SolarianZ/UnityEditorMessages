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

        /// <summary>
        /// 打开消息查看器窗口。
        /// </summary>
        /// <param name="messages">消息列表。</param>
        /// <param name="source">调用源。当调用源变为null时，消息查看器窗口会自动关闭。若传入null，则消息查看器窗口不会自动关闭。</param>
        /// <param name="sourceName">调用源的名字。会出现在消息查看器窗口标题中。</param>
        /// <returns></returns>
        public static MessageViewer Open(IList<Message> messages, object source, string sourceName,
            bool allowClearMessages = true)
        {
            if (source == null)
            {
                if (!_sourcelessInstance)
                {
                    _sourcelessInstance = CreateInstance<MessageViewer>();
                    _sourcelessInstance.titleContent = new GUIContent("Message Viewer");
                    _sourcelessInstance._sourceless = true;
                }

                _sourcelessInstance._showClearButton = allowClearMessages;
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
            viewer._showClearButton = allowClearMessages;
            viewer.SetMessages(messages);
            viewer.Show();
            viewer.Focus();
            return viewer;
        }

        public static MessageViewer Open(object source, string sourceName, bool allowClearMessages = true)
        {
            return Open(null, source, sourceName, allowClearMessages);
        }


        private bool _createGuiEnd;
        private ToolbarToggle _lineNumberToggle;
        private ToolbarToggle _timestampToggle;
        private DropdownField _tagDropdown;
        private ToolbarSearchField _searchField;
        private Image _regexErrorImage;
        private ToolbarToggle _regexToggle;
        private MessageTypeToggle _infoMessageToggle;
        private MessageTypeToggle _warningMessageToggle;
        private MessageTypeToggle _errorMessageToggle;
        private ToolbarButton _clearButton;
        private ListView _messageListView;
        private MessageDetailsElement _messageDetailsElement;

        public object Source { get; private set; }
        public IList<Message> Messages { get; private set; }

        private bool _sourceless;
        private int _messageCountCache;
        private readonly List<string> _tagList = new List<string>() { TagAll };
        private readonly List<Message> _filteredMessageList = new List<Message>();
        private Action<Message> _customDataHandler;


        #region Serialized Fields

        public const string TagAll = "All";

        [SerializeField]
        [HideInInspector]
        private bool _showLineNumber;
        [SerializeField]
        [HideInInspector]
        private bool _showTimestamp;
        [SerializeField]
        [HideInInspector]
        private string _selectedTag = TagAll;
        [SerializeField]
        [HideInInspector]
        private string _searchPattern = string.Empty;
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
        [SerializeField]
        [HideInInspector]
        private bool _showClearButton;

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
            float iconSize = EditorMessageUtility.GlobalIconSize;


            #region Toolbar

            // Toolbar
            Toolbar toolbar = new Toolbar();
            rootVisualElement.Add(toolbar);

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
                text = "Ts",
                tooltip = "Show Timestamp",
                style =
                {
                    flexShrink = 0,
                }
            };
            _timestampToggle.RegisterValueChangedCallback(OnTimestampToggleChanged);
            toolbar.Add(_timestampToggle);

            // Tag
            _tagDropdown = new DropdownField(_tagList, _selectedTag)
            {
                tooltip = "Filter by Tag",
                formatSelectedValueCallback = item => string.IsNullOrWhiteSpace(item) ? TagAll : item,
                style =
                {
                    flexShrink = 0,
                }
            };
            _tagDropdown.RegisterValueChangedCallback(OnSelectedTagChanged);
            _tagDropdown.Q(className: DropdownField.inputUssClassName).style.minWidth = StyleKeyword.Auto;
            toolbar.Add(_tagDropdown);

            // Search Field
            _searchField = new ToolbarSearchField
            {
                value = _searchPattern,
                style =
                {
                    flexGrow = 1,
                    flexShrink = 1,
                    marginRight = 2,
                }
            };
            _searchField.RegisterValueChangedCallback(OnSearchPatternChanged);
            toolbar.Add(_searchField);

            // Regex Error Image
            _regexErrorImage = EditorMessageUtility.NewImage(EditorMessageUtility.GetErrorIcon(),
                display: DisplayStyle.None);
            toolbar.Add(_regexErrorImage);

            // Regex Toggle
            _regexToggle = new ToolbarToggle
            {
                value = _useRegex,
                text = ".*",
                tooltip = "Use Regular Expression",
                style =
                {
                    marginLeft = 2,
                }
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

            // Clear Button
            _clearButton = new ToolbarImageButton(EditorMessageUtility.GetClearIcon(), ClearMessages)
            {
                tooltip = "Clear All Messages",
                style =
                {
                    width = 22,
                    flexShrink = 0,
                    display = _showClearButton ? DisplayStyle.Flex : DisplayStyle.None,
                }
            };
            toolbar.Add(_clearButton);

            #endregion


            // Vertical Split View
            TwoPaneSplitView splitView = new TwoPaneSplitView(1, 50, TwoPaneSplitViewOrientation.Vertical);
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
                    flexDirection = FlexDirection.Row,
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

            // Message Details View
            _messageDetailsElement = new MessageDetailsElement();
            messageDetailsContainer.Add(_messageDetailsElement);

            _createGuiEnd = true;
            Refresh();
        }

        private void Update()
        {
            if (Messages != null && Messages.Count != _messageCountCache)
            {
                Refresh();
            }

            TryClose();
        }

        private void OnDisable()
        {
            if (Source != null)
            {
                _sourcedInstanceDict.Remove(Source);
            }
        }

        private void ShowButton(Rect pos)
        {
            if (GUI.Button(pos, EditorGUIUtility.IconContent("_Help"), GUI.skin.FindStyle("IconButton")))
            {
                Application.OpenURL("https://github.com/SolarianZ/UnityEditorMessages");
            }
        }

        #endregion


        public void SetMessages(IList<Message> messages)
        {
            Messages = messages;
            ClearFilters(false);
            Refresh();
        }

        public void ClearFilters(bool clearTypeFilter)
        {
            if (!_createGuiEnd)
            {
                _selectedTag = TagAll;
                _searchPattern = string.Empty;
                if (clearTypeFilter)
                {
                    _showInfoMessages = true;
                    _showWarningMessages = true;
                    _showErrorMessage = true;
                }

                return;
            }

            _tagDropdown.value = TagAll;
            _searchField.value = string.Empty;
            if (clearTypeFilter)
            {
                _infoMessageToggle.value = true;
                _warningMessageToggle.value = true;
                _errorMessageToggle.value = true;
            }
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

            _tagList.Clear();
            HashSet<string> tagSet = Messages.CollectTags();
            if (tagSet != null)
            {
                foreach (string tag in tagSet)
                {
                    if (string.IsNullOrWhiteSpace(tag) ||
                        string.Equals(tag, TagAll, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    _tagList.Add(tag);
                }
                _tagList.Sort();
            }
            _tagList.Insert(0, TagAll);

            Messages.CountByType(out int infoCount, out int warningCount, out int errorCount);
            _messageCountCache = infoCount + warningCount + errorCount;
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

            _regexErrorImage.tooltip = null;
            _regexErrorImage.style.display = DisplayStyle.None;
            for (int i = 0; i < Messages.Count; i++)
            {
                Message message = Messages[i];
                if (!TestMessageType(message) || !TestMessageTag(message) || !TestMessageSearchPattern(message))
                {
                    continue;
                }

                _filteredMessageList.Add(message);
            }

            RebuildMessageListView();
        }

        private bool TestMessageType(Message message)
        {
            switch (message.type)
            {
                case MessageType.Info:
                    return _showInfoMessages;

                case MessageType.Warning:
                    return _showWarningMessages;

                case MessageType.Error:
                    return _showErrorMessage;

                default:
                    throw new ArgumentOutOfRangeException(nameof(message.type), message.type, null);
            }

        }

        private bool TestMessageTag(Message message)
        {
            return string.IsNullOrWhiteSpace(_selectedTag) ||
                string.Equals(_selectedTag, TagAll, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(_selectedTag, message.tag, StringComparison.OrdinalIgnoreCase);
        }

        private bool TestMessageSearchPattern(Message message)
        {
            bool noSearchPattern = string.IsNullOrEmpty(_searchPattern);
            if (noSearchPattern)
            {
                return true;
            }

            if (string.IsNullOrEmpty(message.message))
            {
                return false;
            }

            if (_useRegex)
            {
                try
                {
                    return Regex.IsMatch(message.message, _searchPattern, RegexOptions.IgnoreCase);
                }
                catch (Exception ex)
                {
                    _regexErrorImage.tooltip = ex.Message;
                    _regexErrorImage.style.display = DisplayStyle.Flex;
                    return false;
                }
            }

            return message.message.Contains(_searchPattern, StringComparison.OrdinalIgnoreCase);
        }

        private void ClearMessages()
        {
            Messages?.Clear();
            Refresh();
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
            _messageDetailsElement.SetMessage(selectedMessage);
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

        private void OnSelectedTagChanged(ChangeEvent<string> evt)
        {
            _selectedTag = _tagDropdown.value;
            if (string.IsNullOrWhiteSpace(_selectedTag))
            {
                _selectedTag = TagAll;
            }

            FilterMessages();
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