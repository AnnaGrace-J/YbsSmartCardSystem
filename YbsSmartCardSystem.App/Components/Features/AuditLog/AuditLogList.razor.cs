using Microsoft.AspNetCore.Components;
using YbsSmartCardSystem.App.Services;
using YbsSmartCardSystem.Contracts.Features.AuditLog;
using YbsSmartCardSystem.Domain.Common;

namespace YbsSmartCardSystem.App.Components.Features.AuditLog;

public partial class AuditLogList : ComponentBase, IDisposable
{
    [Inject] private ApiService Api { get; set; } = default!;
    [Inject] private AuthStateService AuthState { get; set; } = default!;
    [Inject] private NavigationManager NavManager { get; set; } = default!;

    private AuditLogListRequestModel Request { get; set; } = new() { PageNo = 1, PageSize = 10 };
    private Result<AuditLogListResponseModel>? Response { get; set; }
    private bool IsLoading { get; set; }
    private string? ErrorMessage { get; set; }
    private long? ExpandedId { get; set; }

    private int TotalPages => Response?.Data == null || Request.PageSize == 0
        ? 1
        : (int)Math.Ceiling((double)Response.Data.TotalCount / Request.PageSize);

    protected override async Task OnInitializedAsync()
    {
        AuthState.OnChange += StateHasChanged;
        if (!AuthState.IsAuthenticated || !AuthState.HasPermission("AuditLog.View"))
            return;

        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        IsLoading = true;
        ErrorMessage = null;
        StateHasChanged();

        var result = await Api.GetAuditLogs(Request);
        Response = result;
        if (!result.IsSuccess)
            ErrorMessage = result.Message ?? "Failed to load audit logs.";

        IsLoading = false;
        StateHasChanged();
    }

    private async Task SearchAsync()
    {
        Request.PageNo = 1;
        await LoadAsync();
    }

    private async Task ResetAsync()
    {
        Request = new AuditLogListRequestModel { PageNo = 1, PageSize = 10 };
        ExpandedId = null;
        await LoadAsync();
    }

    private async Task PrevPage()
    {
        if (Request.PageNo > 1)
        {
            Request.PageNo--;
            await LoadAsync();
        }
    }

    private async Task NextPage()
    {
        if (Request.PageNo < TotalPages)
        {
            Request.PageNo++;
            await LoadAsync();
        }
    }

    private void ToggleDetails(long id)
    {
        ExpandedId = ExpandedId == id ? null : id;
    }

    private string GetActionBadgeClass(string action)
    {
        return action.ToLowerInvariant() switch
        {
            "delete" or "remove" => "bg-rose-50 text-rose-700 border-rose-200",
            "create" or "register" or "add" => "bg-emerald-50 text-emerald-700 border-emerald-200",
            "update" or "edit" or "patch" or "topup" => "bg-amber-50 text-amber-800 border-amber-500/50",
            "login" => "bg-blue-50 text-blue-700 border-blue-200",
            "logout" => "bg-slate-100 text-slate-600 border-slate-200",
            _ => "bg-slate-100 text-slate-700 border-slate-200"
        };
    }

    private string GetActionIcon(string action)
    {
        return action.ToLowerInvariant() switch
        {
            "delete" or "remove" => "bi-trash",
            "create" or "register" or "add" => "bi-plus-lg",
            "update" or "edit" or "patch" or "topup" => "bi-pencil",
            "login" => "bi-box-arrow-in-right",
            "logout" => "bi-box-arrow-right",
            _ => "bi-activity"
        };
    }

    public void Dispose()
    {
        AuthState.OnChange -= StateHasChanged;
    }
}
