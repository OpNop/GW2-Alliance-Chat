using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using DLG.ToolBox.Enc;

namespace DLG.ToolBox.Utils
{

    public class Fingerprint
    //Fingerprints the hardware
    {
        public string Value()
        {
            return pack(cpuId()
                        + biosId()
                        + diskId()
                        + baseId()
                        + videoId()
                        + macId());
        }

        private static string identifier(string wmiClass, string wmiProperty, string wmiMustBeTrue)
        //Return a hardware identifier
        {
            var result = "";
            var mc = new ManagementClass(wmiClass);
            var moc = mc.GetInstances();
            foreach (ManagementObject mo in moc)
            {
                if (mo[wmiMustBeTrue].ToString() == "True")
                {

                    //Only get the first one
                    if (result == "")
                    {
                        try
                        {
                            result = mo[wmiProperty].ToString();
                            break;
                        }
                        catch
                        {
                        }
                    }

                }
            }
            return result;
        }

        private static string identifier(string wmiClass, string wmiProperty)
        //Return a hardware identifier
        {
            var result = "";
            var mc = new ManagementClass(wmiClass);
            var moc = mc.GetInstances();
            foreach (var mo in moc)
            {

                //Only get the first one
                if (result == "")
                {
                    try
                    {
                        result = mo[wmiProperty].ToString();
                        break;
                    }
                    catch
                    {
                    }
                }

            }
            return result;
        }

        private static string cpuId()
        {
            //Uses first CPU identifier available in order of preference
            //Don't get all identifiers, as very time consuming
            var retVal = identifier("Win32_Processor", "UniqueId");
            if (retVal == "") //If no UniqueID, use ProcessorID
            {
                retVal = identifier("Win32_Processor", "ProcessorId");

                if (retVal == "") //If no ProcessorId, use Name
                {
                    retVal = identifier("Win32_Processor", "Name");


                    if (retVal == "") //If no Name, use Manufacturer
                    {
                        retVal = identifier("Win32_Processor", "Manufacturer");
                    }

                    //Add clock speed for extra security
                    retVal += identifier("Win32_Processor", "MaxClockSpeed");
                }
            }

            return retVal;

        }

        private static string biosId()
        //BIOS Identifier
        {
            return identifier("Win32_BIOS", "Manufacturer")
                   + identifier("Win32_BIOS", "SMBIOSBIOSVersion")
                   + identifier("Win32_BIOS", "IdentificationCode")
                   + identifier("Win32_BIOS", "SerialNumber")
                   + identifier("Win32_BIOS", "ReleaseDate")
                   + identifier("Win32_BIOS", "Version");
        }

        private static string diskId()
        //Main physical hard drive ID
        {
            return identifier("Win32_DiskDrive", "Model")
                   + identifier("Win32_DiskDrive", "Manufacturer")
                   + identifier("Win32_DiskDrive", "Signature")
                   + identifier("Win32_DiskDrive", "TotalHeads");
        }

        private static string baseId()
        //Motherboard ID
        {
            return identifier("Win32_BaseBoard", "Model")
                   + identifier("Win32_BaseBoard", "Manufacturer")
                   + identifier("Win32_BaseBoard", "Name")
                   + identifier("Win32_BaseBoard", "SerialNumber");
        }

        private static string videoId()
        //Primary video controller ID
        {
            return identifier("Win32_VideoController", "DriverVersion")
                   + identifier("Win32_VideoController", "Name");
        }

        private static string macId()
        //First enabled network card ID
        {
            return identifier("Win32_NetworkAdapterConfiguration", "MACAddress", "IPEnabled");
        }

        private static string pack(string text)
        //Packs the string to 8 digits
        {
            
            var crc = new Crc32();
            return crc.ComputeHash(Encoding.UTF8.GetBytes(text)).Aggregate(string.Empty, (current, b) => current + b.ToString("x2").ToUpper());
            var x = 0;
            var y = 0;
            foreach (var n in text)
            {
                y++;
                x += (n * y);
            }

            var retVal = x + "00000000";

            return retVal.Substring(0, 8);
        }

    }
}