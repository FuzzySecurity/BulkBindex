using BulkBindex;
using System.Text.RegularExpressions;

Int32 iBuildName = Array.FindIndex(args, s => new Regex(@"(?i)(-|--|/)(b|Build)$").Match(s).Success);
Int32 iYearMonth = Array.FindIndex(args, s => new Regex(@"(?i)(-|--|/)(d|Date)$").Match(s).Success);

if (iBuildName != -1 && iYearMonth != -1)
{
    try
    {
        String BuildName = args[iBuildName + 1];
        String YearMonth = args[iYearMonth + 1];
        
        // Run the worker
        Worker.Run(BuildName, YearMonth);

        // DEBUG CODE
        //--------------
        
        //List<Helper.FileData> lFiles = Helper.ProcessGZJSON(@"C:\Users\b33f\tools\Dev\BulkBindex\BulkBindex\BulkBindex\bin\Debug\net6.0\Worker-Output\Compressed\ntdll.dll.json.gz", YearMonth, BuildName);
        //
        //// Print the results
        //foreach (Helper.FileData file in lFiles)
        //{
        //    Console.WriteLine("\n[+] File: " + file.FileName);
        //    Console.WriteLine("    OS Version: " + file.OSVersion);
        //    Console.WriteLine("    Release Date: " + file.ReleaseDate);
        //    Console.WriteLine("    File Version: " + file.FileVersion);
        //    Console.WriteLine("    MD5: " + file.md5);
        //    Console.WriteLine("    Timestamp: " + file.Timestamp);
        //    Console.WriteLine("    Virtual Size: " + file.VirtualSize);
        //    Console.WriteLine("    Download URL: " + file.DownloadURL);
        //}
    } catch (Exception ex)
    {
        Console.WriteLine($"[!] Error: {ex.Message}");
    }
}
else
{
    Console.WriteLine("[!] Missing required arguments. Please use the following format:");
    Console.WriteLine("    BulkBindex.exe -b <BuildName> -d <MonthYear>");
    Console.WriteLine("    Example: BulkBindex.exe -b 11-22H2 -d 2023-04");
}