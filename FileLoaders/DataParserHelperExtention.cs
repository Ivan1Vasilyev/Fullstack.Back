using Backend.FileLoaders.Tariffs.Models;
using System.Text.RegularExpressions;

namespace Backend.FileLoaders
{
    public static partial class DataParserHelperExtention
    {
        private static readonly Regex _numCheck = NumberRegex();
        private static readonly Regex _spaces = SpaceRegex();

        public static int? ParseInteger(this string data)
        {
            if (string.IsNullOrWhiteSpace(data))
            {
                return null;
            }

            data = data.Trim();

            if (int.TryParse(data, out var value))
            {
                return value;
            }

            if (_numCheck.IsMatch(data))
            {
                if (int.TryParse(_spaces.Replace(data, ""), out value))
                {
                    return value;
                }
            }

            return null;
        }

        public static bool ParseBoolean(this string data)
        {
            if (data == null)
            {
                return false;
            }
            else
            {
                data = data.Trim().ToLower();

                return data == "1" ||
                    data == "да" ||
                    data == "есть" ||
                    data == "в комплекте";
            }
        }

        public static long? ParseLong(this string data)
        {
            if (string.IsNullOrWhiteSpace(data))
            {
                return null;
            }

            data = data.Trim();

            if (long.TryParse(data, out var value))
            {
                return value;
            }

            if (_numCheck.IsMatch(data))
            {
                if (long.TryParse(_spaces.Replace(data, ""), out value))
                {
                    return value;
                }
            }

            return null;
        }

        [GeneratedRegex("^[\\s\\d]+$")]
        private static partial Regex NumberRegex();

        [GeneratedRegex("\\s+")]
        private static partial Regex SpaceRegex();

        public static InternetConnectionTehnologyEnum ParseInternetTechnology(this string data)
        {
            if (data == null)
            {
                return InternetConnectionTehnologyEnum.Unknown;
            }
            else
            {
                if (Enum.TryParse<InternetConnectionTehnologyEnum>(data, out var value))
                {
                    return value;
                }
                return InternetConnectionTehnologyEnum.Unknown;
            }
        }
    }
}
