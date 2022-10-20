
using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using XiCore.StringTools;
using XiRenameTool.Utils;

namespace XiRenameTool
{
    /// <summary>Values that represent category conventions.</summary>
    public enum ENameConvention { PascalCase, CamelCase, LowercaseUnderscore, LowercaseDash, UppercaseUnderscore, UppercaseDash }

    /// <summary>Values that represent token types.</summary>
    public enum ETokenType { Prefix, Name, Suffix, Variant };

    /// <summary>An xi rename settings.</summary>
    [CreateAssetMenu(fileName = "XiRenameSettings", menuName = "Xi/Settins/XiRenameSettings", order = 0)]
    public class XiRenameSettings : ScriptableObject
    {
        /// <summary>True to write log.</summary>
        public bool WriteLog;
        /// <summary>The cleanName tokens order.</summary>
        public List<ETokenType> nameTokensOrder = new List<ETokenType>();

        /// <summary>Default naming convention.</summary>
        public ENameConvention namingConvention = ENameConvention.PascalCase;
        /// <summary>The prefix default precision.</summary>
        public int prefixPrecision = 2;
        /// <summary>The suffix default precision.</summary>
        public int suffixPrecision = 2;
        /// <summary>Full pathname of the ignore item.</summary>
        public string[] ignorePath;

        /// <summary>List of item types.</summary>
        public List<FileType> fileTypes = new List<FileType>();

        ///--------------------------------------------------------------------
        /// <summary>Gets options for controlling the item category.</summary>
        ///
        /// <value>Options that control the item category.</value>
        ///--------------------------------------------------------------------

        public string[] FileCategoryOptions => _fileCategoryOptions ??= fileTypes.Select(o => o.Category).Distinct().ToArray();

        /// <summary>Options for controlling the item category.</summary>
        private string[] _fileCategoryOptions;

        ///--------------------------------------------------------------------
        /// <summary>Query if 'filePath' is ignored.</summary>
        ///
        /// <param cleanName="filePath">Full pathname of the item.</param>
        ///
        /// <returns>True if ignored, false if not.</returns>
        ///--------------------------------------------------------------------

        public bool IsIgnored(string filePath)
        {
            if (_ignorePathRegexArray == null)
                _ignorePathRegexArray = ignorePath.Select(o => new WildcardPattern(o)).ToArray();
            foreach (var ignore in _ignorePathRegexArray)
            {
                if (ignore.IsMatch(filePath))
                    return true;
            }
            return false;
        }

        WildcardPattern[] _ignorePathRegexArray;

        ///--------------------------------------------------------------------
        /// <summary>Validates the cleanName for simgle category.</summary>
        ///
        /// <param cleanName="item">        The item descriptor.</param>
        /// <param cleanName="category">    Category the item belongs to.</param>
        ///
        /// <returns>An EFileState.</returns>
        ///--------------------------------------------------------------------

        public EFileState ValidateName(RenamableObject item, string category)
        {
            if (!item.IsValid)
                return EFileState.Invalid;
            if (item.IsFile && IsIgnored(item.DirectoryPath))
                return EFileState.Ignored;
            var result = EFileState.Invalid;
            var categories = 0;
            foreach (var type in fileTypes)
            {
                if (type.Category == category)
                {
                    categories++;
                    result = type.ValidateName(item);
                    if (result == EFileState.Valid)
                        return result;
                }
            }
            // Ignore all files without math category
            return categories != 0 ? EFileState.Ignored : EFileState.Undefined;
        }

        ///--------------------------------------------------------------------
        /// <summary>Automatic validate name.</summary>
        ///
        /// <param cleanName="item">The item descriptor.</param>
        ///
        /// <returns>An EFileState.</returns>
        ///--------------------------------------------------------------------

        public EFileState AutoValidateName(RenamableObject item)
        {
            if (!item.IsValid)
                return EFileState.Invalid;
            if (item.IsFile && IsIgnored(item.DirectoryPath))
                return EFileState.Ignored;
            var result = EFileState.Invalid;
            var categories = 0;
            foreach (var type in fileTypes)
            {
                result = type.ValidateName(item);
                if (result == EFileState.Valid)
                    return result;
                if (result != EFileState.Undefined)
                    categories++;
            }
            return categories != 0 ? EFileState.Invalid : EFileState.Undefined;
        }

        ///--------------------------------------------------------------------
        /// <summary>Searches for the first match.</summary>
        ///
        /// <param cleanName="path">        The cleanName.</param>
        /// <param cleanName="displayError">    (Optional) True to display
        ///                                     error.</param>
        ///
        /// <returns>A Type.</returns>
        ///--------------------------------------------------------------------

        public FileType FindFileTypeByName(string name, bool displayError = false)
        {
            var result = fileTypes.Find(o => o.Path == name);
            if (displayError && result == null)
                Debug.LogError($"Can't find item type '{name}'", this);
            return result;
        }

        ///--------------------------------------------------------------------
        /// <summary>Searches for the first category.</summary>
        ///
        /// <param cleanName="category">    The category.</param>
        /// <param cleanName="displayError">    (Optional) True to display
        ///                                     error.</param>
        ///
        /// <returns>The found category.</returns>
        ///--------------------------------------------------------------------

        public List<FileType> FindFileTypesByCategory(string category, bool displayError = false)
        {
            var result = fileTypes.FindAll(o => o.Category == category).ToList();
            if (displayError && result.Count == 0)
                Debug.LogError($"Can't find item type '{category}'", this);
            return result;
        }

        ///--------------------------------------------------------------------
        /// <summary>Searches for the first options by category.</summary>
        ///
        /// <param cleanName="category">The category.</param>
        /// <param cleanName="prefixes">The prefixes.</param>
        /// <param cleanName="suffixes">The suffixes.</param>
        ///--------------------------------------------------------------------

        public void FindFileOptionsByCategory(string category, List<StringPair> prefixes, List<StringPair> suffixes)
        {
            Debug.Assert(prefixes != null);
            Debug.Assert(suffixes != null);
            Debug.Assert(category != null);

            foreach (var type in fileTypes)
            {
                if (type.Category == category)
                {
                    foreach (var prefix in type.Prefixes)
                        prefixes.Add(prefix);
                    foreach (var suffix in type.Suffixes)
                        suffixes.Add(suffix);
                }
            }
        }

        ///--------------------------------------------------------------------
        /// <summary>Called when the script is loaded or a value is changed in
        /// the inspector (Called in the editor only)</summary>
        ///--------------------------------------------------------------------

        public void OnValidate()
        {
            _fileCategoryOptions = null;
            _ignorePathRegexArray = null;
            foreach (FileType type in fileTypes)
                type.OnValidate();
        }


        /// <summary>Initializes the defaut.</summary>
        [Button("Reset All Settings To Default Values")]
        public void InitializeDefaut()
        {
            // Define the order of tokens
            nameTokensOrder.Clear();
            nameTokensOrder.Add(ETokenType.Prefix);
            nameTokensOrder.Add(ETokenType.Name);
            nameTokensOrder.Add(ETokenType.Variant);
            nameTokensOrder.Add(ETokenType.Suffix);

            
            ignorePath = new string[] { 
                "*/External", "*/Plugins", "*/Standart Assets" 
            };

            // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
            // Taken from https://github.com/justinwasilenko/Unity-Style-Guide
            // But modified for my needs
            // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

            fileTypes.Clear();

            // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

            var type = Define("Scenes/Defaults", ".unity;.scene;.scenetemplate");
            type.DefinePreffix("Scene", "MAP");
            type.DefineSuffix("Persistent", "P", true);
            type.DefineSuffix("Audio", "Audio");
            type.DefineSuffix("Lighting", "Lighting");
            type.DefineSuffix("Geometry", "Geo");
            type.DefineSuffix("Gameplay", "Gameplay");
       

            // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

            type = Define("Models/FBX", ".fbx;.ma;.mb");
            type.DefinePreffix("Characters", "CH");
            type.DefinePreffix("Vehicles", "VH");
            type.DefinePreffix("Weapons", "WP");
            type.DefinePreffix("Static Mesh", "SM", true);
            type.DefinePreffix("Skeletal Mesh", "SK", true);
            type.DefinePreffix("Skeleton", "SKEL");
            type.DefinePreffix("Rig", "RIG");

            // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

            type = Define("Models/MAX", ".max");
            type.DefineSuffix("MAX Mesh LOD", "mesh_lod0*");
            type.DefineSuffix("MAX Mesh Collider", "collider");

            // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

            type = Define("Animations/Defaults", ".fbx;.ma;.mb");
            type.DefinePreffix("Animation Clip", "A");
            type.DefinePreffix("Animation Controller", "AC");
            type.DefinePreffix("Avatar Mask", "AM");
            type.DefinePreffix("Morph Target", "MT");

            // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

            type = Define("AI/Defaults", ".asset");
            type.DefinePreffix("AI Controller", "AIC");
            type.DefinePreffix("Behavior Tree", "BT");
            type.DefinePreffix("Blackboard", "BB");
            type.DefinePreffix("Decorator", "BTDecorator");
            type.DefinePreffix("Service", "BTService");
            type.DefinePreffix("Task", "BTTask");
            type.DefinePreffix("Environment Query", "EQS");
            type.DefinePreffix("EnvQueryContext", "EQC");
            type.DefineSuffix("EnvQueryContext", "Context");

            // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

            type = Define("Other Assets/Defaults", ".asset");
            type.DefinePreffix("Scriptable Object", "SO");

            // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

            type = Define("Prefabs/Defaults", ".prefab");
            type.DefinePreffix("Prefab", "P");
            
            // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

            type = Define("Materials/Defaults", ".mat;.cubemaps");
            type.DefinePreffix("Material", "M");

            // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

            type = Define("Textures/Defaults", ".tga;.bmp;.jpg;.png;.gif;.psd");
            type.DefinePreffix("Texture", "T", true);
            type.DefinePreffix("Media Texture", "MT");
            type.DefinePreffix("Render Target", "RT");
            type.DefinePreffix("Cube Texture", "TC");
            type.DefinePreffix("Cube Render Target", "RTC");
            type.DefinePreffix("Light Profile", "TLP");
            
            type.DefineSuffix("Albedo", "D", true);
            type.DefineSuffix("Albedo+Opacity", "DA");
            type.DefineSuffix("Opacity", "A");
            type.DefineSuffix("Roughness", "R");
            type.DefineSuffix("Metallic", "MT");
            type.DefineSuffix("Metallic+Roughness", "MTR");
            type.DefineSuffix("Specular", "SP");
            type.DefineSuffix("Emmition", "E");
            type.DefineSuffix("Normal", "N");
            type.DefineSuffix("Displacement", "DP");
            type.DefineSuffix("Ambient Occlusion", "AO");
            type.DefineSuffix("Height Map", "H");
            type.DefineSuffix("Flow Map", "FM");
            type.DefineSuffix("Light Map", "L");
            type.DefineSuffix("Bump", "B");
            type.DefineSuffix("Mask", "M");
            type.DefineSuffix("Specular+Gloss+AO", "SGAO");
            type.DefineSuffix("Roughness+Metalic+AO", "RMAO");

            // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
            
            type = Define("Miscellaneous/Defaults", ".asset");
            type.DefinePreffix("Universal Render Pipeline Asset", "URP");
            type.DefinePreffix("Post Process Volume Profile", "PP");
            
            // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
            
            type = Define("Physics/Defaults", ".physicMaterial;.physicsMaterial2D");
            type.DefinePreffix("Physical Material", "PM");
            
            // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
            
            type = Define("Audio/Defaults", ".mp3;.wav");
            type.DefinePreffix("Audio (Class)", "");
            type.DefinePreffix("Audio (Clip)", "A");
            type.DefinePreffix("Audio (Mixer)", "AM");
            type.DefinePreffix("Audio (Voice)", "AV");
            
            // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
            
            type = Define("UI/Defaults", ".asset");
            type.DefinePreffix("UI (Interface)", "UI");
            type.DefinePreffix("UI (Font)", "Font");
            type.DefinePreffix("UI (Sprite)", "T");
            type.DefineSuffix("UI (Sprite)", "GUI");
            
            // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
            
            type = Define("Effects/Defaults", ".prefab");
            type.DefinePreffix("FX (Particle System)", "PS");
            
            // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
            
            type = Define("Shaders/Defaults", ".compute;.raytrace;.shadervariants;.shader");
            type.DefinePreffix("Shader", "SH");
            
            // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
            
            type = Define("Fonts/Defaults", ".ttf;.otf");
            type.DefinePreffix("F", "F");
            
            // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

        }

        ///--------------------------------------------------------------------
        /// <summary>Defines.</summary>
        ///
        /// <param cleanName="path">      The cleanName.</param>
        /// <param cleanName="category">  The category.</param>
        /// <param cleanName="extentions">(Optional) The extentions.</param>
        ///
        /// <returns>A Type.</returns>
        ///
        /// ### <param cleanName="description"> (Optional) The description.</param>
        ///--------------------------------------------------------------------

        public FileType Define(string path, string extentions, string description = null)
        {
            var type = FindFileTypeByName(path, false);
            if (type == null)
            {
                type = new FileType(path, extentions, description);
                fileTypes.Add(type);
            }
            return type;
        }
    }

    /// <summary>A string pair.</summary>
    [System.Serializable]
    public class StringPair
    {
        /// <summary>The category.</summary>
        public string Name;
        /// <summary>The counter.</summary>
        public string Value;
        /// <summary>Default constructor.</summary>
        public StringPair() { }

        ///--------------------------------------------------------------------
        /// <summary>Constructor.</summary>
        ///
        /// <param category="category">The category.</param>
        /// <param category="counter"> The counter.</param>
        ///--------------------------------------------------------------------

        public StringPair(string name, string value)
        {
            Name = name;
            Value = value;
        }
    }

    /// <summary>A single item type.</summary>
    [System.Serializable]
    public class FileType
    {
        /// <summary>The item type category.</summary>
        public string Path;
        /// <summary>The item type category.</summary>
        public string Category;
        /// <summary>The item type subcategory.</summary>
        public string Subcategory;
        /// <summary>The item type description.</summary>
        public string Description;
        /// <summary>The list of possible item extentions.</summary>
        [TextArea]
        [SerializeField]
        private string extentions;
        /// <summary>The prefixes.</summary>
        [SerializeField]
        private List<StringPair> prefixes = new List<StringPair>();
        /// <summary>The suffixes.</summary>
        [SerializeField]
        private List<StringPair> suffixes = new List<StringPair>();

        /// <summary>The cached extentions array.</summary>
        private WildcardPattern[] _extentionRegexArray;

        ///--------------------------------------------------------------------
        /// <summary>Gets the extentions.</summary>
        ///
        /// <value>The extentions.</value>
        ///--------------------------------------------------------------------

        public WildcardPattern[] ExtentionRegexArray
        {
            get
            {
            
                if (_extentionRegexArray == null)
                {
                    try
                    {
                        _extentionRegexArray = extentions.Split(";").Select(o => new WildcardPattern(o)).ToArray();
                    }
                    catch (System.Exception ex)
                    {
                        throw new System.Exception($"File type {Path} has bad extentiuons list '{extentions}' : {ex.Message}");
                    }
                }
                return _extentionRegexArray;
            }
        }

        ///--------------------------------------------------------------------
        /// <summary>Gets the prefixes.</summary>
        ///
        /// <value>The prefixes.</value>
        ///--------------------------------------------------------------------

        public List<StringPair> Prefixes => prefixes;

        ///--------------------------------------------------------------------
        /// <summary>Gets the suffixes.</summary>
        ///
        /// <value>The suffixes.</value>
        ///--------------------------------------------------------------------

        public List<StringPair> Suffixes => suffixes;

        /// <summary>Default constructor.</summary>
        public FileType()
        {

        }

        ///--------------------------------------------------------------------
        /// <summary>Constructor.</summary>
        ///
        /// <param category="category">  The item type category.</param>
        /// <param category="category">     The category is equal to
        ///                                 cleanName or different.</param>
        /// <param category="extentions">   (Optional) The list of possible
        ///                                 item extentions.</param>
        ///
        /// ### <param category="description">  (Optional) The item type
        ///                                     description.</param>
        ///--------------------------------------------------------------------

        public FileType(string path, string extentions, string description = null)
        {
            Path = path;
            var tokens = path.Split("/");
            Debug.Assert(tokens.Length > 1, "Expected more than 1 token");
            Debug.Assert(tokens.Length < 3, "Expected less than tree tokens");
            Category    = tokens.Length >= 1 ? tokens[0] : string.Empty;
            Subcategory = tokens.Length >= 2 ? tokens[1] : string.Empty;
            Description = description;
            this.extentions = extentions;
        }

        ///--------------------------------------------------------------------
        /// <summary>Verify the extention is belongs of this type.</summary>
        ///
        /// <param category="fileExtention">The extention.</param>
        ///
        /// <returns>True if it succeeds, false if it fails.</returns>
        ///--------------------------------------------------------------------

        public bool VerifyExtention(string fileExtention)
        {
            var regexs = ExtentionRegexArray;
            foreach (var item in regexs)
            {
                if (item.IsMatch(fileExtention))
                    return true;
            }
            return false;
        }

        ///--------------------------------------------------------------------
        /// <summary>Validates the name described by item.</summary>
        ///
        /// <param name="item">The item.</param>
        ///
        /// <returns>An EFileState.</returns>
        ///--------------------------------------------------------------------

        public EFileState ValidateName(RenamableObject item)
        {
            if (!VerifyExtention(item.FileExt))
                return EFileState.Undefined;
            if (item.Tokens.Count == 1)
            {
                // The cleanName does not have preffix or suffix
                // It should be allowed by the empty value 
                var prefix = FindPrefix("");
                var suffix = FindSuffix("");
                return (prefix!=null && suffix!=null) ? EFileState.Valid : EFileState.Invalid;
            }
            else
            {
                int prefixes = 0;
                int suffixes = 0;
                foreach (var token in item.Tokens)
                {
                    var prefix = FindPrefix(token);
                    var suffix = FindSuffix(token);
                    if (prefix != null)
                        prefixes++;
                    if (suffix != null)
                        suffixes++;
                }
                return (prefixes > 0 && suffixes > 0) ? EFileState.Valid : EFileState.Invalid;
            }

        }

        ///--------------------------------------------------------------------
        /// <summary>Searches for the first category.</summary>
        ///
        /// <param category="prefix">File category.</param>
        ///
        /// <returns>The found category or null.</returns>
        ///--------------------------------------------------------------------

        public string FindPrefix(string prefix)
        {
            int idx = Prefixes.FindIndex(o => o.Value == prefix);
            return idx >= 0 ? Prefixes[idx].Value : null;
        }

        ///--------------------------------------------------------------------
        /// <summary>Searches for the first category.</summary>
        ///
        /// <param category="suffix">File category.</param>
        ///
        /// <returns>The found category or null.</returns>
        ///--------------------------------------------------------------------

        public string FindSuffix(string suffix)
        {
            int idx = Suffixes.FindIndex(o => o.Value == suffix);
            return idx >= 0 ? Suffixes[idx].Value : null;
        }

        ///--------------------------------------------------------------------
        /// <summary>Define preffix.</summary>
        ///
        /// <param category="category"> The item type category.</param>
        /// <param cleanName="prefix">  The preffix string.</param>
        /// <param cleanName="priority">    (Optional) True to make it hi
        ///                                 priority.</param>
        ///--------------------------------------------------------------------

        public void DefinePreffix(string name, string prefix, bool priority = true)
        {
            if (priority)
                Prefixes.Insert(0, new StringPair(name, prefix));
            else
                Prefixes.Add(new StringPair(name, prefix));
        }

        ///--------------------------------------------------------------------
        /// <summary>Define suffix.</summary>
        ///
        /// <param category="category"> The item type category.</param>
        /// <param cleanName="prefix">  The suffix string.</param>
        /// <param cleanName="priority">    (Optional) True to make it hi
        ///                                 priority.</param>
        ///--------------------------------------------------------------------

        public void DefineSuffix(string name, string prefix, bool priority = true)
        {
            if (priority)
                Suffixes.Insert(0, new StringPair(name, prefix));
            else
                Suffixes.Add(new StringPair(name, prefix));
        }

        ///--------------------------------------------------------------------
        /// <summary>Called when the script is loaded or a counter is changed
        /// in the inspector (Called in the editor only)</summary>
        ///--------------------------------------------------------------------
        
        public void OnValidate()
        {
            extentions = extentions.Replace(" ", "").Replace(",", ";");
            _extentionRegexArray = null; 
        }
    }

}