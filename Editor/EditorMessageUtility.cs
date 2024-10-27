using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.Text;
//using UObject = UnityEngine.Object;

namespace GBG.EditorMessages.Editor
{
    public static class EditorMessageUtility
    {
        //private static Font _monospaceFont;
        private static FontAsset _monospaceFontAsset;


        public static FontAsset GetMonospaceFontAsset()
        {
            if (!_monospaceFontAsset)
            {
                //_monospaceFont = (Font)EditorGUIUtility.LoadRequired("fonts/robotomono/robotomono-regular.ttf");
                _monospaceFontAsset = (FontAsset)EditorGUIUtility.LoadRequired("fonts/robotomono/robotomono-regular sdf.asset");
            }
            return _monospaceFontAsset;
        }

        public static Texture GetMessageTypeIcon(MessageType messageType)
        {
            switch (messageType)
            {
                case MessageType.Info:
                    return GetInfoIcon();
                case MessageType.Warning:
                    return GetWarningIcon();
                case MessageType.Error:
                    return GetErrorIcon();
                default:
                    throw new ArgumentOutOfRangeException(nameof(messageType), messageType, null);
            }
        }

        public static Texture GetInfoIcon(bool inactive = false)
        {
            if (inactive)
            {
                return EditorGUIUtility.IconContent("console.infoicon.inactive.sml@2x").image;
            }

            return EditorGUIUtility.IconContent("console.infoicon").image;
        }

        public static Texture GetWarningIcon(bool inactive = false)
        {
            if (inactive)
            {
                return EditorGUIUtility.IconContent("console.warnicon.inactive.sml@2x").image;
            }

            return EditorGUIUtility.IconContent("console.warnicon").image;
        }

        public static Texture GetErrorIcon(bool inactive = false)
        {
            if (inactive)
            {
                return EditorGUIUtility.IconContent("console.erroricon.inactive.sml@2x").image;
            }

            return EditorGUIUtility.IconContent("console.erroricon").image;
        }

        public static Texture GetContextIcon()
        {
            return EditorGUIUtility.IconContent("gameobject icon").image;
        }

        public static Texture GetCustomDataIcon()
        {
            return EditorGUIUtility.IconContent("customized").image;
        }


        //public static void ClearAll()
        //{
        //    _monospaceFontAsset.DestroyAuto();
        //}

        //public static void DestroyAuto(this UObject obj)
        //{
        //    if (!obj)
        //    {
        //        return;
        //    }
        //
        //    if (Application.isPlaying)
        //    {
        //        UObject.Destroy(obj);
        //    }
        //    else
        //    {
        //        UObject.DestroyImmediate(obj);
        //    }
        //}
    }
}
