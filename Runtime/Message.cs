using System;
using UObject = UnityEngine.Object;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GBG.EditorMessages
{
    public enum MessageType
    {
        //None = 0,
        Info = 1,
        Warning = 2,
        Error = 3,
    }

    [Serializable]
    public class Message
    {
        public static Message Info(string message, string tag = null, object context = null, string customData = null)
        {
            return new Message(MessageType.Info, DateTime.Now.Ticks, message, tag, context, customData);
        }

        public static Message Warning(string message, string tag = null, object context = null, string customData = null)
        {
            return new Message(MessageType.Warning, DateTime.Now.Ticks, message, tag, context, customData);
        }

        public static Message Error(string message, string tag = null, object context = null, string customData = null)
        {
            return new Message(MessageType.Error, DateTime.Now.Ticks, message, tag, context, customData);
        }


        public MessageType type;
        public long timestamp;
        public string message;
        public string tag;
        public string context;
        public string customData;


        public Message(MessageType type, long timestamp, string message, string tag, object context, string customData = null)
        {
            this.type = type;
            this.timestamp = timestamp;
            this.message = message;
            this.tag = tag;
            this.customData = customData;
            InitializeContextFromObject(context);
        }

        public override string ToString()
        {
            string text = $"{new DateTime(timestamp)} {type} [{tag}] | {message}";

            if (!string.IsNullOrEmpty(context))
            {
                UObject unityContextObject = GetUnityContextObject();
                if (unityContextObject)
                {
                    text = $"{text} | {unityContextObject}";
                }
                else
                {
                    text = $"{text} | {context}";
                }
            }

            if (!string.IsNullOrEmpty(customData))
            {
                text = $"{text} | {customData}";
            }

            return text;
        }


        #region Context

        private UObject _contextObjectCache;
#if UNITY_EDITOR
        private bool _contextIsNotUnityObject;
#endif


        private void InitializeContextFromObject(object context)
        {
            if (context == null)
            {
                return;
            }

            _contextObjectCache = context as UObject;
            if (!_contextObjectCache)
            {
                return;
            }

#if UNITY_EDITOR
            if (_contextObjectCache)
            {
                GlobalObjectId globalObjId = GlobalObjectId.GetGlobalObjectIdSlow(_contextObjectCache);
                this.context = globalObjId.ToString();
                return;
            }
#endif

            this.context = context.ToString();
        }

        public UObject GetUnityContextObject()
        {
            if (_contextObjectCache)
            {
                return _contextObjectCache;
            }

#if UNITY_EDITOR
            if (_contextIsNotUnityObject)
            {
                return null;
            }

            if (!string.IsNullOrWhiteSpace(context) && GlobalObjectId.TryParse(context, out GlobalObjectId globalObjectId))
            {
                _contextObjectCache = GlobalObjectId.GlobalObjectIdentifierToObjectSlow(globalObjectId);
                //_contextIsNotUnityObject = !_contextObjectCache; // 若目标是Scene中的对象且Scene未打开，也会返回null，但这时不能认定目标不是UnityObject
                return _contextObjectCache;
            }

            _contextIsNotUnityObject = true;
#endif

            return null;
        }

        public string GetContextDisplayText()
        {
            UObject unityObject = GetUnityContextObject();
            if (unityObject)
            {
                return unityObject.ToString();
            }

            return context;
        }

        #endregion
    }
}
