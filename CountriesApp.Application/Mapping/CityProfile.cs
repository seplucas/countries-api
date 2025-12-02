using AutoMapper;
using CountriesApp.Domain.Entities;
using CountriesApp.Application.DTOs.Cities;
using CountriesApp.Domain.Shared;

namespace CountriesApp.Application.Mapping;

public class CityProfile : Profile
{
    public CityProfile()
    {
        CreateMap<City, CityResponseDto>();

        CreateMap<PaginationResult<City>, PaginationResult<CityResponseDto>>()
            .ConvertUsing((src, dest, context) =>
            {
                var mapper = context.Mapper;
                return new PaginationResult<CityResponseDto>
                {
                    Items = mapper.Map<List<CityResponseDto>>(src.Items),
                    TotalCount = src.TotalCount,
                    Page = src.Page,
                    PageSize = src.PageSize
                };
            });
    }
}