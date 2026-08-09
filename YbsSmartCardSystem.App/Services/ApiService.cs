using YbsSmartCardSystem.Domain.Common;
using System.Net.Http.Json;
using YbsSmartCardSystem.Domain;
using YbsSmartCardSystem.Contracts.Features.Card;
using YbsSmartCardSystem.Contracts.Features.TopUp;
using YbsSmartCardSystem.Contracts.Features.BusPayment;
using YbsSmartCardSystem.Contracts.Features.Transaction;
using YbsSmartCardSystem.Contracts.Features.Auth;
using YbsSmartCardSystem.Contracts.Features.RolePermission;
using YbsSmartCardSystem.Contracts.Features.AuditLog;

namespace YbsSmartCardSystem.App.Services;

public class ApiService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly AuthStateService _authState;
    private readonly string _baseUrl;

    public ApiService(IHttpClientFactory httpClientFactory, IConfiguration configuration, AuthStateService authState)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _authState = authState;
        _baseUrl = _configuration.GetValue<string>("BackendApiUrl")!;
    }

    private HttpClient CreateClient()
    {
        var httpClient = _httpClientFactory.CreateClient();
        httpClient.BaseAddress = new Uri(_baseUrl);

        if (_authState.IsAuthenticated)
        {
            httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _authState.Token);
        }

        return httpClient;
    }

    // ── Auth ────────────────────────────────────────────────────────────────

    public async Task<Result<LoginResponseModel>> Login(LoginRequestModel request)
    {
        try
        {
            var httpClient = CreateClient();

            var response = await httpClient.PostAsJsonAsync(ApiEndpoints.Login, request);
            var result = await response.Content.ReadFromJsonAsync<Result<LoginResponseModel>>();
            return result ?? new Result<LoginResponseModel>
            {
                IsSuccess = false,
                Message   = "Invalid response from API."
            };
        }
        catch (Exception ex)
        {
            return new Result<LoginResponseModel>
            {
                IsSuccess = false,
                Message   = $"Failed to reach API: {ex.Message}"
            };
        }
    }

    public async Task<Result<CurrentUserPermissionsResponseModel>> GetPermissions()
    {
        try
        {
            var httpClient = CreateClient();
            var response = await httpClient.GetAsync(ApiEndpoints.Permissions);
            return await ReadResultAsync<CurrentUserPermissionsResponseModel>(response, "load permissions");
        }
        catch (Exception ex)
        {
            return new Result<CurrentUserPermissionsResponseModel>
            {
                IsSuccess = false,
                Message   = $"Failed to reach API: {ex.Message}"
            };
        }
    }

    public async Task<Result<UserRegistrationSendOtpResponseModel>> SendUserRegistrationOtp(UserRegistrationSendOtpRequestModel request)
    {
        try
        {
            var httpClient = CreateClient();
            var response = await httpClient.PostAsJsonAsync(ApiEndpoints.UserRegistrationSendOtp, request);
            var result = await response.Content.ReadFromJsonAsync<Result<UserRegistrationSendOtpResponseModel>>();
            return result ?? new Result<UserRegistrationSendOtpResponseModel> { IsSuccess = false, Message = "Invalid response from API." };
        }
        catch (Exception ex)
        {
            return new Result<UserRegistrationSendOtpResponseModel> { IsSuccess = false, Message = $"Failed to reach API: {ex.Message}" };
        }
    }

    public async Task<Result<UserRegisterResponseModel>> Register(UserRegisterRequestModel request)
    {
        try
        {
            var httpClient = CreateClient();
            var response = await httpClient.PostAsJsonAsync(ApiEndpoints.UserRegister, request);
            var result = await response.Content.ReadFromJsonAsync<Result<UserRegisterResponseModel>>();
            return result ?? new Result<UserRegisterResponseModel> { IsSuccess = false, Message = "Invalid response from API." };
        }
        catch (Exception ex)
        {
            return new Result<UserRegisterResponseModel> { IsSuccess = false, Message = $"Failed to reach API: {ex.Message}" };
        }
    }

    public async Task<Result<UserDashboardResponseModel>> GetUserDashboard()
    {
        try
        {
            var httpClient = CreateClient();
            var response = await httpClient.GetAsync(ApiEndpoints.UserDashboard);
            var result = await response.Content.ReadFromJsonAsync<Result<UserDashboardResponseModel>>();
            return result ?? new Result<UserDashboardResponseModel> { IsSuccess = false, Message = "Invalid response from API." };
        }
        catch (Exception ex)
        {
            return new Result<UserDashboardResponseModel> { IsSuccess = false, Message = $"Failed to reach API: {ex.Message}" };
        }
    }

    public async Task<Result<AuditLogListResponseModel>> GetAuditLogs(AuditLogListRequestModel request)
    {
        try
        {
            var httpClient = CreateClient();
            var queryParts = new List<string>
            {
                $"pageNo={request.PageNo}",
                $"pageSize={request.PageSize}"
            };
            if (request.UserId.HasValue)      queryParts.Add($"userId={request.UserId}");
            if (!string.IsNullOrWhiteSpace(request.Action))      queryParts.Add($"action={Uri.EscapeDataString(request.Action)}");
            if (!string.IsNullOrWhiteSpace(request.FeatureName)) queryParts.Add($"featureName={Uri.EscapeDataString(request.FeatureName)}");
            if (!string.IsNullOrWhiteSpace(request.EntityName))  queryParts.Add($"entityName={Uri.EscapeDataString(request.EntityName)}");
            if (request.FromDate.HasValue)    queryParts.Add($"fromDate={request.FromDate:yyyy-MM-dd}");
            if (request.ToDate.HasValue)      queryParts.Add($"toDate={request.ToDate:yyyy-MM-dd}");

            var url = $"{ApiEndpoints.AuditLogList}?{string.Join("&", queryParts)}";
            var response = await httpClient.GetAsync(url);
            var result = await response.Content.ReadFromJsonAsync<Result<AuditLogListResponseModel>>();
            return result ?? new Result<AuditLogListResponseModel> { IsSuccess = false, Message = "Invalid response from API." };
        }
        catch (Exception ex)
        {
            return new Result<AuditLogListResponseModel> { IsSuccess = false, Message = $"Failed to reach API: {ex.Message}" };
        }
    }

    // ── Card ────────────────────────────────────────────────────────────────

    public async Task<Result<CardListResponseModel>> GetCards(CardListRequestModel request)
    {
        try
        {
            var httpClient = CreateClient();

            var search = string.IsNullOrWhiteSpace(request.Search) ? "" : $"&search={Uri.EscapeDataString(request.Search)}";
            var dateFilter = request.FilterDate.HasValue ? $"&filterDate={request.FilterDate.Value:yyyy-MM-dd}" : "";
            var isDeletedFilter = request.IsDeleted.HasValue ? $"&isDeleted={request.IsDeleted.Value}" : "";
            var url = $"{ApiEndpoints.CardList}?pageNo={request.PageNo}&pageSize={request.PageSize}{search}{dateFilter}{isDeletedFilter}";

            var response = await httpClient.GetAsync(url);
            try
            {
                var result = await response.Content.ReadFromJsonAsync<Result<CardListResponseModel>>();
                return result ?? new Result<CardListResponseModel>
                {
                    IsSuccess = false,
                    Message   = "Invalid response from API."
                };
            }
            catch (Exception ex)
            {
                var raw = await response.Content.ReadAsStringAsync();
                System.IO.File.WriteAllText(System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "api_error_log.txt"), raw);
                return new Result<CardListResponseModel> { IsSuccess = false, Message = $"Failed to reach API: {ex.Message}" };
            }
        }
        catch (Exception ex)
        {
            return new Result<CardListResponseModel> { IsSuccess = false, Message = ex.Message };
        }
    }

    public async Task<Result<CardCreateResponseModel>> CardCreate(CardCreateRequestModel request)
    {
        try
        {
            var httpClient = CreateClient();
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

    public async Task<Result<CardRegistrationSendOtpResponseModel>> SendCardRegistrationOtp(CardRegistrationSendOtpRequestModel request)
    {
        try
        {
            var httpClient = CreateClient();
            var response = await httpClient.PostAsJsonAsync(ApiEndpoints.CardRegistrationSendOtp, request);
            var result = await response.Content.ReadFromJsonAsync<Result<CardRegistrationSendOtpResponseModel>>();
            return result ?? new Result<CardRegistrationSendOtpResponseModel> { IsSuccess = false, Message = "Invalid response from API." };
        }
        catch (Exception ex)
        {
            return new Result<CardRegistrationSendOtpResponseModel> { IsSuccess = false, Message = $"Failed to reach API: {ex.Message}" };
        }
    }

    public async Task<Result<CardRegistrationVerifyOtpResponseModel>> VerifyCardRegistrationOtp(CardRegistrationVerifyOtpRequestModel request)
    {
        try
        {
            var httpClient = CreateClient();
            var response = await httpClient.PostAsJsonAsync(ApiEndpoints.CardRegistrationVerifyOtp, request);
            var result = await response.Content.ReadFromJsonAsync<Result<CardRegistrationVerifyOtpResponseModel>>();
            return result ?? new Result<CardRegistrationVerifyOtpResponseModel> { IsSuccess = false, Message = "Invalid response from API." };
        }
        catch (Exception ex)
        {
            return new Result<CardRegistrationVerifyOtpResponseModel> { IsSuccess = false, Message = $"Failed to reach API: {ex.Message}" };
        }
    }

    public async Task<Result<CardModel>> CardPatch(int id, CardPatchRequestModel request)
    {
        try
        {
            var httpClient = CreateClient();
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

    public async Task<Result<CardModel>> CardDelete(int id)
    {
        try
        {
            var httpClient = CreateClient();
            var response = await httpClient.DeleteAsync(ApiEndpoints.CardDetail(id));
            var result = await response.Content.ReadFromJsonAsync<Result<CardModel>>();
            return result ?? new Result<CardModel>
            {
                IsSuccess  = false,
                Message    = "Invalid response from API."
            };
        }
        catch (Exception ex)
        {
            return new Result<CardModel>
            {
                IsSuccess  = false,
                Message    = $"Failed to reach API: {ex.Message}"
            };
        }
    }

    public async Task<Result<CardModel?>> GetMyCard()
    {
        try
        {
            var httpClient = CreateClient();
            var response = await httpClient.GetAsync("api/Card/my-card");
            var result = await response.Content.ReadFromJsonAsync<Result<CardModel?>>();
            return result ?? new Result<CardModel?>
            {
                IsSuccess = false,
                Message   = "Invalid response from API."
            };
        }
        catch (Exception ex)
        {
            return new Result<CardModel?>
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
            var httpClient = CreateClient();
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
            var httpClient = CreateClient();
            var cardFilter = request.CardId > 0 ? $"&cardId={request.CardId}" : "";
            var searchFilter = string.IsNullOrWhiteSpace(request.Search) ? "" : $"&search={Uri.EscapeDataString(request.Search)}";
            var dateFilter = request.FilterDate.HasValue ? $"&filterDate={request.FilterDate.Value:yyyy-MM-dd}" : "";
            var url = $"{ApiEndpoints.TopUpList}?pageNo={request.PageNo}&pageSize={request.PageSize}{cardFilter}{searchFilter}{dateFilter}";
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
            var httpClient = CreateClient();
            var search = string.IsNullOrWhiteSpace(request.Search) ? "" : $"&search={Uri.EscapeDataString(request.Search)}";
            var isDeletedFilter = request.IsDeleted.HasValue ? $"&isDeleted={request.IsDeleted.Value}" : "";
            var url = $"{ApiEndpoints.BusList}?pageNo={request.PageNo}&pageSize={request.PageSize}{search}{isDeletedFilter}";
            var response = await httpClient.GetAsync(url);
            return await ReadResultAsync<BusListResponseModel>(response, "load buses");
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
            var httpClient = CreateClient();
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
            var httpClient = CreateClient();
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
            var httpClient = CreateClient();
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
            var httpClient = CreateClient();
            var search = string.IsNullOrWhiteSpace(request.Search) ? "" : $"&search={Uri.EscapeDataString(request.Search)}";
            var isDeletedFilter = request.IsDeleted.HasValue ? $"&isDeleted={request.IsDeleted.Value}" : "";
            var url = $"{ApiEndpoints.TerminalList}?pageNo={request.PageNo}&pageSize={request.PageSize}{search}{isDeletedFilter}";
            var response = await httpClient.GetAsync(url);
            return await ReadResultAsync<TerminalListResponseModel>(response, "load terminals");
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
            var httpClient = CreateClient();
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
            var httpClient = CreateClient();
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
            var httpClient = CreateClient();
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
            var httpClient = CreateClient();
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
            var httpClient = CreateClient();

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

    // ── RolePermission ──────────────────────────────────────────────────────

    public async Task<Result<RoleListResponseModel>> GetRoles(RoleListRequestModel request)
    {
        try
        {
            var httpClient = CreateClient();
            var search = string.IsNullOrWhiteSpace(request.Search) ? "" : $"&search={Uri.EscapeDataString(request.Search)}";
            var activeFilter = request.IsActive.HasValue ? $"&isActive={request.IsActive.Value}" : "";
            var url = $"{ApiEndpoints.RoleList}?pageNo={request.PageNo}&pageSize={request.PageSize}{search}{activeFilter}";

            var response = await httpClient.GetAsync(url);
            return await ReadResultAsync<RoleListResponseModel>(response, "load roles");
        }
        catch (Exception ex)
        {
            return new Result<RoleListResponseModel> { IsSuccess = false, StatusCode = 500, Message = $"Failed to reach API: {ex.Message}" };
        }
    }

    public async Task<Result<RoleModel>> GetRoleById(int roleId)
    {
        try
        {
            var httpClient = CreateClient();
            var response = await httpClient.GetAsync(ApiEndpoints.RoleDetail(roleId));
            return await ReadResultAsync<RoleModel>(response, "load role details");
        }
        catch (Exception ex)
        {
            return new Result<RoleModel> { IsSuccess = false, StatusCode = 500, Message = $"Failed to reach API: {ex.Message}" };
        }
    }

    public async Task<Result<RoleModel>> RoleCreate(RoleCreateRequestModel request)
    {
        try
        {
            var httpClient = CreateClient();
            var response = await httpClient.PostAsJsonAsync(ApiEndpoints.CreateRole, request);
            return await ReadResultAsync<RoleModel>(response, "create role");
        }
        catch (Exception ex)
        {
            return new Result<RoleModel> { IsSuccess = false, StatusCode = 500, Message = $"Failed to reach API: {ex.Message}" };
        }
    }

    public async Task<Result<RoleModel>> RolePatch(int roleId, RolePatchRequestModel request)
    {
        try
        {
            var httpClient = CreateClient();
            var response = await httpClient.PatchAsJsonAsync(ApiEndpoints.RoleDetail(roleId), request);
            return await ReadResultAsync<RoleModel>(response, "update role");
        }
        catch (Exception ex)
        {
            return new Result<RoleModel> { IsSuccess = false, StatusCode = 500, Message = $"Failed to reach API: {ex.Message}" };
        }
    }

    public async Task<Result<RoleModel>> RoleDelete(int roleId)
    {
        try
        {
            var httpClient = CreateClient();
            var response = await httpClient.DeleteAsync(ApiEndpoints.RoleDetail(roleId));
            return await ReadResultAsync<RoleModel>(response, "delete role");
        }
        catch (Exception ex)
        {
            return new Result<RoleModel> { IsSuccess = false, StatusCode = 500, Message = $"Failed to reach API: {ex.Message}" };
        }
    }

    public async Task<Result<PermissionListResponseModel>> GetPermissions(PermissionListRequestModel request)
    {
        try
        {
            var httpClient = CreateClient();
            var search = string.IsNullOrWhiteSpace(request.Search) ? "" : $"&search={Uri.EscapeDataString(request.Search)}";
            var feature = string.IsNullOrWhiteSpace(request.FeatureName) ? "" : $"&featureName={Uri.EscapeDataString(request.FeatureName)}";
            var activeFilter = request.IsActive.HasValue ? $"&isActive={request.IsActive.Value}" : "";
            var url = $"{ApiEndpoints.PermissionList}?pageNo={request.PageNo}&pageSize={request.PageSize}{search}{feature}{activeFilter}";

            var response = await httpClient.GetAsync(url);
            return await ReadResultAsync<PermissionListResponseModel>(response, "load permissions");
        }
        catch (Exception ex)
        {
            return new Result<PermissionListResponseModel> { IsSuccess = false, StatusCode = 500, Message = $"Failed to reach API: {ex.Message}" };
        }
    }

    public async Task<Result<UserRoleResponseModel>> GetUserRoles(int userId)
    {
        try
        {
            var httpClient = CreateClient();
            var response = await httpClient.GetAsync(ApiEndpoints.UserRoles(userId));
            return await ReadResultAsync<UserRoleResponseModel>(response, "load user roles");
        }
        catch (Exception ex)
        {
            return new Result<UserRoleResponseModel> { IsSuccess = false, StatusCode = 500, Message = $"Failed to reach API: {ex.Message}" };
        }
    }

    public async Task<Result<UserRoleResponseModel>> UpdateUserRoles(UserRoleUpdateRequestModel request)
    {
        try
        {
            var httpClient = CreateClient();
            var response = await httpClient.PutAsJsonAsync(ApiEndpoints.UserRoles(request.UserId), request);
            return await ReadResultAsync<UserRoleResponseModel>(response, "update user roles");
        }
        catch (Exception ex)
        {
            return new Result<UserRoleResponseModel> { IsSuccess = false, StatusCode = 500, Message = $"Failed to reach API: {ex.Message}" };
        }
    }

    public async Task<Result<RolePermissionResponseModel>> GetRolePermissions(int roleId)
    {
        try
        {
            var httpClient = CreateClient();
            var response = await httpClient.GetAsync(ApiEndpoints.RolePermissions(roleId));
            return await ReadResultAsync<RolePermissionResponseModel>(response, "load role permissions");
        }
        catch (Exception ex)
        {
            return new Result<RolePermissionResponseModel> { IsSuccess = false, StatusCode = 500, Message = $"Failed to reach API: {ex.Message}" };
        }
    }

    public async Task<Result<RolePermissionResponseModel>> UpdateRolePermissions(RolePermissionUpdateRequestModel request)
    {
        try
        {
            var httpClient = CreateClient();
            var response = await httpClient.PutAsJsonAsync(ApiEndpoints.RolePermissions(request.RoleId), request);
            return await ReadResultAsync<RolePermissionResponseModel>(response, "update role permissions");
        }
        catch (Exception ex)
        {
            return new Result<RolePermissionResponseModel> { IsSuccess = false, StatusCode = 500, Message = $"Failed to reach API: {ex.Message}" };
        }
    }

    private static async Task<Result<T>> ReadResultAsync<T>(HttpResponseMessage response, string action)
    {
        if (!response.IsSuccessStatusCode)
        {
            return new Result<T>
            {
                IsSuccess = false,
                StatusCode = (int)response.StatusCode,
                Message = GetHttpErrorMessage(response, action)
            };
        }

        try
        {
            var result = await response.Content.ReadFromJsonAsync<Result<T>>();
            return result ?? new Result<T>
            {
                IsSuccess = false,
                StatusCode = (int)response.StatusCode,
                Message = "Invalid response from API."
            };
        }
        catch
        {
            return new Result<T>
            {
                IsSuccess = false,
                StatusCode = (int)response.StatusCode,
                Message = $"API returned an invalid response while trying to {action}."
            };
        }
    }

    private static string GetHttpErrorMessage(HttpResponseMessage response, string action)
    {
        return (int)response.StatusCode switch
        {
            401 => $"Please log in again to {action}.",
            403 => $"You do not have permission to {action}.",
            _   => $"API returned {(int)response.StatusCode} while trying to {action}."
        };
    }
}

public static class ApiEndpoints
{
    // Card
    public const string CardList   = "api/Card";
    public const string CreateCard = "api/Card";
    public const string CardRegistrationSendOtp = "api/Card/Registration/SendOtp";
    public const string CardRegistrationVerifyOtp = "api/Card/Registration/VerifyOtp";
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

    // Auth
    public const string Login = "api/Auth/Login";
    public const string Permissions = "api/Auth/Permissions";
    public const string UserRegistrationSendOtp = "api/Auth/Register/SendOtp";
    public const string UserRegister = "api/Auth/Register";
    public const string UserDashboard = "api/Auth/Dashboard";

    // RolePermission
    public const string RoleList = "api/RolePermission/Roles";
    public const string CreateRole = "api/RolePermission/Roles";
    public static string RoleDetail(int roleId) => $"api/RolePermission/Roles/{roleId}";

    // AuditLog
    public const string AuditLogList = "api/AuditLog";
    public const string PermissionList = "api/RolePermission/Permissions";
    public static string UserRoles(int userId) => $"api/RolePermission/Users/{userId}/Roles";
    public static string RolePermissions(int roleId) => $"api/RolePermission/Roles/{roleId}/Permissions";
}

