using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XiRenameTool.Utils
{
    /// <summary>A suffix has the literal before and after and the number is between</summary>
    /// <example>foo256bar</example>
    internal class FileNumber
    {
        public string prefix;
        public string suffix;
        public int id;


        public FileNumber(string name)
        {
            id = 0;
            var digitStartAt = -1;
            var digitEndsAt = -1;

            // Find end of digits
            for (var i = name.Length - 1; i >= 0; i--)
            {
                var c = name[i];
                if (c >= '0' && c <= '9')
                {
                    digitEndsAt = i;
                    break;
                }
            }

            if (digitEndsAt>0)
            {
                // Find start of digits
                for (var i = digitEndsAt; i >= 0; i--)
                {
                    var c = name[i];
                    if (c >= '0' && c <= '9')
                        continue;
                    digitStartAt = i + 1;
                    break;
                }
            }

            // convert digits to ID
            if (digitStartAt >= 0)
            {
                prefix = name.Substring(0, digitStartAt).Replace(" ", "_").Replace("-", "_"); ;
                var digits = name.Substring(digitStartAt, digitEndsAt - digitStartAt + 1);
                id = int.Parse(digits);

                if (digitEndsAt < name.Length)
                    suffix = name.Substring(digitEndsAt + 1).Replace(" ", "_").Replace("-", "_");
                else
                    suffix = String.Empty;
            }
            else
            {
                prefix = name.Replace(" ", "_").Replace("-", "_"); 
                suffix = String.Empty;
            }
        }

        public string GetString() => $"{prefix}{id}{suffix}";
        public string GetString(string format, bool addNumberToZero, char separator) => GetString(id, format, addNumberToZero, separator);
        public string GetString(int newid, string format, bool addNumberToZero, char separator)
        {
            var sep = new String(separator,1);
            if (id != 0)
                return $"{prefix}{newid.ToString(format)}{suffix}".Replace("_", sep);
            if (addNumberToZero)
                return $"{prefix}{separator}{newid.ToString(format)}{suffix}".Replace("_", sep);
            return $"{prefix}{suffix}".Replace("_", sep);
        }
    }
}
