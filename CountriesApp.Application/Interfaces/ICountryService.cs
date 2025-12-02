using CountriesApp.Application.DTOs.Countries;
using CountriesApp.Domain.Shared;

namespace CountriesApp.Application.Interfaces;

public interface ICountryService
{
    Task<Result<PaginationResult<CountryResponseDto>>> GetCountriesAsync(string? search = null, int page = 1, int pageSize = 10);
    Task<Result<CountryResponseDto?>> GetCountryByIdAsync(Guid id);
    Task<Result<CountryResponseDto>> CreateCountryAsync(CountryCreateDto dto);
    Task<Result<CountryResponseDto>> UpdateCountryAsync(CountryUpdateDto dto);
    Task<Result> DeleteCountryAsync(Guid id);
}