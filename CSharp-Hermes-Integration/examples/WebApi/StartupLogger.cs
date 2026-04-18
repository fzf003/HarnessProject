using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Reflection;
namespace HermesAgent.Examples.WebApi
{
    /// <summary>
    /// 应用程序启动日志记录器
    /// 提供结构化的启动日志输出，便于调试和监控
    /// </summary>
    public static class StartupLogger
    {
        private const string Separator = "========================================";
        private const string SubSeparator = "----------------------------------------";

        /// <summary>
        /// 记录应用程序启动信息
        /// </summary>
        /// <param name="logger">日志记录器</param>
        /// <param name="configuration">配置对象</param>
        /// <param name="environment">环境变量</param>
        public static void LogStartup(
            ILogger logger,
            IConfiguration configuration,
            IHostEnvironment environment)
        {
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(configuration);
            ArgumentNullException.ThrowIfNull(environment);

            var logLevel = environment.IsDevelopment() ? LogLevel.Debug : LogLevel.Information;

            logger.Log(logLevel, Separator);
            logger.Log(logLevel, "Hermes Agent WebApi Starting...");
            logger.Log(logLevel, Separator);

            // 记录环境信息
            LogEnvironmentInfo(logger, environment, logLevel);

            // 记录配置信息
            LogConfigurationInfo(logger, configuration, logLevel);

            logger.Log(logLevel, SubSeparator);
        }

        /// <summary>
        /// 记录服务注册状态
        /// </summary>
        /// <param name="logger">日志记录器</param>
        /// <param name="servicesRegistered">已注册的服务列表</param>
        public static void LogServicesRegistered(
            ILogger logger,
            Dictionary<string, bool> servicesRegistered)
        {
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(servicesRegistered);

            var logLevel = logger.IsEnabled(LogLevel.Debug) ? LogLevel.Debug : LogLevel.Information;

            logger.Log(logLevel, "Services Registered:");
            foreach (var service in servicesRegistered)
            {
                var status = service.Value ? "✓" : "✗";
                logger.Log(logLevel, $"  - {service.Key}: {status}");
            }
            logger.Log(logLevel, SubSeparator);
        }

        /// <summary>
        /// 记录中间件配置状态
        /// </summary>
        /// <param name="logger">日志记录器</param>
        /// <param name="middlewareConfigured">已配置的中间件列表</param>
        public static void LogMiddlewareConfigured(
            ILogger logger,
            Dictionary<string, bool> middlewareConfigured)
        {
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(middlewareConfigured);

            var logLevel = logger.IsEnabled(LogLevel.Debug) ? LogLevel.Debug : LogLevel.Information;

            logger.Log(logLevel, "Middleware Configured:");
            foreach (var middleware in middlewareConfigured)
            {
                var status = middleware.Value ? "✓" : "✗";
                logger.Log(logLevel, $"  - {middleware.Key}: {status}");
            }
            logger.Log(logLevel, Separator);
            logger.Log(logLevel, "Application Started Successfully!");
            logger.Log(logLevel, Separator);
        }

        /// <summary>
        /// 记录环境信息
        /// </summary>
        private static void LogEnvironmentInfo(ILogger logger, IHostEnvironment environment, LogLevel logLevel)
        {
            logger.Log(logLevel, $"Environment: {environment.EnvironmentName}");
            logger.Log(logLevel, $"Application Name: {environment.ApplicationName}");

           

            var version = typeof(Program).Assembly?.GetCustomAttribute<System.Reflection.AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "1.0.0";
            logger.Log(logLevel, $"Version: {version}");
        }

        /// <summary>
        /// 记录配置信息（隐藏敏感数据）
        /// </summary>
        private static void LogConfigurationInfo(ILogger logger, IConfiguration configuration, LogLevel logLevel)
        {
            logger.Log(logLevel, "Configuration Loaded:");

            // HermesAgent 配置
            var hermesSection = configuration.GetSection("HermesAgent");
            if (hermesSection.Exists())
            {
                var baseUrl = hermesSection["BaseUrl"] ?? "Not configured";
                logger.Log(logLevel, $"  - HermesAgent BaseUrl: {baseUrl}");

                // 不记录敏感的 API Key
                var apiKey = hermesSection["ApiKey"];
                if (!string.IsNullOrEmpty(apiKey))
                {
                    var maskedKey = MaskSensitiveData(apiKey);
                    logger.Log(logLevel, $"  - HermesAgent ApiKey: {maskedKey}");
                }
            }

            // Swagger 配置
            var swaggerSection = configuration.GetSection("Swagger");
            if (swaggerSection.Exists())
            {
                var swaggerEnabled = swaggerSection["Enabled"] ?? "True";
                logger.Log(logLevel, $"  - Swagger Enabled: {swaggerEnabled}");
            }

            // URL 配置
            var urls = configuration["Urls"] ?? configuration["ASPNETCORE_URLS"] ?? "http://localhost:5000";
            logger.Log(logLevel, $"  - URLs: {urls}");

            // 日志级别配置
            var logLevelConfig = configuration["Logging:LogLevel:Default"] ?? "Information";
            logger.Log(logLevel, $"  - Log Level: {logLevelConfig}");
        }

        /// <summary>
        /// 脱敏敏感数据
        /// </summary>
        /// <param name="value">原始值</param>
        /// <returns>脱敏后的值</returns>
        private static string MaskSensitiveData(string value)
        {
            if (string.IsNullOrEmpty(value) || value.Length <= 8)
            {
                return "***";
            }

            var visibleLength = Math.Min(4, value.Length / 4);
            return $"{value.Substring(0, visibleLength)}...{value.Substring(value.Length - visibleLength)}";
        }
    }
}
