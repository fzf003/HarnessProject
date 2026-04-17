using System;
using System.Text.Json.Serialization;

namespace HermesAgent.Client.Models
{
    /// <summary>
    /// 聊天请求
    /// </summary>
    public class ChatRequest
    {
        /// <summary>
        /// 消息内容
        /// </summary>
        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// 会话ID
        /// </summary>
        [JsonPropertyName("session_id")]
        public string? SessionId { get; set; }

        /// <summary>
        /// 系统提示
        /// </summary>
        [JsonPropertyName("system_prompt")]
        public string? SystemPrompt { get; set; }

        /// <summary>
        /// 温度参数
        /// </summary>
        [JsonPropertyName("temperature")]
        public double? Temperature { get; set; }

        /// <summary>
        /// 最大token数
        /// </summary>
        [JsonPropertyName("max_tokens")]
        public int? MaxTokens { get; set; }

        /// <summary>
        /// 流式响应
        /// </summary>
        [JsonPropertyName("stream")]
        public bool Stream { get; set; }
    }

    /// <summary>
    /// 聊天响应
    /// </summary>
    public class ChatResponse
    {
        /// <summary>
        /// 响应内容
        /// </summary>
        [JsonPropertyName("content")]
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// 会话ID
        /// </summary>
        [JsonPropertyName("session_id")]
        public string SessionId { get; set; } = string.Empty;

        /// <summary>
        /// 模型名称
        /// </summary>
        [JsonPropertyName("model")]
        public string Model { get; set; } = string.Empty;

        /// <summary>
        /// 使用token数
        /// </summary>
        [JsonPropertyName("usage")]
        public TokenUsage? Usage { get; set; }

        /// <summary>
        /// 完成原因
        /// </summary>
        [JsonPropertyName("finish_reason")]
        public string FinishReason { get; set; } = string.Empty;

        /// <summary>
        /// 时间戳
        /// </summary>
        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Token 使用情况
    /// </summary>
    public class TokenUsage
    {
        /// <summary>
        /// 提示token数
        /// </summary>
        [JsonPropertyName("prompt_tokens")]
        public int PromptTokens { get; set; }

        /// <summary>
        /// 完成token数
        /// </summary>
        [JsonPropertyName("completion_tokens")]
        public int CompletionTokens { get; set; }

        /// <summary>
        /// 总token数
        /// </summary>
        [JsonPropertyName("total_tokens")]
        public int TotalTokens { get; set; }
    }

    /// <summary>
    /// Webhook 事件
    /// </summary>
    public class WebhookEvent
    {
        /// <summary>
        /// 事件类型
        /// </summary>
        [JsonPropertyName("event")]
        public string Event { get; set; } = string.Empty;

        /// <summary>
        /// 事件数据
        /// </summary>
        [JsonPropertyName("data")]
        public object Data { get; set; } = new();

        /// <summary>
        /// 时间戳
        /// </summary>
        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 签名
        /// </summary>
        [JsonPropertyName("signature")]
        public string? Signature { get; set; }
    }

    /// <summary>
    /// 模型信息
    /// </summary>
    public class ModelInfo
    {
        /// <summary>
        /// 模型ID
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// 模型名称
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 提供商
        /// </summary>
        [JsonPropertyName("provider")]
        public string Provider { get; set; } = string.Empty;

        /// <summary>
        /// 上下文长度
        /// </summary>
        [JsonPropertyName("context_length")]
        public int ContextLength { get; set; }

        /// <summary>
        /// 是否支持流式
        /// </summary>
        [JsonPropertyName("supports_streaming")]
        public bool SupportsStreaming { get; set; }
    }

    /// <summary>
    /// 健康检查响应
    /// </summary>
    public class HealthCheckResponse
    {
        /// <summary>
        /// 状态
        /// </summary>
        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// 版本
        /// </summary>
        [JsonPropertyName("version")]
        public string Version { get; set; } = string.Empty;

        /// <summary>
        /// 时间戳
        /// </summary>
        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 运行时间
        /// </summary>
        [JsonPropertyName("uptime")]
        public TimeSpan Uptime { get; set; }
    }
}