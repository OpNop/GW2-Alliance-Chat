using System.IO;
using System.Linq;
using System.Management;
using System.Text;
using Microsoft.Win32;

namespace DLG.ToolBox.Utils
{
    public class SystemInfo
    {
        public static string getTotalPhysicalMemory()
        {
            var size = (int) (new Microsoft.VisualBasic.Devices.ComputerInfo().TotalPhysicalMemory/1024/1024);
            return string.Format("{0:#,##0} MB", size);
        }

        public static string getProcessorModel()
        {
            return Registry.LocalMachine.OpenSubKey(@"Hardware\Description\System\CentralProcessor\0").GetValue("ProcessorNameString").ToString().Replace("(tm)","");
        }

        public static string getProcessorSpeed()
        {
            var speed = (int)Registry.LocalMachine.OpenSubKey(@"Hardware\Description\System\CentralProcessor\0").GetValue("~MHz");
            return string.Format("{0:#.#0} GHz", (float)speed / 1000);
        }

        public static string getMotherboardModel()
        {
            var query = new SelectQuery("Win32_BaseBoard");
            var search = new ManagementObjectSearcher(query);
            foreach (var motherboard in search.Get())
            {
                return motherboard["Product"].ToString();
            }
            return "Could Not Detect!";
        }

        public static string getMotherboardManufacturer()
        {
            var query = new SelectQuery("Win32_BaseBoard");
            var search = new ManagementObjectSearcher(query);
            foreach (var motherboard in search.Get())
            {
                return motherboard["Manufacturer"].ToString();
            }
            return "Could Not Detect!";
        }

        public static string getVideoCard()
        {
            var query = new SelectQuery("Win32_VideoController");
            var search = new ManagementObjectSearcher(query);
            foreach (var videocard in search.Get())
            {
                if(videocard["Name"] != null)
                    return videocard["Name"].ToString();
            }
            return "Could Not Detect!";
        }

        public static string getHardDriveSize()
        {
            var di = DriveInfo.GetDrives();
            var drives = new StringBuilder();
            foreach (var drive in di.Where(drive => drive.DriveType == DriveType.Fixed))
            {
                drives.AppendLine(string.Format("Drive {0} {1} GB ({2:#.##} GB)", drive.Name, (drive.TotalSize / 1000 / 1000 / 1000), (drive.TotalSize / 1024f / 1024f / 1024f)));
            }
            return drives.ToString();
        }
    }
}
