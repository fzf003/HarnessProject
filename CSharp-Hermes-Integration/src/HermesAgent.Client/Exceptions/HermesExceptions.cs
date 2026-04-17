using System;
using System.Runtime.Serialization;

namespace HermesAgent.Client.Exceptions
{
    /// <summary>
    /// Hermes Agent 异常基类
    /// </summary>
    [Serializable]
    public class HermesAgentException : Exception
    {
        /// <summary>
        /// 错误代码
        /// </summary>
        public string ErrorCode { get; }

        /// <summary>
        /// 初始化 Hermes Agent 异常
        /// </summary>
        public HermesAgentException()
            : base("Hermes Agent 异常")
        {
            ErrorCode = "UNKNOWN_ERROR";
        }

        /// <summary>
        /// 初始化 Hermes Agent 异常
        /// </summary>
        /// <param name="message">错误消息</param>
        public HermesAgentException(string message)
            : base(message)
        {
            ErrorCode = "UNKNOWN_ERROR";
        }

        /// <summary>
        /// 初始化 Hermes Agent 异常
        /// </summary>
        /// <param name="message">错误消息</param>
        /// <param name="innerException">内部异常</param>
        public HermesAgentException(string message, Exception innerException)
            : base(message, innerException)
        {
            ErrorCode = "UNKNOWN_ERROR";
        }

        /// <summary>
        /// 初始化 Hermes Agent 异常
        /// </summary>
        /// <param name="errorCode">错误代码</param>
        /// <param name="message">错误消息</param>
        public HermesAgentException(string errorCode, string message)
            : base(message)
        {
            ErrorCode = errorCode;
        }

        /// <summary>
        /// 初始化 Hermes Agent 异常
        /// </summary>
        /// <param name="errorCode">错误代码</param>
        /// <param name="message">错误消息</param>
        /// <param name="innerException">内部异常</param>
        public HermesAgentException(string errorCode, string message, Exception innerException)
            : base(message, innerException)
        {
            ErrorCode = errorCode;
        }

        /// <summary>
        /// 序列化构造函数
        /// </summary>
        protected HermesAgentException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            ErrorCode = info.GetString(nameof(ErrorCode)) ?? "UNKNOWN_ERROR";
        }

        /// <summary>
        /// 获取对象数据
        /// </summary>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(nameof(ErrorCode), ErrorCode);
        }
    }

    /// <summary>
    /// Hermes API 异常
    /// </summary>
    [Serializable]
    public class HermesApiException : HermesAgentException
    {
        /// <summary>
        /// HTTP 状态码
        /// </summary>
        public int StatusCode { get; }

        /// <summary>
        /// 响应内容
        /// </summary>
        public string? ResponseContent { get; }

        /// <summary>
        /// 初始化 Hermes API 异常
        /// </summary>
        /// <param name="statusCode">HTTP 状态码</param>
        /// <param name="message">错误消息</param>
        /// <param name="responseContent">响应内容</param>
        public HermesApiException(int statusCode, string message, string? responseContent = null)
            : base($"API_ERROR_{statusCode}", $"Hermes API 错误 ({statusCode}): {message}")
        {
            StatusCode = statusCode;
            ResponseContent = responseContent;
        }

        /// <summary>
        /// 序列化构造函数
        /// </summary>
        protected HermesApiException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            StatusCode = info.GetInt32(nameof(StatusCode));
            ResponseContent = info.GetString(nameof(ResponseContent));
        }

        /// <summary>
        /// 获取对象数据
        /// </summary>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(nameof(StatusCode), StatusCode);
            info.AddValue(nameof(ResponseContent), ResponseContent);
        }
    }

    /// <summary>
    /// Hermes 进程异常
    /// </summary>
    [Serializable]
    public class HermesProcessException : HermesAgentException
    {
        /// <summary>
        /// 退出代码
        /// </summary>
        public int ExitCode { get; }

        /// <summary>
        /// 标准错误输出
        /// </summary>
        public string ErrorOutput { get; }

        /// <summary>
        /// 初始化 Hermes 进程异常
        /// </summary>
        /// <param name="exitCode">退出代码</param>
        /// <param name="errorOutput">错误输出</param>
        /// <param name="message">错误消息</param>
        public HermesProcessException(int exitCode, string errorOutput, string message)
            : base("PROCESS_ERROR", $"Hermes 进程错误 (退出代码: {exitCode}): {message}")
        {
            ExitCode = exitCode;
            ErrorOutput = errorOutput;
        }

        /// <summary>
        /// 序列化构造函数
        /// </summary>
        protected HermesProcessException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            ExitCode = info.GetInt32(nameof(ExitCode));
            ErrorOutput = info.GetString(nameof(ErrorOutput)) ?? string.Empty;
        }

        /// <summary>
        /// 获取对象数据
        /// </summary>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(nameof(ExitCode), ExitCode);
            info.AddValue(nameof(ErrorOutput), ErrorOutput);
        }
    }

    /// <summary>
    /// Hermes 配置异常
    /// </summary>
    [Serializable]
    public class HermesConfigurationException : HermesAgentException
    {
        /// <summary>
        /// 配置项名称
        /// </summary>
        public string ConfigurationKey { get; }

        /// <summary>
        /// 初始化 Hermes 配置异常
        /// </summary>
        /// <param name="configurationKey">配置项名称</param>
        /// <param name="message">错误消息</param>
        public HermesConfigurationException(string configurationKey, string message)
            : base("CONFIGURATION_ERROR", $"Hermes 配置错误 ({configurationKey}): {message}")
        {
            ConfigurationKey = configurationKey;
        }

        /// <summary>
        /// 序列化构造函数
        /// </summary>
        protected HermesConfigurationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            ConfigurationKey = info.GetString(nameof(ConfigurationKey)) ?? string.Empty;
        }

        /// <summary>
        /// 获取对象数据
        /// </summary>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(nameof(ConfigurationKey), ConfigurationKey);
        }
    }

    /// <summary>
    /// Hermes 验证异常
    /// </summary>
    [Serializable]
    public class HermesValidationException : HermesAgentException
    {
        /// <summary>
        /// 验证错误
        /// </summary>
        public IReadOnlyList<string> ValidationErrors { get; }

        /// <summary>
        /// 初始化 Hermes 验证异常
        /// </summary>
        /// <param name="validationErrors">验证错误列表</param>
        public HermesValidationException(IEnumerable<string> validationErrors)
            : base("VALIDATION_ERROR", "Hermes 验证错误: " + string.Join("; ", validationErrors))
        {
            ValidationErrors = validationErrors.ToList().AsReadOnly();
        }

        /// <summary>
        /// 序列化构造函数
        /// </summary>
        protected HermesValidationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            var errors = info.GetValue(nameof(ValidationErrors), typeof(List<string>)) as List<string>;
            ValidationErrors = errors?.AsReadOnly() ?? new List<string>().AsReadOnly();
        }

        /// <summary>
        /// 获取对象数据
        /// </summary>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(nameof(ValidationErrors), ValidationErrors.ToList());
        }
    }

    /// <summary>
    /// Hermes 超时异常
    /// </summary>
    [Serializable]
    public class HermesTimeoutException : HermesAgentException
    {
        /// <summary>
        /// 超时时间
        /// </summary>
        public TimeSpan Timeout { get; }

        /// <summary>
        /// 初始化 Hermes 超时异常
        /// </summary>
        /// <param name="timeout">超时时间</param>
        /// <param name="operation">操作名称</param>
        public HermesTimeoutException(TimeSpan timeout, string operation)
            : base("TIMEOUT_ERROR", $"Hermes 操作超时 ({operation}): 超过 {timeout.TotalSeconds} 秒")
        {
            Timeout = timeout;
        }

        /// <summary>
        /// 序列化构造函数
        /// </summary>
        protected HermesTimeoutException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            Timeout = (TimeSpan)info.GetValue(nameof(Timeout), typeof(TimeSpan))!;
        }

        /// <summary>
        /// 获取对象数据
        /// </summary>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(nameof(Timeout), Timeout);
        }
    }
}