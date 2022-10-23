
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using XiRenameTool.Utils;

namespace XiRenameTool
{
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
            XiRename.DoUpdateGUI |= true;
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
            if (Mode == ERenameAdvancedMode.Format)
            {

            }

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
                        XiRename.DoUpdateGUI |= true;
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
                    XiRename.DoUpdateGUI |= true;
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
                    XiRename.DoUpdateGUI |= true;
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
}