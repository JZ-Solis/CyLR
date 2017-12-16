﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace CyLR
{
    internal static class CollectionPaths
    {
        private static List<string> AllFiles;
        private static List<string> tempPaths;

        private static IEnumerable<string> RunCommand(string OSCommand, string CommandArgs)
        {
            var newPaths = new List<string> { };
            var proc = new Process
            { 
                StartInfo = new ProcessStartInfo
                {
                    FileName = OSCommand,
                    Arguments = CommandArgs,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };
            proc.Start();
            while (!proc.StandardOutput.EndOfStream)
            {
                yield return  proc.StandardOutput.ReadLine();
            };
        }
        private static void GetAllFiles(string path, IList<string> files)
        {
            try
            {
                Directory.GetFiles(@path).ToList()
                    .ForEach(@f => files.Add(@f));

                Directory.GetDirectories(@path).ToList()
                    .ForEach(@f => GetAllFiles(@f, files));
            }
            catch (UnauthorizedAccessException ex)
            {
                //Console.WriteLine(ex);
            }
        }
        public static List<string> GetPaths(Arguments arguments, List<string> additionalPaths)
        {
            var defaultPaths = new List<string> { };
            tempPaths = new List<string>
            {
                        //@"%SYSTEMROOT%\System32\drivers\etc\hosts",
                        @"%SYSTEMROOT%\SchedLgU.Txt"
                        //@"%PROGRAMDATA%\Microsoft\Windows\Start Menu\Programs\Startup",
                        //@"%SYSTEMROOT%\System32\config",
                        //@"%SYSTEMROOT%\System32\winevt\logs",
                        //@"%SYSTEMROOT%\Prefetch",
                        //@"%SYSTEMROOT%\Tasks",
                        //@"%SYSTEMROOT%\System32\LogFiles\W3SVC1",
                        //@"%SystemDrive%\$MFT"
            };
            // Collect file listing
            AllFiles = new List<string> { };
            //AllFiles.AddRange(RunCommand("dir", "/s /b /o:gn %SYSTEMROOT%"));
            GetAllFiles("C:\\", AllFiles);
            Console.WriteLine(AllFiles);
            // Find all *.plist files
            tempPaths.AddRange(AllFiles.Where((stringToCheck => stringToCheck.Contains(".plist"))));
            // Find all .bash_history files
            tempPaths.AddRange(AllFiles.Where((stringToCheck => stringToCheck.Contains(".bash_history"))));
            // Find all .sh_history files
            tempPaths.AddRange(AllFiles.Where((stringToCheck => stringToCheck.Contains(".sh_history"))));
            // Find all Ntuser.dat files
            tempPaths.AddRange(AllFiles.Where((stringToCheck => stringToCheck.Contains("ntuser.dat"))));
            // Find Chrome Preference files
            tempPaths.AddRange(AllFiles.Where((stringToCheck => stringToCheck.Contains("Chrome\\User Data\\Default\\Default\\History"))));
            tempPaths.AddRange(AllFiles.Where((stringToCheck => stringToCheck.Contains("Chrome\\User Data\\Default\\Default\\Cookies"))));
            tempPaths.AddRange(AllFiles.Where((stringToCheck => stringToCheck.Contains("Chrome\\User Data\\Default\\Default\\Bookmarks"))));
            tempPaths.AddRange(AllFiles.Where((stringToCheck => stringToCheck.Contains("Chrome\\User Data\\Default\\Default\\Extensions"))));
            tempPaths.AddRange(AllFiles.Where((stringToCheck => stringToCheck.Contains("Chrome\\User Data\\Default\\Default\\Last"))));
            tempPaths.AddRange(AllFiles.Where((stringToCheck => stringToCheck.Contains("Chrome\\User Data\\Default\\Default\\Shortcuts"))));
            tempPaths.AddRange(AllFiles.Where((stringToCheck => stringToCheck.Contains("Chrome\\User Data\\Default\\Default\\Top"))));
            tempPaths.AddRange(AllFiles.Where((stringToCheck => stringToCheck.Contains("Chrome\\User Data\\Default\\Default\\Visited"))));
            
            // Find FireFox Preference Files
            tempPaths.AddRange(AllFiles.Where((stringToCheck => stringToCheck.Contains("places.sqlite"))));
            tempPaths.AddRange(AllFiles.Where((stringToCheck => stringToCheck.Contains("downloads.sqlite"))));

            // Fix any spaces to work with MacOS naming conventions
            defaultPaths = tempPaths.ConvertAll(stringToCheck => stringToCheck.Replace(" ", " "));

            defaultPaths = defaultPaths.Select(Environment.ExpandEnvironmentVariables).ToList();

            if (Platform.IsUnixLike())
            {
                defaultPaths = new List<string> { };
                tempPaths = new List<string>
                {
                    "/root/.bash_history",
                    "/var/log",
                    "/private/var/log/",
                    "/.fseventsd",
                    "/etc/hosts.allow",
                    "/etc/hosts.deny",
                    "/etc/hosts",
                    "/System/Library/StartupItems",
                    "/System/Library/LaunchAgents",
                    "/System/Library/LaunchDaemons",
                    "/Library/LaunchAgents",
                    "/Library/LaunchDaemons",
                    "/Library/StartupItems",
                    "/etc/passwd",
                    "/etc/group"
                };
                // Collect file listing
                AllFiles = new List<string> { };
                AllFiles.AddRange(RunCommand("/usr/bin/find", "/ -print"));

                // Find all *.plist files
                tempPaths.AddRange(AllFiles.Where((stringToCheck => stringToCheck.Contains(".plist"))));
                // Find all .bash_history files
                tempPaths.AddRange(AllFiles.Where((stringToCheck => stringToCheck.Contains(".bash_history"))));
                // Find all .sh_history files
                tempPaths.AddRange(AllFiles.Where((stringToCheck => stringToCheck.Contains(".sh_history"))));
                // Find Chrome Preference files
                tempPaths.AddRange(AllFiles.Where((stringToCheck => stringToCheck.Contains("Support/Google/Chrome/Default/History"))));
                tempPaths.AddRange(AllFiles.Where((stringToCheck => stringToCheck.Contains("Support/Google/Chrome/Default/Cookies"))));
                tempPaths.AddRange(AllFiles.Where((stringToCheck => stringToCheck.Contains("Support/Google/Chrome/Default/Bookmarks"))));
                tempPaths.AddRange(AllFiles.Where((stringToCheck => stringToCheck.Contains("Support/Google/Chrome/Default/Extensions"))));
                tempPaths.AddRange(AllFiles.Where((stringToCheck => stringToCheck.Contains("Support/Google/Chrome/Default/Last"))));
                tempPaths.AddRange(AllFiles.Where((stringToCheck => stringToCheck.Contains("Support/Google/Chrome/Default/Shortcuts"))));
                tempPaths.AddRange(AllFiles.Where((stringToCheck => stringToCheck.Contains("Support/Google/Chrome/Default/Top"))));
                tempPaths.AddRange(AllFiles.Where((stringToCheck => stringToCheck.Contains("Support/Google/Chrome/Default/Visited"))));

                // Find FireFox Preference Files
                tempPaths.AddRange(AllFiles.Where((stringToCheck => stringToCheck.Contains("places.sqlite"))));
                tempPaths.AddRange(AllFiles.Where((stringToCheck => stringToCheck.Contains("downloads.sqlite"))));

                // Fix any spaces to work with MacOS naming conventions
                defaultPaths = tempPaths.ConvertAll(stringToCheck => stringToCheck.Replace(" ", " "));
            }
            var paths = new List<string>(additionalPaths);

            if (arguments.CollectionFilePath != ".")
            {
                if (File.Exists(arguments.CollectionFilePath))
                {
                    paths.AddRange(File.ReadAllLines(arguments.CollectionFilePath).Select(Environment.ExpandEnvironmentVariables));
                }
                else
                {
                    Console.WriteLine("Error: Could not find file: {0}", arguments.CollectionFilePath);
                    Console.WriteLine("Exiting");
                    throw new ArgumentException();
                }
            }

            if (arguments.CollectionFiles != null)
            {
                paths.AddRange(arguments.CollectionFiles);
            }

            if (paths.Count == 1)
            {
                if (paths[0] == "")
                {
                    return defaultPaths;
                }
            }
            return paths.Any() ? paths : defaultPaths;
        }
    }
}
