using Newtonsoft.Json.Linq;

namespace BulkBindex;
using System.IO.Compression;

public class Helper
{
    internal struct FileData
    {
        public String OSVersion;
        public String? ReleaseDate;
        public String FileName;
        public String? FileVersion;
        public String? md5;
        public Int64 Timestamp;
        public Int64 VirtualSize;
        public String DownloadURL;
        public String MachineType;
    }

    internal static List<FileData> ProcessGZJSON(String sFullPath, String sDate, String sBuildName)
    {
        // Result object
        List<FileData> oResult = new List<FileData>();
        
        // gz decompress the file
        String sJSON = String.Empty;
        using (FileStream fs = new FileStream(sFullPath, FileMode.Open))
        {
            using (GZipStream gz = new GZipStream(fs, CompressionMode.Decompress))
            {
                using (StreamReader sr = new StreamReader(gz))
                {
                    sJSON = sr.ReadToEnd();
                }
            }
        }
        
        // Load the JSON into a JObject
        JObject oJSON = JObject.Parse(sJSON);

        // Loop through the json object
        foreach (JProperty oProperty in oJSON.Properties())
        {
            //Console.WriteLine(oProperty.Name);
            JObject? fileInfo = null;
            try
            {
                fileInfo = (JObject)oProperty.Value["fileInfo"]!;
            } catch {continue;}
            
            Int64 timestamp = 0;
            Int64 virtualSize = 0;
            Int64 machineType = 0;
            try
            {
                // Get the timestamp / virtualSize / machineType values from the fileInfo object
                timestamp = (Int64) (fileInfo["timestamp"] ?? 0);
                virtualSize = (Int64) (fileInfo["virtualSize"] ?? 0);
                machineType = (Int64) (fileInfo["machineType"] ?? 0);
                if (timestamp == 0 || virtualSize == 0 || machineType == 0)
                {
                    continue;
                }
            } catch {continue;}
            
            // We only want x86 and x64 files
            String sMachineType = String.Empty;
            if (machineType != 332 && machineType != 34404)
            {
                continue;
            }
            
            // Set the machine type in the result object
            if (machineType == 332)
            {
                sMachineType = "x86";
            }
            else
            {
                sMachineType = "x64";
            }

            String? sVersion = String.Empty;
            String? sMD5 = String.Empty;
            JObject? windowsVersions = null;
            try
            {
                sVersion = (String) fileInfo["version"]!;
                sMD5 = (String) fileInfo["md5"]!;
                windowsVersions = (JObject) oProperty.Value["windowsVersions"]!;
            } catch {continue;}
            
            // Loop through each version in the windowsVersions object
            foreach (JProperty versionProperty in windowsVersions.Properties())
            {
                // Get the version object from the current version property
                JObject version = (JObject) versionProperty.Value;
                
                // Loop through the version object
                foreach (JProperty versionProperty2 in version.Properties())
                {
                    JObject? windowsVersionInfo = null;
                    try
                    {
                        windowsVersionInfo = (JObject) versionProperty2.Value["windowsVersionInfo"]!;
                    }
                    catch {}
                    
                    JArray? otherWindowsVersions = null;
                    if (windowsVersionInfo == null)
                    {
                        try
                        {
                            windowsVersionInfo = (JObject) versionProperty2.Value["updateInfo"]!;
                        } catch {continue;}
                        
                        try
                        {
                            // If we have updateInfo, there can be an array of otherWindowsVersions
                            // Stop it ok, just make a sane format MSFT ffs..
                            otherWindowsVersions = (JArray)windowsVersionInfo["otherWindowsVersions"]!;
                        } catch {}
                    }
                    
                    String? releaseDate = String.Empty;
                    try
                    {
                        releaseDate = (String) windowsVersionInfo["releaseDate"]!;
                    } catch {continue;}
                    
                    // Do we want this file?
                    if (otherWindowsVersions != null)
                    {
                        try
                        {
                            if ((!otherWindowsVersions.Contains(sDate) && !versionProperty.Name.Contains(sBuildName)) || !releaseDate.Contains(sDate))
                            {
                                continue;
                            }
                        } catch {continue;}
                        
                    }
                    else
                    {
                        try
                        {
                            if (!releaseDate.Contains(sDate) || !versionProperty.Name.Contains(sBuildName))
                            {
                                continue;
                            }
                        } catch {continue;}
                    }

                    // Get name (Win)
                    String sFile = sFullPath.Split('\\').Last();
                    // Get name (Lin)
                    sFile = sFile.Split('/').Last();
                    sFile = sFile.Remove(sFile.Length - 8);
                    
                    // We only want the first part of the version if there are spaces
                    if (!String.IsNullOrEmpty(sVersion))
                    {
                        if (sVersion.Contains(' '))
                        {
                            sVersion = sVersion.Split(' ').First();
                        }
                    }
                    
                    // Add the result to the list
                    oResult.Add(new FileData
                    {
                        OSVersion = versionProperty.Name,
                        ReleaseDate = releaseDate,
                        FileName = sFile,
                        md5 = sMD5,
                        FileVersion = sVersion,
                        Timestamp = timestamp,
                        VirtualSize = virtualSize,
                        DownloadURL = MakeMSFTSymbolDownloadLink(sFile, timestamp, virtualSize),
                        MachineType = sMachineType
                    });
                }
            }
        }

        // Return the result
        return oResult;
    }

    public static String MakeMSFTSymbolDownloadLink(String peName, Int64 timeStamp, Int64 imageSize)
    {
        // "%s/%s/%08X%x/%s" % (serverName, peName, timeStamp, imageSize, peName)
        // https://randomascii.wordpress.com/2013/03/09/symbols-the-microsoft-way/

        String timeStampHex = timeStamp.ToString("X").ToUpper();
        String paddedTimeStampHex = "0000000".Substring(0, 8 - timeStampHex.Length) + timeStampHex;
        String imageSizeHex = imageSize.ToString("x").ToLower();
        
        String fileId = $"{paddedTimeStampHex}{imageSizeHex}";
        return $"https://msdl.microsoft.com/download/symbols/{peName}/{fileId}/{peName}";
    }
    
    internal static void DisplayProgress(Int64 index, Int64 count)
    {
        Double progress = (Double) index / count;
        Int32 progressInChars = (Int32)Math.Floor(progress * 50);

        Console.SetCursorPosition(0, Console.CursorTop);
        Console.Write('[');

        for (Int32 i = 0; i < 50; i++)
        {
            if (i < progressInChars)
            {
                Console.Write('=');
            }
            else if (i == progressInChars)
            {
                Console.Write('>');
            }
            else
            {
                Console.Write(' ');
            }
        }

        Int32 progressPercentage = (Int32)Math.Floor(progress * 100);
        Console.Write($"] {progressPercentage}%  ({index}/{count})");
        Console.SetCursorPosition(0, Console.CursorTop);
    }
}