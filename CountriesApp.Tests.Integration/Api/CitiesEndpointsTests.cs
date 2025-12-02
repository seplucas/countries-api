using System.Net;
using System.Net.Http.Json;
using CountriesApp.Application.DTOs.Cities;
using CountriesApp.Application.DTOs.Countries;
using CountriesApp.Tests.Integration.Infrastructure;
using FluentAssertions;

namespace CountriesApp.Tests.Integration.Api;

public class CitiesEndpointsTests : IClassFixture<IntegrationTestWebAppFactory>
{
    private readonly HttpClient _client;

    public CitiesEndpointsTests(IntegrationTestWebAppFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateCity_ReturnsCreated_WhenValidDataProvided()
    {
        var countryDto = new CountryCreateDto { Name = "Brazil", Code = "BR" };
        var countryResponse = await _client.PostAsJsonAsync("/countries", countryDto);
        var country = await countryResponse.Content.ReadFromJsonAsync<CountryResponseDto>();

        var cityDto = new CityCreateDto
        {
            Name = "São Paulo",
            CountryId = country!.Id
        };

        var response = await _client.PostAsJsonAsync("/cities", cityDto);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var city = await response.Content.ReadFromJsonAsync<CityResponseDto>();
        city.Should().NotBeNull();
        city!.Name.Should().Be("São Paulo");
        city.CountryId.Should().Be(country.Id);
    }

    [Fact]
    public async Task GetCities_ByCountryId_ReturnsOnlyCitiesFromThatCountry()
    {
        var country1Response = await _client.PostAsJsonAsync("/countries", new CountryCreateDto { Name = "USA", Code = "US" });
        var country1 = await country1Response.Content.ReadFromJsonAsync<CountryResponseDto>();

        var country2Response = await _client.PostAsJsonAsync("/countries", new CountryCreateDto { Name = "Canada", Code = "CA" });
        var country2 = await country2Response.Content.ReadFromJsonAsync<CountryResponseDto>();

        await _client.PostAsJsonAsync("/cities", new CityCreateDto { Name = "New York", CountryId = country1!.Id });
        await _client.PostAsJsonAsync("/cities", new CityCreateDto { Name = "Toronto", CountryId = country2!.Id });

        var response = await _client.GetAsync($"/cities?countryId={country1.Id}&page=1&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PaginationResultDto>();
        result!.Items.Should().OnlyContain(c => c.CountryId == country1.Id);
    }

    [Fact]
    public async Task UpdateCity_ReturnsOk_WhenValidDataProvided()
    {
        var countryResponse = await _client.PostAsJsonAsync("/countries", new CountryCreateDto { Name = "France", Code = "FR" });
        var country = await countryResponse.Content.ReadFromJsonAsync<CountryResponseDto>();

        var cityResponse = await _client.PostAsJsonAsync("/cities", new CityCreateDto { Name = "Paris", CountryId = country!.Id });
        var city = await cityResponse.Content.ReadFromJsonAsync<CityResponseDto>();

        var updateDto = new CityUpdateDto
        {
            Id = city!.Id,
            Name = "Paris Updated",
            CountryId = country.Id
        };

        var response = await _client.PutAsJsonAsync($"/cities/{city.Id}", updateDto);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updatedCity = await response.Content.ReadFromJsonAsync<CityResponseDto>();
        updatedCity!.Name.Should().Be("Paris Updated");
    }

    [Fact]
    public async Task DeleteCity_ReturnsNoContent_WhenCityExists()
    {
        var countryResponse = await _client.PostAsJsonAsync("/countries", new CountryCreateDto { Name = "Italy", Code = "IT" });
        var country = await countryResponse.Content.ReadFromJsonAsync<CountryResponseDto>();

        var cityResponse = await _client.PostAsJsonAsync("/cities", new CityCreateDto { Name = "Rome", CountryId = country!.Id });
        var city = await cityResponse.Content.ReadFromJsonAsync<CityResponseDto>();

        var response = await _client.DeleteAsync($"/cities/{city!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await _client.GetAsync($"/cities/{city.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteCountry_CascadesDeleteToCities()
    {
        var countryResponse = await _client.PostAsJsonAsync("/countries", new CountryCreateDto { Name = "Spain", Code = "ES" });
        var country = await countryResponse.Content.ReadFromJsonAsync<CountryResponseDto>();

        var cityResponse = await _client.PostAsJsonAsync("/cities", new CityCreateDto { Name = "Madrid", CountryId = country!.Id });
        var city = await cityResponse.Content.ReadFromJsonAsync<CityResponseDto>();

        await _client.DeleteAsync($"/countries/{country.Id}");

        var getCityResponse = await _client.GetAsync($"/cities/{city!.Id}");
        getCityResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private class PaginationResultDto
    {
        public List<CityResponseDto> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }
}
