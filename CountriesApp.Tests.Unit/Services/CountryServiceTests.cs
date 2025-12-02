using AutoMapper;
using CountriesApp.Application.DTOs.Countries;
using CountriesApp.Application.Services;
using CountriesApp.Domain.Entities;
using CountriesApp.Domain.Interfaces;
using CountriesApp.Domain.Shared;
using FluentAssertions;
using Moq;

namespace CountriesApp.Tests.Unit.Services;

public class CountryServiceTests
{
    private readonly Mock<ICountryRepository> _mockRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly CountryService _service;

    public CountryServiceTests()
    {
        _mockRepository = new Mock<ICountryRepository>();
        _mockMapper = new Mock<IMapper>();
        _service = new CountryService(_mockRepository.Object, _mockMapper.Object);
    }

    [Fact]
    public async Task GetCountriesAsync_WithNoSearch_ReturnsAllCountries()
    {
        var country1 = Country.Create("Brazil", "BR").Value!;
        var country2 = Country.Create("USA", "US").Value!;
        var countries = new List<Country> { country1, country2 };
        
        var pagedResult = new PaginationResult<Country>
        {
            Items = countries,
            TotalCount = 2,
            Page = 1,
            PageSize = 10
        };

        var expectedDtos = new PaginationResult<CountryResponseDto>
        {
            Items = new List<CountryResponseDto>
            {
                new() { Id = country1.Id, Name = "Brazil", Code = "BR" },
                new() { Id = country2.Id, Name = "USA", Code = "US" }
            },
            TotalCount = 2,
            Page = 1,
            PageSize = 10
        };

        _mockRepository.Setup(r => r.GetPagedAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Country, bool>>>(), 1, 10))
            .ReturnsAsync(pagedResult);
        
        _mockMapper.Setup(m => m.Map<PaginationResult<CountryResponseDto>>(pagedResult))
            .Returns(expectedDtos);

        var result = await _service.GetCountriesAsync(null, 1, 10);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Items.Should().HaveCount(2);
        result.Value.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task GetCountriesAsync_WithSearch_ReturnsFilteredCountries()
    {
        var country = Country.Create("Brazil", "BR").Value!;
        var countries = new List<Country> { country };
        
        var pagedResult = new PaginationResult<Country>
        {
            Items = countries,
            TotalCount = 1,
            Page = 1,
            PageSize = 10
        };

        var expectedDtos = new PaginationResult<CountryResponseDto>
        {
            Items = new List<CountryResponseDto>
            {
                new() { Id = countries[0].Id, Name = "Brazil", Code = "BR" }
            },
            TotalCount = 1,
            Page = 1,
            PageSize = 10
        };

        _mockRepository.Setup(r => r.GetPagedAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Country, bool>>>(), 1, 10))
            .ReturnsAsync(pagedResult);
        
        _mockMapper.Setup(m => m.Map<PaginationResult<CountryResponseDto>>(pagedResult))
            .Returns(expectedDtos);

        var result = await _service.GetCountriesAsync("Brazil", 1, 10);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().HaveCount(1);
        result.Value.Items.First().Name.Should().Be("Brazil");
    }

    [Fact]
    public async Task GetCountryByIdAsync_WithValidId_ReturnsCountry()
    {
        var countryId = Guid.NewGuid();
        var country = Country.Create("Brazil", "BR").Value!;
        var expectedDto = new CountryResponseDto { Id = countryId, Name = "Brazil", Code = "BR" };

        _mockRepository.Setup(r => r.GetByIdAsync(countryId))
            .ReturnsAsync(Result<Country?>.Success(country));
        
        _mockMapper.Setup(m => m.Map<CountryResponseDto>(country))
            .Returns(expectedDto);

        var result = await _service.GetCountryByIdAsync(countryId);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Name.Should().Be("Brazil");
    }

    [Fact]
    public async Task GetCountryByIdAsync_WithInvalidId_ReturnsFailure()
    {
        var countryId = Guid.NewGuid();
        
        _mockRepository.Setup(r => r.GetByIdAsync(countryId))
            .ReturnsAsync(Result<Country?>.Failure(Error.NotFound("Country not found")));

        var result = await _service.GetCountryByIdAsync(countryId);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateCountryAsync_WithValidData_CreatesCountry()
    {
        var dto = new CountryCreateDto { Name = "Brazil", Code = "BR" };
        var country = Country.Create("Brazil", "BR").Value!;
        var expectedDto = new CountryResponseDto { Id = country.Id, Name = "Brazil", Code = "BR" };

        _mockRepository.Setup(r => r.AddAsync(It.IsAny<Country>()))
            .ReturnsAsync(Result<Country>.Success(country));
        
        _mockMapper.Setup(m => m.Map<CountryResponseDto>(It.IsAny<Country>()))
            .Returns(expectedDto);

        var result = await _service.CreateCountryAsync(dto);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Name.Should().Be("Brazil");
        _mockRepository.Verify(r => r.AddAsync(It.IsAny<Country>()), Times.Once);
    }

    [Fact]
    public async Task CreateCountryAsync_WithEmptyName_ReturnsValidationError()
    {
        var dto = new CountryCreateDto { Name = "", Code = "BR" };

        var result = await _service.CreateCountryAsync(dto);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be("ValidationError");
    }

    [Fact]
    public async Task UpdateCountryAsync_WithValidData_UpdatesCountry()
    {
        var countryId = Guid.NewGuid();
        var dto = new CountryUpdateDto { Id = countryId, Name = "Brazil Updated", Code = "BR" };
        var existingCountry = Country.Create("Brazil", "BR").Value!;
        var updatedCountry = Country.Create("Brazil Updated", "BR").Value!;
        var expectedDto = new CountryResponseDto { Id = countryId, Name = "Brazil Updated", Code = "BR" };

        _mockRepository.Setup(r => r.GetByIdAsync(countryId))
            .ReturnsAsync(Result<Country?>.Success(existingCountry));
        
        _mockMapper.Setup(m => m.Map(dto, existingCountry))
            .Returns(updatedCountry);
        
        _mockRepository.Setup(r => r.UpdateAsync(existingCountry))
            .ReturnsAsync(Result<Country>.Success(updatedCountry));
        
        _mockMapper.Setup(m => m.Map<CountryResponseDto>(updatedCountry))
            .Returns(expectedDto);

        var result = await _service.UpdateCountryAsync(dto);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Name.Should().Be("Brazil Updated");
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Country>()), Times.Once);
    }

    [Fact]
    public async Task UpdateCountryAsync_WithNonExistentId_ReturnsFailure()
    {
        var dto = new CountryUpdateDto { Id = Guid.NewGuid(), Name = "Brazil", Code = "BR" };
        
        _mockRepository.Setup(r => r.GetByIdAsync(dto.Id))
            .ReturnsAsync(Result<Country?>.Failure(Error.NotFound("Country not found")));

        var result = await _service.UpdateCountryAsync(dto);

        result.IsSuccess.Should().BeFalse();
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Country>()), Times.Never);
    }

    [Fact]
    public async Task DeleteCountryAsync_WithValidId_DeletesCountry()
    {
        var countryId = Guid.NewGuid();
        
        _mockRepository.Setup(r => r.DeleteAsync(countryId))
            .ReturnsAsync(Result.Success());

        var result = await _service.DeleteCountryAsync(countryId);

        result.IsSuccess.Should().BeTrue();
        _mockRepository.Verify(r => r.DeleteAsync(countryId), Times.Once);
    }

    [Fact]
    public async Task DeleteCountryAsync_WithNonExistentId_ReturnsFailure()
    {
        var countryId = Guid.NewGuid();
        
        _mockRepository.Setup(r => r.DeleteAsync(countryId))
            .ReturnsAsync(Result.Failure(Error.NotFound("Country not found")));

        var result = await _service.DeleteCountryAsync(countryId);

        result.IsSuccess.Should().BeFalse();
    }
}
