﻿using System;
using System.Runtime.InteropServices;

namespace DLG.ToolBox.Utils
{
    public static class OSInfo
    {
        /// <summary>
        /// Determines if the current application is 32 or 64-bit.
        /// </summary>
        public static int Bits
        {
            get { return Environment.Is64BitOperatingSystem ? 64 : 32; }
        }

        private static string _sEdition;

        /// <summary>
        /// Gets the edition of the operating system running on this computer.
        /// </summary>
        public static string Edition
        {
            get
            {
                if (_sEdition != null)
                    return _sEdition; //***** RETURN *****//

                var edition = String.Empty;
                var osVersion = Environment.OSVersion;
                var osVersionInfo = new OSVERSIONINFOEX();
                osVersionInfo.dwOSVersionInfoSize = Marshal.SizeOf(typeof (OSVERSIONINFOEX));

                if (GetVersionEx(ref osVersionInfo))
                {
                    var majorVersion = osVersion.Version.Major;
                    var minorVersion = osVersion.Version.Minor;
                    var productType = osVersionInfo.wProductType;
                    var suiteMask = osVersionInfo.wSuiteMask;

                    switch (majorVersion)
                    {
                        case 4:
                            switch (productType)
                            {
                                case VER_NT_WORKSTATION:
                                    edition = "Workstation";
                                    break;
                                case VER_NT_SERVER:
                                    edition = (suiteMask & VER_SUITE_ENTERPRISE) != 0 ? "Enterprise Server" : "Standard Server";
                                    break;
                            }
                            break;
                        case 5:
                            switch (productType)
                            {
                                case VER_NT_WORKSTATION:
                                    edition = (suiteMask & VER_SUITE_PERSONAL) != 0 ? "Home" : "Professional";
                                    break;
                                case VER_NT_SERVER:
                                    if (minorVersion == 0)
                                    {
                                        if ((suiteMask & VER_SUITE_DATACENTER) != 0)
                                        {
                                            // Windows 2000 Datacenter Server
                                            edition = "Datacenter Server";
                                        }
                                        else if ((suiteMask & VER_SUITE_ENTERPRISE) != 0)
                                        {
                                            // Windows 2000 Advanced Server
                                            edition = "Advanced Server";
                                        }
                                        else
                                        {
                                            // Windows 2000 Server
                                            edition = "Server";
                                        }
                                    }
                                    else
                                    {
                                        if ((suiteMask & VER_SUITE_DATACENTER) != 0)
                                        {
                                            // Windows Server 2003 Datacenter Edition
                                            edition = "Datacenter";
                                        }
                                        else if ((suiteMask & VER_SUITE_ENTERPRISE) != 0)
                                        {
                                            // Windows Server 2003 Enterprise Edition
                                            edition = "Enterprise";
                                        }
                                        else if ((suiteMask & VER_SUITE_BLADE) != 0)
                                        {
                                            // Windows Server 2003 Web Edition
                                            edition = "Web Edition";
                                        }
                                        else
                                        {
                                            // Windows Server 2003 Standard Edition
                                            edition = "Standard";
                                        }
                                    }
                                    break;
                            }
                            break;
                        case 6:
                            {
                                int ed;
                                if (GetProductInfo(majorVersion, minorVersion, osVersionInfo.wServicePackMajor, osVersionInfo.wServicePackMinor, out ed))
                                {
                                    switch (ed)
                                    {
                                        case PRODUCT_BUSINESS:
                                            edition = "Business";
                                            break;
                                        case PRODUCT_BUSINESS_N:
                                            edition = "Business N";
                                            break;
                                        case PRODUCT_CLUSTER_SERVER:
                                            edition = "HPC Edition";
                                            break;
                                        case PRODUCT_DATACENTER_SERVER:
                                            edition = "Datacenter Server";
                                            break;
                                        case PRODUCT_DATACENTER_SERVER_CORE:
                                            edition = "Datacenter Server (core installation)";
                                            break;
                                        case PRODUCT_ENTERPRISE:
                                            edition = "Enterprise";
                                            break;
                                        case PRODUCT_ENTERPRISE_N:
                                            edition = "Enterprise N";
                                            break;
                                        case PRODUCT_ENTERPRISE_SERVER:
                                            edition = "Enterprise Server";
                                            break;
                                        case PRODUCT_ENTERPRISE_SERVER_CORE:
                                            edition = "Enterprise Server (core installation)";
                                            break;
                                        case PRODUCT_ENTERPRISE_SERVER_CORE_V:
                                            edition = "Enterprise Server without Hyper-V (core installation)";
                                            break;
                                        case PRODUCT_ENTERPRISE_SERVER_IA64:
                                            edition = "Enterprise Server for Itanium-based Systems";
                                            break;
                                        case PRODUCT_ENTERPRISE_SERVER_V:
                                            edition = "Enterprise Server without Hyper-V";
                                            break;
                                        case PRODUCT_HOME_BASIC:
                                            edition = "Home Basic";
                                            break;
                                        case PRODUCT_HOME_BASIC_N:
                                            edition = "Home Basic N";
                                            break;
                                        case PRODUCT_HOME_PREMIUM:
                                            edition = "Home Premium";
                                            break;
                                        case PRODUCT_HOME_PREMIUM_N:
                                            edition = "Home Premium N";
                                            break;
                                        case PRODUCT_HYPERV:
                                            edition = "Microsoft Hyper-V Server";
                                            break;
                                        case PRODUCT_MEDIUMBUSINESS_SERVER_MANAGEMENT:
                                            edition = "Windows Essential Business Management Server";
                                            break;
                                        case PRODUCT_MEDIUMBUSINESS_SERVER_MESSAGING:
                                            edition = "Windows Essential Business Messaging Server";
                                            break;
                                        case PRODUCT_MEDIUMBUSINESS_SERVER_SECURITY:
                                            edition = "Windows Essential Business Security Server";
                                            break;
                                        case PRODUCT_SERVER_FOR_SMALLBUSINESS:
                                            edition = "Windows Essential Server Solutions";
                                            break;
                                        case PRODUCT_SERVER_FOR_SMALLBUSINESS_V:
                                            edition = "Windows Essential Server Solutions without Hyper-V";
                                            break;
                                        case PRODUCT_SMALLBUSINESS_SERVER:
                                            edition = "Windows Small Business Server";
                                            break;
                                        case PRODUCT_STANDARD_SERVER:
                                            edition = "Standard Server";
                                            break;
                                        case PRODUCT_STANDARD_SERVER_CORE:
                                            edition = "Standard Server (core installation)";
                                            break;
                                        case PRODUCT_STANDARD_SERVER_CORE_V:
                                            edition = "Standard Server without Hyper-V (core installation)";
                                            break;
                                        case PRODUCT_STANDARD_SERVER_V:
                                            edition = "Standard Server without Hyper-V";
                                            break;
                                        case PRODUCT_STARTER:
                                            edition = "Starter";
                                            break;
                                        case PRODUCT_STORAGE_ENTERPRISE_SERVER:
                                            edition = "Enterprise Storage Server";
                                            break;
                                        case PRODUCT_STORAGE_EXPRESS_SERVER:
                                            edition = "Express Storage Server";
                                            break;
                                        case PRODUCT_STORAGE_STANDARD_SERVER:
                                            edition = "Standard Storage Server";
                                            break;
                                        case PRODUCT_STORAGE_WORKGROUP_SERVER:
                                            edition = "Workgroup Storage Server";
                                            break;
                                        case PRODUCT_UNDEFINED:
                                            edition = "Unknown product";
                                            break;
                                        case PRODUCT_ULTIMATE:
                                            edition = "Ultimate";
                                            break;
                                        case PRODUCT_ULTIMATE_N:
                                            edition = "Ultimate N";
                                            break;
                                        case PRODUCT_WEB_SERVER:
                                            edition = "Web Server";
                                            break;
                                        case PRODUCT_WEB_SERVER_CORE:
                                            edition = "Web Server (core installation)";
                                            break;
                                    }
                                }
                            }
                            break;
                    }
                }

                _sEdition = edition;
                return edition;
            }
        }

        private static string _sName;

        /// <summary>
        /// Gets the name of the operating system running on this computer.
        /// </summary>
        public static string Name
        {
            get
            {
                if (_sName != null)
                    return _sName; //***** RETURN *****//

                var name = "unknown";
                var osVersion = Environment.OSVersion;
                var osVersionInfo = new OSVERSIONINFOEX();
                osVersionInfo.dwOSVersionInfoSize = Marshal.SizeOf(typeof (OSVERSIONINFOEX));

                if (GetVersionEx(ref osVersionInfo))
                {
                    var majorVersion = osVersion.Version.Major;
                    var minorVersion = osVersion.Version.Minor;

                    switch (osVersion.Platform)
                    {
                        case PlatformID.Win32Windows:
                            {
                                if (majorVersion == 4)
                                {
                                    var csdVersion = osVersionInfo.szCSDVersion;
                                    switch (minorVersion)
                                    {
                                        case 0:
                                            if (csdVersion == "B" || csdVersion == "C")
                                                name = "Windows 95 OSR2";
                                            else
                                                name = "Windows 95";
                                            break;
                                        case 10:
                                            name = csdVersion == "A" ? "Windows 98 Second Edition" : "Windows 98";
                                            break;
                                        case 90:
                                            name = "Windows Me";
                                            break;
                                    }
                                }
                                break;
                            }

                        case PlatformID.Win32NT:

                            var productType = osVersionInfo.wProductType;

                            switch (majorVersion)
                            {
                                case 3:
                                    name = "Windows NT 3.51";
                                    break;
                                case 4:
                                    switch (productType)
                                    {
                                        case VER_NT_WORKSTATION:
                                            name = "Windows NT 4.0";
                                            break;
                                        case VER_NT_SERVER:
                                            name = "Windows NT 4.0 Server";
                                            break;
                                    }
                                    break;
                                case 5:
                                    switch (minorVersion)
                                    {
                                        case 0:
                                            name = "Windows 2000";
                                            break;
                                        case 1:
                                            name = "Windows XP";
                                            break;
                                        case 2:
                                            name = "Windows Server 2003";
                                            break;
                                    }
                                    break;
                                case 6:
                                    switch (productType)
                                    {
                                        case VER_NT_WORKSTATION:
                                            switch (minorVersion)
                                            {
                                                case 0:
                                                    name = "Windows Vista";
                                                    break;
                                                case 1:
                                                    name = "Windows 7";
                                                    break;
                                            }
                                            break;
                                        case VER_NT_SERVER:
                                            switch (minorVersion)
                                            {
                                                case 0:
                                                    name = "Windows Server 2008";
                                                    break;
                                                case 1:
                                                    name = "Windows Server 2008 R2";
                                                    break;
                                            }
                                            break;
                                    }
                                    break;
                            }
                            break;
                    }
                }
                _sName = name;
                return name;
            }
        }

        [DllImport("Kernel32.dll")]
        internal static extern bool GetProductInfo(int osMajorVersion, int osMinorVersion, int spMajorVersion, int spMinorVersion, out int edition);

        [DllImport("kernel32.dll")]
        private static extern bool GetVersionEx(ref OSVERSIONINFOEX osVersionInfo);

        [StructLayout(LayoutKind.Sequential)]
        private struct OSVERSIONINFOEX
        {
            public int dwOSVersionInfoSize;
            public int dwMajorVersion;
            public int dwMinorVersion;
            public int dwBuildNumber;
            public int dwPlatformId;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)] public string szCSDVersion;
            public short wServicePackMajor;
            public short wServicePackMinor;
            public short wSuiteMask;
            public byte wProductType;
            public byte wReserved;
        }

        private const int PRODUCT_UNDEFINED = 0x00000000;
        private const int PRODUCT_ULTIMATE = 0x00000001;
        private const int PRODUCT_HOME_BASIC = 0x00000002;
        private const int PRODUCT_HOME_PREMIUM = 0x00000003;
        private const int PRODUCT_ENTERPRISE = 0x00000004;
        private const int PRODUCT_HOME_BASIC_N = 0x00000005;
        private const int PRODUCT_BUSINESS = 0x00000006;
        private const int PRODUCT_STANDARD_SERVER = 0x00000007;
        private const int PRODUCT_DATACENTER_SERVER = 0x00000008;
        private const int PRODUCT_SMALLBUSINESS_SERVER = 0x00000009;
        private const int PRODUCT_ENTERPRISE_SERVER = 0x0000000A;
        private const int PRODUCT_STARTER = 0x0000000B;
        private const int PRODUCT_DATACENTER_SERVER_CORE = 0x0000000C;
        private const int PRODUCT_STANDARD_SERVER_CORE = 0x0000000D;
        private const int PRODUCT_ENTERPRISE_SERVER_CORE = 0x0000000E;
        private const int PRODUCT_ENTERPRISE_SERVER_IA64 = 0x0000000F;
        private const int PRODUCT_BUSINESS_N = 0x00000010;
        private const int PRODUCT_WEB_SERVER = 0x00000011;
        private const int PRODUCT_CLUSTER_SERVER = 0x00000012;
        private const int PRODUCT_HOME_SERVER = 0x00000013;
        private const int PRODUCT_STORAGE_EXPRESS_SERVER = 0x00000014;
        private const int PRODUCT_STORAGE_STANDARD_SERVER = 0x00000015;
        private const int PRODUCT_STORAGE_WORKGROUP_SERVER = 0x00000016;
        private const int PRODUCT_STORAGE_ENTERPRISE_SERVER = 0x00000017;
        private const int PRODUCT_SERVER_FOR_SMALLBUSINESS = 0x00000018;
        private const int PRODUCT_SMALLBUSINESS_SERVER_PREMIUM = 0x00000019;
        private const int PRODUCT_HOME_PREMIUM_N = 0x0000001A;
        private const int PRODUCT_ENTERPRISE_N = 0x0000001B;
        private const int PRODUCT_ULTIMATE_N = 0x0000001C;
        private const int PRODUCT_WEB_SERVER_CORE = 0x0000001D;
        private const int PRODUCT_MEDIUMBUSINESS_SERVER_MANAGEMENT = 0x0000001E;
        private const int PRODUCT_MEDIUMBUSINESS_SERVER_SECURITY = 0x0000001F;
        private const int PRODUCT_MEDIUMBUSINESS_SERVER_MESSAGING = 0x00000020;
        private const int PRODUCT_SERVER_FOR_SMALLBUSINESS_V = 0x00000023;
        private const int PRODUCT_STANDARD_SERVER_V = 0x00000024;
        private const int PRODUCT_ENTERPRISE_SERVER_V = 0x00000026;
        private const int PRODUCT_STANDARD_SERVER_CORE_V = 0x00000028;
        private const int PRODUCT_ENTERPRISE_SERVER_CORE_V = 0x00000029;
        private const int PRODUCT_HYPERV = 0x0000002A;

        private const int VER_NT_WORKSTATION = 1;
        private const int VER_NT_DOMAIN_CONTROLLER = 2;
        private const int VER_NT_SERVER = 3;
        private const int VER_SUITE_SMALLBUSINESS = 1;
        private const int VER_SUITE_ENTERPRISE = 2;
        private const int VER_SUITE_TERMINAL = 16;
        private const int VER_SUITE_DATACENTER = 128;
        private const int VER_SUITE_SINGLEUSERTS = 256;
        private const int VER_SUITE_PERSONAL = 512;
        private const int VER_SUITE_BLADE = 1024;

        /// <summary>
        /// Gets the service pack information of the operating system running on this computer.
        /// </summary>
        public static string ServicePack
        {
            get
            {
                var servicePack = String.Empty;
                var osVersionInfo = new OSVERSIONINFOEX();

                osVersionInfo.dwOSVersionInfoSize = Marshal.SizeOf(typeof (OSVERSIONINFOEX));

                if (GetVersionEx(ref osVersionInfo))
                {
                    servicePack = osVersionInfo.szCSDVersion;
                }

                return servicePack;
            }
        }

        /// <summary>
        /// Gets the build version number of the operating system running on this computer.
        /// </summary>
        public static int BuildVersion
        {
            get { return Environment.OSVersion.Version.Build; }
        }

        /// <summary>
        /// Gets the full version string of the operating system running on this computer.
        /// </summary>
        public static string VersionString
        {
            get { return Environment.OSVersion.Version.ToString(); }
        }

        /// <summary>
        /// Gets the full version of the operating system running on this computer.
        /// </summary>
        public static Version Version
        {
            get { return Environment.OSVersion.Version; }
        }

        /// <summary>
        /// Gets the major version number of the operating system running on this computer.
        /// </summary>
        public static int MajorVersion
        {
            get { return Environment.OSVersion.Version.Major; }
        }

        /// <summary>
        /// Gets the minor version number of the operating system running on this computer.
        /// </summary>
        public static int MinorVersion
        {
            get { return Environment.OSVersion.Version.Minor; }
        }

        /// <summary>
        /// Gets the revision version number of the operating system running on this computer.
        /// </summary>
        public static int RevisionVersion
        {
            get { return Environment.OSVersion.Version.Revision; }
        }
    }
}