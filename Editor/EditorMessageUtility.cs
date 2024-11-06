using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UObject = UnityEngine.Object;

namespace GBG.EditorMessages.Editor
{
    public static class EditorMessageUtility
    {
        #region Style

        public static int GlobalIconSize = 16;

        public static Color ActiveColor => EditorGUIUtility.isProSkin
                ? new Color(0.22f, 0.22f, 0.22f, 1f)
                : new Color(0.76f, 0.76f, 0.76f, 1f);
        public static Color InactiveColor => EditorGUIUtility.isProSkin
                ? new Color(1f, 1f, 1f, 0.15f)
                : new Color(1f, 1f, 1f, 0.26f);

        #endregion


        #region Icon & Font

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

            return EditorGUIUtility.IconContent("console.infoicon.sml").image;
        }

        public static Texture GetWarningIcon(bool inactive = false)
        {
            if (inactive)
            {
                return EditorGUIUtility.IconContent("console.warnicon.inactive.sml@2x").image;
            }

            return EditorGUIUtility.IconContent("console.warnicon.sml").image;
        }

        public static Texture GetErrorIcon(bool inactive = false)
        {
            if (inactive)
            {
                return EditorGUIUtility.IconContent("console.erroricon.inactive.sml").image;
            }

            return EditorGUIUtility.IconContent("console.erroricon.sml").image;
        }

        public static Texture GetContextIcon()
        {
            return EditorGUIUtility.IconContent("gameobject icon").image;
        }

        public static Texture GetCustomDataIcon()
        {
            return EditorGUIUtility.IconContent("animation.play").image;
        }

        #endregion


        #region Scene Object

        public static void TryPingContextObject(this Message message, bool openOwnerScene)
        {
            if (message == null || string.IsNullOrWhiteSpace(message.context))
            {
                return;
            }

            UObject context = message.GetUnityContextObject();
            if (context)
            {
                EditorGUIUtility.PingObject(context);
                return;
            }

            if (!message.TryGetContextOwnerSceneAsset(out SceneAsset sceneAsset))
            {
                return;
            }

            if (!openOwnerScene)
            {
                EditorGUIUtility.PingObject(sceneAsset);
                return;
            }

            AssetDatabase.OpenAsset(sceneAsset);
            message.TryPingContextObject(false);
        }

        public static bool TryGetContextOwnerSceneAsset(this Message message, out SceneAsset sceneAsset)
        {
            sceneAsset = null;
            if (message == null || string.IsNullOrWhiteSpace(message.context))
            {
                return false;
            }

            if (!GlobalObjectId.TryParse(message.context, out GlobalObjectId globalObjectId))
            {
                return false;
            }

            string sceneGuid = globalObjectId.assetGUID.ToString();
            string scenePath = AssetDatabase.GUIDToAssetPath(sceneGuid);
            if (string.IsNullOrEmpty(scenePath) || !scenePath.EndsWith(".unity", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);
            return sceneAsset;
        }

        #endregion


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
