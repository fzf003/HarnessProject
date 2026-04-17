# Hermes Agent C# 进程调用集成方案

## 📋 方案概述

进程调用集成是通过直接调用 `hermes` CLI 命令来实现 C# 与 Hermes Agent 的交互。

### 工作原理

```
┌─────────────────┐         ┌──────────────────┐         ┌─────────────────┐
│   C# 应用程序   │ ──────► │  hermes 进程     │ ──────► │  Hermes Agent   │
│                 │  启动   │  (子进程)        │  执行   │  (Python)       │
└─────────────────┘         └──────────────────┘         └─────────────────┘
       ▲                           │                           │
       │                           │◄──── 标准输出 ────────────┘
       │                           │
       └────────── 捕获输出 ───────┘
```

## 🎯 适用场景

| 场景 | 说明 | 推荐度 |
|------|------|--------|
| 本地脚本自动化 | 定时任务、批处理脚本 | ⭐⭐⭐⭐⭐ |
| 离线任务处理 | 无需网络连接的任务 | ⭐⭐⭐⭐⭐ |
| 开发调试 | 快速测试 Hermes 功能 | ⭐⭐⭐⭐ |
| 简单集成 | 小型工具、CLI 应用 | ⭐⭐⭐⭐ |
| Web 应用后端 | 高并发、服务化 | ⭐⭐ (推荐 HTTP API) |
| 实时交互 | 需要流式响应 | ⭐⭐ (推荐 HTTP API) |

## 📦 项目文件

```
/mnt/d/Hermes/Projects/CSharp-Hermes-Integration/
├── src/
│   ├── HermesAgent.Client/
│   │   └── ProcessClient/
│   │       └── HermesProcessClient.cs      # 核心进程客户端
│   └── HermesAgent.Examples.ConsoleApp/
│       ├── Program.cs                       # 示例程序入口
│       ├── appsettings.json                 # 配置文件
│       ├── HermesAgent.Examples.ConsoleApp.csproj
│       └── README.md                        # 使用说明
├── run-console-app.sh                       # 快速启动脚本
└── docs/
    └── PROCESS-INTEGRATION-GUIDE.md         # 本文档
```

## 🚀 快速开始

### 步骤 1: 检查环境

```bash
# 检查 Hermes Agent
hermes --version

# 检查 .NET SDK
dotnet --version
```

### 步骤 2: 运行示例

```bash
# 方式 1: 使用启动脚本
/mnt/d/Hermes/Projects/CSharp-Hermes-Integration/run-console-app.sh

# 方式 2: 手动运行
cd /mnt/d/Hermes/Projects/CSharp-Hermes-Integration/src/HermesAgent.Examples.ConsoleApp
dotnet run
```

### 步骤 3: 选择功能

程序启动后会显示菜单：
```
═══════════════════════════════════════════════════════
  请选择操作：
═══════════════════════════════════════════════════════
  1. 单次查询（非交互式）
  2. 会话模式（保持上下文）
  3. 执行文件操作任务
  4. 执行终端命令任务
  5. 批量处理任务
  6. 查看会话历史
  7. 进程调用 vs HTTP API 对比
  0. 退出
═══════════════════════════════════════════════════════
```

## 💻 代码使用

### 基础示例

```csharp
using HermesAgent.Client;

// 1. 创建客户端
var client = new HermesProcessClient(
    hermesPath: "hermes",
    workingDirectory: "/home/fzf003/workspace"
);

// 2. 执行单次查询
var response = await client.ExecuteCommandAsync("列出当前目录的文件");
Console.WriteLine(response);

// 3. 清理资源
client.Dispose();
```

### 会话模式

```csharp
using HermesAgent.Client;

var client = new HermesProcessClient();

// 启动会话
var sessionId = await client.StartInteractiveSessionAsync();

// 多轮对话（保持上下文）
await client.SendMessageAsync(sessionId, "我叫张三");
await client.SendMessageAsync(sessionId, "记住我的名字");
var answer = await client.SendMessageAsync(sessionId, "我叫什么名字？");
Console.WriteLine(answer);  // 输出：你叫张三

// 结束会话
await client.StopSessionAsync(sessionId);
```

### 错误处理

```csharp
try
{
    var response = await client.ExecuteCommandAsync("复杂任务...");
}
catch (HermesProcessException ex)
{
    Console.WriteLine($"Hermes 错误：{ex.Message}");
    Console.WriteLine($"错误输出：{ex.ErrorOutput}");
    Console.WriteLine($"退出码：{ex.ExitCode}");
}
catch (TimeoutException ex)
{
    Console.WriteLine($"超时：{ex.Message}");
}
catch (Exception ex)
{
    Console.WriteLine($"未知错误：{ex.Message}");
}
```

### 批量处理

```csharp
var tasks = new[]
{
    "用 C#写一个 Hello World",
    "解释依赖注入",
    "列出.cs 文件"
};

var results = await Task.WhenAll(
    tasks.Select(async task =>
    {
        try
        {
            var response = await client.ExecuteCommandAsync(task);
            return new { Task = task, Response = response, Success = true };
        }
        catch (Exception ex)
        {
            return new { Task = task, Response = ex.Message, Success = false };
        }
    })
);

// 处理结果
foreach (var result in results)
{
    Console.WriteLine($"{result.Task}: {(result.Success ? "✓" : "✗")}");
}
```

## ⚙️ 配置选项

### appsettings.json

```json
{
  "HermesAgent": {
    "ProcessClient": {
      "HermesPath": "hermes",           // Hermes 可执行文件路径
      "WorkingDirectory": ".",          // 工作目录
      "TimeoutSeconds": 300             // 超时时间（秒）
    },
    "Logging": {
      "Enabled": true,
      "LogLevel": "Information"
    }
  }
}
```

### 环境变量

```bash
# 设置 Hermes 路径
export HERMES_PATH=/home/fzf003/.local/bin/hermes

# C# 代码中读取
var hermesPath = Environment.GetEnvironmentVariable("HERMES_PATH") ?? "hermes";
```

## 📊 性能对比

| 指标 | 进程调用 | HTTP API |
|------|----------|----------|
| 首次调用延迟 | ~2-3 秒 | ~100ms |
| 后续调用延迟 | ~2-3 秒 | ~50ms |
| 内存占用 | ~50MB/进程 | ~10MB |
| CPU 占用 | 中 | 低 |
| 并发能力 | 1-5 | 50+ |
| 会话保持 | 手动 | 自动 |

## ⚠️ 注意事项

### 1. 性能优化

```csharp
// ❌ 不推荐：频繁创建客户端
for (int i = 0; i < 100; i++)
{
    var client = new HermesProcessClient();
    await client.ExecuteCommandAsync("任务");
    client.Dispose();
}

// ✅ 推荐：复用客户端
var client = new HermesProcessClient();
try
{
    for (int i = 0; i < 100; i++)
    {
        await client.ExecuteCommandAsync("任务");
    }
}
finally
{
    client.Dispose();
}
```

### 2. 会话管理

```csharp
// ✅ 正确：使用 using 语句
using var client = new HermesProcessClient();
var sessionId = await client.StartInteractiveSessionAsync();
try
{
    // 使用会话...
}
finally
{
    await client.StopSessionAsync(sessionId);
}
```

### 3. 超时设置

```csharp
// 长时间任务设置更长的超时
var client = new HermesProcessClient(
    timeout: TimeSpan.FromMinutes(10)  // 默认 5 分钟
);
```

## 🔧 故障排除

### 问题 1: 找不到 hermes 命令

**症状**：
```
System.ComponentModel.Win32Exception: No such file or directory
```

**解决方案**：
```bash
# 1. 检查 hermes 位置
which hermes

# 2. 添加到 PATH
export PATH=$PATH:~/.local/bin

# 3. 或在代码中指定完整路径
var client = new HermesProcessClient("/home/fzf003/.local/bin/hermes");
```

### 问题 2: 命令执行超时

**症状**：
```
TimeoutException: Hermes 命令执行超时（300 秒）
```

**解决方案**：
```csharp
// 增加超时时间
var client = new HermesProcessClient(
    timeout: TimeSpan.FromMinutes(10)
);

// 或检查任务复杂度，拆分大任务
```

### 问题 3: 会话无法保持

**症状**：
```
Hermes 不记得之前的对话内容
```

**解决方案**：
```csharp
// 确保使用相同的 sessionId
var sessionId = "my-session-123";
await client.ExecuteCommandAsync("问题 1", sessionId);
await client.ExecuteCommandAsync("问题 2", sessionId);  // 同一会话
```

### 问题 4: 中文输出乱码

**症状**：
```
输出包含 或乱码字符
```

**解决方案**：
```csharp
// 在 Program.cs 中设置编码
Console.OutputEncoding = Encoding.UTF8;
Console.InputEncoding = Encoding.UTF8;
```

## 📈 最佳实践

### 1. 客户端管理

```csharp
// 单例模式
public class HermesClientProvider
{
    private static readonly Lazy<HermesProcessClient> _client = new(() =>
        new HermesProcessClient());

    public static HermesProcessClient Instance => _client.Value;
}

// 使用
var client = HermesClientProvider.Instance;
var response = await client.ExecuteCommandAsync("任务");
```

### 2. 重试策略

```csharp
public async Task<string> ExecuteWithRetryAsync(
    string command,
    int maxRetries = 3,
    TimeSpan? delay = null)
{
    delay ??= TimeSpan.FromSeconds(1);
    var retryCount = 0;

    while (true)
    {
        try
        {
            return await client.ExecuteCommandAsync(command);
        }
        catch (HermesProcessException ex) when (retryCount < maxRetries)
        {
            retryCount++;
            await Task.Delay(delay.Value * retryCount);
        }
    }
}
```

### 3. 日志记录

```csharp
public class LoggedHermesClient : IDisposable
{
    private readonly HermesProcessClient _client;
    private readonly ILogger _logger;

    public LoggedHermesClient(HermesProcessClient client, ILogger logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<string> ExecuteCommandAsync(string command)
    {
        _logger.LogInformation("执行 Hermes 命令：{Command}", command);
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var response = await _client.ExecuteCommandAsync(command);
            stopwatch.Stop();
            _logger.LogInformation("命令完成，耗时 {Ms}ms", stopwatch.ElapsedMilliseconds);
            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "命令失败，耗时 {Ms}ms", stopwatch.ElapsedMilliseconds);
            throw;
        }
    }

    public void Dispose() => _client.Dispose();
}
```

## 🔗 相关资源

- [ConsoleApp README](../src/HermesAgent.Examples.ConsoleApp/README.md)
- [HTTP API 集成方案](./HTTP-API-INTEGRATION.md)
- [Webhook 集成方案](./WEBHOOK-INTEGRATION.md)
- [Hermes Agent 官方文档](https://hermes-agent.nousresearch.com/docs/)

## 📄 许可证

MIT License
