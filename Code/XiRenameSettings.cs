using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XiCore.StringTools;

namespace XiRenameTool
{
    /// <summary>Values that represent category conventions.</summary>
    public enum ENameConvention { PascalCase, CamelCase, LowercaseUnderscore, LowercaseDash, UppercaseUnderscore, UppercaseDash }

    public enum ETokenType { Prefix, Name, Suffix, Variant };

    /// <summary>An xi rename settings.</summary>
    [CreateAssetMenu(fileName = "XiRenameSettings", menuName = "Xi/Settins/XiRenameSettings", order = 0)]
    public class XiRenameSettings : ScriptableObject
    { 

        public List<ETokenType> fieldOrder = new List<ETokenType>();

        public ENameConvention nameConvention = ENameConvention.PascalCase;
        public int prefixPrecision = 2;
        public int suffixPrecision = 2;
        public string[] ignorePath;

        /// <summary>List of file types.</summary>
        public List<FileType> fileTypes = new List<FileType>();


        ///--------------------------------------------------------------------
        /// <summary>Query if 'filePath' is ignored.</summary>
        ///
        /// <param name="filePath">Full pathname of the file.</param>
        ///
        /// <returns>True if ignored, false if not.</returns>
        ///--------------------------------------------------------------------
        
        public bool IsIgnored(string filePath)
        {
            foreach (var ignore in ignorePath)
            {
                if (filePath.Contains(ignore))
                    return true;
            }
            return false;
        }

        public EFileResult ValidateName(FileDescriptor desc, string fileCategory)
        {
            if (!desc.IsValid)
                return EFileResult.Invalid;
            if (IsIgnored(desc.DirectoryPath))
                return EFileResult.Ignore;
            var result = EFileResult.Invalid;
            var categories = 0;
            foreach (var type in fileTypes)
            {
                if (type.Category == fileCategory)
                {
                    categories++;
                    result = type.ValidateName(desc);
                    if (result == EFileResult.Valid)
                        return result;
                }
            }
            return categories != 0 ? result : EFileResult.Undefined;
        }

        ///--------------------------------------------------------------------
        /// <summary>Searches for the first match.</summary>
        ///
        /// <param name="name">        The name.</param>
        /// <param name="displayError"> (Optional) True to display error.</param>
        ///
        /// <returns>A FileType.</returns>
        ///--------------------------------------------------------------------

        public FileType Find(string name, bool displayError = false)
        {
            var result = fileTypes.Find(o => o.Name == name);
            if (displayError && result == null)
                Debug.LogError($"Can't find file type '{name}'", this);
            return result;
        }

        ///--------------------------------------------------------------------
        /// <summary>Searches for the first category.</summary>
        ///
        /// <param name="category">    The category.</param>
        /// <param name="displayError"> (Optional) True to display error.</param>
        ///
        /// <returns>The found category.</returns>
        ///--------------------------------------------------------------------

        public FileType FindByCategory(string category, bool displayError = false)
        {
            var result = fileTypes.Find(o => o.Category == category);
            if (displayError && result == null)
                Debug.LogError($"Can't find file type '{category}'", this);
            return result;
        }

        ///--------------------------------------------------------------------
        /// <summary>Searches for the first options by category.</summary>
        ///
        /// <param name="category">The category.</param>
        /// <param name="prefixes">The prefixes.</param>
        /// <param name="suffixes">The suffixes.</param>
        ///--------------------------------------------------------------------

        public void FindOptionsByCategory(string category, List<StringPair> prefixes, List<StringPair> suffixes)
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
            foreach (FileType type in fileTypes)
                type.OnValidate();
        }


        /// <summary>Initializes the defaut.</summary>
        [Button()]
        public void InitializeDefaut()
        {
            fieldOrder.Clear();
            fieldOrder.Add(ETokenType.Prefix);
            fieldOrder.Add(ETokenType.Name);
            fieldOrder.Add(ETokenType.Variant);
            fieldOrder.Add(ETokenType.Suffix);

            // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
            // Taken from https://github.com/justinwasilenko/Unity-Style-Guide
            // But modified for my needs
            // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
            fileTypes.Clear();
            var kind = Define("Models/FBX", "Models", ".fbx, .ma, .mb");
            kind.DefinePreffix("Characters", "CH");
            kind.DefinePreffix("Vehicles", "VH");
            kind.DefinePreffix("Weapons", "WP");
            kind.DefinePreffix("Static Mesh", "SM");
            kind.DefinePreffix("Skeletal Mesh", "SK");
            kind.DefinePreffix("Skeleton", "SKEL");
            kind.DefinePreffix("Rig", "RIG");
            // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
            kind = Define("Models/MAX", "Models", ".max");
            kind.DefineSuffix("Mesh", "_mesh_lod0*");
            kind.DefineSuffix("Mesh Collider", "collider");
            // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
            kind = Define("Animations", null, ".fbx, .ma, .mb");
            kind.DefinePreffix("Animation Clip", "A");
            kind.DefinePreffix("Animation Controller", "AC");
            kind.DefinePreffix("Avatar Mask", "AM");
            kind.DefinePreffix("Morph Target", "MT");
            // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
            kind = Define("Artificial Intelligence", "Assets", ".asset");
            kind.DefinePreffix("AI Controller", "AIC");
            kind.DefinePreffix("Behavior Tree", "BT");
            kind.DefinePreffix("Blackboard", "BB");
            kind.DefinePreffix("Decorator", "BTDecorator");
            kind.DefinePreffix("Service", "BTService");
            kind.DefinePreffix("Task", "BTTask");
            kind.DefinePreffix("Environment Query", "EQS");
            kind.DefinePreffix("EnvQueryContext", "EQC");
            kind.DefineSuffix("EnvQueryContext", "Context");
            // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
            kind = Define("Other Assets", "Assets", ".asset");
            kind.DefinePreffix("Scriptable Object", "SO");
            // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
            kind = Define("Prefabs", "Prefabs", ".prefab");
            kind.DefinePreffix("Prefab", "P");
            // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
            kind = Define("Textures", null, ".tga, .bmp, .jpg, .png, .gif, .psd");
            kind.DefinePreffix("Texture", "T");
            kind.DefinePreffix("Texture Cube", "TC");
            kind.DefinePreffix("Media Texture", "MT");
            kind.DefinePreffix("Render Target", "RT");
            kind.DefinePreffix("Cube Render Target", "RTC");
            kind.DefinePreffix("Texture Light Profile", "TLP");
            kind.DefineSuffix("Texture (Diffuse/Albedo/Base Color", "D");
            kind.DefineSuffix("Texture (Normal)", "N");
            kind.DefineSuffix("Texture (Roughness)", "R");
            kind.DefineSuffix("Texture (Alpha/Opacity)", "A");
            kind.DefineSuffix("Texture (Ambient Occlusion)", "AO");
            kind.DefineSuffix("Texture (Bump)", "B");
            kind.DefineSuffix("Texture (Mask)", "E");
            kind.DefineSuffix("Texture (Mask)", "M");
            kind.DefineSuffix("Texture (Specular)", "S");
            // It is generally acceptable to include an Alpha/ Opacity layer 
            // in your Diffuse/ Albedo's alpha channel and as this is common 
            // practice, adding A to the _D category is optional.
            kind.DefineSuffix("Texture (Packed)", "?");
            // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
            kind = Define("Miscellaneous", null, ".asset");
            kind.DefinePreffix("Universal Render Pipeline Asset", "URP");
            kind.DefinePreffix("Post Process Volume Profile", "PP");
            // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
            kind = Define("Physics", null, ".physicMaterial, .physicsMaterial2D");
            kind.DefinePreffix("Physical Material", "PM");
            // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
            kind = Define("Audio", null, ".mp3, .wav");
            kind.DefinePreffix("Audio (Class)", "");
            kind.DefinePreffix("Audio (Clip)", "A");
            kind.DefinePreffix("Audio (Mixer)", "AM");
            kind.DefinePreffix("Audio (Voice)", "AV");
            // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
            kind = Define("User Interface", null, ".asset");
            kind.DefinePreffix("UI (Interface)", "UI");
            kind.DefinePreffix("UI (Font)", "Font");
            kind.DefinePreffix("UI (Sprite)", "T");
            kind.DefineSuffix("UI (Sprite)", "GUI");
            // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
            kind = Define("Effects", "Prefabs", ".prefab");
            kind.DefinePreffix("FX (Particle System)", "PS");
            // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
            kind = Define("Shaders", "Shaders", ".compute, .raytrace, .shadervariants, .shader");
            kind.DefinePreffix("Shader", "SH");
            // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
            kind = Define("Fonts", "Fonts", ".ttf, .otf");
            kind.DefinePreffix("F", "F");
            // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
            kind = Define("Scenes", "Schemes", ".scene, .scenetemplate");
            kind.DefinePreffix("S", "F");
        }

        ///--------------------------------------------------------------------
        /// <summary>Defines.</summary>
        ///
        /// <param name="name">       The name.</param>
        /// <param name="category">   The category.</param>
        /// <param name="extentions"> The extentions.</param>
        /// <param name="description">(Optional) The description.</param>
        ///
        /// <returns>A FileType.</returns>
        ///--------------------------------------------------------------------

        private FileType Define(string name, string category, string extentions, string description = null)
        {
            var type = Find(name, false);
            if (type == null)
            {
                type = new FileType(name, category, extentions);
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

        ///------------------------------------------------------------------------
        /// <summary>Constructor.</summary>
        ///
        /// <param category="category"> The category.</param>
        /// <param category="counter">The counter.</param>
        ///------------------------------------------------------------------------

        public StringPair(string name, string value)
        {
            Name = name;
            Value = value;
        }
    }

    /// <summary>A single file type.</summary>
    [System.Serializable]
    public class FileType
    {
        /// <summary>The file type category.</summary>
        public string Name;
        /// <summary>The file type category.</summary>
        public string Category;
        /// <summary>The file type description.</summary>
        public string Description;
        /// <summary>The list of possible file extentions.</summary>
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
        private string[] _cachedExtentions;

        public string[] Extentions => _cachedExtentions ??= extentions.Replace(" ", "").Split(",");
        public List<StringPair> Prefixes => prefixes;
        public List<StringPair> Suffixes => suffixes;

        /// <summary>Default constructor.</summary>
        public FileType()
        {

        }

        ///------------------------------------------------------------------------
        /// <summary>Constructor.</summary>
        ///
        /// <param category="category">   The file type category.</param>
        /// <param category="category">   The category is equal to name or different.</param>
        /// <param category="extentions">  The list of possible file extentions.</param>
        /// <param category="description"> (Optional) The file type description.</param>
        ///------------------------------------------------------------------------

        public FileType(string name, string category, string extentions, string description = null)
        {
            Name = name;
            Category = category ?? name;
            Description = description;
            this.extentions = extentions;
        }

        ///------------------------------------------------------------------------
        /// <summary>Verify the extention is belongs of this type.</summary>
        ///
        /// <param category="fileExtention">The extention.</param>
        ///
        /// <returns>True if it succeeds, false if it fails.</returns>
        ///------------------------------------------------------------------------

        public bool VerifyExtention(string fileExtention)
        {
            return System.Array.IndexOf<string>(Extentions, fileExtention) >= 0;
        }

        public EFileResult ValidateName(FileDescriptor desc)
        {
            if (!VerifyExtention(desc.FileExt))
                return EFileResult.Undefined;
            if (desc.Tokens.Count == 1)
            {
                // The name does not have preffix or suffix
                // It should be allowed by the empty value 
                var prefix = FindPrefix("");
                var suffix = FindSuffix("");
                return (prefix!=null && suffix!=null) ? EFileResult.Valid : EFileResult.Invalid;
            }
            else
            {
                int prefixes = 0;
                int suffixes = 0;
                foreach (var token in desc.Tokens)
                {
                    var prefix = FindPrefix(token);
                    var suffix = FindSuffix(token);
                    if (prefix != null)
                        prefixes++;
                    if (suffix != null)
                        suffixes++;
                }
                return (prefixes > 0 && suffixes > 0) ? EFileResult.Valid : EFileResult.Invalid;
            }

        }

        ///------------------------------------------------------------------------
        /// <summary>Searches for the first category.</summary>
        ///
        /// <param category="prefix">File category.</param>
        ///
        /// <returns>The found category or null.</returns>
        ///
        /// ### <param category="fileExtention">The extention.</param>
        ///------------------------------------------------------------------------

        public string FindPrefix(string prefix)
        {
            int idx = Prefixes.FindIndex(o => o.Value == prefix);
            return idx >= 0 ? Prefixes[idx].Value : null;
        }

        ///------------------------------------------------------------------------
        /// <summary>Searches for the first category.</summary>
        ///
        /// <param category="suffix">File category.</param>
        ///
        /// <returns>The found category or null.</returns>
        ///------------------------------------------------------------------------

        public string FindSuffix(string suffix)
        {
            int idx = Suffixes.FindIndex(o => o.Value == suffix);
            return idx >= 0 ? Suffixes[idx].Value : null;
        }

        public void DefinePreffix(string name, string prefix, bool priority = true)
        {
            if (priority)
                Prefixes.Insert(0, new StringPair(name, prefix));
            else
                Prefixes.Add(new StringPair(name, prefix));
        }
        public void DefineSuffix(string name, string prefix, bool priority = true)
        {
            if (priority)
                Suffixes.Insert(0, new StringPair(name, prefix));
            else
                Suffixes.Add(new StringPair(name, prefix));
        }

        ///------------------------------------------------------------------------
        /// <summary>Called when the script is loaded or a counter is changed in the
        /// inspector (Called in the editor only)</summary>
        ///------------------------------------------------------------------------

        public void OnValidate()
        {
            _cachedExtentions = null;
        }
    }

}