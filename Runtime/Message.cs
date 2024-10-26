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
        public static Message Info(string content, string tag, object context, string customData = null)
        {
            return new Message(MessageType.Info, DateTime.Now.Ticks, content, tag, context, customData);
        }

        public static Message Warning(string content, string tag, object context, string customData = null)
        {
            return new Message(MessageType.Warning, DateTime.Now.Ticks, content, tag, context, customData);
        }

        public static Message Error(string content, string tag, object context, string customData = null)
        {
            return new Message(MessageType.Error, DateTime.Now.Ticks, content, tag, context, customData);
        }


        public MessageType Type;
        public long Timestamp;
        public string Content;
        public string Tag;
        public string Context;
        public string CustomData;


        public Message(MessageType type, long timestamp, string content, string tag, object context, string customData = null)
        {
            Type = type;
            Timestamp = timestamp;
            Content = content;
            Tag = tag;
            CustomData = customData;
            InitializeContextFromObject(context);
        }

        public override string ToString()
        {
            string text = $"{new DateTime(Timestamp)} {Type} [{Tag}] | {Content}";

            if (!string.IsNullOrEmpty(Context))
            {
                UObject unityContextObject = GetUnityContextObject();
                if (unityContextObject)
                {
                    text = $"{text} | {unityContextObject}";
                }
                else
                {
                    text = $"{text} | {Context}";
                }
            }

            if (!string.IsNullOrEmpty(CustomData))
            {
                text = $"{text} | {CustomData}";
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
#if UNITY_EDITOR
            if (_contextObjectCache)
            {
                GlobalObjectId globalObjId = GlobalObjectId.GetGlobalObjectIdSlow(_contextObjectCache);
                Context = globalObjId.ToString();
                return;
            }
#endif

            Context = context.ToString();
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

            if (!string.IsNullOrWhiteSpace(Context) && GlobalObjectId.TryParse(Context, out GlobalObjectId globalObjectId))
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

            return Context;
        }

        #endregion
    }
}
