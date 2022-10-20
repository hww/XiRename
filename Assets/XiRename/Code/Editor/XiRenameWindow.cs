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
        private static Texture banner;

        [MenuItem("Xi/XiRename/Rename Tool...")]

        /// <summary>Shows the window.</summary>
        public static void ShowWindow()
        {
            EditorWindow.GetWindow(typeof(XiRenameWindow));
        }

        public ReorderableList orderableList;

        /// <summary>Executes the 'selection changed' action.</summary>
        void OnSelectionChanged()
        {
            selectedFiles.Clear();
            // Try to work out what folder we're clicking on. This code is from google.
            foreach (var obj in Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.Assets))
            {
                var item = new RenamableObject(obj);
                if (item.IsFile)
                    selectedFiles.Add(item);
            }
            if (selectedFiles.Count == 0)
            {
                foreach (var obj in Selection.gameObjects)
                {
                    var item = new RenamableObject(obj);
                    selectedFiles.Add(item);
                }
            }

            orderableList = new ReorderableList(selectedFiles, typeof(RenamableObject));
            orderableList.drawElementCallback = OnRenderOrderableItem;
            orderableList.displayAdd = false;
            XiRename.DoUpdateGUI = true;
            this.Repaint();
        }

        ///--------------------------------------------------------------------
        /// <summary>Executes the 'render orderable item' action.</summary>
        ///
        /// <param name="rect">     The rectangle.</param>
        /// <param name="index">    Zero-based index of the.</param>
        /// <param name="isActive"> True if is active, false if not.</param>
        /// <param name="isFocused">True if is focused, false if not.</param>
        ///--------------------------------------------------------------------

        void OnRenderOrderableItem(Rect rect, int index, bool isActive, bool isFocused)
        {
            var item = selectedFiles[index];
            XiRename.ValidateName(item, XiRename.FileCategory);
            rect.y += 2; // FIXME to centrify text field need +2 WTF?
            const int gapWidth = 4;
            const int imgWidth = 4;
            const int imgWidthAndGap = imgWidth + gapWidth;
            var col2Width = rect.width * 0.5f;
            var col1Width = col2Width - imgWidthAndGap - gapWidth;
            var x = rect.x;
            var rectI = new Rect(x, rect.y, imgWidth, EditorGUIUtility.singleLineHeight);
            x += imgWidthAndGap;
            var rectL = new Rect(x, rect.y, col1Width, EditorGUIUtility.singleLineHeight);
            x += (col1Width + gapWidth);
            var rectR = new Rect(x, rect.y, col2Width, EditorGUIUtility.singleLineHeight);

            EditorGUILayout.BeginHorizontal();
            EditorGUI.DrawRect(rectI, item.StateColor);
            if (item.IsRenamable)
            {

                EditorGUI.LabelField(rectL, item.FileName);
                item.ResultName = XiRename.GetString(item, item.Index, XiRename.FileCategory);
                item.ResultOrCustomName = EditorGUI.TextField(rectR, item.ResultOrCustomName);
            }
            else
            {
                EditorGUI.LabelField(rectL, item.FileName);
                EditorGUI.LabelField(rectR, string.Empty);
            }
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>Executes the 'restrucure orderableList' action.</summary>
        void OnBeforeRenderOrderableList()
        {
            var index = 0;
            foreach (var item in selectedFiles)
            {
                XiRename.ValidateName(item, XiRename.FileCategory);
                if (item.IsRenamable)
                    item.Index = index++;
            }
        }

        /// <summary>The name items.</summary>
        private List<RenamableObject> selectedFiles = new List<RenamableObject>(100);


        /// <summary>Called when the object becomes enabled and active.</summary>
        private void OnEnable()
        {
            banner = (Texture)Resources.Load("XiRename/T_Banner_Sprite", typeof(Texture));
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
            GUILayout.Box(banner);
            DrawUILine(uiLineColor, 2, 0);
            GUILayout.Label("Settings", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Choose one of the possible file naming conventions and file type.", MessageType.None);
            XiRename.TargetConvention = (ENameConvention)EditorGUILayout.EnumPopup("Target Convention:", XiRename.TargetConvention);
            XiRename.FileCategoryIndex = EditorGUILayout.Popup("File Types:", XiRename.FileCategoryIndex, XiRename.FileCategoryOptions);
            // - - - - - - - - - - - - - - - - - -
            var fieldOrder = XiRename.FieldOrder;
            for (int i = 0; i < fieldOrder.Count; i++)
                OnGUI_Chapter(fieldOrder[i], i);
            // - - - - - - - - - - - - - - - - - -
            DrawUILine(uiLineColor);

            EditorGUILayout.HelpBox(XiRename.GetHint(3), MessageType.Info);

            // - - - - - - - - - - - - - - - - - -
            // - Preview result 
            DrawUILine(uiLineColor);
            GUILayout.Label("Preview:", EditorStyles.boldLabel);

            if (selectedFiles.Count == 0)
            {
                EditorGUILayout.HelpBox("Select files in the projects.", MessageType.Warning);
            }
            else
            {
                EditorGUILayout.HelpBox("Edit field to make a custom name or clear it to use a generated one.", MessageType.None);
                OnBeforeRenderOrderableList();
                orderableList?.DoLayoutList();
            }

            GUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Legends:");
            var h = EditorGUIUtility.singleLineHeight / 2;
            const float size = 6; 
            var legendValues = System.Enum.GetValues(typeof(EFileState));
            foreach (var legend in legendValues)
            {
                Rect rect = EditorGUILayout.GetControlRect(GUILayout.Width(size), GUILayout.Height(size));
                rect.y += h - size / 2;
                EditorGUI.DrawRect(rect, RenamableObject.GetStateColor((EFileState)legend));
                GUILayout.Label(legend.ToString());
            }
            GUILayout.EndHorizontal();
            // - - - - - - - - - - - - - - - - - -
            DrawUILine(uiLineColor);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Validate Selection"))
                XiRenameValidator.ValidateSelectetItems();
            if (GUILayout.Button("Rename All"))
                RenameAllFiles(false);
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
                if (item.IsFile)
                    RenameAsset(item, dryRun);
                else if (item.IsGameObject)
                {
                    var go = item.Reference as GameObject;
                    var oldName = go.name;
                    var newName = item.ResultNameWithExtention;
                    XiRenameLogger.Log("rename", $"'Scene/{go.scene.name}/{oldName}' -> '{newName}'");
                    go.name = newName;
                }
            }
            selectedFiles.Clear();
            Selection.objects = new Object[0];
        }


        private void RenameAsset(RenamableObject item, bool dryRun)
        {
            if (!item.IsRenamable)
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

        private static void OnGUI_Chapter(ETokenType chapter, int idx)
        {
            DrawUILine(uiLineColor);
            switch (chapter)
            {
                case ETokenType.Name:
                    GUILayout.Label($"[{idx}] CleanName Tool:", EditorStyles.boldLabel);
                    EditorGUILayout.HelpBox("How should the file name change except for changing the prefix and suffix.", MessageType.None);
                    XiRename.RenameMode = (ERenameMode)EditorGUILayout.EnumPopup("Rename Action:", XiRename.RenameMode);
                    if (XiRename.renameMode != ERenameMode.Keep)
                        XiRename.RenameTo = EditorGUILayout.TextField("New CleanName:", XiRename.RenameTo);
                    break;
                case ETokenType.Prefix:
                    OnGUI_Desinator($"[{idx}] Prefix Tool", XiRename.prefix);
                    break;
                case ETokenType.Variant:
                    OnGUI_Desinator($"[{idx}] Variant Tool", XiRename.variant);
                    break;
                case ETokenType.Suffix:
                    OnGUI_Desinator($"[{idx}] Suffix Tool", XiRename.suffix);
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
                    designator.PrefixIndex = EditorGUILayout.Popup("Convention:", designator.PrefixIndex, designator.PrefixOptions);
                designator.Starts = EditorGUILayout.TextField("Starts:", designator.Starts);

                if (designator.WithCounter)
                {
                    designator.UseCounter = EditorGUILayout.Toggle("Counter:", designator.UseCounter);
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