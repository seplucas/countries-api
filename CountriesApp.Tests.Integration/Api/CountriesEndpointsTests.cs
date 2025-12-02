using System.Net;
using System.Net.Http.Json;
using CountriesApp.Application.DTOs.Countries;
using CountriesApp.Tests.Integration.Infrastructure;
using FluentAssertions;

namespace CountriesApp.Tests.Integration.Api;

public class CountriesEndpointsTests : IClassFixture<IntegrationTestWebAppFactory>
{
    private readonly HttpClient _client;
    private readonly IntegrationTestWebAppFactory _factory;

    public CountriesEndpointsTests(IntegrationTestWebAppFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetCountries_ReturnsEmptyList_WhenNoCountriesExist()
    {
        var response = await _client.GetAsync("/countries?page=1&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PaginationResultDto>();
        result.Should().NotBeNull();
        result!.TotalCount.Should().Be(0);
        result.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateCountry_ReturnsCreated_WhenValidDataProvided()
    {
        var createDto = new CountryCreateDto
        {
            Name = "Brazil",
            Code = "BR"
        };

        var response = await _client.PostAsJsonAsync("/countries", createDto);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var country = await response.Content.ReadFromJsonAsync<CountryResponseDto>();
        country.Should().NotBeNull();
        country!.Name.Should().Be("Brazil");
        country.Code.Should().Be("BR");
        country.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task GetCountryById_ReturnsCountry_WhenCountryExists()
    {
        var createDto = new CountryCreateDto { Name = "USA", Code = "US" };
        var createResponse = await _client.PostAsJsonAsync("/countries", createDto);
        var createdCountry = await createResponse.Content.ReadFromJsonAsync<CountryResponseDto>();

        var response = await _client.GetAsync($"/countries/{createdCountry!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var country = await response.Content.ReadFromJsonAsync<CountryResponseDto>();
        country.Should().NotBeNull();
        country!.Name.Should().Be("USA");
    }

    [Fact]
    public async Task GetCountryById_ReturnsNotFound_WhenCountryDoesNotExist()
    {
        var nonExistentId = Guid.NewGuid();

        var response = await _client.GetAsync($"/countries/{nonExistentId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateCountry_ReturnsOk_WhenValidDataProvided()
    {
        var createDto = new CountryCreateDto { Name = "Canada", Code = "CA" };
        var createResponse = await _client.PostAsJsonAsync("/countries", createDto);
        var createdCountry = await createResponse.Content.ReadFromJsonAsync<CountryResponseDto>();

        var updateDto = new CountryUpdateDto
        {
            Id = createdCountry!.Id,
            Name = "Canada Updated",
            Code = "CAN"
        };

        var response = await _client.PutAsJsonAsync($"/countries/{createdCountry.Id}", updateDto);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updatedCountry = await response.Content.ReadFromJsonAsync<CountryResponseDto>();
        updatedCountry!.Name.Should().Be("Canada Updated");
        updatedCountry.Code.Should().Be("CAN");
    }

    [Fact]
    public async Task DeleteCountry_ReturnsNoContent_WhenCountryExists()
    {
        var createDto = new CountryCreateDto { Name = "Mexico", Code = "MX" };
        var createResponse = await _client.PostAsJsonAsync("/countries", createDto);
        var createdCountry = await createResponse.Content.ReadFromJsonAsync<CountryResponseDto>();

        var response = await _client.DeleteAsync($"/countries/{createdCountry!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await _client.GetAsync($"/countries/{createdCountry.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetCountries_WithSearchTerm_ReturnsFilteredResults()
    {
        await _client.PostAsJsonAsync("/countries", new CountryCreateDto { Name = "Argentina", Code = "AR" });
        await _client.PostAsJsonAsync("/countries", new CountryCreateDto { Name = "Australia", Code = "AU" });
        await _client.PostAsJsonAsync("/countries", new CountryCreateDto { Name = "Belgium", Code = "BE" });

        var response = await _client.GetAsync("/countries?search=Ar&page=1&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PaginationResultDto>();
        result!.TotalCount.Should().BeGreaterOrEqualTo(1);
    }

    [Fact]
    public async Task GetCountries_WithPagination_ReturnsCorrectPage()
    {
        for (int i = 1; i <= 15; i++)
        {
            await _client.PostAsJsonAsync("/countries", new CountryCreateDto { Name = $"Country {i}", Code = $"C{i}" });
        }

        var response = await _client.GetAsync("/countries?page=2&pageSize=5");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PaginationResultDto>();
        result!.Page.Should().Be(2);
        result.PageSize.Should().Be(5);
        result.Items.Should().HaveCountLessOrEqualTo(5);
    }

    private class PaginationResultDto
    {
        public List<CountryResponseDto> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }
}
