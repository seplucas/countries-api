using Microsoft.EntityFrameworkCore;
using CountriesApp.Domain.Shared;
using CountriesApp.Domain.Entities;
using CountriesApp.Domain.Interfaces;
using CountriesApp.Infrastructure.Data;
using System.Linq.Expressions;

namespace CountriesApp.Infrastructure.Repositories;

public class CityRepository(CountriesAppDbContext context) : ICityRepository
{
    private readonly CountriesAppDbContext _context = context;

    public async Task<List<City>> GetAllAsync(Expression<Func<City, bool>> predicate)
    {
        return await _context.Cities.Where(predicate).ToListAsync();
    }

    public async Task<PaginationResult<City>> GetPagedAsync(Expression<Func<City, bool>> predicate, int page = 1, int pageSize = 10)
    {
        page = Math.Max(1, page);
        pageSize = Math.Max(1, Math.Min(100, pageSize));

        var query = _context.Cities.Where(predicate);
        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PaginationResult<City>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<Result<City?>> GetByIdAsync(Guid id)
    {
        try
        {
            var city = await _context.Cities.FindAsync(id);
            if (city == null)
            {
                return Result<City?>.Failure(Error.NotFound($"City with ID {id} not found."));
            }
            return Result<City?>.Success(city);
        }
        catch (Exception ex)
        {
            return Result<City?>.Failure(Error.Unexpected(ex.Message));
        }
    }

    public async Task<Result<City>> AddAsync(City city)
    {
        try
        {
            _context.Cities.Add(city);
            await _context.SaveChangesAsync();
            return Result<City>.Success(city);
        }
        catch (Exception ex)
        {
            return Result<City>.Failure(Error.Unexpected(ex.Message));
        }
    }

    public async Task<Result<City>> UpdateAsync(City city)
    {
        try
        {
            _context.Entry(city).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return Result<City>.Success(city);
        }
        catch (Exception ex)
        {
            return Result<City>.Failure(Error.Unexpected(ex.Message));
        }
    }

    public async Task<Result> DeleteAsync(Guid id)
    {
        try
        {
            var city = await _context.Cities.FindAsync(id);
            if (city == null)
            {
                return Result.Failure(Error.NotFound($"City with ID {id} not found."));
            }

            _context.Cities.Remove(city);
            await _context.SaveChangesAsync();
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(Error.Unexpected(ex.Message));
        }
    }
}