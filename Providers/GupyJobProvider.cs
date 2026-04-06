using System.Text.Json;
using System.Text.RegularExpressions;
using JobNotifierBot.Models;

namespace JobNotifierBot.Providers;

public class GupyJobProvider : IJobProvider
{
    private readonly HttpClient _httpClient;
    private readonly string _jobsPageUrl;

    public GupyJobProvider(HttpClient httpClient, string jobsPageUrl)
    {
        _httpClient = httpClient;
        _jobsPageUrl = jobsPageUrl;
    }

    public async Task<List<JobVacancy>> GetVacanciesAsync()
    {
        var html = await _httpClient.GetStringAsync(_jobsPageUrl);

        var nextDataJson = ExtractNextDataJson(html);

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var root = JsonSerializer.Deserialize<GupyRoot>(nextDataJson, options);

        var jobs = root?.Props?.PageProps?.Jobs;

        if (jobs is null || jobs.Count == 0)
            return new List<JobVacancy>();

        return jobs.Select(job => new JobVacancy
        {
            Id = job.Id.ToString(),
            Title = job.Title ?? string.Empty,
            Company = root?.Props?.PageProps?.CareerPage?.Name ?? "Não informado",
            Location = BuildLocation(job),
            Url = BuildJobUrl(job),
            Description = job.Department ?? string.Empty
        }).ToList();
    }

    private static string ExtractNextDataJson(string html)
    {
        var match = Regex.Match(
            html,
            "<script id=\"__NEXT_DATA__\" type=\"application/json\">(?<json>.*?)</script>",
            RegexOptions.Singleline);

        if (!match.Success)
            throw new Exception("Não foi possível encontrar o __NEXT_DATA__ no HTML.");

        return match.Groups["json"].Value;
    }

    private static string BuildLocation(GupyJob job)
    {
        var city = job.Workplace?.Address?.City;
        var state = job.Workplace?.Address?.State;

        if (!string.IsNullOrWhiteSpace(city) && !string.IsNullOrWhiteSpace(state))
            return $"{city} - {state}";

        return "Não informado";
    }

    private string BuildJobUrl(GupyJob job)
    {
        return $"{_jobsPageUrl.TrimEnd('/')}/jobs/{job.Id}";
    }

    private class GupyRoot
    {
        public GupyProps? Props { get; set; }
    }

    private class GupyProps
    {
        public GupyPageProps? PageProps { get; set; }
    }

    private class GupyPageProps
    {
        public List<GupyJob>? Jobs { get; set; }
        public GupyCareerPage? CareerPage { get; set; }
    }

    private class GupyCareerPage
    {
        public string? Name { get; set; }
    }

    private class GupyJob
    {
        public long Id { get; set; }
        public string? Title { get; set; }
        public string? Type { get; set; }
        public string? Department { get; set; }
        public GupyWorkplace? Workplace { get; set; }
    }

    private class GupyWorkplace
    {
        public string? Type { get; set; }
        public GupyAddress? Address { get; set; }
    }

    private class GupyAddress
    {
        public string? City { get; set; }
        public string? State { get; set; }
    }
}