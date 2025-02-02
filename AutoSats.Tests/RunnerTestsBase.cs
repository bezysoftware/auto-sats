﻿using AutoSats.Configuration;
using AutoSats.Data;
using AutoSats.Execution;
using AutoSats.Execution.Services;
using ExchangeSharp;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;

namespace AutoSats.Tests;

public abstract class RunnerTestsBase
{
    protected const string Exchange = "exchange";

    protected readonly List<ExchangeOptions> options;
    protected readonly Mock<IWalletService> wallet;
    protected readonly Mock<IExchangeAPIProvider> apiProvider;
    protected readonly Mock<IExchangeAPI> api;
    protected readonly ExchangeService service;
    protected readonly Mock<IExchangeServiceFactory> serviceProvider;
    protected readonly ExchangeScheduleRunner runner;
    protected readonly SatsContext db;

    public RunnerTestsBase()
    {
        // db
        var connection = new SqliteConnection("Filename=:memory:");
        connection.Open();
        var opts = new DbContextOptionsBuilder<SatsContext>().UseSqlite(connection).Options;
        this.db = new SatsContext(opts);
        this.db.Database.EnsureCreated();

        // api
        this.api = new Mock<IExchangeAPI>();
        this.apiProvider = new Mock<IExchangeAPIProvider>();
        this.apiProvider.Setup(x => x.GetApiAsync(Exchange)).ReturnsAsync(() => this.api.Object);

        // service
        this.options = new List<ExchangeOptions>();
        this.wallet = new Mock<IWalletService>();
        this.service = new ExchangeService(Mock.Of<ILogger<ExchangeService>>(), this.apiProvider.Object);
        this.serviceProvider = new Mock<IExchangeServiceFactory>();
        this.serviceProvider
            .Setup(x => x.GetServiceAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Returns((string exchange, string filename) => this.service.InitializeAsync(exchange, filename));
        this.runner = new ExchangeScheduleRunner(
            this.db,
            Mock.Of<ILogger<ExchangeScheduleRunner>>(),
            this.serviceProvider.Object,
            this.wallet.Object,
            Mock.Of<INotificationService>(),
            this.options);
    }

    protected void AddSchedule(decimal spend, string spendCurrency, string symbol, ExchangeWithdrawalType withdrawalType = ExchangeWithdrawalType.None, decimal withdrawalLimit = 0, string withdrawalAddress = null)
    {
        this.db.Add(new ExchangeSchedule
        {
            Spend = spend,
            SpendCurrency = spendCurrency,
            Symbol = symbol,
            Exchange = Exchange,
            Cron = "",
            WithdrawalType = withdrawalType,
            WithdrawalAddress = withdrawalAddress,
            WithdrawalLimit = withdrawalLimit
        });
        this.db.SaveChanges();
    }

    protected bool Verify(object expected, object actual)
    {
        actual.Should().BeEquivalentTo(expected);
        return true;
    }
}
