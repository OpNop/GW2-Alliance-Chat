using System;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32;

namespace DLG.ToolBox.Utils
{
    public static class Reg
    {
        #region PInvoke
        [DllImport("Advapi32.dll")]
        static extern uint RegOpenKeyEx(UIntPtr hKey, string lpSubKey, uint ulOptions, int samDesired, out int phkResult);

        [DllImport("Advapi32.dll")]
        static extern uint RegCloseKey(int hKey);

        [DllImport("advapi32.dll", EntryPoint = "RegQueryValueEx")]
        public static extern int RegQueryValueEx(int hKey, string lpValueName, int lpReserved, ref RegistryValueKind lpType, StringBuilder lpData, ref uint lpcbData);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, EntryPoint = "RegQueryValueEx")]
        private static extern int RegQueryValueEx(int hKey, string lpValueName, int lpReserved, ref RegistryValueKind lpType, [Out] byte[] lpData, ref uint lpcbData);
        #endregion

        public static UIntPtr HKEY_LOCAL_MACHINE = new UIntPtr(0x80000002u);
        public static UIntPtr HKEY_CURRENT_USER = new UIntPtr(0x80000001u);

        public static string getRegKey64(this RegistryKey inKey, String inPropertyName)
        {
            var strKey = inKey.ToString();
            var regHive = strKey.Split('\\')[0];
            var regPath = strKey.Substring(strKey.IndexOf('\\') + 1);
            return getRegKey64(getRegHiveFromString(regHive), regPath, RegSAM.WOW64_64Key, inPropertyName);
        }

        public static string getRegKey32(this RegistryKey inKey, String inPropertyName)
        {
            var strKey = inKey.ToString();
            var regHive = strKey.Split('\\')[0];
            var regPath = strKey.Substring(strKey.IndexOf('\\') + 1);
            return getRegKey64(getRegHiveFromString(regHive), regPath, RegSAM.WOW64_32Key, inPropertyName);
        }

        public static byte[] getRegKey64AsByteArray(this RegistryKey inKey, String inPropertyName)
        {
            var strKey = inKey.ToString();
            var regHive = strKey.Split('\\')[0];
            var regPath = strKey.Substring(strKey.IndexOf('\\') + 1);
            return getRegKey64AsByteArray(getRegHiveFromString(regHive), regPath, RegSAM.WOW64_64Key, inPropertyName);
        }

        public static byte[] getRegKey32AsByteArray(this RegistryKey inKey, String inPropertyName)
        {
            var strKey = inKey.ToString();
            var regHive = strKey.Split('\\')[0];
            var regPath = strKey.Substring(strKey.IndexOf('\\') + 1);
            return getRegKey64AsByteArray(getRegHiveFromString(regHive), regPath, RegSAM.WOW64_32Key, inPropertyName);
        }

        private static UIntPtr getRegHiveFromString(string inString)
        {
            if (inString == "HKEY_LOCAL_MACHINE")
                return HKEY_LOCAL_MACHINE;
            if (inString == "HKEY_CURRENT_USER")
                return HKEY_CURRENT_USER;
            return UIntPtr.Zero;
        }

        static public string getRegKey64(UIntPtr inHive, String inKeyName, RegSAM in32or64key, String inPropertyName)
        {
            //UIntPtr HKEY_LOCAL_MACHINE = (UIntPtr)0x80000002;
            var hkey = 0;

            try
            {
                var lResult = RegOpenKeyEx(inHive, inKeyName, 0, (int)RegSAM.QueryValue | (int)in32or64key, out hkey);
                if (0 != lResult) return null;
                RegistryValueKind lpType = 0;
                uint lpcbData = 1024;
                var strBuffer = new StringBuilder(1024);
                RegQueryValueEx(hkey, inPropertyName, 0, ref lpType, strBuffer, ref lpcbData);
                var value = strBuffer.ToString();
                return value;
            }
            finally
            {
                if (0 != hkey) RegCloseKey(hkey);
            }
        }

        static public byte[] getRegKey64AsByteArray(UIntPtr inHive, String inKeyName, RegSAM in32or64key, String inPropertyName)
        {
            var hkey = 0;

            try
            {
                var lResult = RegOpenKeyEx(inHive, inKeyName, 0, (int)RegSAM.QueryValue | (int)in32or64key, out hkey);
                if (0 != lResult) return null;
                RegistryValueKind lpType = 0;
                uint lpcbData = 2048;

                // Just make a big buffer the first time
                var byteBuffer = new byte[1000];
                // The first time, get the real size
                RegQueryValueEx(hkey, inPropertyName, 0, ref lpType, byteBuffer, ref lpcbData);
                // Now create a correctly sized buffer
                byteBuffer = new byte[lpcbData];
                // now get the real value
                RegQueryValueEx(hkey, inPropertyName, 0, ref lpType, byteBuffer, ref lpcbData);

                return byteBuffer;
            }
            finally
            {
                if (0 != hkey) RegCloseKey(hkey);
            }
        }

        public enum RegSAM
        {
            QueryValue = 0x0001,
            SetValue = 0x0002,
            CreateSubKey = 0x0004,
            EnumerateSubKeys = 0x0008,
            Notify = 0x0010,
            CreateLink = 0x0020,
            WOW64_32Key = 0x0200,
            WOW64_64Key = 0x0100,
            WOW64_Res = 0x0300,
            Read = 0x00020019,
            Write = 0x00020006,
            Execute = 0x00020019,
            AllAccess = 0x000f003f
        }
    }
}
