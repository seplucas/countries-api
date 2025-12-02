using System.Linq.Expressions;
using CountriesApp.Domain.Entities;
using CountriesApp.Domain.Shared;

namespace CountriesApp.Domain.Interfaces;

public interface ICityRepository
{
    Task<PaginationResult<City>> GetPagedAsync(Expression<Func<City, bool>> predicate, int page = 1, int pageSize = 10);
    Task<Result<City?>> GetByIdAsync(Guid id);
    Task<Result<City>> AddAsync(City city);
    Task<Result<City>> UpdateAsync(City city);
    Task<Result> DeleteAsync(Guid id);
}