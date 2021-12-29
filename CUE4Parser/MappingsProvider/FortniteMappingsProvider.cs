using System;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Serilog;

namespace CUE4Parse.MappingsProvider
{
    // Retrieves mappings for Fortnite
    public class FortniteMappingsProvider : UsmapTypeMappingsProvider
    {
        private readonly string? _specificVersion;
        private readonly string _gameName;
        private readonly bool _isWindows64Bit;

        public FortniteMappingsProvider(string gameName, string? specificVersion = null)
        {
            _specificVersion = specificVersion;
            _gameName = gameName;
            _isWindows64Bit = Environment.Is64BitOperatingSystem && RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
            Reload();
        }
        
        public const string BenMappingsEndpoint = "https://benbot.app/api/v1/mappings";
        
        private readonly HttpClient _client = new HttpClient { Timeout = TimeSpan.FromSeconds(2), DefaultRequestHeaders = { { "User-Agent", "CUE4Parse" } }};
        
        public sealed override bool Reload()
        {
            return ReloadAsync().GetAwaiter().GetResult();
        }

        public sealed override async Task<bool> ReloadAsync()
        {
            byte[] usmapBytes = Array.Empty<byte>();
            string? usmapUrl = null;
            string? usmapName = null;

            try
            {
                string MappingsFile = Path.Combine(TModel.Preferences.StorageFolder, "BenbotMappings.usmap");

                if (!File.Exists(MappingsFile))
                {
#if false
                    var jsonText = _specificVersion != null
    ? await LoadEndpoint(BenMappingsEndpoint + $"?version={_specificVersion}")
    : await LoadEndpoint(BenMappingsEndpoint);
                    if (jsonText == null)
                    {
                        Log.Warning("Failed to get BenBot Mappings Endpoint");
                        return false;
                    }
                    JArray json = JArray.Parse(jsonText);
                    string preferredCompression = _isWindows64Bit ? "Oodle" : "Brotli";

                    if (!json.HasValues)
                    {
                        Log.Warning("Couldn't reload mappings, json array was empty");
                        return false;
                    }

                    foreach (var arrayEntry in json)
                    {
                        var method = arrayEntry["meta"]?["compressionMethod"]?.ToString();
                        if (method != null && method == preferredCompression)
                        {
                            usmapUrl = arrayEntry["url"]?.ToString();
                            usmapName = arrayEntry["fileName"]?.ToString();
                            break;
                        }
                    }

                    if (usmapUrl == null)
                    {
                        usmapUrl = json[0]["url"]?.ToString()!;
                        usmapName = json[0]["fileName"]?.ToString()!;
                    }
                    usmapBytes = await LoadEndpointBytes(usmapUrl);
#else
                    usmapBytes = await LoadEndpointBytes(@"https://benbot.app/api/v1/mappings/++Fortnite+Release-19.01-CL-18489740-Windows_oo.usmap");
#endif
                    if (usmapBytes == null)
                    {
                        Log.Warning("Failed to download usmap");
                        return false;
                    }

                    File.WriteAllBytes(MappingsFile, usmapBytes);
                }
                else
                {
                    usmapBytes = File.ReadAllBytes(MappingsFile);
                }

                AddUsmap(usmapBytes, _gameName);
                return true;
            }
            catch (Exception e)
            {
                Log.Warning(e, "Uncaught exception while reloading mappings from BenBot");
                return false;
            }
        }
        
        private async Task<string?> LoadEndpoint(string url)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            try
            {
                var response = await _client.SendAsync(request, HttpCompletionOption.ResponseContentRead).ConfigureAwait(false);
                return await response.Content.ReadAsStringAsync();
            }
            catch
            {
                return null;
            }
        }
        
        private async Task<byte[]?> LoadEndpointBytes(string url)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            try
            {
                var response = await _client.SendAsync(request, HttpCompletionOption.ResponseContentRead).ConfigureAwait(false);
                return await response.Content.ReadAsByteArrayAsync();
            }
            catch
            {
                return null;
            }
        }
    }
}