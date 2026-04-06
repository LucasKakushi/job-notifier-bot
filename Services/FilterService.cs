using JobNotifierBot.Models;

namespace JobNotifierBot.Services;

public class FilterService
{
    private readonly List<string> _techKeywords;
    private readonly List<string> _levelKeywords;
    private readonly List<string> _excludedKeywords;

    public FilterService(
        List<string> techKeywords,
        List<string> levelKeywords,
        List<string> excludedKeywords)
    {
        _techKeywords = techKeywords;
        _levelKeywords = levelKeywords;
        _excludedKeywords = excludedKeywords;
    }

    public List<JobVacancy> FilterRelevantVacancies(List<JobVacancy> vacancies)
    {
        return vacancies.Where(IsRelevant).ToList();
    }

    private bool IsRelevant(JobVacancy vacancy)
    {
        var combinedText =
            $"{vacancy.Title} {vacancy.Description}".ToLowerInvariant();

        var hasTechKeyword = _techKeywords.Any(keyword =>
            combinedText.Contains(keyword.ToLowerInvariant()));

        var hasLevelKeyword = _levelKeywords.Any(keyword =>
            combinedText.Contains(keyword.ToLowerInvariant()));

        var hasExcludedKeyword = _excludedKeywords.Any(keyword =>
            combinedText.Contains(keyword.ToLowerInvariant()));

        return hasTechKeyword && !hasExcludedKeyword;
    }
}