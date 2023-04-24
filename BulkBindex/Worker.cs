using System.Net;
using System.Reflection;
using System.IO.Compression;
using System.Text.RegularExpressions;

namespace BulkBindex;

public class Worker
{
    internal static void Run(String BuildName, String YearMonth)
    {
        // Get the current directory of the executable
        String CurrentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
        
        // Create the output directory
        String OutputDirectory = Path.Combine(CurrentDirectory, "Worker-Output");
        
        // If directory exists, delete it and all of its contents
        if (Directory.Exists(OutputDirectory))
        {
            Directory.Delete(OutputDirectory, true);
        }
        Directory.CreateDirectory(OutputDirectory);
        
        // Download zip to output directory
        // https://github.com/m417z/winbindex/archive/refs/heads/gh-pages.zip
        String zFile = Path.Combine(OutputDirectory, "winbindex.zip");
        String ZipURL = "https://github.com/m417z/winbindex/archive/refs/heads/gh-pages.zip";
        Console.WriteLine($"[+] Downloading winbindex from GitHub to {zFile}");
        using (WebClient Client = new WebClient())
        {
            Client.DownloadFile(ZipURL, zFile);
        }
        
        // Get file size of zip file
        FileInfo ZipFileInfo = new FileInfo(zFile);
        Console.WriteLine($"[>] Downloaded {ZipFileInfo.Length} bytes");
        String sCompressedPath = Path.Combine(OutputDirectory, "Compressed");
        Directory.CreateDirectory(sCompressedPath);
        
        // Extract zip file to output directory
        Console.WriteLine($"[+] Extracting compressed files to {sCompressedPath}");
        
        using (ZipArchive archive = ZipFile.OpenRead(zFile))
        {
            foreach (ZipArchiveEntry entry in archive.Entries)
            {
                if (entry.FullName.StartsWith("winbindex-gh-pages/data/by_filename_compressed/"))
                {
                    String sPattern = @"\.exe|\.dll|\.sys|\.winmd|\.cpl|\.ax|\.node|\.ocx|\.efi|\.acm|\.scr|\.tsp|\.drv";
                    if (Regex.IsMatch(entry.FullName, sPattern))
                    {
                        // Extract the file
                        entry.ExtractToFile(Path.Combine(sCompressedPath, entry.Name));
                    }
                }
            }
        }
        Console.WriteLine("[>] Extraction complete.");

        // Delete the zip file
        File.Delete(zFile);
        
        // Make output directory for downloaded files
        String sDownloadedPath = Path.Combine(OutputDirectory, "Download");
        String sDownloadedPathx86 = Path.Combine(sDownloadedPath, "x86");
        String sDownloadedPathx64 = Path.Combine(sDownloadedPath, "x64");
        Directory.CreateDirectory(sDownloadedPath);
        Directory.CreateDirectory(sDownloadedPathx86);
        Directory.CreateDirectory(sDownloadedPathx64);
        Console.WriteLine($"[DEBUG] Downloaded files will be saved to {sDownloadedPath}");
        Console.WriteLine($"[+] Downloading all binaries for {BuildName} {YearMonth}..");

        // Process the files
        // 1. Decompress
        // 2. Read as JSON
        // 3. Loop through each file
        
        // Get count of files
        Int64 iFileCount = Directory.GetFiles(sCompressedPath).Length;
        Int64 iCurrentFile = 0;
        List<String> lFailedDownloads = new List<String>();
        foreach (String sGZInstance in Directory.GetFiles(sCompressedPath))
        {
            // Pass the full path to the helper
            List<Helper.FileData> fileList = Helper.ProcessGZJSON(sGZInstance, YearMonth, BuildName);
            
            // Print the results
            foreach (Helper.FileData file in fileList)
            {
                // Create the file name
                String sFileName = String.Empty;
                if (!String.IsNullOrEmpty(file.FileVersion))
                {
                    sFileName = file.FileName + "-" + file.FileVersion + "-" + file.md5 + ".blob";
                }
                else
                {
                    sFileName = file.FileName + "-" + file.md5 + ".blob";
                }
                
                // Download the file
                String sDownloadPath = String.Empty;
                if (file.MachineType == "x86")
                {
                    sDownloadPath = Path.Combine(sDownloadedPathx86, sFileName);
                }
                else
                {
                    sDownloadPath = Path.Combine(sDownloadedPathx64, sFileName);
                }
                //Console.WriteLine("[DEBUG] " + sDownloadPath);
                
                try
                {
                    using (WebClient Client = new WebClient())
                    {
                        Client.DownloadFile(file.DownloadURL, sDownloadPath);
                    }
                }
                catch (Exception ex)
                {
                    String sError = " - " + file.DownloadURL + "\n   |_ " + ex.Message;
                    lFailedDownloads.Add(sError);
                }
            }
            
            // Delete the compressed file
            File.Delete(sGZInstance);
            
            // Increment and print progress
            iCurrentFile++;
            Helper.DisplayProgress(iCurrentFile, iFileCount);
        }
        
        Console.WriteLine("\n[>] Download complete.");
        // Print failed downloads
        if (lFailedDownloads.Count > 0)
        {
            Console.WriteLine("\n[!] Failed file downloads..");
            foreach (String sError in lFailedDownloads)
            {
                Console.WriteLine(sError);
            }
        }
    }
}