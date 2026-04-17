using System.ComponentModel.DataAnnotations;

namespace HermesAgent.Examples.WebApi.Models
{
    /// <summary>
    /// 聊天请求模型
    /// </summary>
    public class ChatRequest
    {
        /// <summary>
        /// 获取或设置聊天消息
        /// </summary>
        [Required(ErrorMessage = "Message is required")]
        [StringLength(10000, MinimumLength = 1, ErrorMessage = "Message must be between 1 and 10000 characters")]
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// 获取或设置会话ID
        /// </summary>
        [StringLength(256, ErrorMessage = "SessionId must not exceed 256 characters")]
        public string? SessionId { get; set; }

        /// <summary>
        /// 获取或设置系统提示词
        /// </summary>
        [StringLength(5000, ErrorMessage = "SystemPrompt must not exceed 5000 characters")]
        public string? SystemPrompt { get; set; }

        /// <summary>
        /// 获取或设置温度参数（0-1）
        /// </summary>
        [Range(0, 1, ErrorMessage = "Temperature must be between 0 and 1")]
        public double? Temperature { get; set; } = 0.7;

        /// <summary>
        /// 获取或设置最大令牌数
        /// </summary>
        [Range(1, 4096, ErrorMessage = "MaxTokens must be between 1 and 4096")]
        public int? MaxTokens { get; set; } = 2048;
    }

    /// <summary>
    /// 创建会话请求模型
    /// </summary>
    public class CreateSessionRequest
    {
        /// <summary>
        /// 获取或设置初始消息
        /// </summary>
        [StringLength(5000, ErrorMessage = "InitialMessage must not exceed 5000 characters")]
        public string? InitialMessage { get; set; }

        /// <summary>
        /// 获取或设置会话元数据
        /// </summary>
        public Dictionary<string, string>? Metadata { get; set; }
    }

    /// <summary>
    /// 批量聊天请求模型
    /// </summary>
    public class BatchChatRequest
    {
        /// <summary>
        /// 获取或设置消息列表
        /// </summary>
        [Required(ErrorMessage = "Messages is required")]
        [MinLength(1, ErrorMessage = "At least one message is required")]
        public List<string> Messages { get; set; } = new();

        /// <summary>
        /// 获取或设置会话ID
        /// </summary>
        [StringLength(256, ErrorMessage = "SessionId must not exceed 256 characters")]
        public string? SessionId { get; set; }

        /// <summary>
        /// 获取或设置是否并行处理
        /// </summary>
        public bool Parallel { get; set; } = false;
    }
}
