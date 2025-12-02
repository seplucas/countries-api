using System.Linq.Expressions;
using CountriesApp.Domain.Entities;
using CountriesApp.Domain.Shared;

namespace CountriesApp.Domain.Interfaces;

public interface ICountryRepository
{
    Task<PaginationResult<Country>> GetPagedAsync(Expression<Func<Country, bool>> predicate, int page = 1, int pageSize = 10);
    Task<Result<Country?>> GetByIdAsync(Guid id);
    Task<Result<Country>> AddAsync(Country country);
    Task<Result<Country>> UpdateAsync(Country country);
    Task<Result> DeleteAsync(Guid id);
}