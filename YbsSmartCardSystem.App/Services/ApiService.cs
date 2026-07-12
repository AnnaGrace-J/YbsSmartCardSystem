using System.Net.Http.Json;
using YbsSmartCardSystem.Domain;
using YbsSmartCardSystem.Domain.Features.Card.Models;
using YbsSmartCardSystem.Domain.Features.TopUp.Models;

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

    // ── Card ────────────────────────────────────────────────────────────────

    public async Task<Result<CardListResponseModel>> GetCards(CardListRequestModel request)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient();
            httpClient.BaseAddress = new Uri(_baseUrl);

            var search = string.IsNullOrWhiteSpace(request.Search) ? "" : $"&search={Uri.EscapeDataString(request.Search)}";
            var url = $"{ApiEndpoints.CardList}?pageNo={request.PageNo}&pageSize={request.PageSize}{search}";

            var response = await httpClient.GetAsync(url);
            var result = await response.Content.ReadFromJsonAsync<Result<CardListResponseModel>>();
            return result ?? new Result<CardListResponseModel>
            {
                IsSuccess = false,
                Message   = "Invalid response from API."
            };
        }
        catch (Exception ex)
        {
            return new Result<CardListResponseModel>
            {
                IsSuccess = false,
                Message   = $"Failed to reach API: {ex.Message}"
            };
        }
    }

    public async Task<Result<CardCreateResponseModel>> CardCreate(CardCreateRequestModel request)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient();
            httpClient.BaseAddress = new Uri(_baseUrl);
            var response = await httpClient.PostAsJsonAsync(ApiEndpoints.CreateCard, request);
            var result = await response.Content.ReadFromJsonAsync<Result<CardCreateResponseModel>>();
            return result ?? new Result<CardCreateResponseModel>
            {
                IsSuccess = false,
                Message   = "Invalid response from API."
            };
        }
        catch (Exception ex)
        {
            return new Result<CardCreateResponseModel>
            {
                IsSuccess = false,
                Message   = $"Failed to reach API: {ex.Message}"
            };
        }
    }

    public async Task<Result<CardModel>> CardPatch(int id, CardPatchRequestModel request)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient();
            httpClient.BaseAddress = new Uri(_baseUrl);
            var httpRequest = new HttpRequestMessage(HttpMethod.Patch, ApiEndpoints.CardDetail(id))
            {
                Content = JsonContent.Create(request)
            };
            var response = await httpClient.SendAsync(httpRequest);
            var result = await response.Content.ReadFromJsonAsync<Result<CardModel>>();
            return result ?? new Result<CardModel>
            {
                IsSuccess = false,
                Message   = "Invalid response from API."
            };
        }
        catch (Exception ex)
        {
            return new Result<CardModel>
            {
                IsSuccess = false,
                Message   = $"Failed to reach API: {ex.Message}"
            };
        }
    }

    // ── TopUp ───────────────────────────────────────────────────────────────

    public async Task<Result<TopUpCreateResponseModel>> TopUpCreate(TopUpCreateRequestModel request)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient();
            httpClient.BaseAddress = new Uri(_baseUrl);
            var response = await httpClient.PostAsJsonAsync(ApiEndpoints.TopUpCreate, request);
            var result = await response.Content.ReadFromJsonAsync<Result<TopUpCreateResponseModel>>();
            return result ?? new Result<TopUpCreateResponseModel>
            {
                IsSuccess = false,
                Message   = "Invalid response from API."
            };
        }
        catch (Exception ex)
        {
            return new Result<TopUpCreateResponseModel>
            {
                IsSuccess = false,
                Message   = $"Failed to reach API: {ex.Message}"
            };
        }
    }

    public async Task<Result<TopUpListResponseModel>> GetTopUps(TopUpListRequestModel request)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient();
            httpClient.BaseAddress = new Uri(_baseUrl);
            var cardFilter = request.CardId > 0 ? $"&cardId={request.CardId}" : "";
            var url = $"{ApiEndpoints.TopUpList}?pageNo={request.PageNo}&pageSize={request.PageSize}{cardFilter}";
            var response = await httpClient.GetAsync(url);
            var result = await response.Content.ReadFromJsonAsync<Result<TopUpListResponseModel>>();
            return result ?? new Result<TopUpListResponseModel>
            {
                IsSuccess = false,
                Message   = "Invalid response from API."
            };
        }
        catch (Exception ex)
        {
            return new Result<TopUpListResponseModel>
            {
                IsSuccess = false,
                Message   = $"Failed to reach API: {ex.Message}"
            };
        }
    }
}

public static class ApiEndpoints
{
    // Card
    public const string CardList   = "api/Card";
    public const string CreateCard = "api/Card";
    public static string CardDetail(int cardId) => $"api/Card/{cardId}";

    // TopUp
    public const string TopUpCreate = "api/TopUp";
    public const string TopUpList   = "api/TopUp";
}