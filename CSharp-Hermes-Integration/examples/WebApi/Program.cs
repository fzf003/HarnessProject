using HermesAgent.Client;
using Microsoft.AspNetCore.Mvc;

namespace HermesAgent.Examples.WebApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            
            // 添加 Hermes Agent 客户端
            builder.Services.AddHermesAgentClient(builder.Configuration);
            builder.Services.AddHermesAgentMonitoring();
            builder.Services.AddHermesAgentLogging();
            
            // 添加控制器
            builder.Services.AddControllers();
            
            // 添加 Swagger
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new() { Title = "Hermes Agent API", Version = "v1" });
            });
            
            var app = builder.Build();
            
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            
            //app.UseHttpsRedirection();
            //app.UseAuthorization();
            app.MapControllers();
            
            app.Run();
        }
    }
    
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly IHermesAgentClient _hermesClient;
        private readonly ILogger<ChatController> _logger;
        
        public ChatController(IHermesAgentClient hermesClient, ILogger<ChatController> logger)
        {
            _hermesClient = hermesClient;
            _logger = logger;
        }
        
        /// <summary>
        /// 发送聊天消息
        /// </summary>
        [HttpPost("message")]
        public async Task<IActionResult> SendMessage([FromBody] ChatRequest request)
        {
            _logger.LogInformation("收到聊天请求: {Message}", request.Message);
            
            var response = await _hermesClient.ChatAsync(
                request.Message, 
                request.SessionId,
                request.SystemPrompt,
                request.Temperature.ToString(),
                request.MaxTokens);
            
            return Ok(new ChatResponse
            {
                Message = response.Content,
                SessionId = request.SessionId,
                Timestamp = DateTime.UtcNow
            });
        }
        
        /// <summary>
        /// 流式聊天
        /// </summary>
        [HttpPost("stream")]
        public async Task StreamMessage([FromBody] ChatRequest request)
        {
            Response.ContentType = "text/event-stream";
            
            _logger.LogInformation("开始流式聊天: {Message}", request.Message);
            
            await foreach (var chunk in _hermesClient.ChatStreamAsync(
                request.Message,
                request.SessionId,
                request.SystemPrompt,
                request.Temperature.ToString(),
                request.MaxTokens))
            {
                await Response.WriteAsync($"data: {System.Text.Json.JsonSerializer.Serialize(new { chunk })}\n\n");
                await Response.Body.FlushAsync();
            }
            
            await Response.WriteAsync("data: [DONE]\n\n");
            await Response.Body.FlushAsync();
        }
        
        /// <summary>
        /// 创建新会话
        /// </summary>
        [HttpPost("session")]
        public async Task<IActionResult> CreateSession([FromBody] CreateSessionRequest request)
        {
            var sessionId = await _hermesClient.CreateSessionAsync(
                request.InitialMessage,
                request.SystemPrompt);
            
            return Ok(new
            {
                SessionId = sessionId,
                Message = "会话创建成功",
                Timestamp = DateTime.UtcNow
            });
        }
        
        /// <summary>
        /// 获取模型列表
        /// </summary>
        [HttpGet("models")]
        public async Task<IActionResult> GetModels()
        {
            var models = await _hermesClient.GetModelsAsync();
            return Ok(models);
        }
        
        /// <summary>
        /// 健康检查
        /// </summary>
        [HttpGet("health")]
        public async Task<IActionResult> HealthCheck()
        {
            var isHealthy = await _hermesClient.HealthCheckAsync();
            return Ok(new
            {
                Status = isHealthy ? "healthy" : "unhealthy",
                Timestamp = DateTime.UtcNow
            });
        }
    }
    
    public class ChatRequest
    {
        public string Message { get; set; }
        public string SessionId { get; set; }
        public string SystemPrompt { get; set; }
        public double? Temperature { get; set; } = 0.7;
        public int? MaxTokens { get; set; }
    }
    
    public class ChatResponse
    {
        public string Message { get; set; }
        public string SessionId { get; set; }
        public DateTime Timestamp { get; set; }
    }
    
    public class CreateSessionRequest
    {
        public string InitialMessage { get; set; }
        public string SystemPrompt { get; set; }
    }
}