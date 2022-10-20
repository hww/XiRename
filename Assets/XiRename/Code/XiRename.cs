
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
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
        Replace
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
    public class XiRename : MonoBehaviour
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
            XiRenameLogger.WriteLog = _settings.WriteLog;
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
        /// <summary>The rename Mode.</summary>
        public static ERenameMode renameMode;
        /// <summary>The rename to.</summary>
        public static string renameTo;

        /// <summary>The prefix.</summary>
        public static TokenGenerator prefix = new TokenGenerator(ETokenType.Prefix);
        /// <summary>The suffix.</summary>
        public static TokenGenerator suffix = new TokenGenerator(ETokenType.Suffix);
        /// <summary>The variant.</summary>
        public static TokenGenerator variant = new TokenGenerator(ETokenType.Variant);

        /// <summary>True to do update graphical user interface.</summary>
        public static bool DoUpdateGUI;

        public static string RenameTo
        {
            get => renameTo;
            set { DoUpdateGUI = renameTo != value; renameTo = value;  }
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
                if (string.IsNullOrEmpty(renameTo))
                    newName = StringTools.MakeName(item.CleanName, XiRename.TargetConvention);
                else
                    newName = renameTo;
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
            DoUpdateGUI = true;
#if UNITY_EDITOR
            EditorPrefs.SetInt(string.Format(preferenceFormat, nameof(targetConvention)), (int)targetConvention);
            EditorPrefs.SetInt(string.Format(preferenceFormat, nameof(renameMode)), (int)renameMode);
            EditorPrefs.SetString(string.Format(preferenceFormat, nameof(renameTo)), renameTo);
            EditorPrefs.SetInt(string.Format(preferenceFormat, nameof(fileCategoryIndex)), (int)fileCategoryIndex);
#endif
        }

        /// <summary>Loads the preferences.</summary>
        private static void LoadPreferences()
        {
#if UNITY_EDITOR
            targetConvention = _settings.namingConvention;
            renameMode = ERenameMode.Keep;
            prefix.Precision = Settings.prefixPrecision;
            suffix.Precision = Settings.suffixPrecision;
            targetConvention = (ENameConvention)EditorPrefs.GetInt(string.Format(preferenceFormat, nameof(targetConvention)), (int)targetConvention);
            renameMode = (ERenameMode)EditorPrefs.GetInt(string.Format(preferenceFormat, nameof(renameMode)), (int)renameMode);
            renameTo =  EditorPrefs.GetString(string.Format(preferenceFormat, nameof(renameTo)), renameTo);
            fileCategoryIndex = EditorPrefs.GetInt(string.Format(preferenceFormat, nameof(fileCategoryIndex)), (int)fileCategoryIndex);
#endif
            OnChangeCtegory();
        }

        /// <summary>Clears the preferences.</summary>
        private static void ClearPreferences()
        {
#if UNITY_EDITOR
            EditorPrefs.DeleteKey(string.Format(preferenceFormat, nameof(targetConvention)));
            EditorPrefs.DeleteKey(string.Format(preferenceFormat, nameof(renameMode)));
            EditorPrefs.DeleteKey(string.Format(preferenceFormat, nameof(renameTo)));
            EditorPrefs.DeleteKey(string.Format(preferenceFormat, nameof(fileCategoryIndex)));
#endif
        }
    }

    ///------------------------------------------------------------------------
    /// <summary>A token is the part of the file cleanName. Fore example
    /// 'foo_bar_baz' has 3 tokens ["foo","bar","baz"]. The token generator
    /// produces posible names for a token.</summary>
    ///------------------------------------------------------------------------

    [System.Serializable]
    public class TokenGenerator
    {
        /// <summary>(Immutable) the formats.</summary>
        private static readonly string[] formats = new string[] { "", "0", "00", "000", "0000", "00000", "000000", "0000000", "00000000", "000000000" };

        /// <summary>The type.</summary>
        private ETokenType type;
        /// <summary>The suffix Mode.</summary>
        private ERenameAdvancedMode mode;
        /// <summary>The name starts with.</summary>
        private string starts;
        /// <summary>The name ends with.</summary>
        private string ends;
        /// <summary>The counter.</summary>
        private int counter;
        /// <summary>The delta.</summary>
        private int delta = 1;
        /// <summary>The precision.</summary>
        private int precision = 2;
        /// <summary>True if has counter, false if not.</summary>
        private bool useCounter;
        /// <summary>The preference format.</summary>
        private string preferenceFormat;
        /// <summary>Target convention.</summary>
        private ENameConvention targetConvention = ENameConvention.PascalCase;

        /// <summary>Executes the 'modify' action.</summary>
        private void OnModify()
        {
            XiRename.DoUpdateGUI = true;
            SavePreferences();
        }

        /// <summary>True if has counter, false if not.</summary>
        public bool WithCounter => type != ETokenType.Name;

        /// <summary>True if has pop up, false if not.</summary>
        public bool WithPopUp => (type == ETokenType.Prefix || type == ETokenType.Suffix) && Options.Count>0; 

        ///--------------------------------------------------------------------
        /// <summary>Gets or sets target convention.</summary>
        ///
        /// <value>The target convention.</value>
        ///--------------------------------------------------------------------
        public ENameConvention TargetConvention 
        { 
            get { return targetConvention; } 
            set { var isModified = targetConvention != value; targetConvention = value; if  (isModified) OnModify(); } 
        }

        ///--------------------------------------------------------------------
        /// <summary>Gets or sets the mode.</summary>
        ///
        /// <value>The mode.</value>
        ///--------------------------------------------------------------------

        public ERenameAdvancedMode Mode { 
            get { return mode; } 
            set { var isModified = mode != value; mode = value; if  (isModified) OnModify(); } 
        }

        ///--------------------------------------------------------------------
        /// <summary>Gets or sets the starts.</summary>
        ///
        /// <value>The starts.</value>
        ///--------------------------------------------------------------------

        public string Starts { 
            get { return starts; } 
            set { var isModified = starts != value; starts = value; if  (isModified) OnModify(); } 
        }

        ///--------------------------------------------------------------------
        /// <summary>Gets or sets the ends.</summary>
        ///
        /// <value>The ends.</value>
        ///--------------------------------------------------------------------

        public string Ends {
            get { return ends; }
            set { var isModified = ends != value; ends = value; if  (isModified) OnModify(); } 
        }

        ///--------------------------------------------------------------------
        /// <summary>Gets or sets the counter.</summary>
        ///
        /// <value>The counter.</value>
        ///--------------------------------------------------------------------

        public int Counter { 
            get { return counter; } 
            set { var isModified = counter != value; counter = System.Math.Abs(value); if  (isModified) OnModify(); } 
        }

        ///--------------------------------------------------------------------
        /// <summary>Gets or sets the precision.</summary>
        ///
        /// <value>The precision.</value>
        ///--------------------------------------------------------------------

        public int Precision { 
            get { return precision; } 
            set { var isModified = precision != value; precision = System.Math.Abs(value); if  (isModified) OnModify(); } 
        }

        ///--------------------------------------------------------------------
        /// <summary>Gets or sets the delta.</summary>
        ///
        /// <value>The delta.</value>
        ///--------------------------------------------------------------------

        public int Delta { 
            get { return delta; } 
            set { var isModified = delta != value; delta = value; if  (isModified) OnModify(); } 
        }

        ///--------------------------------------------------------------------
        /// <summary>Gets or sets a value indicating whether this object use
        /// counter.</summary>
        ///
        /// <value>True if use counter, false if not.</value>
        ///--------------------------------------------------------------------

        public bool UseCounter { 
            get { return useCounter; } 
            set { var isModified = useCounter != value; useCounter = value; if  (isModified) OnModify(); } 
        }

        ///--------------------------------------------------------------------
        /// <summary>Constructor.</summary>
        ///
        /// <param cleanName="name">      The cleanName.</param>
        /// <param cleanName="hasPopUp">     True if has pop up, false if not.</param>
        /// <param cleanName="hasCounter">   True if has counter, false if not.</param>
        ///--------------------------------------------------------------------

        public TokenGenerator(ETokenType type)
        {
            this.type = type;
            preferenceFormat = $"XiRename_{type.ToString()}_{{0}}";
   
            LoadPreferences();
        }

        ///--------------------------------------------------------------------
        /// <summary>Gets the format to use.</summary>
        ///
        /// <value>The format.</value>
        ///--------------------------------------------------------------------

        public string Format => formats[precision];

        ///--------------------------------------------------------------------
        /// <summary>Gets the full string.</summary>
        ///
        /// <returns>The full string.</returns>
        ///--------------------------------------------------------------------

        public string GetString()
        {
            if (Mode == ERenameAdvancedMode.Keep)
                return string.Empty;

            if (useCounter)
                return Starts + (precision == 0 ? string.Empty : System.Math.Max(0,counter).ToString(Format)) + ends;
            return Starts;
        }

        ///--------------------------------------------------------------------
        /// <summary>Gets the full string.</summary>
        ///
        /// <param cleanName="idx">Zero-based index of the.</param>
        ///
        /// <returns>The string.</returns>
        ///--------------------------------------------------------------------

        public string GetString(int idx)
        {
            if (Mode == ERenameAdvancedMode.Keep)
                return string.Empty;
            if (useCounter)
            {
                var cnt = System.Math.Max(0, Delta * idx + counter);
                return Starts + (precision == 0 ? string.Empty : cnt.ToString(Format)) + ends;
            }
            return Starts;
        }

        ///--------------------------------------------------------------------
        /// <summary>Gets or sets the counter string.</summary>
        ///
        /// <value>The value string.</value>
        ///--------------------------------------------------------------------

        public string ValueString
        {
            get => counter.ToString(formats[precision]);
            set
            {
                var nCounter = 0;
  
                if (int.TryParse(value, out nCounter)) 
                {
                    Precision = (int)value.Replace(" ", "").Length;
                    if (counter != nCounter)
                    {
                        counter = nCounter;
                        precision = (int)value.Replace(" ", "").Length;
                        XiRename.DoUpdateGUI = true;
                    }
                }
            }
        }

        ///--------------------------------------------------------------------
        /// <summary>Gets or sets the delta string.</summary>
        ///
        /// <value>The delta string.</value>
        ///--------------------------------------------------------------------

        public string DeltaString
        {
            get => delta.ToString();
            set {
                var nDelta = 0;
                
                if (int.TryParse(value, out nDelta) && delta != nDelta)
                {
                    delta = nDelta;
                    XiRename.DoUpdateGUI = true;
                }
            }
        }


        /// <summary>Options for controlling the operation.</summary>
        public List<StringPair> Options = new List<StringPair>();
        /// <summary>Zero-based index of the prefix.</summary>
        private int prefixIndex;

        ///--------------------------------------------------------------------
        /// <summary>Gets or sets the zero-based index of the prefix.</summary>
        ///
        /// <value>The prefix index.</value>
        ///--------------------------------------------------------------------

        public int PrefixIndex
        {
            get => prefixIndex;
            set
            {
                if (value != prefixIndex)
                {
                    Starts = StringTools.MakePrefix(Options[value].Value, targetConvention);
                    XiRename.DoUpdateGUI = true;
                }
                prefixIndex = value;
            }
        }

        ///--------------------------------------------------------------------
        /// <summary>Gets options for controlling the prefix.</summary>
        ///
        /// <value>Options that control the prefix.</value>
        ///--------------------------------------------------------------------

        public string[] PrefixOptions => Options.Select(o => $"{o.Name} ({o.Value})").ToArray();

        /// <summary>Executes the 'change type' action.</summary>
        public void OnChangeType()
        {
            Starts = PrefixOptions.Length == 0 ? String.Empty : Options[0].Value;
            Ends = String.Empty;
            prefixIndex = 0;
            SavePreferences();
        }


        /// <summary>Saves the preferences.</summary>
        private void SavePreferences()
        {
#if UNITY_EDITOR
            EditorPrefs.SetInt(string.Format(preferenceFormat, nameof(mode)), (int)mode);
            EditorPrefs.SetString(string.Format(preferenceFormat, nameof(starts)), starts);
            EditorPrefs.SetString(string.Format(preferenceFormat, nameof(ends)), ends);
            EditorPrefs.SetInt(string.Format(preferenceFormat, nameof(counter)), counter);
            EditorPrefs.SetInt(string.Format(preferenceFormat, nameof(delta)), delta);
            EditorPrefs.SetBool(string.Format(preferenceFormat, nameof(useCounter)), useCounter);
#endif
        }

        /// <summary>Loads the preferences.</summary>
        private void LoadPreferences()
        {
#if UNITY_EDITOR
            mode = (ERenameAdvancedMode)EditorPrefs.GetInt(string.Format(preferenceFormat, nameof(mode)), (int)mode);
            starts = EditorPrefs.GetString(string.Format(preferenceFormat, nameof(starts)), starts);
            ends = EditorPrefs.GetString(string.Format(preferenceFormat, nameof(ends)), ends);
            counter = EditorPrefs.GetInt(string.Format(preferenceFormat, nameof(counter)), counter);
            delta = EditorPrefs.GetInt(string.Format(preferenceFormat, nameof(delta)), delta);
            useCounter = EditorPrefs.GetBool(string.Format(preferenceFormat, nameof(useCounter)), useCounter);
#endif
            OnChangeType();
        }

        /// <summary>Clears the preferences.</summary>
        private void ClearPreferences()
        {
#if UNITY_EDITOR
            EditorPrefs.DeleteKey(string.Format(preferenceFormat, nameof(mode)));
            EditorPrefs.DeleteKey(string.Format(preferenceFormat, nameof(starts)));
            EditorPrefs.DeleteKey(string.Format(preferenceFormat, nameof(ends)));
            EditorPrefs.DeleteKey(string.Format(preferenceFormat, nameof(counter)));
            EditorPrefs.DeleteKey(string.Format(preferenceFormat, nameof(delta)));
            EditorPrefs.DeleteKey(string.Format(preferenceFormat, nameof(useCounter)));
#endif
        }

    }

    ///------------------------------------------------------------------------
    /// <summary>A file descriptor is a caching structure for storing the file
    /// cleanName path and tokens.</summary>
    ///----------- -------------------------------------------------------------

    public class RenamableObject
    {
        public ERenamableType Type;
        /// <summary>The reference to selected object.</summary>
        public UnityEngine.Object Reference;
        /// <summary>The validation status.</summary>
        public EFileState State;
        /// <summary>True if is temporary, false if not.</summary>
        public bool IsTemp;
        /// <summary>CleanName of the file path and the cleanName.</summary>
        public string OriginalPath;
        /// <summary>Full pathname of the file.</summary>
        public string DirectoryPath;
        /// <summary>Extent of the file.</summary>
        public string FileExt;
        /// <summary>Filename of the file.</summary>
        public string FileName;

        /// <summary>The tokens.</summary>
        public List<string> Tokens;
        /// <summary>Zero-based index of the.</summary>
        public int Index = 0;
        /// <summary>The mask.</summary>
        private uint Mask = 0;

        /// <summary>CleanName of the automatic.</summary>
        private string cleanName;

        /// <summary>CleanName of the custom.</summary>
        private string customName;
        /// <summary>Filename of the file.</summary>
        public string resultName;

        ///--------------------------------------------------------------------
        /// <summary>Gets a value indicating whether this object has custom
        /// name.</summary>
        ///
        /// <value>True if this object has custom name, false if not.</value>
        ///--------------------------------------------------------------------

        public bool HasCustomName => !string.IsNullOrEmpty(customName);

        ///--------------------------------------------------------------------
        /// <summary>Gets or sets the name.</summary>
        ///
        /// <value>The name.</value>
        ///--------------------------------------------------------------------

        public string CleanName => cleanName;

        ///--------------------------------------------------------------------
        /// <summary>Gets or sets the name of the result.</summary>
        ///
        /// <value>The name of the result.</value>
        ///--------------------------------------------------------------------
        public string ResultName
        {
            get { return resultName; }
            set
            {
                if (value != resultName)
                {
                    // Make custom name
                    XiRename.DoUpdateGUI |= true;
                    resultName = value;
                }
            }
        }

        ///--------------------------------------------------------------------
        /// <summary>Gets or sets the name of the result or custom.</summary>
        ///
        /// <value>The name of the result or custom.</value>
        ///--------------------------------------------------------------------

        public string ResultOrCustomName 
        {
            get { return string.IsNullOrEmpty(customName) ? resultName : customName; }
            set {
                if (value != resultName)
                {
                    // Make custom name
                    XiRename.DoUpdateGUI |= (customName != value);
                    customName = value;
                }
                else
                {
                    // Make it to use auto name
                    XiRename.DoUpdateGUI |= (customName != string.Empty);
                    customName = string.Empty;
                }
            }
        }
        /// <summary>List of colors of the states.</summary>
        private static Color[] stateColors = new Color[4] { Color.yellow, Color.gray, Color.red, Color.green };

        public static Color GetStateColor(EFileState state) { return stateColors[(int)state];  }

        ///--------------------------------------------------------------------
        /// <summary>Gets the color of the state.</summary>
        ///
        /// <value>The color of the state.</value>
        ///--------------------------------------------------------------------

        public Color StateColor => stateColors[(int)State];

        ///--------------------------------------------------------------------
        /// <summary>Gets a value indicating whether this object is valid.</summary>
        ///
        /// <value>True if this object is valid, false if not.</value>
        ///--------------------------------------------------------------------

        public bool IsValid => Tokens.Count > 0;

        ///--------------------------------------------------------------------
        /// <summary>Gets a value indicating whether this object is
        /// invalid.</summary>
        ///
        /// <value>True if this object is invalid, false if not.</value>
        ///--------------------------------------------------------------------

        public bool IsInvalid => Tokens.Count == 0;

        ///--------------------------------------------------------------------
        /// <summary>Gets a value indicating whether this object is
        /// renamable.</summary>
        ///
        /// <value>True if this object is renamable, false if not.</value>
        ///--------------------------------------------------------------------

        public bool IsRenamable
        {
            get
            {
                switch (Type)
                {
                    case ERenamableType.Directory:
                        return false;
                    case ERenamableType.File:
                        return (State != EFileState.Ignored && State != EFileState.Undefined);
                    case ERenamableType.GameObject:
                        return true;
                }
                return false;
            }
        }

        ///--------------------------------------------------------------------
        /// <summary>Gets a value indicating whether this object is file.</summary>
        ///
        /// <value>True if this object is file, false if not.</value>
        ///--------------------------------------------------------------------

        public bool IsFile => Type == ERenamableType.File;

        ///--------------------------------------------------------------------
        /// <summary>Gets a value indicating whether this object is
        /// direcory.</summary>
        ///
        /// <value>True if this object is direcory, false if not.</value>
        ///--------------------------------------------------------------------

        public bool IsDirecory => Type == ERenamableType.Directory;

        ///--------------------------------------------------------------------
        /// <summary>Gets the result cleanName with extention.</summary>
        ///
        /// <value>The result cleanName with extention.</value>
        ///--------------------------------------------------------------------

        public string ResultNameWithExtention => $"{ResultOrCustomName}{FileExt}";

        ///--------------------------------------------------------------------
        /// <summary>Gets a value indicating whether this object is game
        /// object.</summary>
        ///
        /// <value>True if this object is game object, false if not.</value>
        ///--------------------------------------------------------------------

        public bool IsGameObject => Type == ERenamableType.GameObject;

        ///--------------------------------------------------------------------
        /// <summary>Constructor.</summary>
        ///
        /// <param cleanName="path">Full pathname of the file.</param>
        ///--------------------------------------------------------------------

        public RenamableObject(UnityEngine.Object obj)
        {
            Reference = obj;
            OriginalPath =AssetDatabase.GetAssetPath(obj); 
            DirectoryPath = System.IO.Path.GetDirectoryName(OriginalPath).Replace("\\", "/");
            FileName = System.IO.Path.GetFileNameWithoutExtension(OriginalPath);
            FileExt = System.IO.Path.GetExtension(OriginalPath);
            // TODO Make Regexp
            Tokens = FileName.Replace("  ", "_").Replace(" ", "_").Replace("-", "_").Split("_").ToList();
            IsTemp = (FileName.StartsWith("__"));

            if (System.IO.File.GetAttributes(OriginalPath).HasFlag(System.IO.FileAttributes.Directory))
                Type = ERenamableType.Directory;
            else
                Type = ERenamableType.File;
        }


        public RenamableObject(GameObject obj)
        {
            Reference = obj;
            OriginalPath = string.Empty;
            DirectoryPath = string.Empty;
            FileName = obj.name;
            FileExt = string.Empty;
            // TODO Make Regexp
            Tokens = FileName.Replace("  ", "_").Replace(" ", "_").Replace("-", "_").Split("_").ToList();
            IsTemp = (FileName.StartsWith("__"));
            Type = ERenamableType.GameObject;
        }


        /// <summary>Updates the mask. Each bit equal (1) will remove one of Tokens</summary>
        public void UpdateName()
        {
            Mask = 0;
            UpdateMask_Bit(XiRename.prefix.Mode, 0);
            UpdateMask_Bit(XiRename.variant.Mode, -2);
            UpdateMask_Bit(XiRename.suffix.Mode, -1);
            string str = string.Empty;
            var bitMask = 1;
            for (var i = 0; i < Tokens.Count; i++)
            {
                if ((bitMask & Mask) == 0)
                {
                    if (i != 0)
                        str += $"_{Tokens[i]}";
                    else
                        str += Tokens[i];
                }
                bitMask <<= 1;
            }
            cleanName = str;
        }

        ///--------------------------------------------------------------------
        /// <summary>Removes this object.</summary>
        ///
        /// <param cleanName="mode">The mode.</param>
        /// <param cleanName="idx"> Zero-based index of the.</param>
        ///
        /// <returns>True if it succeeds, false if it fails.</returns>
        ///--------------------------------------------------------------------

        public void UpdateMask_Bit(ERenameAdvancedMode mode, int idx)
        {
            if (mode != ERenameAdvancedMode.Keep && mode != ERenameAdvancedMode.Add)
                UpdateMask_Bit(idx);
        }

        ///--------------------------------------------------------------------
        /// <summary>Make a bitfield for removing cleanName items.</summary>
        ///
        /// <param cleanName="idx">Zero-based index of the.</param>
        ///
        /// <returns>True if it succeeds, false if it fails.</returns>
        ///--------------------------------------------------------------------

        public void UpdateMask_Bit(int idx)
        {
            if (Tokens.Count <= 1)
                return;
            if (idx < 0)
                idx = Tokens.Count + idx;
            if (idx < 0)
                return;
            if (idx >= Tokens.Count)
                return;
            Mask |= (uint)1 << idx;
        }
    }
}