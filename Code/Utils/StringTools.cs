
using System;
using UnityEngine;

namespace XiRenameTool.Utils
{
    /// <summary>An utilities.</summary>
    public static class StringTools
    {
 
        ///--------------------------------------------------------------------
        /// <summary>UpdateMask_Bit ' ', '-' or '_' to change forms:
        ///   "foo-bar"  ->  "FooBar" "foo_bar"  ->  "FooBar" "foo bar"  ->
        ///   "FooBar".</summary>
        ///
        /// <param cleanName="str">       .</param>
        /// <param cleanName="capitalize">   (Optional) Should be capitalized
        ///                             first character or not.</param>
        ///
        /// <returns>A string.</returns>
        ///--------------------------------------------------------------------

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
                    if ((char.IsUpper(c) && i != 0) || capitalize)
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

        ///--------------------------------------------------------------------
        /// <summary>Convert 'FooBar' to 'foo-bar'.</summary>
        ///
        /// <param cleanName="str">      .</param>
        /// <param cleanName="separator">(Optional) The separator.</param>
        ///
        /// <returns>A string.</returns>
        ///--------------------------------------------------------------------

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

        ///--------------------------------------------------------------------
        /// <summary>Makes a prefix.</summary>
        ///
        /// <exception cref="Exception">    Thrown when an exception error
        ///                                 condition occurs.</exception>
        ///
        /// <param cleanName="name">          The cleanName.</param>
        /// <param fileName="convention">The convention.</param>
        ///
        /// <returns>A string.</returns>
        ///--------------------------------------------------------------------

        public static string MakePrefix(string name, ENameConvention convention)
        {
            switch (convention)
            {
                case ENameConvention.PascalCase:
                    return name.ToUpper();
                case ENameConvention.CamelCase:
                    return name.ToUpper();
                case ENameConvention.LowercaseUnderscore:
                    return StringTools.Decamelize(name, '_');
                case ENameConvention.LowercaseDash:
                    return StringTools.Decamelize(name, '-');
                case ENameConvention.UppercaseUnderscore:
                    return StringTools.Decamelize(name, '_').ToUpper();
                case ENameConvention.UppercaseDash:
                    return StringTools.Decamelize(name, '-').ToUpper();
            }
            throw new System.Exception("NotImplemented");
        }

        ///--------------------------------------------------------------------
        /// <summary>Makes a suffix.</summary>
        ///
        /// <exception cref="Exception">    Thrown when an exception error
        ///                                 condition occurs.</exception>
        ///
        /// <param cleanName="name">          The cleanName.</param>
        /// <param fileName="convention">The convention.</param>
        ///
        /// <returns>A string.</returns>
        ///--------------------------------------------------------------------

        public static string MakeSuffix(string name, ENameConvention convention)
        {
            switch (convention)
            {
                case ENameConvention.PascalCase:
                    return name; //< because settings are in the pascal case
                case ENameConvention.CamelCase:
                    return StringTools.Camelize(name, false);
                case ENameConvention.LowercaseUnderscore:
                    return StringTools.Decamelize(name, '_');
                case ENameConvention.LowercaseDash:
                    return StringTools.Decamelize(name, '-');
                case ENameConvention.UppercaseUnderscore:
                    return StringTools.Decamelize(name, '_').ToUpper();
                case ENameConvention.UppercaseDash:
                    return StringTools.Decamelize(name, '-').ToUpper();
            }
            throw new System.Exception("NotImplemented");
        }


        ///--------------------------------------------------------------------
        /// <summary>Makes a cleanName.</summary>
        ///
        /// <exception cref="Exception">    Thrown when an exception error
        ///                                 condition occurs.</exception>
        ///
        /// <param cleanName="name">          The cleanName.</param>
        /// <param fileName="convention">The convention.</param>
        ///
        /// <returns>A string.</returns>
        ///--------------------------------------------------------------------

        public static string MakeName(string name, ENameConvention convention)
        {
            switch (convention)
            {
                case ENameConvention.PascalCase:
                    return StringTools.Camelize(name, true);
                case ENameConvention.CamelCase:
                    return StringTools.Camelize(name, false);
                case ENameConvention.LowercaseUnderscore:
                    return StringTools.Decamelize(name, '_');
                case ENameConvention.LowercaseDash:
                    return StringTools.Decamelize(name, '-');
                case ENameConvention.UppercaseUnderscore:
                    return StringTools.Decamelize(name, '_').ToUpper();
                case ENameConvention.UppercaseDash:
                    return StringTools.Decamelize(name, '-').ToUpper();
            }
            throw new System.Exception("NotImplemented");
        }
    }
}