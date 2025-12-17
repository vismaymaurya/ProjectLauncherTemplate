using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace ProjectLauncherTemplate.Services
{
    public class GameService
    {
        public bool IsGameInstalled(string installPath)
        {
            if (string.IsNullOrWhiteSpace(installPath) || !Directory.Exists(installPath)) return false;
            // Simple check: does the directory exist and have at least one executable?
            // A more robust check might look for a specific file or the version file.
            return Directory.GetFiles(installPath, "*.exe", SearchOption.TopDirectoryOnly).Any();
        }

        public void LaunchGame(string installPath)
        {
            if (!IsGameInstalled(installPath))
            {
                throw new FileNotFoundException("Game files not found.");
            }

            // Find the executable. 
            // Priority:
            // 1. "TestBuild.exe" (Guessing based on zip name)
            // 2. Any .exe that is NOT "UnityCrashHandler*.exe"
            
            var exeMatches = Directory.GetFiles(installPath, "*.exe");
            string? exePath = exeMatches.FirstOrDefault(f => Path.GetFileName(f).Equals("TestBuild.exe", StringComparison.OrdinalIgnoreCase));

            if (exePath == null)
            {
                exePath = exeMatches.FirstOrDefault(f => !Path.GetFileName(f).StartsWith("UnityCrashHandler", StringComparison.OrdinalIgnoreCase));
            }

            if (exePath != null)
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = exePath,
                    WorkingDirectory = installPath,
                    UseShellExecute = true
                });
            }
            else
            {
                throw new FileNotFoundException("No suitable executable found in game directory.");
            }
        }
    }
}
