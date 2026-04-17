using HermesAgent.Examples.WebApi.Models;
using System.Net;
using System.Text.Json;

namespace HermesAgent.Examples.WebApi.Middleware
{
    /// <summary>
    /// 全局异常处理中间件
    /// </summary>
    public class GlobalExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;
        private readonly IHostEnvironment _environment;

        /// <summary>
        /// 初始化异常处理中间件
        /// </summary>
        public GlobalExceptionHandlerMiddleware(
            RequestDelegate next,
            ILogger<GlobalExceptionHandlerMiddleware> logger,
            IHostEnvironment environment)
        {
            _next = next;
            _logger = logger;
            _environment = environment;
        }

        /// <summary>
        /// 处理请求
        /// </summary>
        public async Task InvokeAsync(HttpContext context)
        {
            var requestId = context.TraceIdentifier;

            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Unhandled exception in request {RequestId}: {ExceptionMessage}",
                    requestId,
                    ex.Message);

                await HandleExceptionAsync(context, ex, requestId);
            }
        }

        /// <summary>
        /// 处理异常并返回响应
        /// </summary>
        private Task HandleExceptionAsync(HttpContext context, Exception exception, string requestId)
        {
            context.Response.ContentType = "application/json";

            var response = exception switch
            {
                ArgumentNullException => CreateErrorResponse(
                    context,
                    HttpStatusCode.BadRequest,
                    "Required parameter is missing",
                    exception,
                    requestId),

                ArgumentException => CreateErrorResponse(
                    context,
                    HttpStatusCode.BadRequest,
                    "Invalid argument",
                    exception,
                    requestId),

                InvalidOperationException => CreateErrorResponse(
                    context,
                    HttpStatusCode.BadRequest,
                    "Invalid operation",
                    exception,
                    requestId),

                HttpRequestException => CreateErrorResponse(
                    context,
                    HttpStatusCode.ServiceUnavailable,
                    "External service error",
                    exception,
                    requestId),

                TimeoutException => CreateErrorResponse(
                    context,
                    HttpStatusCode.RequestTimeout,
                    "Request timeout",
                    exception,
                    requestId),

                _ => CreateErrorResponse(
                    context,
                    HttpStatusCode.InternalServerError,
                    "Internal server error",
                    exception,
                    requestId)
            };

            context.Response.StatusCode = response.Code;

            return context.Response.WriteAsJsonAsync(response);
        }

        /// <summary>
        /// 创建错误响应
        /// </summary>
        private ApiResponse CreateErrorResponse(
            HttpContext context,
            HttpStatusCode statusCode,
            string message,
            Exception exception,
            string requestId)
        {
            var errorDetails = new ErrorDetails
            {
                ErrorCode = ((int)statusCode).ToString(),
                ErrorType = exception.GetType().Name,
                Description = exception.Message,
                StackTrace = _environment.IsDevelopment() ? exception.StackTrace : null
            };

            // 处理内部异常
            if (exception.InnerException != null)
            {
                errorDetails.InnerError = new ErrorDetails
                {
                    ErrorType = exception.InnerException.GetType().Name,
                    Description = exception.InnerException.Message,
                    StackTrace = _environment.IsDevelopment() ? exception.InnerException.StackTrace : null
                };
            }

            return new ApiResponse
            {
                Success = false,
                Code = (int)statusCode,
                Message = message,
                RequestId = requestId,
                Timestamp = DateTime.UtcNow,
                Error = errorDetails
            };
        }
    }

    /// <summary>
    /// 请求日志中间件
    /// </summary>
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        /// <summary>
        /// 初始化请求日志中间件
        /// </summary>
        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        /// <summary>
        /// 处理请求并记录日志
        /// </summary>
        public async Task InvokeAsync(HttpContext context)
        {
            var startTime = DateTime.UtcNow;
            var requestId = context.TraceIdentifier;

            _logger.LogInformation(
                "HTTP {Method} {Path} starting - RequestId: {RequestId}",
                context.Request.Method,
                context.Request.Path,
                requestId);

            try
            {
                await _next(context);
            }
            finally
            {
                var elapsed = DateTime.UtcNow - startTime;

                _logger.LogInformation(
                    "HTTP {Method} {Path} completed with {StatusCode} in {ElapsedMilliseconds}ms - RequestId: {RequestId}",
                    context.Request.Method,
                    context.Request.Path,
                    context.Response.StatusCode,
                    elapsed.TotalMilliseconds,
                    requestId);
            }
        }
    }
}
