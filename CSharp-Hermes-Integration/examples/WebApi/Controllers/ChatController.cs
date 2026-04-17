using HermesAgent.Client;
using HermesAgent.Examples.WebApi.Constants;
using HermesAgent.Examples.WebApi.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace HermesAgent.Examples.WebApi.Controllers
{
    /// <summary>
    /// 聊天控制器
    /// 提供聊天相关的API端点
    /// </summary>
    [ApiController]
    [Route("api/v1/[controller]")]
    [Produces("application/json")]
    public class ChatController : ControllerBase
    {
        private readonly IHermesAgentClient _hermesClient;
        private readonly ILogger<ChatController> _logger;

        /// <summary>
        /// 初始化聊天控制器
        /// </summary>
        public ChatController(IHermesAgentClient hermesClient, ILogger<ChatController> logger)
        {
            _hermesClient = hermesClient ?? throw new ArgumentNullException(nameof(hermesClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// 发送聊天消息
        /// </summary>
        /// <param name="request">聊天请求</param>
        /// <returns>聊天响应</returns>
        /// <response code="200">成功返回聊天响应</response>
        /// <response code="400">请求参数无效</response>
        /// <response code="503">Hermes Agent 服务不可用</response>
        [HttpPost("message")]
        [ProducesResponseType(typeof(ApiResponse<ChatResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status503ServiceUnavailable)]
        public async Task<IActionResult> SendMessage([FromBody] ChatRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            var stopwatch = Stopwatch.StartNew();
            var requestId = HttpContext.TraceIdentifier;

            try
            {
                _logger.LogInformation(
                    "Processing chat message request - RequestId: {RequestId}, MessageLength: {MessageLength}",
                    requestId,
                    request.Message.Length);

                var (sessionId, response) = await _hermesClient.ChatAsync(
                    request.Message,
                    sessionId: request.SessionId,
                    systemPrompt: request.SystemPrompt,
                    temperature: request.Temperature ?? 0.7,
                    maxTokens: request.MaxTokens ?? 2048);

                stopwatch.Stop();

                var chatResponse = new ChatResponse
                {
                    Message = response,
                    SessionId = sessionId ?? request.SessionId,
                    Timestamp = DateTime.UtcNow,
                    ElapsedMilliseconds = stopwatch.ElapsedMilliseconds
                };

                _logger.LogInformation(
                    "Chat message processed successfully - RequestId: {RequestId}, ElapsedMs: {ElapsedMs}",
                    requestId,
                    stopwatch.ElapsedMilliseconds);

                return Ok(ApiResponse<ChatResponse>.SuccessResponse(
                    data: chatResponse,
                    message: "Chat message processed successfully",
                    requestId: requestId));
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(
                    ex,
                    "Hermes Agent service error - RequestId: {RequestId}",
                    requestId);

                return StatusCode(
                    StatusCodes.Status503ServiceUnavailable,
                    ApiResponse.ErrorResponse(
                        StatusCodes.Status503ServiceUnavailable,
                        "Hermes Agent service is unavailable",
                        requestId));
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogError(
                    ex,
                    "Request timeout - RequestId: {RequestId}",
                    requestId);

                return StatusCode(
                    StatusCodes.Status504GatewayTimeout,
                    ApiResponse.ErrorResponse(
                        StatusCodes.Status504GatewayTimeout,
                        "Request timeout",
                        requestId));
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Unexpected error in chat message processing - RequestId: {RequestId}",
                    requestId);

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse.ErrorResponse(
                        StatusCodes.Status500InternalServerError,
                        "An unexpected error occurred",
                        requestId));
            }
        }

        /// <summary>
        /// 流式聊天
        /// </summary>
        /// <param name="request">聊天请求</param>
        /// <returns>流式聊天响应</returns>
        /// <response code="200">成功开启流式聊天</response>
        /// <response code="400">请求参数无效</response>
        /// <response code="503">Hermes Agent 服务不可用</response>
        [HttpPost("stream")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status503ServiceUnavailable)]
        public async Task StreamMessage([FromBody] ChatRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            Response.ContentType = ApiConstants.ContentTypes.EventStream;
            Response.Headers.Add("Cache-Control", "no-cache");
            Response.Headers.Add("Connection", "keep-alive");

            var requestId = HttpContext.TraceIdentifier;

            _logger.LogInformation(
                "Starting streaming chat - RequestId: {RequestId}, MessageLength: {MessageLength}",
                requestId,
                request.Message.Length);

            try
            {
                var streamCount = 0;
                await foreach (var chunk in _hermesClient.ChatStreamAsync(
                    request.Message,
                    sessionId: request.SessionId,
                    systemPrompt: request.SystemPrompt,
                    temperature: request.Temperature ?? 0.7,
                    maxTokens: request.MaxTokens ?? 2048))
                {
                    var eventData = new
                    {
                        chunk,
                        timestamp = DateTime.UtcNow,
                        requestId = requestId
                    };

                    await Response.WriteAsync(
                        $"data: {System.Text.Json.JsonSerializer.Serialize(eventData)}\n\n");
                    await Response.Body.FlushAsync();

                    streamCount++;
                }

                // 发送完成信号
                await Response.WriteAsync($"data: [DONE]\n\n");
                await Response.WriteAsync($"data: {{\"requestId\": \"{requestId}\", \"chunkCount\": {streamCount}}}\n\n");

                _logger.LogInformation(
                    "Streaming chat completed successfully - RequestId: {RequestId}, ChunkCount: {ChunkCount}",
                    requestId,
                    streamCount);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(
                    ex,
                    "Hermes Agent service error in streaming - RequestId: {RequestId}",
                    requestId);

                await Response.WriteAsync(
                    $"data: {{\"error\": \"Hermes Agent service is unavailable\", \"requestId\": \"{requestId}\"}}\n\n");
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Unexpected error in streaming chat - RequestId: {RequestId}",
                    requestId);

                await Response.WriteAsync(
                    $"data: {{\"error\": \"An unexpected error occurred\", \"requestId\": \"{requestId}\"}}\n\n");
            }
        }

        /// <summary>
        /// 创建新会话
        /// </summary>
        /// <param name="request">创建会话请求</param>
        /// <returns>会话创建响应</returns>
        /// <response code="201">会话创建成功</response>
        /// <response code="503">Hermes Agent 服务不可用</response>
        [HttpPost("session")]
        [ProducesResponseType(typeof(ApiResponse<SessionCreateResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status503ServiceUnavailable)]
        public async Task<IActionResult> CreateSession([FromBody] CreateSessionRequest? request = null)
        {
            var requestId = HttpContext.TraceIdentifier;

            try
            {
                _logger.LogInformation(
                    "Creating new session - RequestId: {RequestId}",
                    requestId);

                var sessionId = await _hermesClient.CreateSessionAsync(
                    initialMessage: request?.InitialMessage);

                var response = new SessionCreateResponse
                {
                    SessionId = sessionId,
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddHours(1) // 假设会话有效期为1小时
                };

                _logger.LogInformation(
                    "Session created successfully - RequestId: {RequestId}, SessionId: {SessionId}",
                    requestId,
                    sessionId);

                return Created(
                    $"/api/v1/chat/session/{sessionId}",
                    ApiResponse<SessionCreateResponse>.SuccessResponse(
                        data: response,
                        message: "Session created successfully",
                        requestId: requestId));
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(
                    ex,
                    "Hermes Agent service error - RequestId: {RequestId}",
                    requestId);

                return StatusCode(
                    StatusCodes.Status503ServiceUnavailable,
                    ApiResponse.ErrorResponse(
                        StatusCodes.Status503ServiceUnavailable,
                        "Hermes Agent service is unavailable",
                        requestId));
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Unexpected error in session creation - RequestId: {RequestId}",
                    requestId);

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse.ErrorResponse(
                        StatusCodes.Status500InternalServerError,
                        "An unexpected error occurred",
                        requestId));
            }
        }

        /// <summary>
        /// 批量聊天
        /// </summary>
        /// <param name="request">批量聊天请求</param>
        /// <returns>批量聊天响应</returns>
        /// <response code="200">批量聊天成功</response>
        /// <response code="400">请求参数无效</response>
        /// <response code="503">Hermes Agent 服务不可用</response>
        [HttpPost("batch")]
        [ProducesResponseType(typeof(ApiResponse<List<ChatResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status503ServiceUnavailable)]
        public async Task<IActionResult> BatchChat([FromBody] BatchChatRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            var requestId = HttpContext.TraceIdentifier;

            try
            {
                _logger.LogInformation(
                    "Processing batch chat request - RequestId: {RequestId}, MessageCount: {MessageCount}, Parallel: {Parallel}",
                    requestId,
                    request.Messages.Count,
                    request.Parallel);

                var stopwatch = Stopwatch.StartNew();
                var responses = new List<ChatResponse>();

                if (request.Parallel)
                {
                    var tasks = request.Messages.Select(async msg => 
                    {
                        var (sessionId, content) = await _hermesClient.ChatAsync(msg, sessionId: request.SessionId);
                        return (content, sessionId, msg);
                    });
                    var results = await Task.WhenAll(tasks);
                    responses = results.Select(r => new ChatResponse
                    {
                        Message = r.content,
                        SessionId = r.sessionId,
                        Timestamp = DateTime.UtcNow,
                        ElapsedMilliseconds = stopwatch.ElapsedMilliseconds
                    }).ToList();
                }
                else
                {
                    foreach (var message in request.Messages)
                    {
                        var (sessionId, response) = await _hermesClient.ChatAsync(message, sessionId: request.SessionId);
                        responses.Add(new ChatResponse
                        {
                            Message = response,
                            SessionId = sessionId,
                            Timestamp = DateTime.UtcNow,
                            ElapsedMilliseconds = stopwatch.ElapsedMilliseconds
                        });
                    }
                }

                stopwatch.Stop();

                _logger.LogInformation(
                    "Batch chat processed successfully - RequestId: {RequestId}, MessageCount: {MessageCount}, ElapsedMs: {ElapsedMs}",
                    requestId,
                    request.Messages.Count,
                    stopwatch.ElapsedMilliseconds);

                return Ok(ApiResponse<List<ChatResponse>>.SuccessResponse(
                    data: responses,
                    message: $"Batch chat processed successfully ({responses.Count} messages)",
                    requestId: requestId));
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(
                    ex,
                    "Hermes Agent service error - RequestId: {RequestId}",
                    requestId);

                return StatusCode(
                    StatusCodes.Status503ServiceUnavailable,
                    ApiResponse.ErrorResponse(
                        StatusCodes.Status503ServiceUnavailable,
                        "Hermes Agent service is unavailable",
                        requestId));
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Unexpected error in batch chat - RequestId: {RequestId}",
                    requestId);

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse.ErrorResponse(
                        StatusCodes.Status500InternalServerError,
                        "An unexpected error occurred",
                        requestId));
            }
        }
    }
}
