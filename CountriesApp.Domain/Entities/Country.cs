using CountriesApp.Domain.Shared;

namespace CountriesApp.Domain.Entities;

public class Country
{
    public Country() { }

    private Country(string name, string? code)
    {
        Id = Guid.NewGuid();
        Name = name.Trim();
        Code = string.IsNullOrWhiteSpace(code) ? null : code.Trim().ToUpperInvariant();
    }

    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Code { get; private set; }
    public List<City> Cities { get; private set; } = new();

    public static Result<Country> Create(string name, string? code = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result<Country>.Failure(Error.Validation("Country name is required."));
        }

        if (name.Length > 100)
        {
            return Result<Country>.Failure(Error.Validation("Country name must not exceed 100 characters."));
        }

        if (!string.IsNullOrWhiteSpace(code) && code.Length > 10)
        {
            return Result<Country>.Failure(Error.Validation("Country code must not exceed 10 characters."));
        }

        var country = new Country(name, code);
        return Result<Country>.Success(country);
    }

    public Result Update(string name, string? code = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure(Error.Validation("Country name is required."));
        }

        if (name.Length > 100)
        {
            return Result.Failure(Error.Validation("Country name must not exceed 100 characters."));
        }

        if (!string.IsNullOrWhiteSpace(code) && code.Length > 10)
        {
            return Result.Failure(Error.Validation("Country code must not exceed 10 characters."));
        }

        Name = name.Trim();
        Code = string.IsNullOrWhiteSpace(code) ? null : code.Trim().ToUpperInvariant();

        return Result.Success();
    }
}