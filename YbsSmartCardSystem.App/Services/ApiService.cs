using System.Net.Http.Json;
using YbsSmartCardSystem.Domain;
using YbsSmartCardSystem.Domain.Features.Card.Models;
using YbsSmartCardSystem.Domain.Features.TopUp.Models;
using YbsSmartCardSystem.Domain.Features.Bus.Models;
using YbsSmartCardSystem.Domain.Features.Terminal.Models;
using YbsSmartCardSystem.Domain.Features.Transaction.Models;

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

    // ── Bus ─────────────────────────────────────────────────────────────────

    public async Task<Result<BusListResponseModel>> GetBuses(BusListRequestModel request)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient();
            httpClient.BaseAddress = new Uri(_baseUrl);
            var url = $"{ApiEndpoints.BusList}?pageNo={request.PageNo}&pageSize={request.PageSize}";
            var response = await httpClient.GetAsync(url);
            var result = await response.Content.ReadFromJsonAsync<Result<BusListResponseModel>>();
            return result ?? new Result<BusListResponseModel>
            {
                IsSuccess  = false,
                StatusCode = (int)response.StatusCode,
                Message    = "Invalid response from API."
            };
        }
        catch (Exception ex)
        {
            return new Result<BusListResponseModel>
            {
                IsSuccess  = false,
                StatusCode = 500,
                Message    = $"Failed to reach API: {ex.Message}"
            };
        }
    }

    public async Task<Result<BusCreateResponseModel>> BusCreate(BusCreateRequestModel request)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient();
            httpClient.BaseAddress = new Uri(_baseUrl);
            var response = await httpClient.PostAsJsonAsync(ApiEndpoints.CreateBus, request);
            var result = await response.Content.ReadFromJsonAsync<Result<BusCreateResponseModel>>();
            return result ?? new Result<BusCreateResponseModel>
            {
                IsSuccess  = false,
                StatusCode = (int)response.StatusCode,
                Message    = "Invalid response from API."
            };
        }
        catch (Exception ex)
        {
            return new Result<BusCreateResponseModel>
            {
                IsSuccess  = false,
                StatusCode = 500,
                Message    = $"Failed to reach API: {ex.Message}"
            };
        }
    }

    public async Task<Result<BusModel>> BusPatch(int id, BusPatchRequestModel request)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient();
            httpClient.BaseAddress = new Uri(_baseUrl);
            var response = await httpClient.PatchAsJsonAsync(ApiEndpoints.BusDetail(id), request);
            var result = await response.Content.ReadFromJsonAsync<Result<BusModel>>();
            return result ?? new Result<BusModel>
            {
                IsSuccess  = false,
                StatusCode = (int)response.StatusCode,
                Message    = "Invalid response from API."
            };
        }
        catch (Exception ex)
        {
            return new Result<BusModel>
            {
                IsSuccess  = false,
                StatusCode = 500,
                Message    = $"Failed to reach API: {ex.Message}"
            };
        }
    }

    public async Task<Result<BusModel>> BusDelete(int id)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient();
            httpClient.BaseAddress = new Uri(_baseUrl);
            var response = await httpClient.DeleteAsync(ApiEndpoints.BusDetail(id));
            var result = await response.Content.ReadFromJsonAsync<Result<BusModel>>();
            return result ?? new Result<BusModel>
            {
                IsSuccess  = false,
                StatusCode = (int)response.StatusCode,
                Message    = "Invalid response from API."
            };
        }
        catch (Exception ex)
        {
            return new Result<BusModel>
            {
                IsSuccess  = false,
                StatusCode = 500,
                Message    = $"Failed to reach API: {ex.Message}"
            };
        }
    }

    // ── Terminal ─────────────────────────────────────────────────────────────

    public async Task<Result<TerminalListResponseModel>> GetTerminals(TerminalListRequestModel request)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient();
            httpClient.BaseAddress = new Uri(_baseUrl);
            var url = $"{ApiEndpoints.TerminalList}?pageNo={request.PageNo}&pageSize={request.PageSize}";
            var response = await httpClient.GetAsync(url);
            var result = await response.Content.ReadFromJsonAsync<Result<TerminalListResponseModel>>();
            return result ?? new Result<TerminalListResponseModel>
            {
                IsSuccess  = false,
                StatusCode = (int)response.StatusCode,
                Message    = "Invalid response from API."
            };
        }
        catch (Exception ex)
        {
            return new Result<TerminalListResponseModel>
            {
                IsSuccess  = false,
                StatusCode = 500,
                Message    = $"Failed to reach API: {ex.Message}"
            };
        }
    }

    public async Task<Result<TerminalCreateResponseModel>> TerminalCreate(TerminalCreateRequestModel request)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient();
            httpClient.BaseAddress = new Uri(_baseUrl);
            var response = await httpClient.PostAsJsonAsync(ApiEndpoints.CreateTerminal, request);
            var result = await response.Content.ReadFromJsonAsync<Result<TerminalCreateResponseModel>>();
            return result ?? new Result<TerminalCreateResponseModel>
            {
                IsSuccess  = false,
                StatusCode = (int)response.StatusCode,
                Message    = "Invalid response from API."
            };
        }
        catch (Exception ex)
        {
            return new Result<TerminalCreateResponseModel>
            {
                IsSuccess  = false,
                StatusCode = 500,
                Message    = $"Failed to reach API: {ex.Message}"
            };
        }
    }

    public async Task<Result<TerminalModel>> TerminalPatch(int id, TerminalPatchRequestModel request)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient();
            httpClient.BaseAddress = new Uri(_baseUrl);
            var response = await httpClient.PatchAsJsonAsync(ApiEndpoints.TerminalDetail(id), request);
            var result = await response.Content.ReadFromJsonAsync<Result<TerminalModel>>();
            return result ?? new Result<TerminalModel>
            {
                IsSuccess  = false,
                StatusCode = (int)response.StatusCode,
                Message    = "Invalid response from API."
            };
        }
        catch (Exception ex)
        {
            return new Result<TerminalModel>
            {
                IsSuccess  = false,
                StatusCode = 500,
                Message    = $"Failed to reach API: {ex.Message}"
            };
        }
    }

    public async Task<Result<TerminalModel>> TerminalDelete(int id)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient();
            httpClient.BaseAddress = new Uri(_baseUrl);
            var response = await httpClient.DeleteAsync(ApiEndpoints.TerminalDetail(id));
            var result = await response.Content.ReadFromJsonAsync<Result<TerminalModel>>();
            return result ?? new Result<TerminalModel>
            {
                IsSuccess  = false,
                StatusCode = (int)response.StatusCode,
                Message    = "Invalid response from API."
            };
        }
        catch (Exception ex)
        {
            return new Result<TerminalModel>
            {
                IsSuccess  = false,
                StatusCode = 500,
                Message    = $"Failed to reach API: {ex.Message}"
            };
        }
    }

    public async Task<Result<TransactionCreateResponseModel>> TransactionCreate(TransactionCreateRequestModel request)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient();
            httpClient.BaseAddress = new Uri(_baseUrl);
            var response = await httpClient.PostAsJsonAsync(ApiEndpoints.CreateTransaction, request);
            var result = await response.Content.ReadFromJsonAsync<Result<TransactionCreateResponseModel>>();
            return result ?? new Result<TransactionCreateResponseModel>
            {
                IsSuccess = false,
                StatusCode = (int)response.StatusCode,
                Message = "Invalid response from API."
            };
        }
        catch (Exception ex)
        {
            return new Result<TransactionCreateResponseModel>
            {
                IsSuccess = false,
                StatusCode = 500,
                Message = $"Failed to reach API: {ex.Message}"
            };
        }
    }

    public async Task<Result<TransactionListResponseModel>> GetTransactions(TransactionListRequestModel request)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient();
            httpClient.BaseAddress = new Uri(_baseUrl);

            var cardFilter = string.IsNullOrWhiteSpace(request.CardNum)
                ? string.Empty
                : $"&cardNum={Uri.EscapeDataString(request.CardNum)}";
            var terminalFilter = string.IsNullOrWhiteSpace(request.TerminalSerialNo)
                ? string.Empty
                : $"&terminalSerialNo={Uri.EscapeDataString(request.TerminalSerialNo)}";
            var url = $"{ApiEndpoints.TransactionList}?pageNo={request.PageNo}&pageSize={request.PageSize}{cardFilter}{terminalFilter}";

            var response = await httpClient.GetAsync(url);
            var result = await response.Content.ReadFromJsonAsync<Result<TransactionListResponseModel>>();
            return result ?? new Result<TransactionListResponseModel>
            {
                IsSuccess = false,
                StatusCode = (int)response.StatusCode,
                Message = "Invalid response from API."
            };
        }
        catch (Exception ex)
        {
            return new Result<TransactionListResponseModel>
            {
                IsSuccess = false,
                StatusCode = 500,
                Message = $"Failed to reach API: {ex.Message}"
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

    // Bus
    public const string BusList = "api/Bus";
    public const string CreateBus = "api/Bus";
    public static string BusDetail(int busId) => $"api/Bus/{busId}";

    // Terminal
    public const string TerminalList   = "api/Terminal";
    public const string CreateTerminal = "api/Terminal";
    public static string TerminalDetail(int terminalId) => $"api/Terminal/{terminalId}";

    // Transaction
    public const string CreateTransaction = "api/Transaction";
    public const string TransactionList = "api/Transaction";
}
