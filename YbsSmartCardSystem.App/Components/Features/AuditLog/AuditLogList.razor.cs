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

    private AuditLogListRequestModel Request { get; set; } = new() { PageNo = 1, PageSize = 20 };
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
        Request = new AuditLogListRequestModel { PageNo = 1, PageSize = 20 };
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

    public void Dispose()
    {
        AuthState.OnChange -= StateHasChanged;
    }
}
