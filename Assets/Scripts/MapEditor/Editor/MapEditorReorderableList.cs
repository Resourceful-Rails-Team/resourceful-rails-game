using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Rails.MapEditor.Editor
{
    public class MapEditorReorderableList
    {
        public delegate float MapEditorReorderableListDrawElementCallback(Rect rect, int index, bool isActive, bool isFocused);

        private ReorderableList _list;
        private List<float> _heights;
        private SerializedProperty _elements;

        public string Header { get; set; }
        public MapEditorReorderableListDrawElementCallback DrawElementCallback { get; set; }

        public SerializedProperty SerializedProperty => _elements;

        public MapEditorReorderableList(SerializedObject serializedObject, SerializedProperty elements, bool draggable, bool displayHeader, bool displayAddButton, bool displayRemoveButton)
        {
            _heights = new List<float>();
            _elements = elements;

            _list = new ReorderableList(serializedObject, elements, draggable, displayHeader, displayAddButton, displayRemoveButton);
            _list.drawHeaderCallback = DrawHeader;
            _list.drawElementCallback = DrawElement;
            _list.elementHeightCallback = GetElementHeight;

        }

        private void DrawHeader(Rect rect)
        {
            EditorGUI.LabelField(rect, Header);
        }

        private void DrawElement(Rect rect, int index, bool isActive, bool isFocused)
        {

            var height = DrawElementCallback?.Invoke(rect, index, isActive, isFocused) ?? 0;
            _heights[index] = height;
        }

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

        public void DoLayoutList()
        {
            _list.DoLayoutList();
        }
        
        public void DoList(Rect rect)
        {
            _list.DoList(rect);
        }
    }
}
