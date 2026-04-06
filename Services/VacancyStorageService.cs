using System.Text.Json;

namespace JobNotifierBot.Services;

public class VacancyStorageService
{
    private readonly string _filePath = Path.Combine("Data", "sent_jobs.json");

    public VacancyStorageService()
    {
        var directory = Path.GetDirectoryName(_filePath);

        if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        if (!File.Exists(_filePath))
        {
            File.WriteAllText(_filePath, "[]");
        }
    }

    public async Task<HashSet<string>> GetSentVacancyIdsAsync()
    {
        var json = await File.ReadAllTextAsync(_filePath);
        var ids = JsonSerializer.Deserialize<HashSet<string>>(json);

        return ids ?? new HashSet<string>();
    }

    public async Task SaveSentVacancyIdsAsync(HashSet<string> ids)
    {
        var json = JsonSerializer.Serialize(ids, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        await File.WriteAllTextAsync(_filePath, json);
    }
}