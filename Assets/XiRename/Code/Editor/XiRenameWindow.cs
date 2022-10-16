using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;
using XiRenameTool;

namespace XiRenameTool.Editor
{
    public class XiRenameWindow : EditorWindow
    {
        [MenuItem("Xi/XiRename")]

        public static void ShowWindow()
        {
            EditorWindow.GetWindow(typeof(XiRenameWindow));

        }

        void OnSelectionChanged()
        {
            nameItemsReport.Clear();
            nameItems.Clear();
            // Try to work out what folder we're clicking on. This code is from google.
            foreach (UnityEngine.Object obj in Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.Assets))
            {
                var path = AssetDatabase.GetAssetPath(obj);
                var item = new NameItem(path);
                nameItems.Add(item);
                nameItemsReport.AppendLine(XiRename.GetString(item));
            }
            this.Repaint();
        }


        StringBuilder nameItemsReport = new StringBuilder();
        List<NameItem> nameItems = new List<NameItem>(100);


        private void OnEnable()
        {
            Selection.selectionChanged += OnSelectionChanged;
        }

        private void OnDisable()
        {
            Selection.selectionChanged -= OnSelectionChanged;
        }


        void OnGUI()
        {
            DrawUILine(lineColor);
            GUILayout.Label("Name Tool", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Choose one of the possible file naming conventions.", MessageType.None);
            XiRename.TargetConvention = (ENameConvention)EditorGUILayout.EnumPopup("Target Convention:", XiRename.TargetConvention);

            // - - - - - - - - - - - - - - - - - -
            DrawUILine(lineColor);
            XiRename.FileTypeIndex = EditorGUILayout.Popup("File Types:", XiRename.FileTypeIndex, XiRename.FileTypesOptions);
            // - - - - - - - - - - - - - - - - - -
            var fieldOrder = XiRename.FieldOrder;
            foreach (var field in fieldOrder)
                OnGUI_Chapter(field);
            // - - - - - - - - - - - - - - - - - -
            DrawUILine(lineColor);
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.TextField("Hint:", XiRename.GetHint(3), EditorStyles.boldLabel);
            EditorGUI.EndDisabledGroup();
            // - - - - - - - - - - - - - - - - - -
            // - Preview result 
            DrawUILine(lineColor);
            GUILayout.Label("Preview:", EditorStyles.boldLabel);
            GUILayout.TextArea(nameItemsReport.ToString());
            // - - - - - - - - - - - - - - - - - -
            DrawUILine(lineColor);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Dry Rename All"))
                XiRename.DoUpdateGUI = true;
            if (GUILayout.Button("Rename All"))
                Debug.Log("2");
            if (GUILayout.Button("Rename One"))
                Debug.Log("3");
            GUILayout.EndHorizontal();
            if (XiRename.DoUpdateGUI)
            {
                XiRename.DoUpdateGUI = false;
                OnSelectionChanged();
            }
        }

        private static void OnGUI_Chapter(ENameOrder chapter)
        {
            DrawUILine(lineColor);
            switch (chapter)
            {
                case ENameOrder.Name:
                    EditorGUILayout.HelpBox("How should the file name change except for changing the prefix and suffix.", MessageType.None);
                    XiRename.RenameMode = (ERenameMode)EditorGUILayout.EnumPopup("Rename Action:", XiRename.RenameMode);
                    break;
                case ENameOrder.Prefix:
                    OnGUI_Desinator("Starts Tool", XiRename.prefix);
                    break;
                case ENameOrder.Variant:
                    OnGUI_Desinator("Variant Tool", XiRename.variant);
                    break;
                case ENameOrder.Suffix:
                    OnGUI_Desinator("Suffix Tool", XiRename.suffix);
                    break;

            }
        }

        private static void OnGUI_Desinator(string label, Designator designator)
        {
            GUILayout.Label(label, EditorStyles.boldLabel);
            designator.Mode = (ERenameAdvancedMode)EditorGUILayout.Popup("Action:", (int)designator.Mode, System.Enum.GetNames(typeof(ERenameAdvancedMode)));
            if (designator.HasPopUp)
                designator.PrefixIndex = EditorGUILayout.Popup("Starts Options:", designator.PrefixIndex, designator.PrefixOptions);
            designator.Starts = EditorGUILayout.TextField("Starts:", designator.Starts);
            designator.HasValue = EditorGUILayout.Toggle("Has Counter:", designator.HasValue);
            if (designator.HasValue)
            {
                designator.ValueString = EditorGUILayout.TextField("Value:", designator.ValueString);
                designator.DeltaString = EditorGUILayout.TextField("Delta:", designator.DeltaString);
                designator.Ends = EditorGUILayout.TextField("Ends:", designator.Ends);
            }
        }

        private static string[] GetPrefixOptions()
        {
            return new string[] { "ss" };
        }

        private static string[] GetSuffixOptions()
        {
            return new string[] { "ss" };
        }

        private static Color lineColor = new Color(0.1f, 0.1f, 0.1f);

        public static void DrawUILine(Color color, int thickness = 2, int padding = 10)
        {
            Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(padding + thickness));
            r.height = thickness;
            r.y += padding / 2;
            r.x -= 2;
            r.width += 6;
            EditorGUI.DrawRect(r, color);
        }
    }
}