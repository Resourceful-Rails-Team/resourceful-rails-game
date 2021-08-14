using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Rails.MapEditor.Editor
{
    /// <summary>
    /// Reorderable list wrapper for the map editor Cities and Goods sections.
    /// Allows for custom element drawing.
    /// </summary>
    public class MapEditorReorderableList
    {
        /// <summary>
        /// Draw element callback delegate.
        /// Returns the height of the element.
        /// </summary>
        /// <param name="rect">Current render rectangle</param>
        /// <param name="index">Index in collection of current element to render</param>
        /// <param name="isActive">Whether or not the current element is active in the GUI</param>
        /// <param name="isFocused">Whether or not the current element is focused in the GUI</param>
        /// <returns>The height of the element</returns>
        public delegate float MapEditorReorderableListDrawElementCallback(Rect rect, int index, bool isActive, bool isFocused);

        /// <summary>
        /// Underlying reorderable list.
        /// </summary>
        private ReorderableList _list;
        
        /// <summary>
        /// Height of each element.
        /// </summary>
        private List<float> _heights;
        
        /// <summary>
        /// Elements as a serialized property.
        /// </summary>
        private SerializedProperty _elements;

        /// <summary>
        /// List header in inspector.
        /// </summary>
        public string Header { get; set; }

        /// <summary>
        /// Draw element callback.
        /// </summary>
        public MapEditorReorderableListDrawElementCallback DrawElementCallback { get; set; }

        /// <summary>
        /// Getter for collection serialized property.
        /// </summary>
        public SerializedProperty SerializedProperty => _elements;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="serializedObject">Serialized object containing the list</param>
        /// <param name="elements">Serialized property the list is rendering and editing</param>
        /// <param name="draggable">Are elements draggable</param>
        /// <param name="displayHeader">Whether or not to display the header in the inspector</param>
        /// <param name="displayAddButton">Whether or not to display the add element button in the inspector</param>
        /// <param name="displayRemoveButton">Whether or not to display the remove element button in the inspector</param>
        public MapEditorReorderableList(SerializedObject serializedObject, SerializedProperty elements, bool draggable, bool displayHeader, bool displayAddButton, bool displayRemoveButton)
        {
            _heights = new List<float>();
            _elements = elements;

            _list = new ReorderableList(serializedObject, elements, draggable, displayHeader, displayAddButton, displayRemoveButton);
            _list.drawHeaderCallback = DrawHeader;
            _list.drawElementCallback = DrawElement;
            _list.elementHeightCallback = GetElementHeight;

        }

        /// <summary>
        /// Raised when the underlying ReorderableList wants to render the header.
        /// </summary>
        private void DrawHeader(Rect rect)
        {
            EditorGUI.LabelField(rect, Header);
        }

        /// <summary>
        /// Raised when the underlying ReorderableList wants to render an element.
        /// </summary>
        private void DrawElement(Rect rect, int index, bool isActive, bool isFocused)
        {

            var height = DrawElementCallback?.Invoke(rect, index, isActive, isFocused) ?? 0;
            _heights[index] = height;
        }

        /// <summary>
        /// Returns the items height.
        /// </summary>
        private float GetElementHeight(int index)
        {
            float height = 0;

            try
            {
                height = _heights[index];
            }
            catch (ArgumentOutOfRangeException e)
            {
                Debug.LogWarning(e.Message);
            }
            finally
            {
                float[] floats = _heights.ToArray();
                Array.Resize(ref floats, _elements.arraySize);
                _heights = floats.ToList();
            }

            return height;
        }

        /// <summary>
        /// Wrapper that renders the list.
        /// </summary>
        public void DoLayoutList()
        {
            _list.DoLayoutList();
        }
        
        /// <summary>
        /// Wrapper the renders the list in the given rect.
        /// </summary>
        public void DoList(Rect rect)
        {
            _list.DoList(rect);
        }
    }
}
