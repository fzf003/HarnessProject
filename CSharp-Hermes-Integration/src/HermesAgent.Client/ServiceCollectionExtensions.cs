using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HermesAgent.Client
{
    /// <summary>
    /// Hermes Agent 客户端依赖注入扩展
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// 添加 Hermes Agent HTTP 客户端
        /// </summary>
        /// <param name="services">服务集合</param>
        /// <param name="configuration">配置</param>
        /// <returns>服务集合</returns>
        public static IServiceCollection AddHermesAgentClient(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // 绑定配置
            services.Configure<HermesAgentOptions>(configuration.GetSection("HermesAgent"));
            
            // 注册 HTTP 客户端
            services.AddHttpClient<IHermesAgentClient, HermesHttpClient>((sp, client) =>
                {
                    var options = sp.GetRequiredService<IOptions<HermesAgentOptions>>().Value;
                    client.BaseAddress = new Uri(options.BaseUrl);
                    client.Timeout = TimeSpan.FromSeconds(30);//options.Timeout;
                    if (!string.IsNullOrEmpty(options.ApiKey))
                    {
                        client.DefaultRequestHeaders.Authorization =
                            new System.Net.Http.Headers.AuthenticationHeaderValue(
                                "Bearer",
                                options.ApiKey
                            );
                    }
                }
            );
            
            // 注册 Webhook 客户端
            services.AddHttpClient<IHermesWebhookClient, HermesWebhookClient>((sp, client) =>
            {
                var options = sp.GetRequiredService<IOptions<HermesAgentOptions>>().Value;
                client.BaseAddress = new Uri(options.WebhookBaseUrl);
                client.Timeout = TimeSpan.FromSeconds(options.Timeout > 0 ? options.Timeout : 30);
            });
            
            // 注册选项验证器
            services.AddSingleton<IValidateOptions<HermesAgentOptions>, HermesAgentOptionsValidator>();
            
            return services;
        }

        /// <summary>
        /// 添加 Hermes Agent 客户端（使用自定义配置）
        /// </summary>
        /// <param name="services">服务集合</param>
        /// <param name="configureOptions">配置委托</param>
        /// <returns>服务集合</returns>
        public static IServiceCollection AddHermesAgentClient(
            this IServiceCollection services,
            Action<HermesAgentOptions> configureOptions)
        {
            // 配置选项
            services.Configure(configureOptions);
            
            // 注册 HTTP 客户端
            services.AddHttpClient<IHermesAgentClient, HermesHttpClient>((sp, client) =>
            {
                var options = sp.GetRequiredService<IOptions<HermesAgentOptions>>().Value;
                client.BaseAddress = new Uri(options.BaseUrl);
                client.Timeout = TimeSpan.FromSeconds(30);//options.Timeout;
                if (!string.IsNullOrEmpty(options.ApiKey))
                {
                    client.DefaultRequestHeaders.Authorization = 
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", options.ApiKey);
                }
            });
            
            // 注册 Webhook 客户端
            services.AddHttpClient<IHermesWebhookClient, HermesWebhookClient>((sp, client) =>
            {
                var options = sp.GetRequiredService<IOptions<HermesAgentOptions>>().Value;
                client.BaseAddress = new Uri(options.WebhookBaseUrl);
                client.Timeout = TimeSpan.FromSeconds(options.Timeout > 0 ? options.Timeout : 30);
            });
            
            // 注册选项验证器
            services.AddSingleton<IValidateOptions<HermesAgentOptions>, HermesAgentOptionsValidator>();
            
            return services;
        }

        /// <summary>
        /// 添加 Hermes Agent 监控服务
        /// </summary>
        /// <param name="services">服务集合</param>
        /// <returns>服务集合</returns>
        public static IServiceCollection AddHermesAgentMonitoring(this IServiceCollection services)
        {
            services.AddSingleton<HermesMetricsCollector>();
            services.AddHostedService<HermesHealthCheckService>();
            
            return services;
        }

        /// <summary>
        /// 添加 Hermes Agent 日志记录
        /// </summary>
        /// <param name="services">服务集合</param>
        /// <returns>服务集合</returns>
        public static IServiceCollection AddHermesAgentLogging(this IServiceCollection services)
        {
            services.AddSingleton<HermesLogger>();
            
            return services;
        }
    }

    /// <summary>
    /// Hermes Agent 配置选项
    /// </summary>
    public class HermesAgentOptions
    {
        /// <summary>
        /// Hermes Agent API 基础地址
        /// </summary>
        public string BaseUrl { get; set; } = "http://localhost:8642";
        
        /// <summary>
        /// API 密钥
        /// </summary>
        public string ApiKey { get; set; }
        
        /// <summary>
        /// Webhook 密钥
        /// </summary>
        public string WebhookSecret { get; set; }
        
        /// <summary>
        /// 超时时间（秒）
        /// </summary>
        public int Timeout { get; set; }
        
        /// <summary>
        /// 最大重试次数
        /// </summary>
        public int MaxRetries { get; set; } = 3;
        
        /// <summary>
        /// 默认模型
        /// </summary>
        public string DefaultModel { get; set; } = "hermes-agent";
        
        /// <summary>
        /// 温度参数
        /// </summary>
        public double Temperature { get; set; } = 0.7;
        
        /// <summary>
        /// 最大 token 数
        /// </summary>
        public int? MaxTokens { get; set; }
        
        /// <summary>
        /// 启用文件工具
        /// </summary>
        public bool EnableFileTools { get; set; } = true;
        
        /// <summary>
        /// 启用终端工具
        /// </summary>
        public bool EnableTerminalTools { get; set; } = true;
        
        /// <summary>
        /// 启用网络工具
        /// </summary>
        public bool EnableWebTools { get; set; } = false;
        
        /// <summary>
        /// Webhook 路由名称
        /// </summary>
        public string WebhookRoute { get; set; } = "dotnet-webhook";

        /// <summary>
        /// Webhook 基础地址
        /// </summary>
        public string WebhookBaseUrl { get; set; } = "http://localhost:8644";
        
        /// <summary>
        /// 是否启用健康检查
        /// </summary>
        public bool EnableHealthCheck { get; set; } = true;
        
        /// <summary>
        /// 健康检查间隔（秒）
        /// </summary>
        public int HealthCheckInterval { get; set; } = 60;
    }

    /// <summary>
    /// Hermes Agent 选项验证器
    /// </summary>
    public class HermesAgentOptionsValidator : IValidateOptions<HermesAgentOptions>
    {
        public ValidateOptionsResult Validate(string name, HermesAgentOptions options)
        {
            var errors = new List<string>();
            
            if (string.IsNullOrWhiteSpace(options.BaseUrl))
                errors.Add($"{nameof(options.BaseUrl)} 不能为空");
            
            if (!Uri.TryCreate(options.BaseUrl, UriKind.Absolute, out _))
                errors.Add($"{nameof(options.BaseUrl)} 必须是有效的 URL");
            
            if (TimeSpan.FromSeconds(options.Timeout) <= TimeSpan.Zero)
                errors.Add($"{nameof(options.Timeout)} 必须大于零");
            
            if (options.MaxRetries < 0)
                errors.Add($"{nameof(options.MaxRetries)} 不能为负数");
            
            if (options.Temperature < 0 || options.Temperature > 2)
                errors.Add($"{nameof(options.Temperature)} 必须在 0 到 2 之间");
            
            if (options.MaxTokens.HasValue && options.MaxTokens <= 0)
                errors.Add($"{nameof(options.MaxTokens)} 必须大于零");
            
            if (options.HealthCheckInterval <= 0)
                errors.Add($"{nameof(options.HealthCheckInterval)} 必须大于零");
            
            return errors.Count > 0 
                ? ValidateOptionsResult.Fail(errors) 
                : ValidateOptionsResult.Success;
        }
    }

    /// <summary>
    /// Hermes Agent 监控收集器
    /// </summary>
    public class HermesMetricsCollector
    {
        private readonly ILogger<HermesMetricsCollector> _logger;
        
        public HermesMetricsCollector(ILogger<HermesMetricsCollector> logger)
        {
            _logger = logger;
        }
        
        public void RecordRequest(string operation, TimeSpan duration, bool success)
        {
            _logger.LogInformation("Hermes 请求 - 操作: {Operation}, 耗时: {Duration}ms, 成功: {Success}",
                operation, duration.TotalMilliseconds, success);
        }
        
        public void RecordError(string operation, Exception exception)
        {
            _logger.LogError(exception, "Hermes 错误 - 操作: {Operation}", operation);
        }
    }

    /// <summary>
    /// Hermes Agent 健康检查服务
    /// </summary>
    public class HermesHealthCheckService : BackgroundService
    {
        private readonly IHermesAgentClient _client;
        private readonly HermesMetricsCollector _metrics;
        private readonly ILogger<HermesHealthCheckService> _logger;
        private readonly HermesAgentOptions _options;
        
        public HermesHealthCheckService(
            IHermesAgentClient client,
            HermesMetricsCollector metrics,
            ILogger<HermesHealthCheckService> logger,
            IOptions<HermesAgentOptions> options)
        {
            _client = client;
            _metrics = metrics;
            _logger = logger;
            _options = options.Value;
        }
        
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!_options.EnableHealthCheck)
                return;
                
            _logger.LogInformation("Hermes Agent 健康检查服务已启动，间隔: {Interval}秒", 
                _options.HealthCheckInterval);
            
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                    var isHealthy = await _client.HealthCheckAsync();
                    stopwatch.Stop();
                    
                    _metrics.RecordRequest("HealthCheck", stopwatch.Elapsed, isHealthy);
                    
                    if (!isHealthy)
                    {
                        _logger.LogWarning("Hermes Agent 健康检查失败");
                    }
                }
                catch (Exception ex)
                {
                    _metrics.RecordError("HealthCheck", ex);
                    _logger.LogError(ex, "Hermes Agent 健康检查异常");
                }
                
                await Task.Delay(TimeSpan.FromSeconds(_options.HealthCheckInterval), stoppingToken);
            }
        }
    }

    /// <summary>
    /// Hermes Agent 日志记录器
    /// </summary>
    public class HermesLogger
    {
        private readonly ILogger _logger;
        
        public HermesLogger(ILogger<HermesLogger> logger)
        {
            _logger = logger;
        }
        
        public void LogRequest(string sessionId, string operation, string message, int messageLength)
        {
            _logger.LogInformation("Hermes 请求 - 会话: {SessionId}, 操作: {Operation}, 消息长度: {Length}", 
                sessionId, operation, messageLength);
        }
        
        public void LogResponse(string sessionId, string operation, string response, TimeSpan duration)
        {
            _logger.LogInformation("Hermes 响应 - 会话: {SessionId}, 操作: {Operation}, 耗时: {Duration}ms, 响应长度: {Length}",
                sessionId, operation, duration.TotalMilliseconds, response.Length);
        }
        
        public void LogStreamStart(string sessionId, string operation, string message)
        {
            _logger.LogInformation("Hermes 流式开始 - 会话: {SessionId}, 操作: {Operation}", 
                sessionId, operation);
        }
        
        public void LogStreamChunk(string sessionId, string operation, string chunk)
        {
            _logger.LogDebug("Hermes 流式块 - 会话: {SessionId}, 操作: {Operation}, 块长度: {Length}", 
                sessionId, operation, chunk.Length);
        }
        
        public void LogStreamEnd(string sessionId, string operation, TimeSpan duration)
        {
            _logger.LogInformation("Hermes 流式结束 - 会话: {SessionId}, 操作: {Operation}, 总耗时: {Duration}ms",
                sessionId, operation, duration.TotalMilliseconds);
        }
    }
}