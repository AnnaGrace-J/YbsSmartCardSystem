using YbsSmartCardSystem.Domain.Common;
using Microsoft.EntityFrameworkCore;
using YbsSmartCardSystem.Database.AppDbContextModels;
using YbsSmartCardSystem.Contracts.Features.BusPayment;
using YbsSmartCardSystem.Infrastructure.Services;

namespace YbsSmartCardSystem.Domain.Features.Terminal;

public class TerminalService
{
    private readonly AppDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public TerminalService(AppDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public Result<TerminalListResponseModel> GetList(TerminalListRequestModel request)
    {
        try
        {
            var query = _db.TblTerminals
                .AsNoTracking()
                .Include(x => x.Bus)
                .AsQueryable();

            if (request.IsDeleted.HasValue)
            {
                query = query.Where(x => x.DeleteFlag == request.IsDeleted.Value);
            }

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var search = request.Search.Trim();
                query = query.Where(x => x.TerminalSerialNo.Contains(search) ||
                                         x.Bus.BusNo.Contains(search) ||
                                         x.Bus.BusLicense.Contains(search));
            }

            var totalCount = query.Count();

            var terminals = query
                .OrderByDescending(x => x.TerminalId)
                .Skip((request.PageNo - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(x => new TerminalModel
                {
                    TerminalId       = x.TerminalId,
                    TerminalSerialNo = x.TerminalSerialNo,
                    BusId            = x.BusId,
                    BusNo            = x.Bus.BusNo,
                    BusLicense       = x.Bus.BusLicense,
                    IsActive         = x.IsActive,
                    CreatedByName    = x.CreatedUser != null ? x.CreatedUser.UserName : null,
                    CreatedByRole    = x.CreatedUser != null ? _db.TblUserRoles.Where(ur => ur.UserId == x.CreatedBy && !ur.DeleteFlag).Select(ur => ur.Role.RoleName).FirstOrDefault() : null,
                    DeleteFlag       = x.DeleteFlag
                })
                .ToList();

            return new Result<TerminalListResponseModel>
            {
                IsSuccess  = true,
                StatusCode = 200,
                Message    = "Terminals retrieved successfully.",
                Data       = new TerminalListResponseModel
                {
                    Terminals  = terminals,
                    TotalCount = totalCount
                }
            };
        }
        catch (Exception)
        {
            return new Result<TerminalListResponseModel>
            {
                IsSuccess  = false,
                StatusCode = 500,
                Message    = "An unexpected error occurred."
            };
        }
    }

    public Result<TerminalModel> GetById(int id)
    {
        try
        {
            if (id <= 0)
            {
                return new Result<TerminalModel>
                {
                    IsSuccess  = false,
                    StatusCode = 400,
                    Message    = "TerminalId is required.",
                };
            }

            var item = _db.TblTerminals
                .AsNoTracking()
                .Include(x => x.Bus)
                .FirstOrDefault(x => x.TerminalId == id && x.DeleteFlag == false);

            if (item is null)
            {
                return new Result<TerminalModel>
                {
                    IsSuccess  = false,
                    StatusCode = 404,
                    Message    = "Terminal not found.",
                };
            }

            return new Result<TerminalModel>
            {
                IsSuccess  = true,
                StatusCode = 200,
                Message    = "Terminal retrieved successfully.",
                Data       = new TerminalModel
                {
                    TerminalId       = item.TerminalId,
                    TerminalSerialNo = item.TerminalSerialNo,
                    BusId            = item.BusId,
                    BusNo            = item.Bus.BusNo,
                    BusLicense       = item.Bus.BusLicense,
                    IsActive         = item.IsActive
                }
            };
        }
        catch (Exception)
        {
            return new Result<TerminalModel>
            {
                IsSuccess  = false,
                StatusCode = 500,
                Message    = "An unexpected error occurred."
            };
        }
    }

    public Result<TerminalCreateResponseModel> Create(TerminalCreateRequestModel request)
    {
        try
        {
            if (request is null)
            {
                return new Result<TerminalCreateResponseModel>
                {
                    IsSuccess  = false,
                    StatusCode = 400,
                    Message    = "Request data is required.",
                };
            }

            if (string.IsNullOrWhiteSpace(request.TerminalSerialNo))
            {
                return new Result<TerminalCreateResponseModel>
                {
                    IsSuccess  = false,
                    StatusCode = 400,
                    Message    = "Terminal serial number is required.",
                };
            }

            var terminalSerialNo = request.TerminalSerialNo.Trim();

            if (terminalSerialNo.Length > 100)
            {
                return new Result<TerminalCreateResponseModel>
                {
                    IsSuccess  = false,
                    StatusCode = 400,
                    Message    = "Terminal serial number cannot exceed 100 characters.",
                };
            }

            if (request.BusId <= 0)
            {
                return new Result<TerminalCreateResponseModel>
                {
                    IsSuccess  = false,
                    StatusCode = 400,
                    Message    = "Bus is required.",
                };
            }

            var isDuplicateTerminalSerialNo = _db.TblTerminals
                .AsNoTracking()
                .Any(x => x.TerminalSerialNo == terminalSerialNo && x.DeleteFlag == false);

            if (isDuplicateTerminalSerialNo)
            {
                return new Result<TerminalCreateResponseModel>
                {
                    IsSuccess  = false,
                    StatusCode = 409,
                    Message    = "Terminal serial number already exists.",
                };
            }

            var bus = _db.TblBus
                .AsNoTracking()
                .FirstOrDefault(x => x.BusId == request.BusId && x.DeleteFlag == false);

            if (bus is null)
            {
                return new Result<TerminalCreateResponseModel>
                {
                    IsSuccess  = false,
                    StatusCode = 404,
                    Message    = "Bus not found.",
                };
            }

            var terminal = new TblTerminal
            {
                TerminalSerialNo = terminalSerialNo,
                BusId            = request.BusId,
                IsActive         = request.IsActive,
                DeleteFlag       = false,
                CreatedBy        = _currentUser.UserId
            };

            _db.TblTerminals.Add(terminal);
            _db.SaveChanges();

            return new Result<TerminalCreateResponseModel>
            {
                IsSuccess  = true,
                StatusCode = 200,
                Message    = "Terminal created successfully.",
                Data       = new TerminalCreateResponseModel
                {
                    TerminalId       = terminal.TerminalId,
                    TerminalSerialNo = terminal.TerminalSerialNo,
                    BusId            = bus.BusId,
                    BusNo            = bus.BusNo,
                    BusLicense       = bus.BusLicense,
                    IsActive         = terminal.IsActive
                }
            };
        }
        catch (DbUpdateException)
        {
            return new Result<TerminalCreateResponseModel>
            {
                IsSuccess  = false,
                StatusCode = 409,
                Message    = "Terminal serial number already exists.",
            };
        }
        catch (Exception)
        {
            return new Result<TerminalCreateResponseModel>
            {
                IsSuccess  = false,
                StatusCode = 500,
                Message    = "An unexpected error occurred."
            };
        }
    }

    public Result<TerminalModel> Patch(int id, TerminalPatchRequestModel request)
    {
        try
        {
            if (id <= 0)
            {
                return new Result<TerminalModel>
                {
                    IsSuccess  = false,
                    StatusCode = 400,
                    Message    = "TerminalId is required.",
                };
            }

            if (request is null)
            {
                return new Result<TerminalModel>
                {
                    IsSuccess  = false,
                    StatusCode = 400,
                    Message    = "Request data is required.",
                };
            }

            if (request.TerminalSerialNo is null && request.BusId is null && request.IsActive is null)
            {
                return new Result<TerminalModel>
                {
                    IsSuccess  = false,
                    StatusCode = 400,
                    Message    = "At least one field is required to update.",
                };
            }

            var item = _db.TblTerminals
                .Include(x => x.Bus)
                .FirstOrDefault(x => x.TerminalId == id && x.DeleteFlag == false);

            if (item is null)
            {
                return new Result<TerminalModel>
                {
                    IsSuccess  = false,
                    StatusCode = 404,
                    Message    = "Terminal not found.",
                };
            }

            if (request.TerminalSerialNo is not null)
            {
                if (string.IsNullOrWhiteSpace(request.TerminalSerialNo))
                {
                    return new Result<TerminalModel>
                    {
                        IsSuccess  = false,
                        StatusCode = 400,
                        Message    = "Terminal serial number cannot be empty."
                    };
                }

                var terminalSerialNo = request.TerminalSerialNo.Trim();

                if (terminalSerialNo.Length > 100)
                {
                    return new Result<TerminalModel>
                    {
                        IsSuccess  = false,
                        StatusCode = 400,
                        Message    = "Terminal serial number cannot exceed 100 characters.",
                    };
                }

                var isDuplicateTerminalSerialNo = _db.TblTerminals
                    .AsNoTracking()
                    .Any(x => x.TerminalSerialNo == terminalSerialNo
                        && x.TerminalId != id
                        && x.DeleteFlag == false);

                if (isDuplicateTerminalSerialNo)
                {
                    return new Result<TerminalModel>
                    {
                        IsSuccess  = false,
                        StatusCode = 409,
                        Message    = "Terminal serial number already exists.",
                    };
                }

                item.TerminalSerialNo = terminalSerialNo;
            }

            if (request.BusId is not null)
            {
                if (request.BusId <= 0)
                {
                    return new Result<TerminalModel>
                    {
                        IsSuccess  = false,
                        StatusCode = 400,
                        Message    = "BusId must be greater than 0."
                    };
                }

                var bus = _db.TblBus
                    .AsNoTracking()
                    .FirstOrDefault(x => x.BusId == request.BusId && x.DeleteFlag == false);

                if (bus is null)
                {
                    return new Result<TerminalModel>
                    {
                        IsSuccess  = false,
                        StatusCode = 404,
                        Message    = "Bus not found.",
                    };
                }

                item.BusId = request.BusId.Value;
                item.Bus   = bus;
            }

            if (request.IsActive is not null)
            {
                item.IsActive = request.IsActive.Value;
            }
            _db.SaveChanges();

            return new Result<TerminalModel>
            {
                IsSuccess  = true,
                StatusCode = 200,
                Message    = "Terminal updated successfully.",
                Data       = new TerminalModel
                {
                    TerminalId       = item.TerminalId,
                    TerminalSerialNo = item.TerminalSerialNo,
                    BusId            = item.BusId,
                    BusNo            = item.Bus.BusNo,
                    BusLicense       = item.Bus.BusLicense,
                    IsActive         = item.IsActive
                }
            };
        }
        catch (DbUpdateException)
        {
            return new Result<TerminalModel>
            {
                IsSuccess  = false,
                StatusCode = 409,
                Message    = "Terminal serial number already exists.",
            };
        }
        catch (Exception)
        {
            return new Result<TerminalModel>
            {
                IsSuccess  = false,
                StatusCode = 500,
                Message    = "An unexpected error occurred."
            };
        }
    }

    public Result<TerminalModel> Delete(int id)
    {
        try
        {
            if (id <= 0)
            {
                return new Result<TerminalModel>
                {
                    IsSuccess  = false,
                    StatusCode = 400,
                    Message    = "TerminalId is required.",
                };
            }

            var item = _db.TblTerminals
                .Include(x => x.Bus)
                .FirstOrDefault(x => x.TerminalId == id && x.DeleteFlag == false);

            if (item is null)
            {
                return new Result<TerminalModel>
                {
                    IsSuccess  = false,
                    StatusCode = 404,
                    Message    = "Terminal not found.",
                };
            }

            item.DeleteFlag = true;
            _db.SaveChanges();

            return new Result<TerminalModel>
            {
                IsSuccess  = true,
                StatusCode = 200,
                Message    = "Terminal deleted successfully.",
                Data       = new TerminalModel
                {
                    TerminalId       = item.TerminalId,
                    TerminalSerialNo = item.TerminalSerialNo,
                    BusId            = item.BusId,
                    BusNo            = item.Bus.BusNo,
                    BusLicense       = item.Bus.BusLicense,
                    IsActive         = item.IsActive
                }
            };
        }
        catch (Exception)
        {
            return new Result<TerminalModel>
            {
                IsSuccess  = false,
                StatusCode = 500,
                Message    = "An unexpected error occurred."
            };
        }
    }
}
