using System.ComponentModel.DataAnnotations;

namespace CountriesApp.Application.DTOs.Countries;

public record CountryCreateDto
{
    [Required]
    [StringLength(100)]
    public required string Name { get; init; }

    [StringLength(3)]
    public string? Code { get; init; }
}