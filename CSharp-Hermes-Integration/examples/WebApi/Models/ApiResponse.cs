namespace HermesAgent.Examples.WebApi.Models
{
    /// <summary>
    /// 统一的 API 响应包装类
    /// </summary>
    /// <typeparam name="T">响应数据类型</typeparam>
    public class ApiResponse<T>
    {
        /// <summary>
        /// 获取或设置响应是否成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 获取或设置HTTP状态码
        /// </summary>
        public int Code { get; set; }

        /// <summary>
        /// 获取或设置响应消息
        /// </summary>
        public string? Message { get; set; }

        /// <summary>
        /// 获取或设置响应数据
        /// </summary>
        public T? Data { get; set; }

        /// <summary>
        /// 获取或设置请求ID
        /// </summary>
        public string? RequestId { get; set; }

        /// <summary>
        /// 获取或设置响应时间戳
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 获取或设置错误详情
        /// </summary>
        public ErrorDetails? Error { get; set; }

        /// <summary>
        /// 创建成功响应
        /// </summary>
        public static ApiResponse<T> SuccessResponse(T? data, string? message = null, string? requestId = null)
        {
            return new ApiResponse<T>
            {
                Success = true,
                Code = 200,
                Message = message ?? "Request successful",
                Data = data,
                RequestId = requestId,
                Timestamp = DateTime.UtcNow
            };
        }

        /// <summary>
        /// 创建失败响应
        /// </summary>
        public static ApiResponse<T> ErrorResponse(int code, string message, string? requestId = null, ErrorDetails? errorDetails = null, T? data = default)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Code = code,
                Message = message,
                Data = data,
                RequestId = requestId,
                Timestamp = DateTime.UtcNow,
                Error = errorDetails
            };
        }
    }

    /// <summary>
    /// 非泛型的 API 响应包装类
    /// </summary>
    public class ApiResponse
    {
        /// <summary>
        /// 获取或设置响应是否成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 获取或设置HTTP状态码
        /// </summary>
        public int Code { get; set; }

        /// <summary>
        /// 获取或设置响应消息
        /// </summary>
        public string? Message { get; set; }

        /// <summary>
        /// 获取或设置请求ID
        /// </summary>
        public string? RequestId { get; set; }

        /// <summary>
        /// 获取或设置响应时间戳
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 获取或设置错误详情
        /// </summary>
        public ErrorDetails? Error { get; set; }

        /// <summary>
        /// 创建成功响应
        /// </summary>
        public static ApiResponse SuccessResponse(string? message = null, string? requestId = null)
        {
            return new ApiResponse
            {
                Success = true,
                Code = 200,
                Message = message ?? "Request successful",
                RequestId = requestId,
                Timestamp = DateTime.UtcNow
            };
        }

        /// <summary>
        /// 创建失败响应
        /// </summary>
        public static ApiResponse ErrorResponse(int code, string message, string? requestId = null, ErrorDetails? errorDetails = null)
        {
            return new ApiResponse
            {
                Success = false,
                Code = code,
                Message = message,
                RequestId = requestId,
                Timestamp = DateTime.UtcNow,
                Error = errorDetails
            };
        }
    }

    /// <summary>
    /// 错误详情
    /// </summary>
    public class ErrorDetails
    {
        /// <summary>
        /// 获取或设置错误代码
        /// </summary>
        public string? ErrorCode { get; set; }

        /// <summary>
        /// 获取或设置错误类型
        /// </summary>
        public string? ErrorType { get; set; }

        /// <summary>
        /// 获取或设置错误描述
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 获取或设置堆栈跟踪（仅在开发环境）
        /// </summary>
        public string? StackTrace { get; set; }

        /// <summary>
        /// 获取或设置内部错误
        /// </summary>
        public ErrorDetails? InnerError { get; set; }

        /// <summary>
        /// 获取或设置验证错误集合
        /// </summary>
        public Dictionary<string, string[]>? ValidationErrors { get; set; }
    }
}
