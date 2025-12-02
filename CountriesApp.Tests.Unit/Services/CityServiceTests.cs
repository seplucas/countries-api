using AutoMapper;
using CountriesApp.Application.DTOs.Cities;
using CountriesApp.Application.Services;
using CountriesApp.Domain.Entities;
using CountriesApp.Domain.Interfaces;
using CountriesApp.Domain.Shared;
using FluentAssertions;
using Moq;

namespace CountriesApp.Tests.Unit.Services;

public class CityServiceTests
{
    private readonly Mock<ICityRepository> _mockRepository;
    private readonly Mock<ICountryRepository> _mockCountryRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly CityService _service;

    public CityServiceTests()
    {
        _mockRepository = new Mock<ICityRepository>();
        _mockCountryRepository = new Mock<ICountryRepository>();
        _mockMapper = new Mock<IMapper>();
        _service = new CityService(_mockRepository.Object, _mockCountryRepository.Object, _mockMapper.Object);
    }

    [Fact]
    public async Task GetCitiesAsync_WithNoFilters_ReturnsAllCities()
    {
        var countryId = Guid.NewGuid();
        var cities = new List<City>
        {
            City.Create("São Paulo", countryId).Value!,
            City.Create("Rio de Janeiro", countryId).Value!
        };
        
        var pagedResult = new PaginationResult<City>
        {
            Items = cities,
            TotalCount = 2,
            Page = 1,
            PageSize = 10
        };

        var expectedDtos = new PaginationResult<CityResponseDto>
        {
            Items = new List<CityResponseDto>
            {
                new() { Id = cities[0].Id, Name = "São Paulo", CountryId = countryId },
                new() { Id = cities[1].Id, Name = "Rio de Janeiro", CountryId = countryId }
            },
            TotalCount = 2,
            Page = 1,
            PageSize = 10
        };

        _mockRepository.Setup(r => r.GetPagedAsync(It.IsAny<System.Linq.Expressions.Expression<Func<City, bool>>>(), 1, 10))
            .ReturnsAsync(pagedResult);
        
        _mockMapper.Setup(m => m.Map<PaginationResult<CityResponseDto>>(pagedResult))
            .Returns(expectedDtos);

        var result = await _service.GetCitiesAsync(null, null, 1, 10);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetCitiesAsync_WithSearchTerm_ReturnsFilteredCities()
    {
        var countryId = Guid.NewGuid();
        var cities = new List<City>
        {
            City.Create("São Paulo", countryId).Value!
        };
        
        var pagedResult = new PaginationResult<City>
        {
            Items = cities,
            TotalCount = 1,
            Page = 1,
            PageSize = 10
        };

        var expectedDtos = new PaginationResult<CityResponseDto>
        {
            Items = new List<CityResponseDto>
            {
                new() { Id = cities[0].Id, Name = "São Paulo", CountryId = countryId }
            },
            TotalCount = 1,
            Page = 1,
            PageSize = 10
        };

        _mockRepository.Setup(r => r.GetPagedAsync(It.IsAny<System.Linq.Expressions.Expression<Func<City, bool>>>(), 1, 10))
            .ReturnsAsync(pagedResult);
        
        _mockMapper.Setup(m => m.Map<PaginationResult<CityResponseDto>>(pagedResult))
            .Returns(expectedDtos);

        var result = await _service.GetCitiesAsync("São Paulo", null, 1, 10);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().HaveCount(1);
        result.Value.Items.First().Name.Should().Be("São Paulo");
    }

    [Fact]
    public async Task GetCityByIdAsync_WithValidId_ReturnsCity()
    {
        var cityId = Guid.NewGuid();
        var countryId = Guid.NewGuid();
        var city = City.Create("São Paulo", countryId).Value!;
        var expectedDto = new CityResponseDto { Id = cityId, Name = "São Paulo", CountryId = countryId };

        _mockRepository.Setup(r => r.GetByIdAsync(cityId))
            .ReturnsAsync(Result<City?>.Success(city));
        
        _mockMapper.Setup(m => m.Map<CityResponseDto>(city))
            .Returns(expectedDto);

        var result = await _service.GetCityByIdAsync(cityId);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Name.Should().Be("São Paulo");
    }

    [Fact]
    public async Task CreateCityAsync_WithValidData_CreatesCity()
    {
        var countryId = Guid.NewGuid();
        var dto = new CityCreateDto { Name = "São Paulo", CountryId = countryId };
        var city = City.Create("São Paulo", countryId).Value!;
        var country = Country.Create("Brazil", "BR").Value!;
        var expectedDto = new CityResponseDto { Id = city.Id, Name = "São Paulo", CountryId = countryId };

        _mockCountryRepository.Setup(r => r.GetByIdAsync(countryId))
            .ReturnsAsync(Result<Country?>.Success(country));
        
        _mockMapper.Setup(m => m.Map<City>(dto))
            .Returns(city);
        
        _mockRepository.Setup(r => r.AddAsync(It.IsAny<City>()))
            .ReturnsAsync(Result<City>.Success(city));
        
        _mockMapper.Setup(m => m.Map<CityResponseDto>(city))
            .Returns(expectedDto);

        var result = await _service.CreateCityAsync(dto);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Name.Should().Be("São Paulo");
        _mockRepository.Verify(r => r.AddAsync(It.IsAny<City>()), Times.Once);
    }

    [Fact]
    public async Task CreateCityAsync_WithEmptyName_ReturnsValidationError()
    {
        var countryId = Guid.NewGuid();
        var dto = new CityCreateDto { Name = "", CountryId = countryId };
        var country = Country.Create("Brazil", "BR").Value!;

        _mockCountryRepository.Setup(r => r.GetByIdAsync(countryId))
            .ReturnsAsync(Result<Country?>.Success(country));

        var result = await _service.CreateCityAsync(dto);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be("ValidationError");
    }

    [Fact]
    public async Task CreateCityAsync_WithEmptyCountryId_ReturnsValidationError()
    {
        var dto = new CityCreateDto { Name = "São Paulo", CountryId = Guid.Empty };

        _mockCountryRepository.Setup(r => r.GetByIdAsync(Guid.Empty))
            .ReturnsAsync(Result<Country?>.Failure(Error.NotFound("Country not found")));

        var result = await _service.CreateCityAsync(dto);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be("NotFound");
    }

    [Fact]
    public async Task UpdateCityAsync_WithValidData_UpdatesCity()
    {
        var cityId = Guid.NewGuid();
        var countryId = Guid.NewGuid();
        var dto = new CityUpdateDto { Id = cityId, Name = "São Paulo Updated", CountryId = countryId };
        var existingCity = City.Create("São Paulo", countryId).Value!;
        var updatedCity = City.Create("São Paulo Updated", countryId).Value!;
        var country = Country.Create("Brazil", "BR").Value!;
        var expectedDto = new CityResponseDto { Id = cityId, Name = "São Paulo Updated", CountryId = countryId };

        _mockRepository.Setup(r => r.GetByIdAsync(cityId))
            .ReturnsAsync(Result<City?>.Success(existingCity));
        
        _mockCountryRepository.Setup(r => r.GetByIdAsync(countryId))
            .ReturnsAsync(Result<Country?>.Success(country));
        
        _mockMapper.Setup(m => m.Map(dto, existingCity))
            .Returns(updatedCity);
        
        _mockRepository.Setup(r => r.UpdateAsync(existingCity))
            .ReturnsAsync(Result<City>.Success(existingCity));
        
        _mockMapper.Setup(m => m.Map<CityResponseDto>(existingCity))
            .Returns(expectedDto);

        var result = await _service.UpdateCityAsync(dto);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Name.Should().Be("São Paulo Updated");
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<City>()), Times.Once);
    }

    [Fact]
    public async Task DeleteCityAsync_WithValidId_DeletesCity()
    {
        var cityId = Guid.NewGuid();
        
        _mockRepository.Setup(r => r.DeleteAsync(cityId))
            .ReturnsAsync(Result.Success());

        var result = await _service.DeleteCityAsync(cityId);

        result.IsSuccess.Should().BeTrue();
        _mockRepository.Verify(r => r.DeleteAsync(cityId), Times.Once);
    }
}
