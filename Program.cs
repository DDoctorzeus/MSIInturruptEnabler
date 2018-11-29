using System;
using System.Management;
using System.Collections.Generic;
using Microsoft.Win32;

/*© Techtonic Software 2018 - http://www.techtonicsoftware.com/
 * This Program Is Lisenced Under GNU V3 and comes with ABSOLUTELY NO WARRANTY. 
 * You may distribute, modify and run it however you must not claim it as your own nor sublisence it.
 * Any distribution must include this readme file.
*/

namespace MSIInteruptEnabler
{
    class MainClass
    {
        private const String REGWORKINGPATH = @"SYSTEM\CurrentControlSet\Enum\";
        private const String SUBKEYLOC = @"\Device Parameters\Interrupt Management\MessageSignaledInterruptProperties";

        private static void PrintSpacer()
        {
            String printStr;

            printStr = "";
            for (UInt16 i = 0; i < 50; i++)
                printStr += "-";

            Console.WriteLine(printStr);
        }

        private static void ShowExitMessage()
        {
            Console.WriteLine("\nPress Return To Exit..");
            Console.ReadLine();
        }

        public static void Main(String[] args)
        {
			ManagementObjectSearcher objectSearcher;
            String devName, devInstancePath;
            List<ManagementObject> foundObjects;
            UInt16 objectsCount, selectedItemNum;
            ManagementObject selectedDevice;
            RegistryKey deviceInstanceKey;

            //Show Warning
            Console.WriteLine("WARNING! This program comes with absolutely no warranty and while doing this is normally safe you still agree that by using it that it is entirely your faullt if you screw up your system! I recommend you read up on what Message Signal Interrupts are before using this.\n");
            Console.WriteLine("Remember To Run This Application As Admin!");

            //Init
            objectSearcher =  new ManagementObjectSearcher("SELECT * FROM Win32_PnPSignedDriver WHERE DeviceName LIKE '%NVIDIA%' OR DeviceName LIKE '%AMD%'");
            foundObjects = new List<ManagementObject>();

            //Search For Video Cards
            PrintSpacer();
            objectsCount = 0;
            foreach (ManagementObject obj in objectSearcher.Get())
			{
                devName = obj.GetPropertyValue("DeviceName").ToString();
                ++objectsCount;
                foundObjects.Add(obj);
                Console.WriteLine(objectsCount.ToString() + ". " + devName);
            }
            PrintSpacer();

            //If No Objects Found Exit
            if (objectsCount == 0)
            {
                Console.WriteLine("No Devices Found!");
                ShowExitMessage();
                return;
            }

            //Get Device Index and its index path (CompatID)
            do
            {
                Console.WriteLine("Enter The Device Number You Wish To Enable MSI On: ");
            } while (!UInt16.TryParse(Console.ReadLine(), out selectedItemNum) || (selectedItemNum - 1) >= objectsCount);
            selectedDevice = foundObjects[(selectedItemNum - 1)];
            devInstancePath = REGWORKINGPATH + selectedDevice.GetPropertyValue("DeviceID").ToString() + SUBKEYLOC;

            //Check If Key Exists, create if not
            try
            {
                if ((deviceInstanceKey = Registry.LocalMachine.OpenSubKey(devInstancePath, true)) == null)
                {
                    Console.WriteLine("Creating Key HKEY_LOCAL_MACHINE\\" + devInstancePath);
                    deviceInstanceKey = Registry.LocalMachine.CreateSubKey(devInstancePath);
                }

                //Set Value
                    Console.WriteLine("Trying To Enable MSI");
                deviceInstanceKey.SetValue("MSISupported", 1);

                deviceInstanceKey.Close();
                deviceInstanceKey = null;

                //Show Message And Exit
                Console.WriteLine("MSI is now enabled and you should reboot your system to apply these settings. ");
            }
            catch (UnauthorizedAccessException e)
            {
                Console.WriteLine("Permission denied while editing registry (did you run this program as admin?).");
            }
            catch (Exception e)
            {
                Console.WriteLine("Error Editing Registry: " + e.ToString());
            }

            ShowExitMessage();
        }
    }
}
