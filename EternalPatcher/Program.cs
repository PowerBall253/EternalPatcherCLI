using System;
using System.Runtime.InteropServices;
using System.Windows;

namespace EternalPatcher
{
    public class EternalPatcher
    {
        public static void Main(string[] args)
        {
            // Used for command line option parsing
            bool performUpdate = false;
            string filePath = string.Empty;
            bool patch = false;

            // Parse command line arguments
            if (args != null & args.Length > 0)
            {
                for (var i = 0; i < args.Length; i++)
                {
                    if (args[i].Equals("--patch", StringComparison.InvariantCultureIgnoreCase) && string.IsNullOrEmpty(filePath))
                    {
                        patch = true;
                        if (i + 1 < args.Length)
                        {
                            filePath = args[i + 1];
                            continue;
                        }
                    }
                    else if (args[i].Equals("--update", StringComparison.InvariantCultureIgnoreCase) && !performUpdate)
                    {
                        performUpdate = true;
                        continue;
                    }
                    else
                    {
                        if (patch == false)
                        {
                            Console.WriteLine("Unknown argument! Run 'mono EternalPatcher.exe' to see all the possible arguments.");
                            System.Environment.Exit(1);
                        }
                    }
                }
            }
            else
            {
                Console.WriteLine("EternalPatcher by proteh, adapted for Mono by PowerBall253.");
                Console.WriteLine("");
                Console.WriteLine("Usage:");
                Console.WriteLine("");
                Console.WriteLine("mono EternalPatcher.exe (--update) (--patch /path/to/DOOMEternalx64vk.exe)");
                Console.WriteLine("    --update        Updates the patch definitions.");
                Console.WriteLine("    --patch         Patches the game executable using the definitions.");
                Console.WriteLine("");
                System.Environment.Exit(1);
            }
            if (performUpdate == false & String.IsNullOrEmpty(filePath))
            {
                Console.WriteLine("No executable was specified for patching!");
                System.Environment.Exit(1);

            }

                // Update first if required
                try
                {
                    if (performUpdate)
                    {
                        Console.WriteLine("Checking for updates...");

                        if (Patcher.AnyUpdateAvailable())
                        {
                            Console.WriteLine("Downloading latest patch definitions...");
                            Patcher.DownloadLatestPatchDefinitions();
                            Console.WriteLine("Done.");
                        }
                        else
                        {
                            Console.WriteLine("No updates available.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"An error occured while checking for updates: {ex}");
                    System.Environment.Exit(1);
                    return;
                }

                // Stop here if no path was specified
                if (string.IsNullOrEmpty(filePath))
                {
                    System.Environment.Exit(0);
                    return;
                }

                // Load the patch definitions file
                try
                {
                    Console.WriteLine("Loading patch definitions file...");
                    Patcher.LoadPatchDefinitions();
                    Console.WriteLine("Done.");
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"An error occured while loading the patch definitions file: {ex}");
                    System.Environment.Exit(1);
                    return;
                }

                // Stop if there are no patches loaded
                if (!Patcher.AnyPatchesLoaded())
                {
                    Console.Out.WriteLine($"Unable to patch: 0 patches loaded");
                    System.Environment.Exit(1);
                    return;
                }

                // Check game build
                GameBuild gameBuild = null;

                try
                {
                    Console.WriteLine("Checking game build...");
                    gameBuild = Patcher.GetGameBuild(filePath);

                    if (gameBuild == null)
                    {
                        Console.Out.WriteLine($"Unable to apply patches: unsupported game build detected");
                        System.Environment.Exit(1);
                        return;
                    }

                    Console.WriteLine($"{gameBuild.Id} detected.");
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"An error occured while checking the game build: {ex}");
                    System.Environment.Exit(1);
                    return;
                }

                // Patch the specified file
                int successes = 0;

                try
                {
                    Console.WriteLine("Applying patches...");

                    foreach (var patchResult in Patcher.ApplyPatches(filePath, gameBuild.Patches))
                    {
                        if (patchResult.Success)
                        {
                            successes++;
                        }
                        Console.WriteLine($"{patchResult.Patch.Description} : {(patchResult.Success ? "Success" : "Failure")}");
                    }

                    Console.WriteLine($"\n{successes} out of {gameBuild.Patches.Count} applied.");
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"An error occured while patching the game executable: {ex}");
                    System.Environment.Exit(1);
                    return;
                }
                System.Environment.Exit(successes == gameBuild.Patches.Count ? 0 : 1);
        }
    }
}
