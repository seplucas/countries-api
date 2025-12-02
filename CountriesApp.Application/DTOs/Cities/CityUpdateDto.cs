using System.ComponentModel.DataAnnotations;

namespace CountriesApp.Application.DTOs.Cities;

public record CityUpdateDto
{
    [Required]
    public Guid Id { get; set; }
    
    [Required]
    [StringLength(100)]
    public required string Name { get; init; }

    [Required]
    public required Guid CountryId { get; init; }
}