using AutoMapper;
using CountriesApp.Domain.Entities;
using CountriesApp.Application.DTOs.Countries;
using CountriesApp.Domain.Shared;

namespace CountriesApp.Application.Mapping;

public class CountryProfile : Profile
{
    public CountryProfile()
    {
        CreateMap<Country, CountryResponseDto>();

        CreateMap<PaginationResult<Country>, PaginationResult<CountryResponseDto>>()
            .ConvertUsing((src, _, context) => new PaginationResult<CountryResponseDto>
            {
                Items = context.Mapper.Map<List<CountryResponseDto>>(src.Items),
                TotalCount = src.TotalCount,
                Page = src.Page,
                PageSize = src.PageSize
            });
    }
}