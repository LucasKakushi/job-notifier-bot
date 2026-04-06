using JobNotifierBot.Providers;
using JobNotifierBot.Services;
using Microsoft.Extensions.Configuration;

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

var telegramToken = configuration["Telegram:BotToken"]!;
var telegramChatId = configuration["Telegram:ChatId"]!;

var techKeywords = configuration
    .GetSection("Filters:TechKeywords")
    .Get<List<string>>() ?? new List<string>();

var levelKeywords = configuration
    .GetSection("Filters:LevelKeywords")
    .Get<List<string>>() ?? new List<string>();

var excludedKeywords = configuration
    .GetSection("Filters:ExcludedKeywords")
    .Get<List<string>>() ?? new List<string>();

using var httpClient = new HttpClient();

IJobProvider provider = new GupyJobProvider(
    httpClient,
    "https://paschoalotto.gupy.io");

var filterService = new FilterService(
    techKeywords,
    levelKeywords,
    excludedKeywords
);
var storageService = new VacancyStorageService();
var telegramService = new TelegramService(httpClient, telegramToken, telegramChatId);

Console.WriteLine("Buscando vagas...");
var vacancies = await provider.GetVacanciesAsync();

Console.WriteLine($"Total encontrado: {vacancies.Count}");

var relevantVacancies = filterService.FilterRelevantVacancies(vacancies);
Console.WriteLine($"Vagas relevantes: {relevantVacancies.Count}");

var sentIds = await storageService.GetSentVacancyIdsAsync();

var newVacancies = relevantVacancies
    .Where(v => !sentIds.Contains(v.Id))
    .ToList();

Console.WriteLine($"Novas vagas para enviar: {newVacancies.Count}");

foreach (var vacancy in newVacancies)
{
    await telegramService.SendVacancyAsync(
        vacancy.Title,
        vacancy.Company,
        vacancy.Location,
        vacancy.Url);

    sentIds.Add(vacancy.Id);

    Console.WriteLine($"Enviada: {vacancy.Title}");
}

await storageService.SaveSentVacancyIdsAsync(sentIds);

Console.WriteLine("Processo finalizado.");