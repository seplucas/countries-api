namespace CountriesApp.Application.DTOs.Cities;

public record CityResponseDto
{
    public Guid Id { get; init; }
    public required string Name { get; init; }
    public Guid CountryId { get; init; }
}