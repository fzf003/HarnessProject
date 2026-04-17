using FluentAssertions;
using HermesAgent.Client;
using HermesAgent.Client.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using System.Net;
using System.Text.Json;
using Xunit;

namespace HermesAgent.Client.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddHermesAgentClient_ShouldRegisterServices()
        {
            // Arrange
            var services = new ServiceCollection();
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["HermesAgent:BaseUrl"] = "http://localhost:8642",
                    ["HermesAgent:ApiKey"] = "test-key",
                    ["HermesAgent:Timeout"] = "60"
                })
                .Build();

            // Act
            services.AddHermesAgentClient(configuration);
            var serviceProvider = services.BuildServiceProvider();

            // Assert
            serviceProvider.GetService<IHermesAgentClient>().Should().NotBeNull();
            serviceProvider.GetService<IHermesWebhookClient>().Should().NotBeNull();
            serviceProvider.GetService<IOptions<HermesAgentOptions>>().Should().NotBeNull();
        }

        [Fact]
        public void AddHermesAgentClient_WithConfigureOptions_ShouldRegisterServices()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddHermesAgentClient(options =>
            {
                options.BaseUrl = "http://localhost:8642";
                options.ApiKey = "test-key";
                options.Timeout = 60;
            });
            var serviceProvider = services.BuildServiceProvider();

            // Assert
            serviceProvider.GetService<IHermesAgentClient>().Should().NotBeNull();
            serviceProvider.GetService<IHermesWebhookClient>().Should().NotBeNull();
            var options = serviceProvider.GetService<IOptions<HermesAgentOptions>>()!.Value;
            options.BaseUrl.Should().Be("http://localhost:8642");
            options.ApiKey.Should().Be("test-key");
            options.Timeout.Should().Be(60);
        }

        [Fact]
        public void AddHermesAgentMonitoring_ShouldRegisterServices()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddLogging(); // 添加日志记录器服务

            // Act
            services.AddHermesAgentMonitoring();
            var serviceProvider = services.BuildServiceProvider();

            // Assert
            serviceProvider.GetService<HermesMetricsCollector>().Should().NotBeNull();
        }

        [Fact]
        public void AddHermesAgentLogging_ShouldRegisterServices()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddLogging(); // 添加日志记录器服务

            // Act
            services.AddHermesAgentLogging();
            var serviceProvider = services.BuildServiceProvider();

            // Assert
            serviceProvider.GetService<HermesLogger>().Should().NotBeNull();
        }
    }

    public class HermesAgentOptionsValidatorTests
    {
        private readonly HermesAgentOptionsValidator _validator = new();

        [Theory]
        [InlineData("", false)]
        [InlineData("not-a-url", false)]
        [InlineData("http://localhost:8642", true)]
        [InlineData("https://api.example.com", true)]
        public void Validate_BaseUrl_ShouldValidateCorrectly(string baseUrl, bool isValid)
        {
            // Arrange
            var options = new HermesAgentOptions { BaseUrl = baseUrl };

            // Act
            var result = _validator.Validate(null, options);

            // Assert
            result.Succeeded.Should().Be(isValid);
        }

        [Theory]
        [InlineData(0, false)]
        [InlineData(-1, false)]
        [InlineData(1, true)]
        [InlineData(30, true)]
        public void Validate_Timeout_ShouldValidateCorrectly(int timeout, bool isValid)
        {
            // Arrange
            var options = new HermesAgentOptions
            {
                BaseUrl = "http://localhost:8642",
                Timeout = timeout
            };

            // Act
            var result = _validator.Validate(null, options);

            // Assert
            result.Succeeded.Should().Be(isValid);
        }

        [Theory]
        [InlineData(-1, false)]
        [InlineData(0, true)]
        [InlineData(3, true)]
        [InlineData(10, true)]
        public void Validate_MaxRetries_ShouldValidateCorrectly(int maxRetries, bool isValid)
        {
            // Arrange
            var options = new HermesAgentOptions
            {
                BaseUrl = "http://localhost:8642",
                MaxRetries = maxRetries
            };

            // Act
            var result = _validator.Validate(null, options);

            // Assert
            result.Succeeded.Should().Be(isValid);
        }

        [Theory]
        [InlineData(-0.1, false)]
        [InlineData(0.0, true)]
        [InlineData(0.7, true)]
        [InlineData(1.0, true)]
        [InlineData(2.0, true)]
        [InlineData(2.1, false)]
        public void Validate_Temperature_ShouldValidateCorrectly(double temperature, bool isValid)
        {
            // Arrange
            var options = new HermesAgentOptions
            {
                BaseUrl = "http://localhost:8642",
                Temperature = temperature
            };

            // Act
            var result = _validator.Validate(null, options);

            // Assert
            result.Succeeded.Should().Be(isValid);
        }

        [Theory]
        [InlineData(null, true)]
        [InlineData(0, false)]
        [InlineData(-1, false)]
        [InlineData(1, true)]
        [InlineData(1000, true)]
        public void Validate_MaxTokens_ShouldValidateCorrectly(int? maxTokens, bool isValid)
        {
            // Arrange
            var options = new HermesAgentOptions
            {
                BaseUrl = "http://localhost:8642",
                MaxTokens = maxTokens
            };

            // Act
            var result = _validator.Validate(null, options);

            // Assert
            result.Succeeded.Should().Be(isValid);
        }
    }

    public class HermesAgentOptionsTests
    {
        [Fact]
        public void Constructor_ShouldSetDefaultValues()
        {
            // Act
            var options = new HermesAgentOptions();

            // Assert
            options.BaseUrl.Should().Be("http://localhost:8642");
            options.ApiKey.Should().BeNull();
            options.Timeout.Should().Be(30);
            options.MaxRetries.Should().Be(3);
            options.DefaultModel.Should().Be("hermes-agent");
            options.Temperature.Should().Be(0.7);
            options.MaxTokens.Should().BeNull();
            options.EnableFileTools.Should().BeTrue();
            options.EnableTerminalTools.Should().BeTrue();
            options.EnableWebTools.Should().BeFalse();
            options.WebhookRoute.Should().Be("dotnet-webhook");
            options.WebhookBaseUrl.Should().Be("http://localhost:8644");
            options.EnableHealthCheck.Should().BeTrue();
            options.HealthCheckInterval.Should().Be(60);
        }
    }
}