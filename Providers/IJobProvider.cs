using JobNotifierBot.Models;

namespace JobNotifierBot.Providers;

public interface IJobProvider
{
    Task<List<JobVacancy>> GetVacanciesAsync();
}