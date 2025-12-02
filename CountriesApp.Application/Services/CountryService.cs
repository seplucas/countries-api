using AutoMapper;
using CountriesApp.Application.Interfaces;
using CountriesApp.Domain.Shared;
using CountriesApp.Domain.Entities;
using CountriesApp.Domain.Interfaces;
using CountriesApp.Application.DTOs.Countries;
using System.Linq.Expressions;

namespace CountriesApp.Application.Services;

public class CountryService(ICountryRepository countryRepository, IMapper mapper) : ICountryService
{
    public async Task<Result<PaginationResult<CountryResponseDto>>> GetCountriesAsync(string? search = null, int page = 1, int pageSize = 10)
    {
        try
        {
            Expression<Func<Country, bool>> predicate = country => true;

            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchTerm = search.Trim().ToLowerInvariant();
                predicate = country => country.Name.ToLower().Contains(searchTerm) ||
                                      (!string.IsNullOrEmpty(country.Code) && country.Code.ToLower().Contains(searchTerm));
            }

            var pagedResult = await countryRepository.GetPagedAsync(predicate, page, pageSize);
            var responsePaged = mapper.Map<PaginationResult<CountryResponseDto>>(pagedResult);

            return Result<PaginationResult<CountryResponseDto>>.Success(responsePaged);
        }
        catch (Exception ex)
        {
            return Result<PaginationResult<CountryResponseDto>>.Failure(Error.Unexpected(ex.Message));
        }
    }

    public async Task<Result<CountryResponseDto?>> GetCountryByIdAsync(Guid id)
    {
        var result = await countryRepository.GetByIdAsync(id);
        if (!result.IsSuccess)
        {
            return Result<CountryResponseDto?>.Failure(result.Error!);
        }

        var response = mapper.Map<CountryResponseDto>(result.Value);
        return Result<CountryResponseDto?>.Success(response);
    }

    public async Task<Result<CountryResponseDto>> CreateCountryAsync(CountryCreateDto dto)
    {
        var countryResult = Country.Create(dto.Name, dto.Code);
        if (!countryResult.IsSuccess)
        {
            return Result<CountryResponseDto>.Failure(countryResult.Error!);
        }

        var result = await countryRepository.AddAsync(countryResult.Value!);
        if (!result.IsSuccess)
        {
            return Result<CountryResponseDto>.Failure(result.Error!);
        }

        var response = mapper.Map<CountryResponseDto>(result.Value);
        return Result<CountryResponseDto>.Success(response);
    }

    public async Task<Result<CountryResponseDto>> UpdateCountryAsync(CountryUpdateDto dto)
    {
        var getResult = await countryRepository.GetByIdAsync(dto.Id);
        if (!getResult.IsSuccess)
        {
            return Result<CountryResponseDto>.Failure(getResult.Error!);
        }

        var existing = getResult.Value!;
        var updateValidation = existing.Update(dto.Name, dto.Code);
        if (!updateValidation.IsSuccess)
        {
            return Result<CountryResponseDto>.Failure(updateValidation.Error!);
        }

        var updateResult = await countryRepository.UpdateAsync(existing);
        if (!updateResult.IsSuccess)
        {
            return Result<CountryResponseDto>.Failure(updateResult.Error!);
        }

        var response = mapper.Map<CountryResponseDto>(updateResult.Value);
        return Result<CountryResponseDto>.Success(response);
    }

    public async Task<Result> DeleteCountryAsync(Guid id)
    {
        try
        {
            var result = await countryRepository.DeleteAsync(id);
            return result;
        }
        catch (Exception ex)
        {
            return Result.Failure(Error.Unexpected(ex.Message));
        }
    }
}