using System.Text.Json.Serialization;

namespace HermesAgent.Examples.WebApi.Models
{
    /// <summary>
    /// 聊天响应模型
    /// </summary>
    public class ChatResponse
    {
        /// <summary>
        /// 获取或设置响应消息
        /// </summary>
        [JsonPropertyName("message")]
        public string? Message { get; set; }

        /// <summary>
        /// 获取或设置会话ID
        /// </summary>
        [JsonPropertyName("sessionId")]
        public string? SessionId { get; set; }

        /// <summary>
        /// 获取或设置时间戳
        /// </summary>
        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 获取或设置处理耗时（毫秒）
        /// </summary>
        [JsonPropertyName("elapsedMilliseconds")]
        public long? ElapsedMilliseconds { get; set; }

        /// <summary>
        /// 获取或设置令牌使用情况
        /// </summary>
        [JsonPropertyName("tokenUsage")]
        public TokenUsage? TokenUsage { get; set; }
    }

    /// <summary>
    /// 令牌使用情况
    /// </summary>
    public class TokenUsage
    {
        /// <summary>
        /// 获取或设置提示令牌数
        /// </summary>
        [JsonPropertyName("promptTokens")]
        public int PromptTokens { get; set; }

        /// <summary>
        /// 获取或设置完成令牌数
        /// </summary>
        [JsonPropertyName("completionTokens")]
        public int CompletionTokens { get; set; }

        /// <summary>
        /// 获取或设置总令牌数
        /// </summary>
        [JsonPropertyName("totalTokens")]
        public int TotalTokens { get; set; }
    }

    /// <summary>
    /// 会话创建响应
    /// </summary>
    public class SessionCreateResponse
    {
        /// <summary>
        /// 获取或设置会话ID
        /// </summary>
        [JsonPropertyName("sessionId")]
        public string? SessionId { get; set; }

        /// <summary>
        /// 获取或设置创建时间
        /// </summary>
        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 获取或设置过期时间
        /// </summary>
        [JsonPropertyName("expiresAt")]
        public DateTime? ExpiresAt { get; set; }
    }

    /// <summary>
    /// 系统信息响应
    /// </summary>
    public class SystemInfoResponse
    {
        /// <summary>
        /// 获取或设置应用名称
        /// </summary>
        [JsonPropertyName("application")]
        public string? Application { get; set; }

        /// <summary>
        /// 获取或设置版本
        /// </summary>
        [JsonPropertyName("version")]
        public string? Version { get; set; }

        /// <summary>
        /// 获取或设置环境
        /// </summary>
        [JsonPropertyName("environment")]
        public string? Environment { get; set; }

        /// <summary>
        /// 获取或设置时间戳
        /// </summary>
        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 获取或设置状态
        /// </summary>
        [JsonPropertyName("status")]
        public string? Status { get; set; }

        /// <summary>
        /// 获取或设置运行时间
        /// </summary>
        [JsonPropertyName("uptime")]
        public TimeSpan Uptime { get; set; }

        /// <summary>
        /// 获取或设置功能列表
        /// </summary>
        [JsonPropertyName("features")]
        public List<string>? Features { get; set; }

        /// <summary>
        /// 获取或设置统计信息
        /// </summary>
        [JsonPropertyName("statistics")]
        public SystemStatistics? Statistics { get; set; }
    }

    /// <summary>
    /// 系统统计信息
    /// </summary>
    public class SystemStatistics
    {
        /// <summary>
        /// 获取或设置总请求数
        /// </summary>
        [JsonPropertyName("totalRequests")]
        public long TotalRequests { get; set; }

        /// <summary>
        /// 获取或设置活跃会话数
        /// </summary>
        [JsonPropertyName("activeSessions")]
        public long ActiveSessions { get; set; }

        /// <summary>
        /// 获取或设置平均响应时间（毫秒）
        /// </summary>
        [JsonPropertyName("averageResponseTime")]
        public double AverageResponseTime { get; set; }
    }

    /// <summary>
    /// 版本信息响应
    /// </summary>
    public class VersionInfoResponse
    {
        /// <summary>
        /// 获取或设置API版本
        /// </summary>
        [JsonPropertyName("apiVersion")]
        public string? ApiVersion { get; set; }

        /// <summary>
        /// 获取或设置构建号
        /// </summary>
        [JsonPropertyName("buildNumber")]
        public string? BuildNumber { get; set; }

        /// <summary>
        /// 获取或设置构建日期
        /// </summary>
        [JsonPropertyName("buildDate")]
        public DateTime BuildDate { get; set; }

        /// <summary>
        /// 获取或设置支持的版本
        /// </summary>
        [JsonPropertyName("supportedVersions")]
        public List<string>? SupportedVersions { get; set; }

        /// <summary>
        /// 获取或设置已弃用的版本
        /// </summary>
        [JsonPropertyName("deprecatedVersions")]
        public List<string>? DeprecatedVersions { get; set; }

        /// <summary>
        /// 获取或设置生命周期结束时间
        /// </summary>
        [JsonPropertyName("endOfLife")]
        public DateTime? EndOfLife { get; set; }
    }

    /// <summary>
    /// 状态响应
    /// </summary>
    public class StatusResponse
    {
        /// <summary>
        /// 获取或设置服务名称
        /// </summary>
        [JsonPropertyName("service")]
        public string? Service { get; set; }

        /// <summary>
        /// 获取或设置服务状态
        /// </summary>
        [JsonPropertyName("status")]
        public string? Status { get; set; }

        /// <summary>
        /// 获取或设置最后更新时间
        /// </summary>
        [JsonPropertyName("lastUpdated")]
        public DateTime LastUpdated { get; set; }

        /// <summary>
        /// 获取或设置组件状态列表
        /// </summary>
        [JsonPropertyName("components")]
        public List<ComponentStatus>? Components { get; set; }

        /// <summary>
        /// 获取或设置度量指标
        /// </summary>
        [JsonPropertyName("metrics")]
        public Dictionary<string, object>? Metrics { get; set; }
    }

    /// <summary>
    /// 组件状态
    /// </summary>
    public class ComponentStatus
    {
        /// <summary>
        /// 获取或设置组件名称
        /// </summary>
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        /// <summary>
        /// 获取或设置组件状态
        /// </summary>
        [JsonPropertyName("status")]
        public string? Status { get; set; }

        /// <summary>
        /// 获取或设置组件消息
        /// </summary>
        [JsonPropertyName("message")]
        public string? Message { get; set; }
    }

    /// <summary>
    /// 配置信息响应
    /// </summary>
    public class ConfigInfoResponse
    {
        /// <summary>
        /// 获取或设置环境名称
        /// </summary>
        [JsonPropertyName("environment")]
        public string? Environment { get; set; }

        /// <summary>
        /// 获取或设置调试模式
        /// </summary>
        [JsonPropertyName("debugMode")]
        public bool DebugMode { get; set; }

        /// <summary>
        /// 获取或设置Hermes Agent配置
        /// </summary>
        [JsonPropertyName("hermesAgent")]
        public HermesAgentConfigInfo? HermesAgent { get; set; }
    }

    /// <summary>
    /// Hermes Agent配置信息
    /// </summary>
    public class HermesAgentConfigInfo
    {
        /// <summary>
        /// 获取或设置基础URL
        /// </summary>
        [JsonPropertyName("baseUrl")]
        public string? BaseUrl { get; set; }

        /// <summary>
        /// 获取或设置超时时间（秒）
        /// </summary>
        [JsonPropertyName("timeout")]
        public int Timeout { get; set; }

        /// <summary>
        /// 获取或设置重试次数
        /// </summary>
        [JsonPropertyName("retryCount")]
        public int RetryCount { get; set; }

        /// <summary>
        /// 获取或设置是否启用监控
        /// </summary>
        [JsonPropertyName("monitoringEnabled")]
        public bool MonitoringEnabled { get; set; }
    }
}
