using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using XiRenameTool;

namespace XiRenameTool.Editor
{
    /// <summary>Form for viewing the xi rename.</summary>
    public class XiRenameWindow : EditorWindow
    {
        [MenuItem("Xi/XiRename")]

        /// <summary>Shows the window.</summary>
        public static void ShowWindow()
        {
            EditorWindow.GetWindow(typeof(XiRenameWindow));

        }

        public ReorderableList list = null;

        /// <summary>Executes the 'selection changed' action.</summary>
        void OnSelectionChanged()
        {
            ignoredFiles.Clear();
            selectedFiles.Clear();
            // Try to work out what folder we're clicking on. This code is from google.
            foreach (UnityEngine.Object obj in Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.Assets))
            {
                var item = new FileDescriptor(obj);
                if (item.IsDirectory)
                {
                    ignoredFiles.Add(item);
                }
                else
                {
                    // to build the report
                    selectedFiles.Add(item);
                }
            }

            this.Repaint();
        }

        void OrderCallBack(Rect rect, int index, bool isActive, bool isFocused)
        {
            var item = selectedFiles[index];
            XiRename.ValidateName(item, XiRename.FileCategory);
            rect.y += 2;
            var rect2 = new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight);
            if (item.ValidationStatus == EFileResult.Ignore || item.ValidationStatus == EFileResult.Undefined)
            {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUI.TextField(rect2, item.Name);
                EditorGUI.EndDisabledGroup();
            }
            else
            {
                item.ResultName = XiRename.GetString(item, index, XiRename.FileCategory);
                item.ResultName = EditorGUI.TextField(rect2, item.ResultName);
            }
        }

        /// <summary>The ignored files.</summary>
        private List<FileDescriptor> ignoredFiles = new List<FileDescriptor>(100);
        /// <summary>The name items.</summary>
        private List<FileDescriptor> selectedFiles = new List<FileDescriptor>(100);


        /// <summary>Called when the object becomes enabled and active.</summary>
        private void OnEnable()
        {
            list = new ReorderableList(selectedFiles, typeof(FileDescriptor));
            list.drawElementCallback = OrderCallBack;
            Selection.selectionChanged += OnSelectionChanged;
        }

        ///--------------------------------------------------------------------
        /// <summary>Called when the behaviour becomes disabled or
        /// inactive.</summary>
        ///--------------------------------------------------------------------

        private void OnDisable()
        {
            Selection.selectionChanged -= OnSelectionChanged;
        }


        /// <summary>Called for rendering and handling GUI events.</summary>
        void OnGUI()
        {
            DrawUILine(uiLineColor);
            GUILayout.Label("Name Tool", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Choose one of the possible file naming conventions.", MessageType.None);
            XiRename.TargetConvention = (ENameConvention)EditorGUILayout.EnumPopup("Target Convention:", XiRename.TargetConvention);

            // - - - - - - - - - - - - - - - - - -
            DrawUILine(uiLineColor);
            XiRename.FileCategoryIndex = EditorGUILayout.Popup("File Types:", XiRename.FileCategoryIndex, XiRename.FileCategoryOptions);
            // - - - - - - - - - - - - - - - - - -
            var fieldOrder = XiRename.FieldOrder;
            foreach (var field in fieldOrder)
                OnGUI_Chapter(field);
            // - - - - - - - - - - - - - - - - - -
            DrawUILine(uiLineColor);
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.TextField("Hint:", XiRename.GetHint(3), EditorStyles.boldLabel);
            EditorGUI.EndDisabledGroup();
            // - - - - - - - - - - - - - - - - - -
            // - Preview result 
            DrawUILine(uiLineColor);
            GUILayout.Label("Preview:", EditorStyles.boldLabel);
            if (ignoredFiles.Count > 0)
                EditorGUILayout.HelpBox($"Ignoted {ignoredFiles.Count} selected items.", MessageType.Info);

            if (selectedFiles.Count == 0)
            {
                EditorGUILayout.HelpBox("Select files in the projects.", MessageType.Warning);
            }
            else
            {
                list?.DoLayoutList();
            }
            // - - - - - - - - - - - - - - - - - -
            DrawUILine(uiLineColor);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Rename All"))
                RenameAllFiles(false);
            if (GUILayout.Button("Rename One"))
                RenameOneFile(false);
            GUILayout.EndHorizontal();
            if (XiRename.DoUpdateGUI)
            {
                XiRename.DoUpdateGUI = false;
            }
        }
        private void RenameAllFiles(bool dryRun)
        {
            foreach (var item in selectedFiles)
            {
                RenameAsset(item, dryRun);
            }
            selectedFiles.Clear();
            Selection.objects = new Object[0];
        }

        private void RenameOneFile(bool dryRun)
        {
            if (selectedFiles.Count > 0)
            {
                var item = selectedFiles[0];
                RenameAsset(item, dryRun);
                selectedFiles.RemoveAt(0);
                Selection.objects = selectedFiles.ConvertAll(o => o.Reference).ToArray();
            }
        }

        private void RenameAsset(FileDescriptor item, bool dryRun)
        {
            if (item.ValidationStatus == EFileResult.Undefined || item.ValidationStatus == EFileResult.Ignore)
                return;

            var oldPath = item.OriginalPath;
            var newName = item.ResultNameWithExtention;


            if (dryRun)
            {
                XiRenameLogger.Log("dry-rename", $"'{oldPath}' -> '{newName}'");
            }
            else
            {
                AssetDatabase.Refresh();
                XiRenameLogger.Log("rename", $"'{oldPath}' -> '{newName}'");
                AssetDatabase.RenameAsset(oldPath, newName);
                AssetDatabase.SaveAssets();
            }

        }

        ///--------------------------------------------------------------------
        /// <summary>Executes the 'graphical user interface chapter'
        /// action.</summary>
        ///
        /// <param name="chapter">The chapter.</param>
        ///--------------------------------------------------------------------

        private static void OnGUI_Chapter(ETokenType chapter)
        {
            DrawUILine(uiLineColor);
            switch (chapter)
            {
                case ETokenType.Name:
                    GUILayout.Label("Name Tool:", EditorStyles.boldLabel);
                    EditorGUILayout.HelpBox("How should the file name change except for changing the prefix and suffix.", MessageType.None);
                    XiRename.RenameMode = (ERenameMode)EditorGUILayout.EnumPopup("Rename Action:", XiRename.RenameMode);
                    break;
                case ETokenType.Prefix:
                    OnGUI_Desinator("Prefix Tool", XiRename.prefix);
                    break;
                case ETokenType.Variant:
                    OnGUI_Desinator("Variant Tool", XiRename.variant);
                    break;
                case ETokenType.Suffix:
                    OnGUI_Desinator("Suffix Tool", XiRename.suffix);
                    break;

            }
        }

        ///--------------------------------------------------------------------
        /// <summary>Executes the 'graphical user interface desinator'
        /// action.</summary>
        ///
        /// <param name="label">     The label.</param>
        /// <param name="designator">The designator.</param>
        ///--------------------------------------------------------------------

        private static void OnGUI_Desinator(string label, TokenGenerator designator)
        {
            GUILayout.Label(label, EditorStyles.boldLabel);
            designator.Mode = (ERenameAdvancedMode)EditorGUILayout.Popup("Action:", (int)designator.Mode, System.Enum.GetNames(typeof(ERenameAdvancedMode)));
            if (designator.Mode != ERenameAdvancedMode.Keep)
            {

                if (designator.WithPopUp)
                    designator.PrefixIndex = EditorGUILayout.Popup("Starts Options:", designator.PrefixIndex, designator.PrefixOptions);
                designator.Starts = EditorGUILayout.TextField("Starts:", designator.Starts);

                if (designator.WithCounter)
                {
                    designator.UseCounter = EditorGUILayout.Toggle("Has Counter:", designator.UseCounter);
                    if (designator.UseCounter)
                    {
                        designator.ValueString = EditorGUILayout.TextField("Value:", designator.ValueString);
                        designator.DeltaString = EditorGUILayout.TextField("Delta:", designator.DeltaString);
                        designator.Ends = EditorGUILayout.TextField("Ends:", designator.Ends);
                    }
                }
            }
        }


        /// <summary>The line color.</summary>
        private static Color uiLineColor = new Color(0.1f, 0.1f, 0.1f);

        ///--------------------------------------------------------------------
        /// <summary>Draw user interface line.</summary>
        ///
        /// <param name="color">    The color.</param>
        /// <param name="thickness">(Optional) The thickness.</param>
        /// <param name="padding">  (Optional) The padding.</param>
        ///--------------------------------------------------------------------

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