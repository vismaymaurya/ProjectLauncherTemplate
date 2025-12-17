using ProjectLauncherTemplate.Models;
using Newtonsoft.Json;
using System;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Threading.Tasks;

namespace ProjectLauncherTemplate.Services
{
    public class UpdateService
    {
        private readonly HttpClient _httpClient;
        // This URL should probably be configurable or passed in, but hardcoding the version check URL as per requirements
        private const string VersionCheckUrl = "https://github.com/vismaymaurya/LauncherTest/releases/download/Launcher/version.json";

        public UpdateService()
        {
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
            // Add a user agent just in case GitHub requires it
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "ProjectLauncherTemplate");
        }

        public async Task<GameVersion?> CheckForUpdatesAsync()
        {
            try
            {
                var json = await _httpClient.GetStringAsync(VersionCheckUrl);
                return JsonConvert.DeserializeObject<GameVersion>(json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to check updates: {ex.Message}");
                return null;
            }
        }

        public async Task DownloadAndExtractAsync(string buildUrl, string destinationPath, IProgress<double> progress)
        {
            try
            {
                if (!Directory.Exists(destinationPath))
                {
                    Directory.CreateDirectory(destinationPath);
                }

                string zipPath = Path.Combine(destinationPath, "update.zip");

                using (var response = await _httpClient.GetAsync(buildUrl, HttpCompletionOption.ResponseHeadersRead))
                {
                    response.EnsureSuccessStatusCode();
                    var totalBytes = response.Content.Headers.ContentLength ?? -1L;
                    var totalRead = 0L;
                    var buffer = new byte[8192];
                    var isMoreToRead = true;

                    using (var sourceStream = await response.Content.ReadAsStreamAsync())
                    using (var fileStream = new FileStream(zipPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
                    {
                        while (isMoreToRead)
                        {
                            var read = await sourceStream.ReadAsync(buffer, 0, buffer.Length);
                            if (read == 0)
                            {
                                isMoreToRead = false;
                            }
                            else
                            {
                                await fileStream.WriteAsync(buffer, 0, read);
                                totalRead += read;

                                if (totalBytes != -1 && progress != null)
                                {
                                    progress.Report((double)totalRead / totalBytes * 100);
                                }
                            }
                        }
                    }
                }

                // Extract
                progress?.Report(-1); // Indeterminate or "Extracting" status signal could be handled by caller
                ZipFile.ExtractToDirectory(zipPath, destinationPath, true);
                File.Delete(zipPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Update failed: {ex.Message}");
                throw; // Rethrow to let ViewModel handle error display
            }
        }
    }
}
