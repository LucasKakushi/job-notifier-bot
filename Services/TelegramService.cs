using System.Text;
using System.Text.Json;

namespace JobNotifierBot.Services;

public class TelegramService
{
    private readonly HttpClient _httpClient;
    private readonly string _botToken;
    private readonly string _chatId;

    public TelegramService(HttpClient httpClient, string botToken, string chatId)
    {
        _httpClient = httpClient;
        _botToken = botToken;
        _chatId = chatId;
    }

    public async Task SendMessageAsync(string message)
    {
        var url = $"https://api.telegram.org/bot{_botToken}/sendMessage";

        var payload = new
        {
            chat_id = _chatId,
            text = message
        };

        var content = new StringContent(
            JsonSerializer.Serialize(payload),
            Encoding.UTF8,
            "application/json");

        var response = await _httpClient.PostAsync(url, content);

        response.EnsureSuccessStatusCode();
    }

    public async Task SendVacancyAsync(string title, string company, string location, string url)
    {
        var message = $"""
        Nova vaga encontrada!

        Título: {title}
        Empresa: {company}
        Localização: {location}
        Link: {url}
        """;

        await SendMessageAsync(message);
    }
}