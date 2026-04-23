# C# 集成 Hermes Agent 方案

## 项目概述

这是一个完整的 C# 集成 Hermes Agent 的方案，提供多种集成方式：
1. HTTP API 集成（推荐）
2. Webhook 集成
3. 进程调用集成

## 目录结构

```
CSharp-Hermes-Integration/
├── src/                    # 源代码
│   ├── HermesAgent.Client/           # 核心客户端库
│   ├── HermesAgent.Extensions/       # 扩展功能
│   └── HermesAgent.Examples/         # 示例代码
├── tests/                  # 测试代码
├── docs/                   # 文档
└── examples/               # 使用示例
```

## 快速开始

### 1. 安装 Hermes Agent

```bash
# 安装 Hermes Agent
curl -fsSL https://raw.githubusercontent.com/NousResearch/hermes-agent/main/scripts/install.sh | bash

# 配置 API Server
hermes config set platforms.api_server.enabled true
hermes config set platforms.api_server.port 8642
hermes config set platforms.api_server.host 0.0.0.0
hermes config set platforms.api_server.key "your-secret-key"

# 启动 Gateway
hermes gateway run
```

### 2. 使用 C# 客户端

```csharp
// 安装 NuGet 包
// dotnet add package HermesAgent.Client

using HermesAgent.Client;

// 创建客户端
var client = new HermesHttpClient("http://localhost:8642", "your-secret-key");

// 发送消息
var response = await client.ChatAsync("Hello, Hermes!");
Console.WriteLine(response);

// 流式响应
await foreach (var chunk in client.ChatStreamAsync("Write a C# Hello World"))
{
    Console.Write(chunk);
}
```

## 集成方式对比

| 方式 | 优点 | 缺点 | 适用场景 |
|------|------|------|----------|
| HTTP API | 标准化、跨语言、支持流式 | 需要网络 | 生产环境、Web应用 |
| Webhook | 事件驱动、异步处理 | 单向通信 | 自动化工作流 |
| 进程调用 | 直接控制、无需网络 | 进程管理复杂 | 本地工具、CLI应用 |

## 详细文档

- [HTTP API 集成](./docs/http-api-integration.md)
- [Webhook 集成](./docs/webhook-integration.md)  
- [进程调用集成](./docs/process-integration.md)
- [ASP.NET Core 集成](./docs/aspnet-core-integration.md)
- [配置说明](./docs/configuration.md)
- [安全指南](./docs/security.md)

## 示例项目

查看 [examples](./examples/) 目录获取完整示例：
- Console 应用
- ASP.NET Core Web API
- Worker Service
- Blazor 应用

## 许可证

MIT License
