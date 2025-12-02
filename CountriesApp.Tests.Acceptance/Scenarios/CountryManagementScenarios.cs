using System.Net;
using System.Net.Http.Json;
using CountriesApp.Application.DTOs.Cities;
using CountriesApp.Application.DTOs.Countries;
using CountriesApp.Tests.Acceptance.Infrastructure;
using FluentAssertions;

namespace CountriesApp.Tests.Acceptance.Scenarios;

public class CountryManagementScenarios : IClassFixture<AcceptanceTestWebAppFactory>
{
    private readonly HttpClient _client;

    public CountryManagementScenarios(AcceptanceTestWebAppFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Scenario_SearchAndFilterCountries()
    {
        // Given: Multiple countries exist in the system
        var countries = new[]
        {
            new CountryCreateDto { Name = "Argentina", Code = "AR" },
            new CountryCreateDto { Name = "Australia", Code = "AU" },
            new CountryCreateDto { Name = "Austria", Code = "AT" },
            new CountryCreateDto { Name = "Belgium", Code = "BE" }
        };

        foreach (var country in countries)
        {
            await _client.PostAsJsonAsync("/countries", country);
        }

        // When: I search for countries starting with "A"
        var searchResponse = await _client.GetAsync("/countries?search=A&page=1&pageSize=10");
        
        // Then: I should get only countries matching the search
        searchResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var searchResult = await searchResponse.Content.ReadFromJsonAsync<PaginationResultDto>();
        searchResult!.Items.Should().HaveCountGreaterOrEqualTo(3);
        searchResult.Items.Should().OnlyContain(c => c.Name.StartsWith("A", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Scenario_PaginationWorksCorrectly()
    {
        // Given: Many countries exist in the system
        for (int i = 1; i <= 25; i++)
        {
            await _client.PostAsJsonAsync("/countries", new CountryCreateDto { Name = $"Country {i:D2}", Code = $"C{i}" });
        }

        // When: I request the first page with 10 items
        var page1Response = await _client.GetAsync("/countries?page=1&pageSize=10");
        var page1 = await page1Response.Content.ReadFromJsonAsync<PaginationResultDto>();

        // Then: I should get 10 items for page 1
        page1!.Items.Should().HaveCount(10);
        page1.Page.Should().Be(1);
        page1.TotalCount.Should().BeGreaterOrEqualTo(25);

        // When: I request the second page
        var page2Response = await _client.GetAsync("/countries?page=2&pageSize=10");
        var page2 = await page2Response.Content.ReadFromJsonAsync<PaginationResultDto>();

        // Then: I should get different items on page 2
        page2!.Items.Should().HaveCount(10);
        page2.Page.Should().Be(2);
        page2.Items.Should().NotIntersectWith(page1.Items);
    }

    [Fact]
    public async Task Scenario_CountryWithCitiesManagement()
    {
        // Given: I create a country
        var countryResponse = await _client.PostAsJsonAsync("/countries", new CountryCreateDto { Name = "Japan", Code = "JP" });
        var country = await countryResponse.Content.ReadFromJsonAsync<CountryResponseDto>();

        // When: I add cities to the country
        var cities = new[]
        {
            new CityCreateDto { Name = "Tokyo", CountryId = country!.Id },
            new CityCreateDto { Name = "Osaka", CountryId = country.Id },
            new CityCreateDto { Name = "Kyoto", CountryId = country.Id }
        };

        foreach (var city in cities)
        {
            var cityResponse = await _client.PostAsJsonAsync("/cities", city);
            cityResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        }

        // Then: I should be able to retrieve all cities for that country
        var citiesResponse = await _client.GetAsync($"/cities?countryId={country.Id}&page=1&pageSize=10");
        citiesResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var citiesResult = await citiesResponse.Content.ReadFromJsonAsync<CityPaginationResultDto>();
        citiesResult!.Items.Should().HaveCount(3);
        citiesResult.Items.Should().OnlyContain(c => c.CountryId == country.Id);

        // When: I delete the country
        var deleteResponse = await _client.DeleteAsync($"/countries/{country.Id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Then: All cities should also be deleted (cascade delete)
        var citiesAfterDeleteResponse = await _client.GetAsync($"/cities?countryId={country.Id}&page=1&pageSize=10");
        var citiesAfterDelete = await citiesAfterDeleteResponse.Content.ReadFromJsonAsync<CityPaginationResultDto>();
        citiesAfterDelete!.Items.Should().BeEmpty();
    }

    private class PaginationResultDto
    {
        public List<CountryResponseDto> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    private class CityPaginationResultDto
    {
        public List<CityResponseDto> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }
}
