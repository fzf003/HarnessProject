# HTTP API 集成指南

## 概述

HTTP API 集成是 C# 与 Hermes Agent 集成的主要方式，通过 OpenAI-compatible REST API 进行通信。

## API 端点

### 基础信息
- **地址**: `http://localhost:8642` (默认)
- **认证**: Bearer Token
- **格式**: JSON

### 主要端点

#### 1. 聊天完成
```
POST /v1/chat/completions
Content-Type: application/json
Authorization: Bearer <api-key>

{
  "model": "hermes-agent",
  "messages": [
    {"role": "system", "content": "你是一个有帮助的助手"},
    {"role": "user", "content": "你好"}
  ],
  "stream": false,
  "temperature": 0.7
}
```

#### 2. 获取模型列表
```
GET /v1/models
Authorization: Bearer <api-key>
```

#### 3. 健康检查
```
GET /health
```

## C# 客户端使用

### 基本用法

```csharp
// 创建客户端
var client = new HermesHttpClient(
    baseUrl: "http://localhost:8642",
    apiKey: "your-secret-key");

// 发送消息
var response = await client.ChatAsync("你好，Hermes！");
Console.WriteLine(response);

// 流式响应
await foreach (var chunk in client.ChatStreamAsync("写一个C#程序"))
{
    Console.Write(chunk);
}
```

### 会话管理

```csharp
// 创建会话（保持上下文）
var sessionId = await client.CreateSessionAsync(
    initialMessage: "我叫张三",
    systemPrompt: "你是一个技术专家");

// 在会话中继续对话
var response1 = await client.ChatAsync("我的名字是什么？", sessionId);
var response2 = await client.ChatAsync("用我的名字写个问候语", sessionId);
```

### 高级参数

```csharp
// 带参数的对话
var response = await client.ChatAsync(
    message: "解释依赖注入",
    systemPrompt: "你是一个.NET架构师",
    temperature: 0.3,      // 更确定的输出
    maxTokens: 500);       // 限制响应长度
```

## 配置 Hermes Agent

### 1. 安装和配置

```bash
# 安装 Hermes Agent
curl -fsSL https://raw.githubusercontent.com/NousResearch/hermes-agent/main/scripts/install.sh | bash

# 启用 API Server
hermes config set platforms.api_server.enabled true
hermes config set platforms.api_server.port 8642
hermes config set platforms.api_server.host 0.0.0.0
hermes config set platforms.api_server.key "your-secret-key"

# 启动服务
hermes gateway run
```

### 2. 配置文件

`~/.hermes/config.yaml`:
```yaml
platforms:
  api_server:
    enabled: true
    port: 8642
    host: "0.0.0.0"
    key: "your-secret-key"
    max_concurrent: 5
```

## ASP.NET Core 集成

### 1. 添加服务

```csharp
// Program.cs
builder.Services.AddHermesAgentClient(builder.Configuration);
builder.Services.AddHermesAgentMonitoring();
builder.Services.AddHermesAgentLogging();
```

### 2. 配置 appsettings.json

```json
{
  "HermesAgent": {
    "BaseUrl": "http://localhost:8642",
    "ApiKey": "your-secret-key",
    "Timeout": 30,
    "MaxRetries": 3
  }
}
```

### 3. 使用控制器

```csharp
[ApiController]
[Route("api/chat")]
public class ChatController : ControllerBase
{
    private readonly IHermesAgentClient _client;
    
    public ChatController(IHermesAgentClient client)
    {
        _client = client;
    }
    
    [HttpPost]
    public async Task<IActionResult> Chat([FromBody] ChatRequest request)
    {
        var response = await _client.ChatAsync(request.Message, request.SessionId);
        return Ok(new { response });
    }
}
```

## 错误处理

### 常见错误

```csharp
try
{
    var response = await client.ChatAsync("Hello");
}
catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
{
    Console.WriteLine("认证失败，请检查 API Key");
}
catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
{
    Console.WriteLine("API 地址错误或服务未运行");
}
catch (TaskCanceledException)
{
    Console.WriteLine("请求超时");
}
catch (Exception ex)
{
    Console.WriteLine($"未知错误: {ex.Message}");
}
```

### 重试策略

```csharp
// 使用 Polly 实现重试
var retryPolicy = Policy
    .Handle<HttpRequestException>()
    .Or<TaskCanceledException>()
    .WaitAndRetryAsync(3, retryAttempt => 
        TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
    
await retryPolicy.ExecuteAsync(async () =>
{
    return await client.ChatAsync("重要消息");
});
```

## 性能优化

### 1. 连接池

```csharp
// 使用 IHttpClientFactory 管理连接
services.AddHttpClient<IHermesAgentClient, HermesHttpClient>()
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
    {
        MaxConnectionsPerServer = 100,
        PooledConnectionLifetime = TimeSpan.FromMinutes(5)
    });
```

### 2. 超时设置

```csharp
var client = new HermesHttpClient(
    baseUrl: "http://localhost:8642",
    timeout: TimeSpan.FromSeconds(60)); // 长任务可延长超时
```

### 3. 批量处理

```csharp
// 批量发送消息
var messages = new[]
{
    "问题1",
    "问题2", 
    "问题3"
};

var results = await client.BatchChatAsync(messages);
```

## 监控和日志

### 启用监控

```csharp
// 添加监控服务
services.AddHermesAgentMonitoring();

// 在控制器中记录
_logger.LogInformation("Hermes请求: {Message}", request.Message);
var stopwatch = Stopwatch.StartNew();
var response = await _client.ChatAsync(request.Message);
stopwatch.Stop();
_logger.LogInformation("Hermes响应时间: {Time}ms", stopwatch.ElapsedMilliseconds);
```

### 健康检查

```csharp
// 定期检查服务状态
var isHealthy = await client.HealthCheckAsync();
if (!isHealthy)
{
    _logger.LogWarning("Hermes Agent 服务异常");
}
```

## 安全考虑

### 1. 保护 API Key

```csharp
// 使用环境变量
var apiKey = Environment.GetEnvironmentVariable("HERMES_API_KEY");

// 或使用机密管理器
// dotnet user-secrets set "HermesAgent:ApiKey" "your-secret-key"
```

### 2. 请求验证

```csharp
public class ChatRequestValidator
{
    public bool Validate(ChatRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
            return false;
            
        if (request.Message.Length > 10000)
            return false;
            
        if (request.Temperature < 0 || request.Temperature > 2)
            return false;
            
        return true;
    }
}
```

### 3. 速率限制

```csharp
// 实现简单的速率限制
public class RateLimiter
{
    private readonly SemaphoreSlim _semaphore = new(10, 10); // 最大10个并发
    
    public async Task<T> ExecuteAsync<T>(Func<Task<T>> operation)
    {
        await _semaphore.WaitAsync();
        try
        {
            return await operation();
        }
        finally
        {
            _semaphore.Release();
        }
    }
}
```

## 部署指南

### 开发环境

1. 本地运行 Hermes Agent
2. 使用 `http://localhost:8642`
3. 开发时禁用认证

### 生产环境

1. 使用专用服务器运行 Hermes Agent
2. 配置 HTTPS
3. 使用强密码 API Key
4. 启用防火墙规则
5. 配置监控告警

### Docker 部署

```dockerfile
# Hermes Agent
docker run -d \
  -p 8642:8642 \
  -v ~/.hermes:/root/.hermes \
  --name hermes-agent \
  hermes-agent

# C# 应用
docker run -d \
  -p 8080:80 \
  -e HERMES_AGENT_URL=http://hermes-agent:8642 \
  -e HERMES_API_KEY=your-secret-key \
  --link hermes-agent \
  your-csharp-app
```

## 故障排除

### 常见问题

1. **连接失败**
   - 检查 Hermes Agent 是否运行: `hermes gateway status`
   - 检查端口是否开放: `netstat -tuln | grep 8642`
   - 检查防火墙设置

2. **认证失败**
   - 验证 API Key 是否正确
   - 检查 `~/.hermes/config.yaml` 配置
   - 重启 Hermes Agent: `hermes gateway restart`

3. **响应超时**
   - 增加超时时间
   - 检查网络延迟
   - 查看 Hermes Agent 日志: `tail -f ~/.hermes/logs/gateway.log`

4. **流式响应中断**
   - 检查客户端是否保持连接
   - 查看网络稳定性
   - 检查 Hermes Agent 内存使用

### 日志分析

```bash
# 查看 Hermes Agent 日志
tail -f ~/.hermes/logs/gateway.log | grep -i "api_server\|error"

# 查看 C# 应用日志
dotnet run | grep -i "hermes\|error\|exception"
```

## 最佳实践

1. **使用依赖注入**：统一管理客户端生命周期
2. **实现重试机制**：处理网络波动
3. **添加监控**：实时了解服务状态
4. **保护敏感信息**：使用环境变量或机密管理器
5. **版本控制**：记录 Hermes Agent 和客户端版本
6. **文档化**：记录配置和集成方式
7. **测试覆盖**：编写单元测试和集成测试

## 下一步

- [Webhook 集成](./webhook-integration.md)
- [进程调用集成](./process-integration.md)
- [安全指南](./security.md)
- [性能优化](./performance.md)