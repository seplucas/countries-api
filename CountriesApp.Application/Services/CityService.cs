using AutoMapper;
using CountriesApp.Application.Interfaces;
using CountriesApp.Domain.Shared;
using CountriesApp.Domain.Entities;
using CountriesApp.Domain.Interfaces;
using CountriesApp.Application.DTOs.Cities;
using System.Linq.Expressions;

namespace CountriesApp.Application.Services;

public class CityService(ICityRepository cityRepository, ICountryRepository countryRepository, IMapper mapper) : ICityService
{
    public async Task<Result<PaginationResult<CityResponseDto>>> GetCitiesAsync(string? search = null, Guid? countryId = null, int page = 1, int pageSize = 10)
    {
        try
        {
            Expression<Func<City, bool>> predicate;

            if (string.IsNullOrWhiteSpace(search) && !countryId.HasValue)
            {
                predicate = city => true;
            }
            else if (string.IsNullOrWhiteSpace(search))
            {
                predicate = city => city.CountryId == countryId;
            }
            else if (!countryId.HasValue)
            {
                var searchTerm = search.Trim().ToLowerInvariant();
                predicate = city => city.Name.ToLower().Contains(searchTerm);
            }
            else
            {
                var searchTerm = search.Trim().ToLowerInvariant();
                predicate = city => city.Name.ToLower().Contains(searchTerm) && city.CountryId == countryId.Value;
            }

            var pagedResult = await cityRepository.GetPagedAsync(predicate, page, pageSize);
            var responsePaged = mapper.Map<PaginationResult<CityResponseDto>>(pagedResult);

            return Result<PaginationResult<CityResponseDto>>.Success(responsePaged);
        }
        catch (Exception ex)
        {
            return Result<PaginationResult<CityResponseDto>>.Failure(Error.Unexpected(ex.Message));
        }
    }

    public async Task<Result<CityResponseDto?>> GetCityByIdAsync(Guid id)
    {
        var result = await cityRepository.GetByIdAsync(id);
        if (!result.IsSuccess)
        {
            return Result<CityResponseDto?>.Failure(result.Error!);
        }

        var response = mapper.Map<CityResponseDto>(result.Value);
        return Result<CityResponseDto?>.Success(response);
    }

    public async Task<Result<CityResponseDto>> CreateCityAsync(CityCreateDto dto)
    {
        var countryValidation = await ValidateCountryExistsAsync(dto.CountryId);
        if (!countryValidation.IsSuccess)
            return Result<CityResponseDto>.Failure(countryValidation.Error!);
            
        var cityResult = City.Create(dto.Name, dto.CountryId);
        if (!cityResult.IsSuccess)
        {
            return Result<CityResponseDto>.Failure(cityResult.Error!);
        }

        var result = await cityRepository.AddAsync(cityResult.Value!);
        if (!result.IsSuccess)
        {
            return Result<CityResponseDto>.Failure(result.Error!);
        }

        var response = mapper.Map<CityResponseDto>(result.Value);
        return Result<CityResponseDto>.Success(response);
    }

    public async Task<Result<CityResponseDto>> UpdateCityAsync(CityUpdateDto dto)
    {
        var getResult = await cityRepository.GetByIdAsync(dto.Id);
        if (!getResult.IsSuccess)
        {
            return Result<CityResponseDto>.Failure(getResult.Error!);
        }

        var countryValidation = await ValidateCountryExistsAsync(dto.CountryId);
        if (!countryValidation.IsSuccess)
            return Result<CityResponseDto>.Failure(countryValidation.Error!);

        var existing = getResult.Value!;
        var updateValidation = existing.Update(dto.Name, dto.CountryId);
        if (!updateValidation.IsSuccess)
        {
            return Result<CityResponseDto>.Failure(updateValidation.Error!);
        }

        var updateResult = await cityRepository.UpdateAsync(existing);
        if (!updateResult.IsSuccess)
        {
            return Result<CityResponseDto>.Failure(updateResult.Error!);
        }

        var response = mapper.Map<CityResponseDto>(updateResult.Value);
        return Result<CityResponseDto>.Success(response);
    }

    public async Task<Result> DeleteCityAsync(Guid id)
    {
        try
        {
            var result = await cityRepository.DeleteAsync(id);
            return result;
        }
        catch (Exception ex)
        {
            return Result.Failure(Error.Unexpected(ex.Message));
        }
    }

        private async Task<Result> ValidateCountryExistsAsync(Guid countryId)
    {
        var countryExists = await countryRepository.GetByIdAsync(countryId);
        if (!countryExists.IsSuccess)
            return Result.Failure(Error.NotFound("Invalid country id"));
            
        return Result.Success();
    }

    
}