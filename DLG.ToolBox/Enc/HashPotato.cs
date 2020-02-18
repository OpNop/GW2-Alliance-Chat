using System.Security.Cryptography;
using System.Text;
using System.Web.Security;

namespace DLG.ToolBox.Enc
{
    public class HashPotato
    {
        // Methods
        public static byte[] DoMD5(string src)
        {
            var bytes = Encoding.Unicode.GetBytes(src);
            var md = new MD5CryptoServiceProvider();
            return md.ComputeHash(bytes);
        }

        public static byte[] DoWebMD5(string src)
        {
            var hash = FormsAuthentication.HashPasswordForStoringInConfigFile(src, "MD5");
            return (hash != null) ? Encoding.UTF8.GetBytes(hash) : null;
        }

        public static byte[] DoSHA1(string src)
        {
            var bytes = Encoding.Unicode.GetBytes(src);
            var sha = new SHA1CryptoServiceProvider();
            return sha.ComputeHash(bytes);
        }

        public static byte[] DoWebSHA1(string src)
        {
            var hash = FormsAuthentication.HashPasswordForStoringInConfigFile(src, "SHA1");
            return (hash != null) ? Encoding.UTF8.GetBytes(hash) : null;
        }

        public static byte[] DoSHA256(string src)
        {
            var bytes = Encoding.Unicode.GetBytes(src);
            var sha = new SHA256Managed();
            return sha.ComputeHash(bytes);
        }

        public static byte[] DoSHA384(string src)
        {
            var bytes = Encoding.Unicode.GetBytes(src);
            var sha = new SHA384Managed();
            return sha.ComputeHash(bytes);
        }

        public static byte[] DoSHA512(string src)
        {
            var bytes = Encoding.Unicode.GetBytes(src);
            var sha = new SHA512Managed();
            return sha.ComputeHash(bytes);
        }
    }
}