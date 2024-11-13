using System;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace GBG.EditorMessages.Editor
{
    internal class ToolbarImageButton : ToolbarButton
    {
        public Texture Image
        {
            get => _image.image;
            set => _image.image = value;
        }

        private readonly Image _image;


        public ToolbarImageButton(Action clickEvent) : base(clickEvent)
        {
            float iconSize = EditorMessageUtility.GlobalIconSize;

            _image = EditorMessageUtility.NewImage();
            Insert(0, _image);
        }

        public ToolbarImageButton(Texture image, Action clickEvent) : this(clickEvent)
        {
            Image = image;
        }
    }
}
