﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CyLR
{
    public class Arguments
    {
        const string BaseHelpMessage = "CyLR Version 1.1\n\nThe CyLR tool collects forensic artifacts from hosts with NTFS file systems quickly, securely and minimizes impact to the host.\n\nThe avalable options are:";
        private static readonly Dictionary<string, string> HelpTopics = new Dictionary<string, string>
        {
            {
                "-o",
                "Defines the directory that the zip archive will be created in. Defaults to current working directory.\nUsage: -o <directory path>"
            },
            {
                "-c",
                "Optional argument to provide custom list of artifact files and directories (one entry per line).\nNOTE: Must use full path including drive letter on each line.  MFT can be collected by \"C:$MFT\" or \"D:$MFT\" and so on.\nUsage: -c <path to config file>"
            },
            {
                "-u",
                "SFTP username"
            },
            {
                "-p",
                "SFTP password"
            },
            {
                "-s",
                "SFTP Server resolvable hostname or IP address and port. If no port is given then 22 is used by default.  Format is <server name>:<port>\n Usage: -s 8.8.8.8:22"
            },
            {
                "-m",
                "ONLY availabe in SFTP mode. Performs the collection entirely in-memory before sending via SFTP. May use a lot of memory depending on the size of files collected."
            }
        };

        public readonly bool HelpRequested01;
        public readonly bool HelpRequested02;

        public readonly string HelpTopic;

        public readonly string CollectionFilePath = ".";
        public readonly string OutputPath = ".";
        public readonly bool SFTPCheck;
        public readonly bool SFTPInMemory;
        public readonly string UserName = string.Empty;
        public readonly string UserPassword = string.Empty;
        public readonly string SFTPServer = string.Empty;

        public Arguments(string[] args)
        {
            HelpRequested01 = args.HasArgument("--help");
            HelpTopic = HelpRequested01 ? args.GetArgumentParameter(false, "--help") : string.Empty;

            HelpRequested02 = args.HasArgument("-h");
            HelpTopic = HelpRequested02 ? args.GetArgumentParameter(false, "-h") : string.Empty;

            //If help has been requested, parse no more arguments
            if (!HelpRequested01 && !HelpRequested02)
            {
                if (args.HasArgument("-o"))
                {
                    OutputPath = args.GetArgumentParameter(true, "-o");
                }

                if (args.HasArgument("-u"))
                {
                    UserName = args.GetArgumentParameter(true, "-u");
                }
                if (args.HasArgument("-p"))
                {
                    UserPassword = args.GetArgumentParameter(true, "-p");
                }
                if (args.HasArgument("-s"))
                {
                    SFTPServer = args.GetArgumentParameter(true, "-s");
                }
                var sftpArgs = new[] { UserName, UserPassword, SFTPServer };
                SFTPCheck = sftpArgs.Any(arg=>!string.IsNullOrEmpty(arg));
                if (SFTPCheck && sftpArgs.Any(string.IsNullOrEmpty))
                {
                    throw new ArgumentException("The flags -u, -p, and -s must all have values to continue.  Please try again.");
                }

                SFTPInMemory = args.HasArgument("-m");
                if (SFTPInMemory && !SFTPCheck)
                {
                    throw new ArgumentException("-m may only be used with the SFTP option.");
                }
                if (args.HasArgument("-c"))
                {
                    CollectionFilePath = args.GetArgumentParameter(true, "-c");
                }
            }
        }

        public string GetHelp(string topic)
        {
            string help;
            if (string.IsNullOrEmpty(topic))
            {
                var helpText = new StringBuilder(BaseHelpMessage).AppendLine();
                foreach (var command in HelpTopics)
                {
                    helpText.AppendLine(command.Key).AppendLine("\t" + command.Value).AppendLine();
                }
                help = helpText.ToString();
            }
            else if (!HelpTopics.TryGetValue(topic, out help))
            {
                help = $@"{topic} is not a valid argument.";
            }
            return help;
        }
    }

    internal static class ArgumentExtentions
    {
        public static bool HasArgument(this IEnumerable<string> arguments, params string[] argumentAliases)
        {
            return arguments.Any(arg => argumentAliases.Any(arg.StartsWith));
        }

        public static string GetArgumentParameter(this IEnumerable<string> arguments, bool requireArgument,
            string argumentAlias)
        {
            var argEnumerator = arguments.GetEnumerator();
            while (argEnumerator.MoveNext())
            {
                var currentArg = argEnumerator.Current;

                if (currentArg.Equals(argumentAlias))
                {
                    if (argEnumerator.MoveNext())
                    {
                        return argEnumerator.Current;
                    }
                    if (requireArgument)
                    {
                        throw new ArgumentException(
                            $"Argument '{argumentAlias}' had no parameters. Use '--help {argumentAlias}' for usage details.");
                    }
                    return string.Empty;
                }


                if (currentArg.StartsWith(argumentAlias))
                {
                    return currentArg.Substring(argumentAlias.Length);
                }
            }

            throw new ArgumentException($"Argument '{argumentAlias}' was not found.");
        }
    }
}