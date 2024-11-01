using System;
using System.Collections.Generic;

namespace GBG.EditorMessages
{
    public static class MessageUtility
    {
        public static bool CountByType(this IEnumerable<Message> messages,
            out int infoCount, out int warningCount, out int errorCount)
        {
            infoCount = warningCount = errorCount = 0;
            if (messages == null)
            {
                return false;
            }

            foreach (Message message in messages)
            {
                switch (message.Type)
                {
                    case MessageType.Info:
                        infoCount++;
                        break;
                    case MessageType.Warning:
                        warningCount++;
                        break;
                    case MessageType.Error:
                        errorCount++;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(message.Type), message.Type, null);
                }
            }

            return true;
        }

        public static HashSet<string> CollectTags(this IEnumerable<Message> messages)
        {
            if (messages == null)
            {
                return null;
            }

            HashSet<string> tags = new HashSet<string>();
            foreach (Message message in messages)
            {
                string tag = message.Tag ?? string.Empty;
                tags.Add(tag);
            }

            return tags;
        }
    }
}
