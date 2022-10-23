
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using XiRenameTool.Utils;

namespace XiRenameTool
{
    /// <summary>Values that represent rename modes.</summary>
    public enum ERenameMode
    {
        /// <summary>An enum constant representing the keep option.</summary>
        Keep,
        /// <summary>An enum constant representing the rename option.</summary>
        Rename
    }

    /// <summary>Values that represent rename advanced modes.</summary>
    public enum ERenameAdvancedMode
    {
        /// <summary>An enum constant representing the keep option.</summary>
        Keep,
        /// <summary>An enum constant representing the add option.</summary>
        Add,
        /// <summary>An enum constant representing the replace option.</summary>
        Replace,
        /// <summary>The same value but different format.</summary>
        Format
    }

    /// <summary>Values that represent file results.</summary>
    public enum EFileState
    {
        /// <summary>An enum constant representing the undefined type option.</summary>
        Undefined,
        /// <summary>An enum constant representing the ignore file option.</summary>
        Ignored,
        /// <summary>An enum constant representing the invalid file cleanName option.</summary>
        Invalid,
        /// <summary>An enum constant representing the valid file cleanName option.</summary>
        Valid
    }

    /// <summary>Values that represent file types.</summary>
    public enum ERenamableType
    {
        Directory, File, GameObject
    }

    /// <summary>An xi rename.</summary>
    public static class XiRename 
    {
        #region Load Settings

        /// <summary>The XiRenameSettings.</summary>
        private static XiRenameSettings _settings;

        ///--------------------------------------------------------------------
        /// <summary>Gets options for controlling the operation.</summary>
        ///
        /// <value>The settings.</value>
        ///--------------------------------------------------------------------

        public static XiRenameSettings Settings => _settings;

        /// <summary>Static constructor.</summary>
        static XiRename()
        {
            _settings = FindDefaultAssetPath(false);

            LoadPreferences();
        }

        ///--------------------------------------------------------------------
        /// <summary>Gets the field order.</summary>
        ///
        /// <value>The field order.</value>
        ///--------------------------------------------------------------------

        public static List<ETokenType> FieldOrder => Settings.nameTokensOrder;

        ///--------------------------------------------------------------------
        /// <summary>Searches for the first default asset filePath.</summary>
        ///
        /// <exception cref="Exception">    Thrown when an exception error
        ///                                 condition occurs.</exception>
        ///
        /// <param fileName="packagePriority">  True to package priority.</param>
        ///
        /// <returns>The found default asset filePath.</returns>
        ///--------------------------------------------------------------------

        public static XiRenameSettings FindDefaultAssetPath(bool packagePriority)
        {
            const string defaultAssetPath1 = "Assets/XiRename/Resources/XiRename/XiRenameSettings.asset";
            const string defaultAssetPath2 = "Packages/com.hww.xigametool/Resources/XiRename/XiRenameSettings.asset";
            const string defaultAssetPath3 = "/Resources/XiRename/XiRenameSettings.asset";
            const string defaultAssetPath4 = "XiRenameSettings.asset";

            var idList = AssetDatabase.FindAssets($"t: {nameof(XiRenameSettings)}");
            if (idList.Length == 0)
                throw new System.Exception($"There must be a single {nameof(Settings)}");
            var pathList = idList.Select(id => UnityEditor.AssetDatabase.GUIDToAssetPath(id)).ToList();

            if (packagePriority)
            {
                return TryReadDefaultAsset(defaultAssetPath2, pathList) ??
                    TryReadDefaultAsset(defaultAssetPath1, pathList) ??
                    TryReadDefaultAsset(defaultAssetPath3, pathList) ??
                    TryReadDefaultAsset(defaultAssetPath4, pathList);
            }
            else
            {
                return TryReadDefaultAsset(defaultAssetPath1, pathList) ??
                    TryReadDefaultAsset(defaultAssetPath2, pathList) ??
                    TryReadDefaultAsset(defaultAssetPath3, pathList) ??
                    TryReadDefaultAsset(defaultAssetPath4, pathList);
            }

        }

        ///--------------------------------------------------------------------
        /// <summary>Read the asset at the filePath if it is exists or return
        /// null TODO Probably could be done by single Unity method (no time
        /// to find)</summary>
        ///
        /// <param fileName="loadPath">         Full pathname of the load
        ///                                     file.</param>
        /// <param fileName="existinPathList">List of existin paths.</param>
        ///
        /// <returns>The XiRenameSettings.</returns>
        ///--------------------------------------------------------------------

        private static XiRenameSettings TryReadDefaultAsset(string loadPath, List<string> existinPathList)
        {
            foreach (var path in existinPathList)
            {
                if (path.Contains(loadPath))
                    return UnityEditor.AssetDatabase.LoadAssetAtPath<XiRenameSettings>(path);
            }
            return null;
        }
        #endregion



        /// <summary>The fileName convention.</summary>
        private static ENameConvention targetConvention = ENameConvention.PascalCase;
        /// <summary>True to make unique.</summary>
        private static bool makeUnique;
        /// <summary>True to add number to zero.</summary>
        private static bool addNumberToZero;
        /// <summary>The rename Mode.</summary>
        public static ERenameMode renameMode;
        /// <summary>The rename to.</summary>
        public static string customTargetName;

        /// <summary>The prefix.</summary>
        public static TokenGenerator prefix = new TokenGenerator(ETokenType.Prefix);
        /// <summary>The suffix.</summary>
        public static TokenGenerator suffix = new TokenGenerator(ETokenType.Suffix);
        /// <summary>The variant.</summary>
        public static TokenGenerator variant = new TokenGenerator(ETokenType.Variant);

        /// <summary>True to do update graphical user interface.</summary>
        public static bool DoUpdateGUI;

        ///--------------------------------------------------------------------
        /// <summary>Gets or sets the rename to.</summary>
        ///
        /// <value>The rename to.</value>
        ///--------------------------------------------------------------------

        public static string CustomTargetName
        {
            get => customTargetName;
            set { DoUpdateGUI |= customTargetName != value; customTargetName = value;  }
        }

        ///--------------------------------------------------------------------
        /// <summary>Gets or sets a value indicating whether the make
        /// unique.</summary>
        ///
        /// <value>True if make unique, false if not.</value>
        ///--------------------------------------------------------------------

        public static bool MakeUnique
        {
            get => makeUnique;
            set { DoUpdateGUI |= makeUnique != value; makeUnique = value; }
        }

        ///--------------------------------------------------------------------
        /// <summary>Gets or sets a value indicating whether the add number to
        /// zero.</summary>
        ///
        /// <value>True if add number to zero, false if not.</value>
        ///--------------------------------------------------------------------

        public static bool AddNumberToZero
        {
            get => addNumberToZero;
            set { DoUpdateGUI |= addNumberToZero != value; addNumberToZero = value; }
        }

        ///--------------------------------------------------------------------
        /// <summary>Gets a hint.</summary>
        ///
        /// <param cleanName="quantity">The quantity.</param>
        /// <param cleanName="ellipse"> (Optional) The ellipse.</param>
        ///
        /// <returns>The hint.</returns>
        ///--------------------------------------------------------------------

        public static string GetHint(int quantity, string ellipse = "...")
        {
            var separator = GetSeparator(targetConvention);
            var name = StringTools.MakeName("FileName", targetConvention);

            var cnt = 0;
            var str = string.Empty;
            var lst = new string[6];
            for (var i = 0; i < quantity; i++)
            {
                lst[cnt++] = GetString(name, i);
            }
            return string.Join(", ", lst, 0, cnt) + ellipse;
        }

        ///--------------------------------------------------------------------
        /// <summary>Gets a string.</summary>
        ///
        /// <param cleanName="name">The cleanName.</param>
        /// <param cleanName="idx"> Zero-based index of the.</param>
        ///
        /// <returns>The string.</returns>
        ///--------------------------------------------------------------------

        public static string GetString(string name, int idx)
        {
            var cnt = 0;
            var str = string.Empty;
            var lst = new string[6];
            var fieldOrder = XiRename.FieldOrder;
            foreach (var field in fieldOrder)
            {
                switch (field)
                {
                    case ETokenType.Name:
                        if (!string.IsNullOrEmpty(name))
                            lst[cnt++] = name;
                        break;
                    case ETokenType.Prefix:
                        str = prefix.GetString(idx);
                        if (!string.IsNullOrEmpty(str))
                            lst[cnt++] = str;
                        break;
                    case ETokenType.Suffix:
                        str = suffix.GetString(idx);
                        if (!string.IsNullOrEmpty(str))
                            lst[cnt++] = str;
                        break;
                    case ETokenType.Variant:
                        str = variant.GetString(idx);
                        if (!string.IsNullOrEmpty(str))
                            lst[cnt++] = str;
                        break;
                }
            }
            var separator = GetSeparator(targetConvention);
            return string.Join(separator, lst, 0, cnt);
        }

        ///--------------------------------------------------------------------
        /// <summary>Gets a string.</summary>
        ///
        /// <param cleanName="item">    The item.</param>
        /// <param cleanName="category">(Optional) The category.</param>
        ///
        /// <returns>The string.</returns>
        ///--------------------------------------------------------------------

        public static string GetString(RenamableObject item, int idx = 0, string category = null)
        {
            item.UpdateName();
            var newName = string.Empty;
            if (renameMode == ERenameMode.Rename)
            {
                if (string.IsNullOrEmpty(customTargetName))
                    newName = StringTools.MakeName(item.CleanName, XiRename.TargetConvention);
                else
                    newName = customTargetName;
            }
            else
                newName = item.CleanName;
            return GetString(newName, idx);
        }

        ///--------------------------------------------------------------------
        /// <summary>Validates the cleanName described by filePath.</summary>
        ///
        /// <param cleanName="item">        Full pathname of the file.</param>
        /// <param cleanName="category">Category the file belongs to.</param>
        ///
        /// <returns>True if it succeeds, false if it fails.</returns>
        ///--------------------------------------------------------------------

        public static EFileState ValidateName(RenamableObject item, string category)
        {
            item.State = Settings.ValidateName(item, category);
            return item.State;
        }

        public static EFileState ValidateIgnorance(RenamableObject item)
        {
            item.State = Settings.ValidateIgnorance(item);
            return item.State;
        }

        ///--------------------------------------------------------------------
        /// <summary>Automatic validate name.</summary>
        ///
        /// <param cleanName="item">Full pathname of the file.</param>
        ///
        /// <returns>An EFileState.</returns>
        ///--------------------------------------------------------------------

        public static EFileState AutoValidateName(RenamableObject item)
        {
            item.State = Settings.AutoValidateName(item);
            return item.State;
        }

        ///--------------------------------------------------------------------
        /// <summary>Gets options for controlling the file types.</summary>
        ///
        /// <value>Options that control the file category.</value>
        ///--------------------------------------------------------------------

        public static string[] FileCategoryOptions => fileCategoryOptions ??= Settings.FileCategoryOptions;

        /// <summary>Options for controlling the file category.</summary>
        private static string[] fileCategoryOptions;

        /// <summary>Zero-based index of the file type.</summary>
        private static int fileCategoryIndex;

        ///--------------------------------------------------------------------
        /// <summary>Gets the category the file belongs to.</summary>
        ///
        /// <value>The file category.</value>
        ///--------------------------------------------------------------------

        public static string FileCategory => FileCategoryOptions[fileCategoryIndex];

        ///--------------------------------------------------------------------
        /// <summary>Gets or sets the zero-based index of the file type.</summary>
        ///
        /// <value>The file category index.</value>
        ///--------------------------------------------------------------------

        public static int FileCategoryIndex
        {
            get => fileCategoryIndex;
            set
            {
                if (fileCategoryIndex != value)
                {
                    fileCategoryIndex = value;
                    OnChangeCtegory();
                }
                else
                    fileCategoryIndex = value;
            }
        }

        /// <summary>Executes the 'change type' action.</summary>
        static void OnChangeCtegory()
        {
            prefix.Options.Clear();
            suffix.Options.Clear();
            Settings.FindFileOptionsByCategory(FileCategory, prefix.Options, suffix.Options);
            prefix.OnChangeType();
            suffix.OnChangeType();
        }

        ///--------------------------------------------------------------------
        /// <summary>Gets a separator.</summary>
        ///
        /// <param fileName="convention">The convention.</param>
        ///
        /// <returns>The separator.</returns>
        ///--------------------------------------------------------------------

        public static char GetSeparator(ENameConvention convention)
        {
            return separators[(int)convention];
        }

        ///--------------------------------------------------------------------
        /// <summary>Gets a separator.</summary>
        ///
        /// <returns>The separator.</returns>
        ///--------------------------------------------------------------------

        public static char GetSeparator()
        {
            return GetSeparator(targetConvention);
        }

        /// <summary>(Immutable) the separators.</summary>
        static readonly char[] separators = new[] { '_', '_', '_', '-', '_', '-' };

        ///--------------------------------------------------------------------
        /// <summary>Gets or sets the fileName convention.</summary>
        ///
        /// <value>The target convention.</value>
        ///--------------------------------------------------------------------

        public static ENameConvention TargetConvention
        {
            get => targetConvention;
            set { 
                targetConvention = value;
                prefix.TargetConvention = value;
                suffix.TargetConvention = value;
                variant.TargetConvention = value;
                SavePreferences(); 
            }
        }

        ///--------------------------------------------------------------------
        /// <summary>Gets or sets the rename Mode.</summary>
        ///
        /// <value>The rename mode.</value>
        ///--------------------------------------------------------------------

        public static ERenameMode RenameMode
        {
            get => renameMode;
            set { renameMode = value; SavePreferences(); }
        }

        /// <summary>(Immutable) the preference format.</summary>
        const string preferenceFormat = "XiRename_{0}";

        /// <summary>Saves the preferences.</summary>
        private static void SavePreferences()
        {
            DoUpdateGUI |= true;
#if UNITY_EDITOR
            EditorPrefs.SetInt(string.Format(preferenceFormat, nameof(targetConvention)), (int)targetConvention);
            EditorPrefs.SetInt(string.Format(preferenceFormat, nameof(renameMode)), (int)renameMode);
            EditorPrefs.SetString(string.Format(preferenceFormat, nameof(customTargetName)), customTargetName);
            EditorPrefs.SetInt(string.Format(preferenceFormat, nameof(fileCategoryIndex)), (int)fileCategoryIndex);
            EditorPrefs.SetBool(string.Format(preferenceFormat, nameof(makeUnique)), makeUnique);
            EditorPrefs.SetBool(string.Format(preferenceFormat, nameof(addNumberToZero)), addNumberToZero);
#endif
        }

        /// <summary>Loads the preferences.</summary>
        private static void LoadPreferences()
        {
            XiRenameLogger.WriteLog = _settings.WriteLog;
            XiRename.AddNumberToZero = _settings.AddNumberToZero;
#if UNITY_EDITOR
            targetConvention = _settings.namingConvention;
            renameMode = ERenameMode.Keep;
            prefix.Precision = Settings.prefixPrecision;
            suffix.Precision = Settings.suffixPrecision;
            targetConvention = (ENameConvention)EditorPrefs.GetInt(string.Format(preferenceFormat, nameof(targetConvention)), (int)targetConvention);
            renameMode = (ERenameMode)EditorPrefs.GetInt(string.Format(preferenceFormat, nameof(renameMode)), (int)renameMode);
            customTargetName =  EditorPrefs.GetString(string.Format(preferenceFormat, nameof(customTargetName)), customTargetName);
            fileCategoryIndex = EditorPrefs.GetInt(string.Format(preferenceFormat, nameof(fileCategoryIndex)), (int)fileCategoryIndex);
            makeUnique = EditorPrefs.GetBool(string.Format(preferenceFormat, nameof(makeUnique)), makeUnique);
            addNumberToZero = EditorPrefs.GetBool(string.Format(preferenceFormat, nameof(addNumberToZero)), addNumberToZero);
#endif
            OnChangeCtegory();
        }

        /// <summary>Clears the preferences.</summary>
        private static void ClearPreferences()
        {
#if UNITY_EDITOR
            EditorPrefs.DeleteKey(string.Format(preferenceFormat, nameof(targetConvention)));
            EditorPrefs.DeleteKey(string.Format(preferenceFormat, nameof(renameMode)));
            EditorPrefs.DeleteKey(string.Format(preferenceFormat, nameof(customTargetName)));
            EditorPrefs.DeleteKey(string.Format(preferenceFormat, nameof(fileCategoryIndex)));
            EditorPrefs.DeleteKey(string.Format(preferenceFormat, nameof(makeUnique)));
            EditorPrefs.DeleteKey(string.Format(preferenceFormat, nameof(addNumberToZero)));
#endif
        }
    }
}