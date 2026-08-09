using YbsSmartCardSystem.Domain.Common;
using Microsoft.EntityFrameworkCore;
using YbsSmartCardSystem.Database.AppDbContextModels;
using YbsSmartCardSystem.Contracts.Features.BusPayment;
using YbsSmartCardSystem.Infrastructure.Services;

namespace YbsSmartCardSystem.Domain.Features.Bus;

public class BusService
{
    private readonly AppDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public BusService(AppDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public Result<BusListResponseModel> GetList(BusListRequestModel request)
    {
        try
        {
            var query = _db.TblBus
                .AsNoTracking()
                .AsQueryable();

            if (request.IsDeleted.HasValue)
            {
                query = query.Where(x => x.DeleteFlag == request.IsDeleted.Value);
            }

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var search = request.Search.Trim();
                query = query.Where(x => x.BusNo.Contains(search) || x.BusLicense.Contains(search));
            }

            var totalCount = query.Count();

            var buses = query
                .OrderByDescending(x => x.BusId)
                .Skip((request.PageNo - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(b => new BusModel
                {
                    BusId      = b.BusId,
                    BusNo      = b.BusNo,
                    BusLicense = b.BusLicense,
                    CreatedByName = b.CreatedUser != null ? b.CreatedUser.UserName : null,
                    CreatedByRole = b.CreatedUser != null ? _db.TblUserRoles.Where(ur => ur.UserId == b.CreatedBy && !ur.DeleteFlag).Select(ur => ur.Role.RoleName).FirstOrDefault() : null,
                    DeleteFlag = b.DeleteFlag
                })
                .ToList();

            return new Result<BusListResponseModel>
            {
                IsSuccess = true,
                Message   = "Buses retrieved successfully.",
                Data      = new BusListResponseModel
                {
                    Buses      = buses,
                    TotalCount = totalCount
                }
            };
        }
        catch (Exception)
        {
            return new Result<BusListResponseModel>
            {
                IsSuccess = false,
                Message = "An unexpected error occurred.",
                StatusCode = 500
                // TODO: Log exception after Infrastructure logging is added.
            };
        }
    }

    public Result<BusModel> GetById(int id)
    {
        try
        {
            if (id <= 0)
            {
                return new Result<BusModel>
                {
                    IsSuccess = false,
                    StatusCode = 400,
                    Message   = "BusId is required.",
                };
            }

            var item = _db.TblBus
                .AsNoTracking()
                .FirstOrDefault(x => x.BusId == id && x.DeleteFlag == false);

            if (item is null)
            {
                return new Result<BusModel>
                {
                    IsSuccess = false,
                    StatusCode = 404,
                    Message   = "Bus not found.",
                };
            }

            return new Result<BusModel>
            {
                IsSuccess = true,
                StatusCode = 200,
                Message   = "Bus retrieved successfully.",
                Data      = new BusModel
                {
                    BusId      = item.BusId,
                    BusNo      = item.BusNo,
                    BusLicense = item.BusLicense
                }
            };
        }
        catch (Exception)
        {
            return new Result<BusModel>
            {
                IsSuccess = false,
                StatusCode = 500,
                Message   = "An unexpected error occurred."
            };
        }
    }

    public Result<BusCreateResponseModel> Create(BusCreateRequestModel request)
    {
        try
        {
            if (request is null)
            {
                return new Result<BusCreateResponseModel>
                {
                    IsSuccess = false,
                    StatusCode = 400,
                    Message   = "Request data is required.",
                };
            }

            if (string.IsNullOrWhiteSpace(request.BusNo))
            {
                return new Result<BusCreateResponseModel>
                {
                    IsSuccess = false,
                    StatusCode = 400,
                    Message   = "Bus number is required.",
                };
            }

            if (string.IsNullOrWhiteSpace(request.BusLicense))
            {
                return new Result<BusCreateResponseModel>
                {
                    IsSuccess = false,
                    StatusCode = 400,
                    Message   = "Bus license is required.",
                };
            }

            var busNo = request.BusNo.Trim();
            var busLicense = request.BusLicense.Trim();

            if (busNo.Length > 50)
            {
                return new Result<BusCreateResponseModel>
                {
                    IsSuccess = false,
                    StatusCode = 400,
                    Message   = "Bus number cannot exceed 50 characters.",
                };
            }

            if (busLicense.Length > 50)
            {
                return new Result<BusCreateResponseModel>
                {
                    IsSuccess = false,
                    StatusCode = 400,
                    Message   = "Bus license cannot exceed 50 characters.",
                };
            }

            var isDuplicateBusLicense = _db.TblBus
                .AsNoTracking()
                .Any(x => x.BusLicense == busLicense && x.DeleteFlag == false);

            if (isDuplicateBusLicense)
            {
                return new Result<BusCreateResponseModel>
                {
                    IsSuccess = false,
                    StatusCode = 409,
                    Message   = "Bus license already exists.",
                };
            }

            var bus = new TblBu
            {
                BusNo       = busNo,
                BusLicense  = busLicense,
                CreatedDate = DateTime.Now,
                DeleteFlag  = false,
                CreatedBy   = _currentUser.UserId
            };

            _db.TblBus.Add(bus);
            _db.SaveChanges();

            return new Result<BusCreateResponseModel>
            {
                IsSuccess = true,
                StatusCode = 200,
                Message   = "Bus created successfully.",
                Data      = new BusCreateResponseModel
                {
                    BusId      = bus.BusId,
                    BusNo      = bus.BusNo,
                    BusLicense = bus.BusLicense
                }
            };
        }
        catch (DbUpdateException)
        {
            return new Result<BusCreateResponseModel>
            {
                IsSuccess = false,
                StatusCode = 409,
                Message   = "Bus license already exists.",
            };
        }
        catch (Exception)
        {
            return new Result<BusCreateResponseModel>
            {
                IsSuccess = false,
                StatusCode = 500,
                Message   = "An unexpected error occurred."
            };
        }
    }

    public Result<BusModel> Patch(int id, BusPatchRequestModel request)
    {
        try
        {
            if (id <= 0)
            {
                return new Result<BusModel>
                {
                    IsSuccess = false,
                    StatusCode = 400,
                    Message   = "BusId is required.",
                };
            }

            if (request is null)
            {
                return new Result<BusModel>
                {
                    IsSuccess = false,
                    StatusCode = 400,
                    Message   = "Request data is required.",
                };
            }

            if (request.BusNo is null && request.BusLicense is null)
            {
                return new Result<BusModel>
                {
                    IsSuccess = false,
                    StatusCode = 400,
                    Message   = "At least one field is required to update.",
                };
            }

            var item = _db.TblBus
                .FirstOrDefault(x => x.BusId == id && x.DeleteFlag == false);

            if (item is null)
            {
                return new Result<BusModel>
                {
                    IsSuccess = false,
                    StatusCode = 404,
                    Message   = "Bus not found.",
                };
            }

            if (request.BusNo is not null)
            {
                if (string.IsNullOrWhiteSpace(request.BusNo))
                {
                    return new Result<BusModel>
                    {
                        IsSuccess = false,
                        StatusCode = 400,
                        Message   = "Bus number cannot be empty."
                    };
                }

                var busNo = request.BusNo.Trim();

                if (busNo.Length > 50)
                {
                    return new Result<BusModel>
                    {
                        IsSuccess = false,
                        StatusCode = 400,
                        Message   = "Bus number cannot exceed 50 characters.",
                    };
                }

                item.BusNo = busNo;
            }

            if (request.BusLicense is not null)
            {
                if (string.IsNullOrWhiteSpace(request.BusLicense))
                {
                    return new Result<BusModel>
                    {
                        IsSuccess = false,
                        StatusCode = 400,
                        Message   = "Bus license cannot be empty."
                    };
                }

                var busLicense = request.BusLicense.Trim();

                if (busLicense.Length > 50)
                {
                    return new Result<BusModel>
                    {
                        IsSuccess = false,
                        StatusCode = 400,
                        Message   = "Bus license cannot exceed 50 characters.",
                    };
                }

                var isDuplicateBusLicense = _db.TblBus
                    .AsNoTracking()
                    .Any(x => x.BusLicense == busLicense && x.BusId != id && x.DeleteFlag == false);

                if (isDuplicateBusLicense)
                {
                    return new Result<BusModel>
                    {
                        IsSuccess = false,
                        StatusCode = 409,
                        Message   = "Bus license already exists.",
                    };
                }

                item.BusLicense = busLicense;
            }
            _db.SaveChanges();

            return new Result<BusModel>
            {
                IsSuccess = true,
                StatusCode = 200,
                Message   = "Bus updated successfully.",
                Data      = new BusModel
                {
                    BusId      = item.BusId,
                    BusNo      = item.BusNo,
                    BusLicense = item.BusLicense
                }
            };
        }
        catch (DbUpdateException)
        {
            return new Result<BusModel>
            {
                IsSuccess = false,
                StatusCode = 409,
                Message   = "Bus license already exists.",
            };
        }
        catch (Exception)
        {
            return new Result<BusModel>
            {
                IsSuccess = false,
                StatusCode = 500,
                Message   = "An unexpected error occurred."
            };
        }
    }

    public Result<BusModel> Delete(int id)
    {
        try
        {
            if (id <= 0)
            {
                return new Result<BusModel>
                {
                    IsSuccess = false,
                    StatusCode = 400,
                    Message   = "BusId is required.",
                };
            }

            var item = _db.TblBus
                .FirstOrDefault(x => x.BusId == id && x.DeleteFlag == false);

            if (item is null)
            {
                return new Result<BusModel>
                {
                    IsSuccess = false,
                    StatusCode = 404,
                    Message   = "Bus not found.",
                };
            }

            item.DeleteFlag = true;
            _db.SaveChanges();

            return new Result<BusModel>
            {
                IsSuccess = true,
                StatusCode = 200,
                Message   = "Bus deleted successfully.",
                Data      = new BusModel
                {
                    BusId      = item.BusId,
                    BusNo      = item.BusNo,
                    BusLicense = item.BusLicense
                }
            };
        }
        catch (Exception)
        {
            return new Result<BusModel>
            {
                IsSuccess = false,
                StatusCode = 500,
                Message   = "An unexpected error occurred."
            };
        }
    }
}
