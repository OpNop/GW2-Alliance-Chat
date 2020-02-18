using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Collections.ObjectModel;

namespace DLG.ToolBox.Utils
{
    public class Strings
    {
        public static string FormatFileSize(ulong size)
        {
            var dims = new[] {"KB", "MB", "GB", "TB"};

            if (size < 1024)
                return String.Format("{0} bytes", size);
            else
            {
                int k;
                for (k = 0; k < dims.Length - 1; k++)
                {
                    if (size < Math.Pow(1024, k + 2))
                        return String.Format("{0:.##} {1}", size / Math.Pow(1024, k + 1), dims[k]);
                }

                return String.Format("{0:.##} {1}", size / Math.Pow(1024, k + 1), dims[k]);
            }
        }

        public class PathClassIdentity
        {
            public string name;
            public int index;

            public PathClassIdentity(string name, int index)
            {
                this.name = name;
                this.index = index;
            }
        }

        public static Collection<PathClassIdentity> ParseClassPath(string path)
        {
            var parts = new Collection<PathClassIdentity>();

            path = path.Replace(" ", "");
            var pathParts = path.Split('.');

            if (pathParts.Length < 1)
                throw new ApplicationException(String.Format("Invalid class path: '{0}'", path));

            foreach (var pathPart in pathParts)
            {
                var name = pathPart;
                var index = 0;

                var match = Regex.Match(pathPart,
                    @"
							[^\w]*
							(?'name'[\w]+)
							\[  (?'index'[\d]+)   \]
							.*
						",
                    RegexOptions.IgnorePatternWhitespace);

                if (match.Success && match.Groups.Count == 3)
                {
                    try
                    {
                        name = match.Groups["name"].Value;
                        index = Convert.ToInt32(match.Groups["index"].Value);
                    }
                    catch
                    {
                    }
                }

                if (Regex.IsMatch(name, @"[^\w\d_]+") || name.Length < 1)
                {
                    if (pathPart == pathParts[pathParts.Length - 1] && name.EndsWith("[]"))
                    {
                        name = name.Remove(name.LastIndexOf("[]"));
                        if (Regex.IsMatch(name, @"[^\w\d_]+"))
                        {
                            throw new ApplicationException(String.Format("Invalid class path: '{0}'\nname: '{1}'", path, name));
                        }
                        else
                        {
                            parts.Add(new PathClassIdentity(name, -1));
                        }
                    }
                    else
                    {
                        throw new ApplicationException(String.Format("Invalid class path: '{0}'\nname: '{1}'", path, name));
                    }
                }
                else
                {
                    parts.Add(new PathClassIdentity(name, index));
                }
            }

            return parts;
        }

        /// <summary>
        /// Encodes string using triple-DES algorythm (returns in base64)
        /// </summary>
        /// <param name="str">source string</param>
        /// <param name="rgbKey">key vector for triple-DES algorythm (16 bytes or 128 bits)</param>
        /// <param name="rgbIV">IV vector for triple-DES algorythm (8 bytes or 64 bits)</param>
        /// <returns>encoded source string in base64</returns>
        public static string Encode(string str, string rgbKey, string rgbIV)
        {
            var cryptoProvider = new TripleDESCryptoServiceProvider();
            var encoder = cryptoProvider.CreateEncryptor(
                Encoding.Default.GetBytes(rgbKey),
                Encoding.Default.GetBytes(rgbIV));

            var strBytes = Encoding.Default.GetBytes(str);
            var strBytesEncoded = encoder.TransformFinalBlock(strBytes, 0, strBytes.Length);

            return Convert.ToBase64String(strBytesEncoded);
        }

        /// <summary>
        /// Decodes string using triple-DES algorythm
        /// </summary>
        /// <param name="str">source encoded string in base64</param>
        /// <param name="rgbKey">key vector for triple-DES algorythm (16 bytes or 128 bits)</param>
        /// <param name="rgbIV">IV vector for triple-DES algorythm (8 bytes or 64 bits)</param>
        /// <returns>decoded source string</returns>
        public static string Decode(string str, string rgbKey, string rgbIV)
        {
            var cryptoProvider = new TripleDESCryptoServiceProvider();
            var decoder = cryptoProvider.CreateDecryptor(
                Encoding.Default.GetBytes(rgbKey),
                Encoding.Default.GetBytes(rgbIV));

            var strBytes = Convert.FromBase64String(str);
            var strBytesDecoded = decoder.TransformFinalBlock(strBytes, 0, strBytes.Length);

            return Encoding.Default.GetString(strBytesDecoded);
        }
    }
}
