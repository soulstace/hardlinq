/* Based on source code provided by Microsoft, modified to create hard links */
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace hardlinq
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            if (args.Length >= 2 &&
                Directory.Exists(Environment.ExpandEnvironmentVariables(args[0])) &&
                Directory.Exists(Environment.ExpandEnvironmentVariables(args[1])))
            {
                string sourcePath = Environment.ExpandEnvironmentVariables(args[0]);
                string destPath = Environment.ExpandEnvironmentVariables(args[1]);

                DirectoryInfo srcDir = new DirectoryInfo(sourcePath);
                DirectoryInfo destDir = new DirectoryInfo(destPath);

                IEnumerable<FileInfo> srcList = srcDir.GetFiles("*", SearchOption.AllDirectories);
                IEnumerable<FileInfo> destList = destDir.GetFiles("*", SearchOption.AllDirectories);

                FileCompare myFileCompare = new FileCompare();
                myFileCompare.compLen = args.Any("--comparelength".Contains);

                if (args.Any("--findlinks".Contains))
                {
                    FindLinks(destList);
                    return;
                }

                bool strip = sourcePath.Equals(".") ? false : args.Any("--strip".Contains);

                bool areSimilar = srcList.SequenceEqual(destList, myFileCompare);
                Console.WriteLine(areSimilar ? 
                    "The contents of the two directories appears to be similar." :
                    "The contents of the two directories are not the same.");

                if (args.Any("--showcommon".Contains))
                {
                    var queryCommonFiles = srcList.Intersect(destList, myFileCompare); /* Intersect appears to drop duplicates. Common file count can be wrong */
                    if (queryCommonFiles.Any())
                    {
                        Console.WriteLine("The following files exist in both directories:");
                        foreach (var f in queryCommonFiles)
                        {
                            Console.WriteLine(strip ? f.FullName.Replace(sourcePath, "") : f.FullName);
                        }
                        Console.WriteLine("Total common files: " + queryCommonFiles.Count());
                    }
                    else
                        Console.WriteLine("There are no common files in the two directories.");
                }

                if (!areSimilar && args.Length > 2)
                {
                    string format = "The following files from \"{0}\" do not exist {1} \"{2}\"";
                    Console.WriteLine(format, sourcePath, myFileCompare.compLen ? "or match in bytes within" : "within", destPath);

                    var queryList1Only = (from file in srcList select file).Except(destList, myFileCompare); /* Bug, similar to Intersect */
                    foreach (var f in queryList1Only)
                    {
                        Console.WriteLine(strip ? f.FullName.Replace(sourcePath, "") : f.FullName);
                    }
                    Console.WriteLine("Total uncommon files: " + queryList1Only.Count());
                }

                if (args.Length == 2)
                {
                    /* Forced mode -- No comparison is made here */
                    CreateHardLinks(srcList, sourcePath, destPath);
                }
            }
            else PrintUsage();
        }

        //[DllImport("Kernel32.dll", CharSet = CharSet.Unicode)]
        //static extern bool CreateHardLink(string lpFileName, string lpExistingFileName, IntPtr lpSecurityAttributes);

        /* Starting in Windows 10, version 1607, MAX_PATH limitations have been removed from common Win32 file and directory functions. 
        However, you must opt-in to the new behavior.
        To enable the new long path behavior, both of the following conditions must be met:
        The registry key Computer\HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\FileSystem\LongPathsEnabled (Type: REG_DWORD) 
        must exist and be set to 1. The key's value will be cached by the system (per process) after the first call to an affected 
        Win32 file or directory function (see below for the list of functions). The registry key will not be reloaded during the 
        lifetime of the process. In order for all apps on the system to recognize the value of the key, a reboot might be required 
        because some processes may have started before the key was set.
        Note: This registry key can also be controlled via Group Policy at 
        Computer Configuration > Administrative Templates > System > Filesystem > Enable Win32 long paths. */
        [DllImport("Kernel32.dll", CharSet = CharSet.Unicode)]
        static extern bool CreateHardLinkW(string lpFileName, string lpExistingFileName, IntPtr lpSecurityAttributes);

        static void CreateHardLinks(IEnumerable<FileInfo> fileList, string sourcePath, string destPath)
        {
            int successful = 0;
            int failed = 0;
            foreach (var f in fileList)
            {
                string link = f.FullName.Replace(sourcePath, destPath);
                Directory.CreateDirectory(f.DirectoryName.Replace(sourcePath, destPath));
                if (CreateHardLinkW(link, f.FullName, IntPtr.Zero))
                {
                    Console.WriteLine("Created hard link: " + link);
                    ++successful;
                }
                else
                {
                    Console.WriteLine("Error creating hard link: " + link + " (Maybe the file or link already exists.)");
                    ++failed;
                }
            }
            Console.WriteLine("Links failed (or already exist): " + failed);
            Console.WriteLine("Links created successfully: " + successful);
        }

        static void FindLinks(IEnumerable<FileInfo> fileList)
        {
            bool eulaAccepted = false;
            RegistryKey r = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Sysinternals\FindLinks",
                RegistryKeyPermissionCheck.ReadSubTree, System.Security.AccessControl.RegistryRights.ReadKey);
            if (r != null)
            {
                eulaAccepted = (int)r.GetValue("EulaAccepted", 0) != 0;
                r.Close();
            }

            if (eulaAccepted)
            {
                //string outputfile = "findlinks.txt";
                //StreamWriter sw;
                try
                {
                    //if (!File.Exists(outputfile))
                    //    sw = File.CreateText(outputfile);
                    //else
                    //    sw = File.AppendText(outputfile);

                    Console.WriteLine("Searching for links. This could take a while...");

                    ProcessStartInfo psi = new ProcessStartInfo
                    {
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        FileName = "findlinks.exe"
                    };

                    foreach (var f in fileList)
                    {
                        psi.Arguments = "-nobanner " + "\"" + f.FullName + "\"";
                        Process p = Process.Start(psi);
                        string output = p.StandardOutput.ReadToEnd();
                        try
                        {
                            var lines = output.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                            var links = lines[2].Split(new string[] { "\t" }, StringSplitOptions.RemoveEmptyEntries);
                            if (int.Parse(links[1]) > 0)
                            {
                                Console.WriteLine(output);
                                //sw.WriteLine(output);
                            }
                        }
                        catch { Console.WriteLine(output); }
                    }
                    //sw.Close();
                    Console.WriteLine("Done searching for links.");// See the resulting findlinks.txt in this program's directory.");
                }
                catch (Exception x)
                {
                    Console.WriteLine(x.Message);
                }
            }
            else Console.WriteLine("You need to run Sysinternals findlinks.exe and accept its EULA.");
        }

        static void PrintUsage()
        {
            Console.WriteLine("hardlinq Copyright (C) 2021 soulstace\n" +
                    "github.com/soulstace/hardlinq\n\n" +
                    "Usage: hardlinq <sourceDir> <destDir> [--test] [--showcommon] [--findlinks]\n" +
                    "  --test\t\ttest mode (don't write, show diff files only)\n" +
                    "  --strip\t\tstrip source path from test output\n" +
                    "  --showcommon\t\tshow common files between the two directories\n" +
                    "  --comparelength\tin addition to name, also compare files by length in bytes\n" +
                    "  --findlinks\t\tfind all links in destDir (requires Sysinternals findlinks.exe in PATH)");
        }
    }

    // This implementation defines a very simple comparison  
    // between two FileInfo objects. It only compares the name  
    // of the files being compared and their length in bytes.  
    class FileCompare : IEqualityComparer<FileInfo>
    {
        public FileCompare() { }

        public bool compLen = false;

        public bool Equals(FileInfo f1, FileInfo f2)
        {
            return compLen ? 
                (f1.Name == f2.Name && f1.Length == f2.Length) : 
                (f1.Name == f2.Name);
        }

        // Return a hash that reflects the comparison criteria. According to the
        // rules for IEqualityComparer<T>, if Equals is true, then the hash codes must  
        // also be equal. Because equality as defined here is a simple value equality, not  
        // reference identity, it is possible that two or more objects will produce the same  
        // hash code.  
        public int GetHashCode(FileInfo fi)
        {
            string s = compLen ? $"{fi.Name}{fi.Length}" : $"{fi.Name}";
            return s.GetHashCode();
        }
    }
}
