using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CountriesApp.Application.DTOs.Cities;

public record CityCreateDto
{
    [Required]
    [StringLength(100)]
    public required string Name { get; init; }

    [Required]
    public required Guid CountryId { get; init; }
}