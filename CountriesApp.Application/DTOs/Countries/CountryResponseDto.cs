using CountriesApp.Application.DTOs.Cities;  

namespace CountriesApp.Application.DTOs.Countries;

public record CountryResponseDto
{
    public Guid Id { get; init; }
    public required string Name { get; init; }
    public string? Code { get; init; }
    public List<CityResponseDto> Cities { get; init; } = new();
}