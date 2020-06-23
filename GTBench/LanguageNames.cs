using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GTBench
{
    public static class LanguageNames
    {
        private static readonly char[] Commas = { ' ', ',', ';' };

        public static string FromList(string list_of_codes)
        {
            var codes = list_of_codes.Split(Commas, StringSplitOptions.RemoveEmptyEntries);
            var sb = new StringBuilder();
            for (int i = 0; i < codes.Length; i++)
            {
                if (i == 0)
                {
                }
                else if (i < codes.Length - 1)
                {
                    sb.Append(", ");
                }
                else if (codes.Length == 2)
                {
                    sb.Append(" and ");
                }
                else
                {
                    sb.Append(", and "); // Oxford comma
                }
                sb.Append(FromCode(codes[i]));
            }
            return sb.ToString();
        }

        public static string FromCode(string code)
        {
            return CultureInfo.GetCultureInfo(code).EnglishName;
        }
    }
}
