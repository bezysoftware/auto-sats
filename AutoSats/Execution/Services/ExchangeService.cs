﻿using AutoSats.Exceptions;
using AutoSats.Models;
using ExchangeSharp;
using System.Globalization;

namespace AutoSats.Execution.Services;

public class ExchangeService : IExchangeService
{
    private IExchangeAPI? api;
    private readonly ILogger<ExchangeService> logger;
    private readonly IExchangeAPIProvider apiProvider;

    private IExchangeAPI Api => this.api ?? throw new InvalidOperationException("ExchangeService has not been initialized");

    public ExchangeService(ILogger<ExchangeService> logger, IExchangeAPIProvider apiProvider)
    {
        this.logger = logger;
        this.apiProvider = apiProvider;
    }

    public virtual async Task<IExchangeService> InitializeAsync(string exchangeName, string? keysFileName)
    {
        var api = await this.apiProvider.GetApiAsync(exchangeName);

        if (keysFileName != null)
        {
            api.LoadAPIKeys(keysFileName);
        }

        this.api = api;

        return this;
    }

    public virtual async Task<IExchangeService> InitializeAsync(string exchangeName, string key1, string key2, string? key3)
    {
        var api = await this.apiProvider.GetApiAsync(exchangeName);

        api.LoadAPIKeysUnsecure(key1, key2, key3);

        this.api = api;

        return this;
    }

    public async Task<BuyResult> BuyAsync(string symbol, decimal amount, BuyOrderType orderType, bool invert)
    {
        var result = orderType switch
        {
            BuyOrderType.Market => await BuyMarketAsync(symbol, amount, invert),
            BuyOrderType.Limit => await BuyLimitAsync(symbol, amount, invert),
            _ => throw new NotImplementedException()
        };

        var orderId = result.OrderId;

        this.logger.LogInformation($"Placed buy order '{orderId}', result: {result.Result} | {result.ResultCode}");

        // if the order details are missing query them
        if (result.Result == ExchangeAPIOrderResult.Unknown && orderId != null)
        {
            await Task.Delay(1000);
            result = await TryGetOrderDetailsAsync(orderId);
        }

        // query order details until it is fully filled
        var counter = 0;
        while (counter++ < 3 && (result.Result == ExchangeAPIOrderResult.FilledPartially || result.Result == ExchangeAPIOrderResult.Open || result.Result == ExchangeAPIOrderResult.PendingOpen))
        {
            await Task.Delay(1000);
            result = await TryGetOrderDetailsAsync(result.OrderId);
        }

        if (result.Result != ExchangeAPIOrderResult.Filled && result.AveragePrice == null && result.Price == null)
        {
            throw new Exception($"{result.Result} : {result.ResultCode}: {result.Message}");
        }

        var filled = result.AmountFilled.GetValueOrDefault();
        if (filled == 0)
        {
            filled = amount;
        }

        return new BuyResult(orderId ?? "unknown", filled, result.AveragePrice ?? result.Price ?? 0);
    }

    public async Task<IEnumerable<Balance>> GetBalancesAsync()
    {
        var balances = await Api.GetAmountsAsync();

        return balances
            .Select(x => new Balance(x.Key, x.Value))
            .OrderByDescending(x => x.Amount)
            .ToArray();
    }

    public async Task<decimal> GetPriceAsync(string symbol)
    {
        var result = await Api.GetTickerAsync(symbol);

        return result.Last;
    }

    public async Task<string> WithdrawAsync(string cryptoCurrency, string? address, decimal amount)
    {
        amount = Math.Floor(amount * 100_000_000) / 100_000_000; // round to satoshis
        var result = await Api.WithdrawAsync(new ExchangeWithdrawalRequest
        {
            Address = address,
            AddressTag = string.IsNullOrEmpty(address) ? "AutoSats" : null, // only set tag when adress is missing
            Amount = amount,
            Currency = cryptoCurrency,
            TakeFeeFromAmount = true
        });

        if (!result.Success && string.IsNullOrEmpty(result.Id))
        {
            throw new ScheduleRunFailedException(result.Message);
        }

        return result.Id ?? "unknown";
    }

    public async Task<IEnumerable<Symbol>> GetSymbolsWithAsync(string currency, char[] prefixes)
    {
        var symbols = await Api.GetMarketSymbolsAsync();

        return symbols
            .Where(x => x.Contains(currency, StringComparison.OrdinalIgnoreCase))
            .Select(x => Symbol.Normalize(x, currency, prefixes))
            .ToArray();
    }

    public void Dispose()
    {
        this.api?.Dispose();
        this.api = null;
    }

    private async Task<ExchangeOrderResult> TryGetOrderDetailsAsync(string orderId)
    {
        var tries = 0;

        while (true)
        {
            try
            {
                return await Api.GetOrderDetailsAsync(orderId);
            }
            catch (Exception ex)
            {
                if (++tries > 3)
                {
                    this.logger.LogError(ex, "Failed to get order details, giving up");
                    throw;
                }

                this.logger.LogError(ex, "Failed to get order details, retrying");
                await Task.Delay(1000);
            }
        }
    }

    private Task<ExchangeOrderResult> BuyMarketAsync(string symbol, decimal amount, bool invert)
    {
        return Api.PlaceOrderAsync(new ExchangeOrderRequest
        {
            Amount = amount,
            IsBuy = !invert,
            MarketSymbol = symbol,
            OrderType = OrderType.Market
        });
    }

    private async Task<ExchangeOrderResult> BuyLimitAsync(string symbol, decimal amount, bool invert)
    {
        // If the exchange doesn't support Market OrderType, use Limit with a 1% price change.
        // Ideally the exchange should clamp the price and use highest last price.
        var price = await GetPriceAsync(symbol);

        // get number of decimal places and match them in the calculated price (some exchanges have a limit on decimal places)
        var decimals = Math.Clamp(price.ToString(CultureInfo.InvariantCulture).SkipWhile(c => c != '.').Count() - 1, 2, 10);
        price = Math.Round(!invert ? price * 1.01m : price * 0.99m, decimals);

        return await Api.PlaceOrderAsync(new ExchangeOrderRequest
        {
            Amount = amount,
            IsBuy = !invert,
            MarketSymbol = symbol,
            OrderType = OrderType.Limit,
            Price = price
        });
    }
}
