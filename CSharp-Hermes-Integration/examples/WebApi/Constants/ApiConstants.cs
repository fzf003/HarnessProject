namespace HermesAgent.Examples.WebApi.Constants
{
    /// <summary>
    /// API 常数定义
    /// </summary>
    public static class ApiConstants
    {
        /// <summary>
        /// API 版本
        /// </summary>
        public const string ApiVersion = "v1";

        /// <summary>
        /// 应用名称
        /// </summary>
        public const string ApplicationName = "Hermes Agent Web API";

        /// <summary>
        /// 应用版本
        /// </summary>
        public const string ApplicationVersion = "1.0.0";

        /// <summary>
        /// HTTP 请求头常数
        /// </summary>
        public static class Headers
        {
            /// <summary>
            /// 请求ID Header名称
            /// </summary>
            public const string RequestId = "X-Request-ID";

            /// <summary>
            /// 追踪ID Header名称
            /// </summary>
            public const string TraceId = "X-Trace-ID";

            /// <summary>
            /// 相关ID Header名称
            /// </summary>
            public const string CorrelationId = "X-Correlation-ID";

            /// <summary>
            /// 用户代理Header名称
            /// </summary>
            public const string UserAgent = "User-Agent";
        }

        /// <summary>
        /// HTTP 状态码相关常数
        /// </summary>
        public static class StatusCodes
        {
            /// <summary>
            /// 成功 (200)
            /// </summary>
            public const int Ok = 200;

            /// <summary>
            /// 创建成功 (201)
            /// </summary>
            public const int Created = 201;

            /// <summary>
            /// 无内容 (204)
            /// </summary>
            public const int NoContent = 204;

            /// <summary>
            /// 错误的请求 (400)
            /// </summary>
            public const int BadRequest = 400;

            /// <summary>
            /// 未授权 (401)
            /// </summary>
            public const int Unauthorized = 401;

            /// <summary>
            /// 禁止访问 (403)
            /// </summary>
            public const int Forbidden = 403;

            /// <summary>
            /// 未找到 (404)
            /// </summary>
            public const int NotFound = 404;

            /// <summary>
            /// 冲突 (409)
            /// </summary>
            public const int Conflict = 409;

            /// <summary>
            /// 内部服务器错误 (500)
            /// </summary>
            public const int InternalServerError = 500;

            /// <summary>
            /// 服务不可用 (503)
            /// </summary>
            public const int ServiceUnavailable = 503;
        }

        /// <summary>
        /// 内容类型常数
        /// </summary>
        public static class ContentTypes
        {
            /// <summary>
            /// JSON 内容类型
            /// </summary>
            public const string Json = "application/json";

            /// <summary>
            /// 事件流内容类型
            /// </summary>
            public const string EventStream = "text/event-stream";

            /// <summary>
            /// 纯文本内容类型
            /// </summary>
            public const string PlainText = "text/plain";
        }

        /// <summary>
        /// 日志相关常数
        /// </summary>
        public static class Logging
        {
            /// <summary>
            /// 请求开始日志消息
            /// </summary>
            public const string RequestStarting = "HTTP {Method} {Path} starting";

            /// <summary>
            /// 请求完成日志消息
            /// </summary>
            public const string RequestCompleted = "HTTP {Method} {Path} completed with {StatusCode} in {ElapsedMilliseconds}ms";

            /// <summary>
            /// 异常发生日志消息
            /// </summary>
            public const string ExceptionOccurred = "An unhandled exception occurred during request processing";
        }

        /// <summary>
        /// 超时常数（秒）
        /// </summary>
        public static class Timeouts
        {
            /// <summary>
            /// HTTP 请求默认超时时间
            /// </summary>
            public const int DefaultHttpTimeoutSeconds = 30;

            /// <summary>
            /// 流式响应超时时间
            /// </summary>
            public const int StreamingTimeoutSeconds = 300;
        }

        /// <summary>
        /// 验证相关常数
        /// </summary>
        public static class Validation
        {
            /// <summary>
            /// 最大消息长度
            /// </summary>
            public const int MaxMessageLength = 10000;

            /// <summary>
            /// 最小消息长度
            /// </summary>
            public const int MinMessageLength = 1;

            /// <summary>
            /// 最大会话ID长度
            /// </summary>
            public const int MaxSessionIdLength = 256;
        }
    }
}
