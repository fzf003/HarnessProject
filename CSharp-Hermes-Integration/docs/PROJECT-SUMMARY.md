# C# 进程调用集成 - 项目总结

## 📁 项目结构

```
/mnt/d/Hermes/Projects/CSharp-Hermes-Integration/
│
├── src/
│   ├── HermesAgent.Client/
│   │   └── ProcessClient/
│   │       └── HermesProcessClient.cs          # 核心进程客户端类
│   │
│   └── HermesAgent.Examples.ConsoleApp/
│       ├── Program.cs                          # 控制台示例程序
│       ├── appsettings.json                    # 应用配置
│       ├── HermesAgent.Examples.ConsoleApp.csproj  # 项目文件
│       └── README.md                           # 使用说明
│
├── docs/
│   ├── PROCESS-INTEGRATION-GUIDE.md            # 进程集成详细指南
│   └── PROJECT-SUMMARY.md                      # 本文档
│
├── run-console-app.sh                          # 快速启动脚本
└── README.md                                   # 总览文档
```

## 🎯 核心组件

### 1. HermesProcessClient 类

**位置**: `src/HermesAgent.Client/ProcessClient/HermesProcessClient.cs`

**功能**:
- ✅ 执行单次查询（非交互式）
- ✅ 启动交互式会话
- ✅ 发送消息到会话
- ✅ 结束会话
- ✅ 批量处理任务
- ✅ 会话管理
- ✅ 错误处理
- ✅ 资源清理

**主要方法**:
```csharp
// 单次查询
Task<string> ExecuteCommandAsync(string command, string sessionId = null)

// 启动会话
Task<string> StartInteractiveSessionAsync(string sessionId = null)

// 发送消息
Task<string> SendMessageAsync(string sessionId, string message)

// 结束会话
Task<bool> StopSessionAsync(string sessionId)

// 获取活动会话
IReadOnlyList<ProcessSession> GetActiveSessions()
```

### 2. 控制台示例程序

**位置**: `src/HermesAgent.Examples.ConsoleApp/Program.cs`

**功能演示**:
1. 单次查询（非交互式）
2. 会话模式（保持上下文）
3. 执行文件操作任务
4. 执行终端命令任务
5. 批量处理任务
6. 查看会话历史
7. 进程调用 vs HTTP API 对比

## 🚀 快速开始

### 方式 1: 使用启动脚本

```bash
/mnt/d/Hermes/Projects/CSharp-Hermes-Integration/run-console-app.sh
```

### 方式 2: 手动运行

```bash
cd /mnt/d/Hermes/Projects/CSharp-Hermes-Integration/src/HermesAgent.Examples.ConsoleApp
dotnet restore
dotnet build
dotnet run
```

## 📖 使用示例

### 示例 1: 基础查询

```csharp
using HermesAgent.Client;

var client = new HermesProcessClient();
var response = await client.ExecuteCommandAsync("列出当前目录的文件");
Console.WriteLine(response);
client.Dispose();
```

### 示例 2: 会话对话

```csharp
var client = new HermesProcessClient();
var sessionId = await client.StartInteractiveSessionAsync();

await client.SendMessageAsync(sessionId, "我叫张三");
var answer = await client.SendMessageAsync(sessionId, "我叫什么名字？");
Console.WriteLine(answer);  // 输出：你叫张三

await client.StopSessionAsync(sessionId);
```

### 示例 3: 错误处理

```csharp
try
{
    var response = await client.ExecuteCommandAsync("任务");
}
catch (HermesProcessException ex)
{
    Console.WriteLine($"Hermes 错误：{ex.Message}");
    Console.WriteLine($"错误输出：{ex.ErrorOutput}");
}
catch (TimeoutException ex)
{
    Console.WriteLine($"超时：{ex.Message}");
}
```

## 📊 性能特性

| 指标 | 数值 | 说明 |
|------|------|------|
| 首次调用延迟 | ~2-3 秒 | 启动新进程时间 |
| 单次查询延迟 | ~2-3 秒 | 每次调用都启动新进程 |
| 会话模式延迟 | ~100-500ms | 已启动进程 |
| 内存占用 | ~50MB/进程 | 每个 Hermes 进程 |
| 推荐并发数 | 1-5 | 过高会影响性能 |

## ⚖️ 优缺点分析

### 优点 ✅

1. **配置简单** - 只需指定 hermes 路径
2. **无需网络** - 本地进程调用，不依赖网络
3. **独立运行** - 不需要启动 Gateway 服务
4. **完全控制** - 直接控制进程生命周期
5. **适合离线** - 可在无网络环境使用

### 缺点 ❌

1. **性能较低** - 每次调用启动新进程
2. **资源消耗** - 每个进程占用 ~50MB 内存
3. **并发限制** - 不适合高并发场景
4. **无流式响应** - 只能等待完整响应
5. **会话管理复杂** - 需要手动管理会话

## 🆚 与 HTTP API 对比

| 特性 | 进程调用 | HTTP API |
|------|----------|----------|
| 配置复杂度 | ⭐ 简单 | ⭐⭐⭐ 中等 |
| 启动速度 | ⭐⭐ 慢 | ⭐⭐⭐⭐⭐ 快 |
| 资源消耗 | ⭐⭐ 高 | ⭐⭐⭐⭐⭐ 低 |
| 并发能力 | ⭐⭐ 低 | ⭐⭐⭐⭐⭐ 高 |
| 流式响应 | ❌ 不支持 | ✅ 支持 |
| 会话管理 | ⭐⭐ 手动 | ⭐⭐⭐⭐⭐ 自动 |
| 网络依赖 | ✅ 无需 | ❌ 需要 |
| 适用场景 | 本地脚本 | Web 应用/服务 |

## 🎓 学习路径

### 初级使用者

1. ✅ 运行控制台示例程序
2. ✅ 尝试单次查询功能
3. ✅ 尝试会话模式功能
4. ✅ 阅读 `README.md`

### 中级使用者

1. ✅ 阅读 `PROCESS-INTEGRATION-GUIDE.md`
2. ✅ 在自己的项目中集成 `HermesProcessClient`
3. ✅ 实现错误处理和重试逻辑
4. ✅ 实现日志记录

### 高级使用者

1. ✅ 自定义进程管理逻辑
2. ✅ 实现连接池和复用
3. ✅ 集成到 DI 容器
4. ✅ 实现监控和指标收集

## 📋 检查清单

使用前请确认：

- [ ] Hermes Agent 已安装 (`hermes --version`)
- [ ] .NET 8 SDK 已安装 (`dotnet --version`)
- [ ] 工作目录有读写权限
- [ ] 了解超时设置（默认 5 分钟）
- [ ] 了解资源清理（使用 `using` 或手动 `Dispose`）

## 🔗 相关文档

- [控制台示例 README](../src/HermesAgent.Examples.ConsoleApp/README.md)
- [进程集成详细指南](./PROCESS-INTEGRATION-GUIDE.md)
- [HTTP API 集成方案](./HTTP-API-INTEGRATION.md)
- [Webhook 集成方案](./WEBHOOK-INTEGRATION.md)
- [Hermes Agent 官方文档](https://hermes-agent.nousresearch.com/docs/)

## 💡 提示和技巧

### 技巧 1: 复用客户端

```csharp
// 创建单例
private static readonly HermesProcessClient _client = new();

// 复用而不是频繁创建
public async Task<string> QueryAsync(string question)
{
    return await _client.ExecuteCommandAsync(question);
}
```

### 技巧 2: 设置超时

```csharp
// 长时间任务
var client = new HermesProcessClient(
    timeout: TimeSpan.FromMinutes(10)
);
```

### 技巧 3: 批量处理

```csharp
var tasks = new[] { "任务 1", "任务 2", "任务 3" };
var results = await Task.WhenAll(
    tasks.Select(t => client.ExecuteCommandAsync(t))
);
```

### 技巧 4: 日志记录

```csharp
Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] 执行：{command}");
var response = await client.ExecuteCommandAsync(command);
Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] 完成，长度：{response.Length}");
```

## 🆘 常见问题

### Q: 为什么响应这么慢？
A: 进程调用每次都会启动新进程（~2-3 秒），建议使用会话模式或改用 HTTP API。

### Q: 如何保持会话上下文？
A: 使用相同的 `sessionId` 调用 `ExecuteCommandAsync(command, sessionId)`。

### Q: 如何处理大量并发请求？
A: 进程调用不适合高并发，建议改用 HTTP API。

### Q: 中文输出乱码怎么办？
A: 设置 `Console.OutputEncoding = Encoding.UTF8;`

### Q: 如何清理资源？
A: 使用 `using` 语句或手动调用 `Dispose()` 方法。

## 📞 技术支持

- Hermes Agent 官方文档：https://hermes-agent.nousresearch.com/docs/
- 项目位置：`/mnt/d/Hermes/Projects/CSharp-Hermes-Integration/`
- 示例程序：`src/HermesAgent.Examples.ConsoleApp/`

## 📄 许可证

MIT License
