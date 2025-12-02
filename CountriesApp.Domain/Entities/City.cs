using CountriesApp.Domain.Shared;

namespace CountriesApp.Domain.Entities;

public class City
{
    public City() { }

    private City(string name, Guid countryId)
    {
        Id = Guid.NewGuid();
        Name = name.Trim();
        CountryId = countryId;
    }

    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public Guid CountryId { get; private set; }
    public Country? Country { get; private set; }

    public static Result<City> Create(string name, Guid countryId)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result<City>.Failure(Error.Validation("City name is required."));
        }

        if (name.Length > 100)
        {
            return Result<City>.Failure(Error.Validation("City name must not exceed 100 characters."));
        }

        if (countryId == Guid.Empty)
        {
            return Result<City>.Failure(Error.Validation("CountryId is required."));
        }

        var city = new City(name, countryId);
        return Result<City>.Success(city);
    }

    public Result Update(string name, Guid countryId)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure(Error.Validation("City name is required."));
        }

        if (name.Length > 100)
        {
            return Result.Failure(Error.Validation("City name must not exceed 100 characters."));
        }

        if (countryId == Guid.Empty)
        {
            return Result.Failure(Error.Validation("CountryId is required."));
        }

        Name = name.Trim();
        CountryId = countryId;

        return Result.Success();
    }
}