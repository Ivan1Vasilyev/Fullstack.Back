using NickBuhro.Translit;
using System.Text.RegularExpressions;

namespace Backend.Application.Utils
{
    public static partial class LoaderCodeGenerator
    {
        public static string GetCode(string text)
        {
            text = Transliteration.CyrillicToLatin(text.Trim(), Language.Russian);
            text = text.ToLower();
            text = text.Replace("`", "").Replace("\'", "").Replace("#", "");
            text = SpaceRegex().Replace(text, "-");
            text = SymbolRegex().Replace(text, "");
            return text;
        }

        [GeneratedRegex("\\s+")]
        private static partial Regex SpaceRegex();

        [GeneratedRegex("[^a-zа-я0-9-]")]
        private static partial Regex SymbolRegex();
    }

}
