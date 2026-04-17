using HermesAgent.Examples.WebApi.Constants;
using HermesAgent.Examples.WebApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace HermesAgent.Examples.WebApi.Controllers
{
    /// <summary>
    /// 信息 API 控制器
    /// 提供系统信息、状态和健康检查相关的API端点
    /// </summary>
    [ApiController]
    [Route("api/v1/[controller]")]
    [Produces("application/json")]
    public class InfoController : ControllerBase
    {
        private readonly ILogger<InfoController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IHostApplicationLifetime _applicationLifetime;
        private static readonly DateTime _startTime = DateTime.UtcNow;

        /// <summary>
        /// 初始化信息控制器
        /// </summary>
        public InfoController(
            ILogger<InfoController> logger,
            IConfiguration configuration,
            IHostApplicationLifetime applicationLifetime)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _applicationLifetime = applicationLifetime ?? throw new ArgumentNullException(nameof(applicationLifetime));
        }

        /// <summary>
        /// 获取系统信息
        /// </summary>
        /// <returns>系统信息 JSON 对象</returns>
        /// <response code="200">成功返回系统信息</response>
        [HttpGet("system")]
        [ProducesResponseType(typeof(ApiResponse<SystemInfoResponse>), StatusCodes.Status200OK)]
        public IActionResult GetSystemInfo()
        {
            _logger.LogInformation("GetSystemInfo requested - RequestId: {RequestId}", HttpContext.TraceIdentifier);

            try
            {
                var response = new SystemInfoResponse
                {
                    Application = ApiConstants.ApplicationName,
                    Version = ApiConstants.ApplicationVersion,
                    Environment = _configuration["ASPNETCORE_ENVIRONMENT"] ?? "Development",
                    Timestamp = DateTime.UtcNow,
                    Status = "Running",
                    Uptime = DateTime.UtcNow - _startTime,
                    Features = new List<string>
                    {
                        "Chat API",
                        "Streaming Chat",
                        "Session Management",
                        "Model Listing",
                        "Health Check",
                        "Batch Processing"
                    },
                    Statistics = new SystemStatistics
                    {
                        TotalRequests = 0,
                        ActiveSessions = 0,
                        AverageResponseTime = 0.0
                    }
                };

                return Ok(ApiResponse<SystemInfoResponse>.SuccessResponse(
                    data: response,
                    message: "System information retrieved successfully",
                    requestId: HttpContext.TraceIdentifier));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving system information - RequestId: {RequestId}", HttpContext.TraceIdentifier);
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse.ErrorResponse(
                        StatusCodes.Status500InternalServerError,
                        "Failed to retrieve system information",
                        HttpContext.TraceIdentifier));
            }
        }

        /// <summary>
        /// 获取 API 版本信息
        /// </summary>
        /// <returns>版本信息 JSON 对象</returns>
        /// <response code="200">成功返回版本信息</response>
        [HttpGet("version")]
        [ProducesResponseType(typeof(ApiResponse<VersionInfoResponse>), StatusCodes.Status200OK)]
        public IActionResult GetVersionInfo()
        {
            _logger.LogInformation("GetVersionInfo requested - RequestId: {RequestId}", HttpContext.TraceIdentifier);

            var response = new VersionInfoResponse
            {
                ApiVersion = ApiConstants.ApiVersion,
                BuildNumber = _configuration["Build:Number"] ?? "1.0.0.0",
                BuildDate = DateTime.Parse(_configuration["Build:Date"] ?? DateTime.UtcNow.ToString("O")),
                SupportedVersions = new List<string> { "v1" },
                DeprecatedVersions = new List<string>(),
                EndOfLife = null
            };

            return Ok(ApiResponse<VersionInfoResponse>.SuccessResponse(
                data: response,
                message: "Version information retrieved successfully",
                requestId: HttpContext.TraceIdentifier));
        }

        /// <summary>
        /// 获取服务状态
        /// </summary>
        /// <returns>状态信息 JSON 对象</returns>
        /// <response code="200">成功返回服务状态</response>
        [HttpGet("status")]
        [ProducesResponseType(typeof(ApiResponse<StatusResponse>), StatusCodes.Status200OK)]
        public IActionResult GetStatus()
        {
            _logger.LogInformation("GetStatus requested - RequestId: {RequestId}", HttpContext.TraceIdentifier);

            try
            {
                var response = new StatusResponse
                {
                    Service = ApiConstants.ApplicationName,
                    Status = "Operational",
                    LastUpdated = DateTime.UtcNow,
                    Components = new List<ComponentStatus>
                    {
                        new ComponentStatus 
                        { 
                            Name = "API Server", 
                            Status = "Healthy", 
                            Message = "Running normally" 
                        },
                        new ComponentStatus 
                        { 
                            Name = "Hermes Agent", 
                            Status = "Healthy", 
                            Message = "Connected and available" 
                        }
                    },
                    Metrics = new Dictionary<string, object>
                    {
                        { "uptime_seconds", (DateTime.UtcNow - _startTime).TotalSeconds },
                        { "active_connections", 0 },
                        { "total_requests", 0 }
                    }
                };

                return Ok(ApiResponse<StatusResponse>.SuccessResponse(
                    data: response,
                    message: "Service status retrieved successfully",
                    requestId: HttpContext.TraceIdentifier));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving service status - RequestId: {RequestId}", HttpContext.TraceIdentifier);
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse.ErrorResponse(
                        StatusCodes.Status500InternalServerError,
                        "Failed to retrieve service status",
                        HttpContext.TraceIdentifier));
            }
        }

        /// <summary>
        /// 获取配置信息（非敏感部分）
        /// </summary>
        /// <returns>配置信息 JSON 对象</returns>
        /// <response code="200">成功返回配置信息</response>
        [HttpGet("config")]
        [ProducesResponseType(typeof(ApiResponse<ConfigInfoResponse>), StatusCodes.Status200OK)]
        public IActionResult GetConfigInfo()
        {
            _logger.LogInformation("GetConfigInfo requested - RequestId: {RequestId}", HttpContext.TraceIdentifier);

            try
            {
                // 只返回非敏感配置信息
                var response = new ConfigInfoResponse
                {
                    Environment = _configuration["ASPNETCORE_ENVIRONMENT"] ?? "Development",
                    DebugMode = _configuration["Debug:Enabled"] == "true",
                    HermesAgent = new HermesAgentConfigInfo
                    {
                        BaseUrl = _configuration["HermesAgent:BaseUrl"] ?? "http://localhost:8642",
                        Timeout = int.Parse(_configuration["HermesAgent:Timeout"] ?? "30"),
                        RetryCount = int.Parse(_configuration["HermesAgent:RetryCount"] ?? "3"),
                        MonitoringEnabled = _configuration["HermesAgent:Monitoring:Enabled"] == "true"
                    }
                };

                return Ok(ApiResponse<ConfigInfoResponse>.SuccessResponse(
                    data: response,
                    message: "Configuration information retrieved successfully",
                    requestId: HttpContext.TraceIdentifier));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving configuration information - RequestId: {RequestId}", HttpContext.TraceIdentifier);
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse.ErrorResponse(
                        StatusCodes.Status500InternalServerError,
                        "Failed to retrieve configuration information",
                        HttpContext.TraceIdentifier));
            }
        }

        /// <summary>
        /// 健康检查端点 (Ping)
        /// </summary>
        /// <returns>Pong 响应</returns>
        /// <response code="200">服务健康</response>
        [HttpGet("health")]
        [HttpHead("health")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult Health()
        {
            return Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
        }

        /// <summary>
        /// 简单的 Ping 端点
        /// </summary>
        /// <returns>Pong 响应</returns>
        /// <response code="200">Pong 响应</response>
        [HttpGet("ping")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult Ping()
        {
            _logger.LogDebug("Ping requested - RequestId: {RequestId}", HttpContext.TraceIdentifier);
            
            return Ok(new 
            { 
                message = "Pong",
                timestamp = DateTime.UtcNow,
                requestId = HttpContext.TraceIdentifier
            });
        }

        /// <summary>
        /// 获取服务器时间
        /// </summary>
        /// <returns>时间信息 JSON 对象</returns>
        /// <response code="200">成功返回服务器时间</response>
        [HttpGet("time")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetServerTime()
        {
            var now = DateTime.Now;
            var utcNow = DateTime.UtcNow;
            var timeZoneInfo = TimeZoneInfo.Local;

            return Ok(new 
            { 
                serverTime = now,
                utcTime = utcNow,
                timezone = timeZoneInfo.DisplayName,
                timezoneOffset = timeZoneInfo.GetUtcOffset(now).TotalHours,
                isDaylightSavingTime = timeZoneInfo.IsDaylightSavingTime(now),
                timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                requestId = HttpContext.TraceIdentifier
            });
        }

        /// <summary>
        /// 就绪检查（Readiness Probe）
        /// </summary>
        /// <returns>就绪检查结果</returns>
        /// <response code="200">服务已就绪</response>
        /// <response code="503">服务未就绪</response>
        [HttpGet("ready")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public IActionResult Ready()
        {
            _logger.LogDebug("Readiness probe requested - RequestId: {RequestId}", HttpContext.TraceIdentifier);

            // 检查必要的依赖
            var isReady = !_applicationLifetime.ApplicationStopping.IsCancellationRequested;

            if (isReady)
            {
                return Ok(new { ready = true, timestamp = DateTime.UtcNow });
            }
            else
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable, 
                    new { ready = false, timestamp = DateTime.UtcNow });
            }
        }

        /// <summary>
        /// 活跃性检查（Liveness Probe）
        /// </summary>
        /// <returns>活跃性检查结果</returns>
        /// <response code="200">服务活跃</response>
        [HttpGet("alive")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult Alive()
        {
            _logger.LogDebug("Liveness probe requested - RequestId: {RequestId}", HttpContext.TraceIdentifier);
            return Ok(new { alive = true, timestamp = DateTime.UtcNow });
        }
    }
}
