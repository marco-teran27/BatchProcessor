using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using ConfigJSON.Models;

namespace ConfigJSON
{
    public class ConfigParser
    {
        public async Task<ConfigStructure> ParseConfigAsync(string filePath)
        {
            using (var reader = new StreamReader(filePath))
            {
                string jsonString = await reader.ReadToEndAsync();
                ConfigStructure? config = JsonSerializer.Deserialize<ConfigStructure>(jsonString);
                return config ?? throw new JsonException("Failed to deserialize config file."); // Handle null
            }
        }
    }
}