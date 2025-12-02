using CountriesApp.Application.DTOs.Cities;
using CountriesApp.Domain.Shared;

namespace CountriesApp.Application.Interfaces;

public interface ICityService
{
    Task<Result<PaginationResult<CityResponseDto>>> GetCitiesAsync(string? search = null, Guid? countryId = null, int page = 1, int pageSize = 10);
    Task<Result<CityResponseDto?>> GetCityByIdAsync(Guid id);
    Task<Result<CityResponseDto>> CreateCityAsync(CityCreateDto dto);
    Task<Result<CityResponseDto>> UpdateCityAsync(CityUpdateDto dto);
    Task<Result> DeleteCityAsync(Guid id);
}