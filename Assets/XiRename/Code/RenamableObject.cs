using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace XiRenameTool
{
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

        ///--------------------------------------------------------------------
        /// <summary>Constructor.</summary>
        ///
        /// <param cleanName="path">Full pathname of the file.</param>
        ///--------------------------------------------------------------------

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