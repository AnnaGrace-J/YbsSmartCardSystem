using Microsoft.EntityFrameworkCore;
using YbsSmartCardSystem.Contracts.Features.Package;
using YbsSmartCardSystem.Database.AppDbContextModels;
using YbsSmartCardSystem.Domain.Common;

namespace YbsSmartCardSystem.Domain.Features.Package;

public class PackageService
{
    private readonly AppDbContext _db;

    public PackageService(AppDbContext db)
    {
        _db = db;
    }

    public Result<PackageListResponseModel> GetList(PackageListRequestModel? request)
    {
        try
        {
            request ??= new PackageListRequestModel();
            
            if (request.PageNo <= 0) 
                return new Result<PackageListResponseModel> { IsSuccess = false, StatusCode = 400, Message = "PageNo must be greater than 0." };
            if (request.PageSize <= 0 || request.PageSize > 100) 
                return new Result<PackageListResponseModel> { IsSuccess = false, StatusCode = 400, Message = "PageSize must be between 1 and 100." };

            var query = _db.TblPackages.AsNoTracking().Where(x => !x.DeleteFlag);

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                query = query.Where(x => x.PackageCode.Contains(request.Search) || x.PackageName.Contains(request.Search));
            }

            if (request.IsActive.HasValue)
            {
                query = query.Where(x => x.IsActive == request.IsActive.Value);
            }

            var totalCount = query.Count();
            var items = query
                .OrderByDescending(x => x.PackageId)
                .Skip((request.PageNo - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(x => new PackageModel
                {
                    PackageId = x.PackageId,
                    PackageCode = x.PackageCode,
                    PackageName = x.PackageName,
                    Price = x.Price,
                    RideLimit = x.RideLimit,
                    ValidDays = x.ValidDays,
                    Description = x.Description,
                    IsActive = x.IsActive
                })
                .ToList();

            var response = new PackageListResponseModel
            {
                TotalCount = totalCount,
                Packages = items
            };

            return new Result<PackageListResponseModel> { IsSuccess = true, Data = response, Message = "Packages retrieved successfully.", StatusCode = 200 };
        }
        catch (Exception)
        {
            return new Result<PackageListResponseModel> { IsSuccess = false, StatusCode = 500, Message = "An unexpected error occurred." };
        }
    }

    public Result<PackageModel> GetById(int id)
    {
        try
        {
            var item = _db.TblPackages.AsNoTracking().FirstOrDefault(x => x.PackageId == id && !x.DeleteFlag);
            if (item == null) 
                return new Result<PackageModel> { IsSuccess = false, StatusCode = 404, Message = "Package not found." };

            var model = new PackageModel
            {
                PackageId = item.PackageId,
                PackageCode = item.PackageCode,
                PackageName = item.PackageName,
                Price = item.Price,
                RideLimit = item.RideLimit,
                ValidDays = item.ValidDays,
                Description = item.Description,
                IsActive = item.IsActive
            };

            return new Result<PackageModel> { IsSuccess = true, Data = model, Message = "Package retrieved successfully.", StatusCode = 200 };
        }
        catch (Exception)
        {
            return new Result<PackageModel> { IsSuccess = false, StatusCode = 500, Message = "An unexpected error occurred." };
        }
    }

    public Result<PackageCreateResponseModel> Create(PackageCreateRequestModel request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.PackageCode) || request.PackageCode.Length > 50) 
                return new Result<PackageCreateResponseModel> { IsSuccess = false, StatusCode = 400, Message = "PackageCode is required and max 50 characters." };
            if (string.IsNullOrWhiteSpace(request.PackageName) || request.PackageName.Length > 100) 
                return new Result<PackageCreateResponseModel> { IsSuccess = false, StatusCode = 400, Message = "PackageName is required and max 100 characters." };
            if (request.Price <= 0) 
                return new Result<PackageCreateResponseModel> { IsSuccess = false, StatusCode = 400, Message = "Price must be greater than 0." };
            if (request.RideLimit.HasValue && request.RideLimit.Value <= 0) 
                return new Result<PackageCreateResponseModel> { IsSuccess = false, StatusCode = 400, Message = "RideLimit must be null or greater than 0." };
            if (request.ValidDays.HasValue && request.ValidDays.Value <= 0) 
                return new Result<PackageCreateResponseModel> { IsSuccess = false, StatusCode = 400, Message = "ValidDays must be null or greater than 0." };
            if (!string.IsNullOrWhiteSpace(request.Description) && request.Description.Length > 250) 
                return new Result<PackageCreateResponseModel> { IsSuccess = false, StatusCode = 400, Message = "Description max 250 characters." };

            if (request.IsActive && _db.TblPackages.Any(x => x.PackageCode == request.PackageCode && x.IsActive && !x.DeleteFlag))
            {
                return new Result<PackageCreateResponseModel> { IsSuccess = false, StatusCode = 409, Message = "An active Package with this code already exists." };
            }

            var entity = new TblPackage
            {
                PackageCode = request.PackageCode,
                PackageName = request.PackageName,
                Price = request.Price,
                RideLimit = request.RideLimit,
                ValidDays = request.ValidDays,
                Description = request.Description,
                IsActive = request.IsActive,
                CreatedDate = DateTime.Now,
                DeleteFlag = false
            };

            _db.TblPackages.Add(entity);
            _db.SaveChanges();

            var response = new PackageCreateResponseModel
            {
                PackageId = entity.PackageId,
                PackageCode = entity.PackageCode,
                PackageName = entity.PackageName,
                Price = entity.Price,
                RideLimit = entity.RideLimit,
                ValidDays = entity.ValidDays,
                Description = entity.Description,
                IsActive = entity.IsActive
            };

            return new Result<PackageCreateResponseModel> { IsSuccess = true, Data = response, Message = "Package created successfully.", StatusCode = 200 };
        }
        catch (Exception)
        {
            return new Result<PackageCreateResponseModel> { IsSuccess = false, StatusCode = 500, Message = "An unexpected error occurred." };
        }
    }

    public Result<PackageModel> Patch(int id, PackagePatchRequestModel request)
    {
        try
        {
            if (request == null) 
                return new Result<PackageModel> { IsSuccess = false, StatusCode = 400, Message = "Request is required." };
            if (id <= 0) 
                return new Result<PackageModel> { IsSuccess = false, StatusCode = 400, Message = "Invalid package ID." };

            var item = _db.TblPackages.FirstOrDefault(x => x.PackageId == id && !x.DeleteFlag);
            if (item == null) 
                return new Result<PackageModel> { IsSuccess = false, StatusCode = 404, Message = "Package not found." };

            bool hasChanges = false;

            if (request.PackageCode != null)
            {
                if (string.IsNullOrWhiteSpace(request.PackageCode) || request.PackageCode.Length > 50) 
                    return new Result<PackageModel> { IsSuccess = false, StatusCode = 400, Message = "PackageCode is required and max 50 characters." };
                item.PackageCode = request.PackageCode;
                hasChanges = true;
            }

            if (request.PackageName != null)
            {
                if (string.IsNullOrWhiteSpace(request.PackageName) || request.PackageName.Length > 100) 
                    return new Result<PackageModel> { IsSuccess = false, StatusCode = 400, Message = "PackageName is required and max 100 characters." };
                item.PackageName = request.PackageName;
                hasChanges = true;
            }

            if (request.Price.HasValue)
            {
                if (request.Price.Value <= 0) 
                    return new Result<PackageModel> { IsSuccess = false, StatusCode = 400, Message = "Price must be greater than 0." };
                item.Price = request.Price.Value;
                hasChanges = true;
            }

            if (request.RideLimit != null)
            {
                if (request.RideLimit.HasValue && request.RideLimit.Value <= 0) 
                    return new Result<PackageModel> { IsSuccess = false, StatusCode = 400, Message = "RideLimit must be null or greater than 0." };
                item.RideLimit = request.RideLimit;
                hasChanges = true;
            }

            if (request.ValidDays != null)
            {
                if (request.ValidDays.HasValue && request.ValidDays.Value <= 0) 
                    return new Result<PackageModel> { IsSuccess = false, StatusCode = 400, Message = "ValidDays must be null or greater than 0." };
                item.ValidDays = request.ValidDays;
                hasChanges = true;
            }

            if (request.Description != null)
            {
                if (!string.IsNullOrWhiteSpace(request.Description) && request.Description.Length > 250) 
                    return new Result<PackageModel> { IsSuccess = false, StatusCode = 400, Message = "Description max 250 characters." };
                item.Description = request.Description;
                hasChanges = true;
            }

            if (request.IsActive.HasValue)
            {
                item.IsActive = request.IsActive.Value;
                hasChanges = true;
            }

            if (!hasChanges) 
                return new Result<PackageModel> { IsSuccess = false, StatusCode = 400, Message = "At least one field must be supplied for patch." };

            if (item.IsActive && _db.TblPackages.Any(x => x.PackageCode == item.PackageCode && x.IsActive && !x.DeleteFlag && x.PackageId != id))
            {
                return new Result<PackageModel> { IsSuccess = false, StatusCode = 409, Message = "Another active Package with this code already exists." };
            }

            item.UpdatedDate = DateTime.Now;
            _db.SaveChanges();

            var model = new PackageModel
            {
                PackageId = item.PackageId,
                PackageCode = item.PackageCode,
                PackageName = item.PackageName,
                Price = item.Price,
                RideLimit = item.RideLimit,
                ValidDays = item.ValidDays,
                Description = item.Description,
                IsActive = item.IsActive
            };

            return new Result<PackageModel> { IsSuccess = true, Data = model, Message = "Package updated successfully.", StatusCode = 200 };
        }
        catch (Exception)
        {
            return new Result<PackageModel> { IsSuccess = false, StatusCode = 500, Message = "An unexpected error occurred." };
        }
    }

    public Result<PackageModel> Delete(int id)
    {
        try
        {
            if (id <= 0) 
                return new Result<PackageModel> { IsSuccess = false, StatusCode = 400, Message = "Invalid package ID." };
            
            var item = _db.TblPackages.FirstOrDefault(x => x.PackageId == id && !x.DeleteFlag);
            if (item == null) 
                return new Result<PackageModel> { IsSuccess = false, StatusCode = 404, Message = "Package not found." };

            item.DeleteFlag = true;
            item.UpdatedDate = DateTime.Now;
            _db.SaveChanges();

            return new Result<PackageModel> { IsSuccess = true, Data = new PackageModel(), Message = "Package deleted successfully.", StatusCode = 200 };
        }
        catch (Exception)
        {
            return new Result<PackageModel> { IsSuccess = false, StatusCode = 500, Message = "An unexpected error occurred." };
        }
    }
}
