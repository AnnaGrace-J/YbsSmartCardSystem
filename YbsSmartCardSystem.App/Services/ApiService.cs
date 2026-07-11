using YbsSmartCardSystem.Domain;
using YbsSmartCardSystem.Domain.Features.Card.Models;

namespace YbsSmartCardSystem.App.Services;

public class ApiService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly string _baseUrl;

    public ApiService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _baseUrl = _configuration.GetValue<string>("BackendApiUrl")!;
    }

    public async Task<Result<CardListResponseModel>> GetCards(CardListRequestModel request)
    {
        var httpClient = _httpClientFactory.CreateClient();
        httpClient.BaseAddress = new Uri(_baseUrl);
        string url = $"{ApiEndpoints.CardList}?pageNo={request.PageNo}&pageSize={request.PageSize}";
        var response = await httpClient.GetAsync(url);
        var result = await response.Content.ReadFromJsonAsync<Result<CardListResponseModel>>();
        return result!;
    }
    public async Task<Result<CardCreateResponseModel>> CardCreate(CardCreateRequestModel request)
    {
        var httpClient = _httpClientFactory.CreateClient();
        httpClient.BaseAddress = new Uri(_baseUrl);
        var response = await httpClient.PostAsJsonAsync(ApiEndpoints.CreateCard, request);
        var result = await response.Content.ReadFromJsonAsync<Result<CardCreateResponseModel>>();
        return result!;
    }

}

public class ApiEndpoints
{
    public const string CardList = "api/Card";
    public const string CardDetail = "api/Card/{cardId}";
    public const string CreateCard = "api/Card";
}