using FluentAssertions;
using ForeignExchange.Api.Database;
using ForeignExchange.Api.Models;
using ForeignExchange.Api.Repositories;
using ForeignExchange.Api.Services;
using ForeignExchange.Api.Validation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;

namespace ForeignExchange.Api.Tests.Unit;

public class QuoteServiceTests
{
    private readonly QuoteService _sut;

    private readonly IRatesRepository _ratesRepository = Substitute.For<IRatesRepository>();

    private readonly ILoggerAdapter<QuoteService> _logger = Substitute.For<ILoggerAdapter<QuoteService>>();

    public QuoteServiceTests()
    {
        _sut = new QuoteService(_ratesRepository, _logger);
    }
    
    [Fact]
    public async Task GetQuoteAsync_ShouldReturnQuote_WhenCurrenciesAreSupported()
    {
        // Arrange
        var fromCurrency = "EUR";
        var toCurrency = "GBP";
        var amount = 10;

        var expectedQuote = new ConversionQuote
        {
            BaseCurrency = fromCurrency,
            QuoteCurrency = toCurrency,
            BaseAmount = 10,
            QuoteAmount = 9
        };
        
        _ratesRepository.GetRateAsync(fromCurrency, toCurrency).Returns(new FxRate
        {   
            Rate = 0.9m,
            FromCurrency = fromCurrency,
            ToCurrency = toCurrency
        });

        // Act
        var result = await _sut.GetQuoteAsync(fromCurrency, toCurrency, amount);

        // Assert
        result.Should().BeEquivalentTo(expectedQuote);
    }
    
    [Fact]
    public async Task GetQuoteAsync_ShouldThrowException_WhenSameCurrencyIsUsed()
    {
        // Arrange
        var fromCurrency = "GBP";
        var toCurrency = "GBP";
        var amount = 100;
    
        // Act
        var resultAction = () => _sut.GetQuoteAsync(fromCurrency, toCurrency, amount);
        
        // Assert
        await resultAction.Should().ThrowAsync<SameCurrencyException>();
    }

    [Fact]
    public async Task GetQuoteAsync_ShouldLogAppropriateMessage_WhenInvoked()
    {
        // Arrange
        var fromCurrency = "GBP";
        var toCurrency = "USD";
        var amount = 100;
        var expectedRate = new FxRate
        {
            FromCurrency = fromCurrency,
            ToCurrency = toCurrency,
            TimestampUtc = DateTime.UtcNow,
            Rate = 1.6m
        };

        _ratesRepository.GetRateAsync(fromCurrency, toCurrency)
            .Returns(expectedRate);

        // Act
        await _sut.GetQuoteAsync(fromCurrency, toCurrency, amount);

        // Assert
        _logger.Received(1).LogInformation("Retrieved quote for currencies {FromCurrency}->{ToCurrency} in {ElapsedMilliseconds}ms",
            Arg.Any<object[]>());
    }
}
