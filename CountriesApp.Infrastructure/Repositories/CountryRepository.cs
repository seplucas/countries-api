using Microsoft.EntityFrameworkCore;
using CountriesApp.Domain.Shared;
using CountriesApp.Domain.Entities;
using CountriesApp.Domain.Interfaces;
using CountriesApp.Infrastructure.Data;
using System.Linq.Expressions;

namespace CountriesApp.Infrastructure.Repositories;

public class CountryRepository(CountriesAppDbContext context) : ICountryRepository
{
    private readonly CountriesAppDbContext _context = context;

    public async Task<List<Country>> GetAllAsync(Expression<Func<Country, bool>> predicate)
    {
        return await _context.Countries.Where(predicate).ToListAsync();
    }

    public async Task<PaginationResult<Country>> GetPagedAsync(Expression<Func<Country, bool>> predicate, int page = 1, int pageSize = 10)
    {
        page = Math.Max(1, page);
        pageSize = Math.Max(1, Math.Min(100, pageSize)); 

        var query = _context.Countries.Where(predicate);
        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PaginationResult<Country>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<Result<Country?>> GetByIdAsync(Guid id)
    {
        try
        {
            var country = await _context.Countries.FindAsync(id);
            if (country == null)
            {
                return Result<Country?>.Failure(Error.NotFound($"Country with ID {id} not found."));
            }
            return Result<Country?>.Success(country);
        }
        catch (Exception ex)
        {
            return Result<Country?>.Failure(Error.Unexpected(ex.Message));
        }
    }

    public async Task<Result<Country>> AddAsync(Country country)
    {
        try
        {
            _context.Countries.Add(country);
            await _context.SaveChangesAsync();
            return Result<Country>.Success(country);
        }
        catch (Exception ex)
        {
            return Result<Country>.Failure(Error.Unexpected(ex.Message));
        }
    }

    public async Task<Result<Country>> UpdateAsync(Country country)
    {
        try
        {
            _context.Entry(country).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return Result<Country>.Success(country);
        }
        catch (Exception ex)
        {
            return Result<Country>.Failure(Error.Unexpected(ex.Message));
        }
    }

    public async Task<Result> DeleteAsync(Guid id)
    {
        try
        {
            var country = await _context.Countries.FindAsync(id);
            if (country == null)
            {
                return Result.Failure(Error.NotFound($"Country with ID {id} not found."));
            }

            _context.Countries.Remove(country);
            await _context.SaveChangesAsync();
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(Error.Unexpected(ex.Message));
        }
    }
}