using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using XiCore.StringTools;
using static XiRename;
using static XiRenameSettings;

public class XiRename : MonoBehaviour
{
    #region Load Settings

    /// <summary>The XiRenameSettings.</summary>
    private static XiRenameSettings _settings;

    ///--------------------------------------------------------------------
    /// <summary>Gets options for controlling the operation.</summary>
    ///
    /// <counter>The settings.</counter>
    ///--------------------------------------------------------------------

    public static XiRenameSettings Settings => _settings;

    /// <summary>Static constructor.</summary>
    static XiRename()
    {
        _settings = FindDefaultAssetPath(false);
        LoadPreferences();
    }

    public static List<ENameOrder> FieldOrder => Settings.fieldOrder;

    ///------------------------------------------------------------------------
    /// <summary>Searches for the first default asset path.</summary>
    ///
    /// <exception cref="Exception">    Thrown when an exception error
    ///                                 condition occurs.</exception>
    ///
    /// <param fileName="packagePriority">True to package priority.</param>
    ///
    /// <returns>The found default asset path.</returns>
    ///------------------------------------------------------------------------

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
    /// <summary>Read the asset at the path if it is exists or return null
    /// TODO Probably could be done by single Unity method (no time to
    /// find)</summary>
    ///
    /// <param fileName="loadPath">         Full pathname of the load file.</param>
    /// <param fileName="existinPathList">  List of existin paths.</param>
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

    /// <summary>Values that represent rename modes.</summary>
    public enum ERenameMode
    {
        Keep,
        Rename
    }

    /// <summary>Values that represent rename advanced modes.</summary>
    public enum ERenameAdvancedMode
    {
        Keep,
        Add,
        Replace,
        Remove
    }

    /// <summary>The fileName convention.</summary>
    private static ENameConvention targetConvention = ENameConvention.PascalCase;
    /// <summary>The rename Mode.</summary>
    public static ERenameMode renameMode;

    /// <summary>The prefix.</summary>
    public static Designator prefix = new Designator(true);
    /// <summary>The suffix.</summary>
    public static Designator suffix = new Designator(true);
    /// <summary>The variant.</summary>
    public static Designator variant = new Designator(false);

    public static bool DoUpdateGUI;

    ///------------------------------------------------------------------------
    /// <summary>Gets a hint.</summary>
    ///
    /// <param name="quantity">The quantity.</param>
    /// <param name="ellipse"> (Optional) The ellipse.</param>
    ///
    /// <returns>The hint.</returns>
    ///------------------------------------------------------------------------

    public static string GetHint(int quantity, string ellipse = "...")
    {
        var separator = GetSeparator(targetConvention);
        var name = MakeName("FileName", targetConvention);

        var cnt = 0;
        var str = string.Empty;
        var lst = new string[6];
        for (var i = 0; i < quantity; i++)
        {
            lst[cnt++] = GetString("file-name", i);
        }
        return string.Join(", ", lst, 0, cnt) + ellipse;
    }

    ///------------------------------------------------------------------------
    /// <summary>Gets a string.</summary>
    ///
    /// <param name="name">The name.</param>
    /// <param name="idx"> Zero-based index of the.</param>
    ///
    /// <returns>The string.</returns>
    ///------------------------------------------------------------------------

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
                case ENameOrder.Name:
                    if (!string.IsNullOrEmpty(name))
                        lst[cnt++] = name;
                    break;
                case ENameOrder.Prefix:
                    str = prefix.GetString(prefix.Counter + idx);
                    if (!string.IsNullOrEmpty(str))
                        lst[cnt++] = str;
                    break;
                case ENameOrder.Suffix:
                    str = suffix.GetString(suffix.Counter + idx);
                    if (!string.IsNullOrEmpty(str))
                        lst[cnt++] = str;
                    break;
                case ENameOrder.Variant:
                    str = variant.GetString(variant.Counter + idx);
                    if (!string.IsNullOrEmpty(str))
                        lst[cnt++] = str;
                    break;
            }
        }
        var separator = GetSeparator(targetConvention);
        return string.Join(separator, lst, 0, cnt);
    }

    public static string GetString(NameItem item)
    {
        item.Remove(XiRename.prefix.Mode, 0);
        item.Remove(XiRename.variant.Mode, -2);
        item.Remove(XiRename.suffix.Mode, 1);
        var newName = string.Empty;
        if (renameMode == ERenameMode.Rename)
            newName = MakeName(item.Name, XiRename.TargetConvention);
        else
            newName = item.Name;
        return GetString(newName,0);
    }

    ///------------------------------------------------------------------------
    /// <summary>Makes a name.</summary>
    ///
    /// <exception cref="Exception">    Thrown when an exception error
    ///                                 condition occurs.</exception>
    ///
    /// <param name="name">          The name.</param>
    /// <param fileName="convention">The convention.</param>
    ///
    /// <returns>A string.</returns>
    ///------------------------------------------------------------------------

    public static string MakeName(string name, ENameConvention convention)
    {
        switch (convention)
        {
            case ENameConvention.PascalCase:
                return Utils.Camelize(name, true);
            case ENameConvention.CamelCase:
                return Utils.Camelize(name, false);
            case ENameConvention.LowercaseUnderscore:
                return Utils.Decamelize(name, '_');
            case ENameConvention.LowercaseDash:
                return Utils.Decamelize(name, '-');
            case ENameConvention.UppercaseUnderscore:
                return Utils.Decamelize(name, '_').ToUpper();
            case ENameConvention.UppercaseDash:
                return Utils.Decamelize(name, '-').ToUpper();
        }
        throw new System.Exception("NotImplemented");
    }

    ///------------------------------------------------------------------------
    /// <summary>Makes a prefix.</summary>
    ///
    /// <exception cref="Exception">    Thrown when an exception error
    ///                                 condition occurs.</exception>
    ///
    /// <param name="name">          The name.</param>
    /// <param fileName="convention">The convention.</param>
    ///
    /// <returns>A string.</returns>
    ///------------------------------------------------------------------------

    public static string MakePrefix(string name, ENameConvention convention)
    {
        switch (convention)
        {
            case ENameConvention.PascalCase:
                return Utils.Camelize(name, true);
            case ENameConvention.CamelCase:
                return Utils.Camelize(name, false);
            case ENameConvention.LowercaseUnderscore:
                return Utils.Decamelize(name, '_');
            case ENameConvention.LowercaseDash:
                return Utils.Decamelize(name, '-');
            case ENameConvention.UppercaseUnderscore:
                return Utils.Decamelize(name, '_').ToUpper();
            case ENameConvention.UppercaseDash:
                return Utils.Decamelize(name, '-').ToUpper();
        }
        throw new System.Exception("NotImplemented");
    }

    ///------------------------------------------------------------------------
    /// <summary>Makes a suffix.</summary>
    ///
    /// <exception cref="Exception">    Thrown when an exception error
    ///                                 condition occurs.</exception>
    ///
    /// <param name="name">          The name.</param>
    /// <param fileName="convention">The convention.</param>
    ///
    /// <returns>A string.</returns>
    ///------------------------------------------------------------------------

    public static string MakeSuffix(string name, ENameConvention convention)
    {
        switch (convention)
        {
            case ENameConvention.PascalCase:
                return Utils.Camelize(name, true);
            case ENameConvention.CamelCase:
                return Utils.Camelize(name, false);
            case ENameConvention.LowercaseUnderscore:
                return Utils.Decamelize(name, '_');
            case ENameConvention.LowercaseDash:
                return Utils.Decamelize(name, '-');
            case ENameConvention.UppercaseUnderscore:
                return Utils.Decamelize(name, '_').ToUpper();
            case ENameConvention.UppercaseDash:
                return Utils.Decamelize(name, '-').ToUpper();
        }
        throw new System.Exception("NotImplemented");
    }

    ///------------------------------------------------------------------------
    /// <summary>Validates the name described by path.</summary>
    ///
    /// <param name="path">Full pathname of the file.</param>
    ///
    /// <returns>True if it succeeds, false if it fails.</returns>
    ///------------------------------------------------------------------------

    public bool ValidateName(string path)
    {
        foreach (var ignore in Settings.ignorePath)
        {
            if (path.Contains(ignore))
                return true;
        }
        var name = System.IO.Path.GetFileName(path);
        var ext = System.IO.Path.GetExtension(path);

        var prefixEnds = Utils.MinIndex(name.IndexOf('_'), name.IndexOf('-'));
        var suffuxStarts = Utils.MinIndex(name.LastIndexOf('_'), name.LastIndexOf('-'));

        foreach (var type in Settings.fileTypes)
        {
            if (type.VerifyExtention(ext))
            {

            }
        }
        return false;
    }

    ///------------------------------------------------------------------------
    /// <summary>Gets options for controlling the file types.</summary>
    ///
    /// <counter>Options that control the file types.</counter>
    ///------------------------------------------------------------------------

    public static string[] FileTypesOptions => Settings.fileTypes.Select(o => o.Name).ToArray();

    /// <summary>Zero-based index of the file type.</summary>
    private static int fileTypeIndex;

    ///------------------------------------------------------------------------
    /// <summary>Gets or sets the zero-based index of the file type.</summary>
    ///
    /// <counter>The file type index.</counter>
    ///------------------------------------------------------------------------

    public static int FileTypeIndex
    {
        get => fileTypeIndex;
        set
        {
            if (fileTypeIndex != value)
            {
                fileTypeIndex = value;
                OnChangeType();
            }
            else
                fileTypeIndex = value;
        }
    }

    /// <summary>Executes the 'change type' action.</summary>
    static void OnChangeType()
    {
        prefix.Options.Clear();
        suffix.Options.Clear();
        var name = FileTypesOptions[fileTypeIndex];
        Settings.FindOptionsByCategory(name, prefix.Options, suffix.Options);
        prefix.OnChangeType();
        suffix.OnChangeType();
    }

    ///------------------------------------------------------------------------
    /// <summary>Gets a separator.</summary>
    ///
    /// <param fileName="convention">The convention.</param>
    ///
    /// <returns>The separator.</returns>
    ///------------------------------------------------------------------------

    public static char GetSeparator(ENameConvention convention)
    {
        return separators[(int)convention];
    }

    static readonly char[] separators = new[] { '_', '_', '_', '-', '_', '-' };

    ///------------------------------------------------------------------------
    /// <summary>Gets or sets the fileName convention.</summary>
    ///
    /// <counter>The fileName convention.</counter>
    ///------------------------------------------------------------------------

    public static ENameConvention TargetConvention
    {
        get => targetConvention;
        set { targetConvention = value; SavePreferences(); }
    }

    ///------------------------------------------------------------------------
    /// <summary>Gets or sets the rename Mode.</summary>
    ///
    /// <counter>The rename Mode.</counter>
    ///------------------------------------------------------------------------

    public static ERenameMode RenameMode
    {
        get => renameMode;
        set { renameMode = value; SavePreferences(); }
    }

    const string preferenceFormat = "XiRename_{0}";

    /// <summary>Saves the preferences.</summary>
    private static void SavePreferences()
    {
        DoUpdateGUI = true;
#if UNITY_EDITOR
        EditorPrefs.SetInt(string.Format(preferenceFormat, nameof(targetConvention)), (int)targetConvention);
        EditorPrefs.SetInt(string.Format(preferenceFormat, nameof(renameMode)), (int)renameMode);
        EditorPrefs.SetInt(string.Format(preferenceFormat, nameof(fileTypeIndex)), (int)fileTypeIndex);
#endif
    }

    /// <summary>Loads the preferences.</summary>
    private static void LoadPreferences()
    {
#if UNITY_EDITOR
        targetConvention = _settings.nameConvention;
        renameMode = ERenameMode.Keep;
        prefix.Precision = Settings.prefixPrecision;
        suffix.Precision = Settings.suffixPrecision;
        targetConvention = (ENameConvention)EditorPrefs.GetInt(string.Format(preferenceFormat, nameof(targetConvention)), (int)targetConvention);
        renameMode = (ERenameMode)EditorPrefs.GetInt(string.Format(preferenceFormat, nameof(renameMode)), (int)renameMode);
        fileTypeIndex = EditorPrefs.GetInt(string.Format(preferenceFormat, nameof(fileTypeIndex)), (int)fileTypeIndex);
#endif
        OnChangeType();
    }

    /// <summary>Clears the preferences.</summary>
    private static void ClearPreferences()
    {
#if UNITY_EDITOR
        EditorPrefs.DeleteKey(string.Format(preferenceFormat, nameof(targetConvention)));
        EditorPrefs.DeleteKey(string.Format(preferenceFormat, nameof(renameMode)));
        EditorPrefs.DeleteKey(string.Format(preferenceFormat, nameof(fileTypeIndex)));
#endif
    }
}
[System.Serializable]
public class Designator
{
    private static readonly string[] formats = new string[] { "", "0", "00", "000", "0000", "00000", "000000", "0000000", "00000000", "000000000" };

    /// <summary>The suffix Mode.</summary>
    private ERenameAdvancedMode mode;
    /// <summary>The name prefix.</summary>
    private string starts;
    /// <summary>The name suffix.</summary>
    private string ends;
    /// <summary>The suffix counter.</summary>
    private int counter;
    /// <summary>The suffix precision.</summary>
    private int precision = 2;
    /// <summary>The suffix delta.</summary>
    private int delta = 1;
    /// <summary>True if has counter, false if not.</summary>
    private bool hasValue;
    /// <summary>True if has pop up, false if not.</summary>
    public bool HasPopUp;


    public ERenameAdvancedMode Mode { get { return mode; } set { mode = value; XiRename.DoUpdateGUI = true; }  }
    public string Starts { get { return starts; } set { starts = value; XiRename.DoUpdateGUI = true; } }
    public string Ends { get { return ends; } set { ends = value; XiRename.DoUpdateGUI = true; } }
    public int Counter { get { return counter; } set { counter = value; XiRename.DoUpdateGUI = true; } }
    public int Precision { get { return precision; } set { precision = value; XiRename.DoUpdateGUI = true; } }
    public int Delta { get { return delta; } set { delta = value; XiRename.DoUpdateGUI = true; } }
    public bool HasValue { get { return hasValue; } set { hasValue = value; XiRename.DoUpdateGUI = true; } }

    ///------------------------------------------------------------------------
    /// <summary>Constructor.</summary>
    ///
    /// <param name="hasPopUp">True if has pop up, false if not.</param>
    ///------------------------------------------------------------------------

    public Designator(bool hasPopUp)
    {
        HasPopUp = hasPopUp;
    }

    ///------------------------------------------------------------------------
    /// <summary>Gets the format to use.</summary>
    ///
    /// <counter>The format.</counter>
    ///------------------------------------------------------------------------

    public string Format => formats[precision];

    ///------------------------------------------------------------------------
    /// <summary>Gets the full string.</summary>
    ///
    /// <returns>The full string.</returns>
    ///------------------------------------------------------------------------

    public string GetString()
    {
        if (Mode == ERenameAdvancedMode.Keep)
            return string.Empty;

        if (hasValue)
            return Starts + (precision == 0 ? string.Empty : counter.ToString(Format)) + ends;
        return Starts;
    }

    ///------------------------------------------------------------------------
    /// <summary>Gets string and increment.</summary>
    ///
    /// <returns>The string and increment.</returns>
    ///------------------------------------------------------------------------

    public string GetStringAndIncrement()
    {
        if (Mode == ERenameAdvancedMode.Keep)
            return string.Empty;
        var str = GetString();
        counter += delta;
        return str;
    }

    ///------------------------------------------------------------------------
    /// <summary>Gets the full string.</summary>
    ///
    /// <param name="idx">Zero-based index of the.</param>
    ///
    /// <returns>The string.</returns>
    ///------------------------------------------------------------------------

    public string GetString(int idx)
    {
        if (Mode == ERenameAdvancedMode.Keep)
            return string.Empty;
        if (hasValue)
            return Starts + (precision == 0 ? string.Empty : idx.ToString(Format)) + ends;
        return Starts;
    }

    ///------------------------------------------------------------------------
    /// <summary>Gets or sets the counter string.</summary>
    ///
    /// <counter>The counter string.</counter>
    ///------------------------------------------------------------------------

    public string ValueString
    {
        get => counter.ToString(formats[precision]);
        set
        {
            var nCounter = int.Parse(value); 
            if (counter != nCounter)
            {
                precision = (int)value.Replace(" ", "").Length;
                DoUpdateGUI = true;
            }
        }
    }

    ///------------------------------------------------------------------------
    /// <summary>Gets or sets the delta string.</summary>
    ///
    /// <counter>The delta string.</counter>
    ///------------------------------------------------------------------------

    public string DeltaString
    {
        get => delta.ToString();
        set { delta = int.Parse(value); DoUpdateGUI = true; }
    }


    /// <summary>Options for controlling the operation.</summary>
    public List<StringPair> Options = new List<StringPair>();
    /// <summary>Zero-based index of the prefix.</summary>
    private int prefixIndex;

    ///------------------------------------------------------------------------
    /// <summary>Gets or sets the zero-based index of the prefix.</summary>
    ///
    /// <counter>The prefix index.</counter>
    ///------------------------------------------------------------------------

    public int PrefixIndex
    {
        get => prefixIndex;
        set
        {
            if (value != prefixIndex)
            {
                Starts = Options[value].Value;
                DoUpdateGUI = true;
            }
            prefixIndex = value;
        }
    }

    ///------------------------------------------------------------------------
    /// <summary>Gets options for controlling the prefix.</summary>
    ///
    /// <counter>Options that control the prefix.</counter>
    ///------------------------------------------------------------------------

    public string[] PrefixOptions => Options.Select(o=>$"{o.Name} ({o.Value})").ToArray();
    
    /// <summary>Executes the 'change type' action.</summary>
    public void OnChangeType()
    {
        prefixIndex = 0;
    }
}


public class NameItem 
{
    public string FullPath;
    public string FilePath;
    public string FileExt;
    public string FileName;


    public bool IsTemp;
    public List<string> Tokens;
    public NameItem(string path)
    {
        FullPath = path;

        FilePath = System.IO.Path.GetFullPath(path);
        FileName = System.IO.Path.GetFileNameWithoutExtension(path);
        FileExt = System.IO.Path.GetExtension(path);
        Tokens = FileName.Replace("-", "_").Split("_").ToList();
        IsTemp = (FileName.StartsWith("__"));
    }

    public bool RemoveAt(int idx)
    {
        if (Tokens.Count <= 1)
            return false;
        if (idx < 0)
            idx = Tokens.Count - idx;
        if (idx < 0)
            return false;
        if (idx >= Tokens.Count)
            return false;
        Tokens.RemoveAt(idx);
        return true;
    }

    public string Name => string.Join("_", Tokens);
    public bool Remove(ERenameAdvancedMode mode, int idx)
    {
        if (mode != ERenameAdvancedMode.Keep && mode != ERenameAdvancedMode.Keep)
            return RemoveAt(idx);
        return true;
    }
}

public static class Utils
{
    public static int MinIndex(int index1, int index2)
    {
        if (index1 < 0)
            return index2;
        if (index2 < 0)
            return index1;
        return index1 < index2 ? index1 : index2;
    }

    public static int MaxIndex(int index1, int index2)
    {
        if (index1 < 0)
            return index2;
        if (index2 < 0)
            return index1;
        return index1 > index2 ? index1 : index2;
    }

    /// <summary>
    /// Remove ' ', '-' or '_' to change forms:
    ///   "foo-bar"  ->  "FooBar"
    ///   "foo_bar"  ->  "FooBar"
    ///   "foo bar"  ->  "FooBar"
    /// </summary>
    /// <param name="str"></param>
    /// <param name="capitalize">Should be capitalized first character or not</param>
    /// <returns></returns>
    public static string Camelize(string str, bool capitalize = true)
    {
        Debug.Assert(str != null);
        var output = string.Empty;
        for (var i = 0; i < str.Length; i++)
        {
            var c = str[i];
            if (c == '-' || c == '_' || c == ' ')
            {
                capitalize = true;
            }
            else
            {
                if ((char.IsUpper(c) && i!=0) || capitalize)
                {
                    output += char.ToUpper(c);
                    capitalize = false;
                }
                else
                {
                    output += char.ToLower(c);
                }
            }
        }
        return output;
    }

    /// <summary>
    /// Convert 'FooBar' to 'foo-bar'
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static string Decamelize(string str, char separator = '-')
    {
        Debug.Assert(str != null);
        var output = string.Empty;
        var small = false;
        var space = false;
        for (var i = 0; i < str.Length; i++)
        {
            var c = str[i];
            if (char.IsUpper(c))
            {
                if (small)
                    output += separator;
                output += char.ToLower(c);
                small = false;
                space = false;
            }
            else if (c == ' ')
            {
                small = true; // make - if next capital
                space = true; // make - if nex down
            }
            else
            {
                if (space)
                    output += separator;
                output += c;
                small = true; // make - if next capital
                space = false; // do not make - if next small
            }
        }
        return output;
    }
}