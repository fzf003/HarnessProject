# 📚 C# 进程调用集成 - 文件索引

> **项目位置**: `/mnt/d/Hermes/Projects/CSharp-Hermes-Integration/`

## 🎯 核心文件

### 进程调用客户端

| 文件 | 路径 | 说明 |
|------|------|------|
| **HermesProcessClient.cs** | `src/HermesAgent.Client/ProcessClient/HermesProcessClient.cs` | ⭐ 核心客户端类 |
| HermesAgent.Client.csproj | `src/HermesAgent.Client/HermesAgent.Client.csproj` | 客户端库项目 |

### 控制台示例

| 文件 | 路径 | 说明 |
|------|------|------|
| **Program.cs** | `src/HermesAgent.Examples.ConsoleApp/Program.cs` | ⭐ 示例程序入口 |
| appsettings.json | `src/HermesAgent.Examples.ConsoleApp/appsettings.json` | 配置文件 |
| HermesAgent.Examples.ConsoleApp.csproj | `src/HermesAgent.Examples.ConsoleApp/HermesAgent.Examples.ConsoleApp.csproj` | 示例项目 |
| README.md | `src/HermesAgent.Examples.ConsoleApp/README.md` | 使用说明 |

### 文档

| 文件 | 路径 | 说明 |
|------|------|------|
| **PROCESS-INTEGRATION-GUIDE.md** | `docs/PROCESS-INTEGRATION-GUIDE.md` | ⭐ 详细集成指南 |
| **PROJECT-SUMMARY.md** | `docs/PROJECT-SUMMARY.md` | ⭐ 项目总结 |
| HTTP-API-INTEGRATION.md | `docs/http-api-integration.md` | HTTP API 集成方案 |
| README.md | `README.md` | 总览文档 |

### 脚本

| 文件 | 路径 | 说明 |
|------|------|------|
| **run-console-app.sh** | `run-console-app.sh` | ⭐ 快速启动脚本 |

## 🚀 快速开始

### 方式 1: 运行启动脚本（推荐）

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

## 📖 阅读顺序

### 新手用户

1. 📄 [PROJECT-SUMMARY.md](docs/PROJECT-SUMMARY.md) - 了解项目概览
2. 🏃 运行 `run-console-app.sh` - 体验功能
3. 📄 [PROCESS-INTEGRATION-GUIDE.md](docs/PROCESS-INTEGRATION-GUIDE.md) - 深入学习

### 开发者

1. 📄 [PROCESS-INTEGRATION-GUIDE.md](docs/PROCESS-INTEGRATION-GUIDE.md) - 了解集成方案
2. 💻 [HermesProcessClient.cs](src/HermesAgent.Client/ProcessClient/HermesProcessClient.cs) - 查看核心代码
3. 💻 [Program.cs](src/HermesAgent.Examples.ConsoleApp/Program.cs) - 参考示例
4. 📄 [README.md](src/HermesAgent.Examples.ConsoleApp/README.md) - 使用说明

## 📂 目录结构

```
/mnt/d/Hermes/Projects/CSharp-Hermes-Integration/
│
├── src/                                    # 源代码
│   ├── HermesAgent.Client/                 # 客户端库
│   │   └── ProcessClient/
│   │       └── HermesProcessClient.cs      # ⭐ 进程客户端
│   │
│   └── HermesAgent.Examples.ConsoleApp/    # 控制台示例
│       ├── Program.cs                      # ⭐ 示例程序
│       ├── appsettings.json
│       ├── HermesAgent.Examples.ConsoleApp.csproj
│       └── README.md
│
├── docs/                                   # 文档
│   ├── PROCESS-INTEGRATION-GUIDE.md        # ⭐ 集成指南
│   ├── PROJECT-SUMMARY.md                  # ⭐ 项目总结
│   └── http-api-integration.md
│
├── examples/                               # 其他示例
│   ├── ConsoleApp/
│   └── WebApi/
│
├── run-console-app.sh                      # ⭐ 启动脚本
└── README.md
```

## 🔑 关键代码位置

### 进程调用核心方法

```
src/HermesAgent.Client/ProcessClient/HermesProcessClient.cs
├── ExecuteCommandAsync()           # 单次查询
├── StartInteractiveSessionAsync()  # 启动会话
├── SendMessageAsync()              # 发送消息
├── StopSessionAsync()              # 结束会话
└── GetActiveSessions()             # 获取会话列表
```

### 示例程序功能

```
src/HermesAgent.Examples.ConsoleApp/Program.cs
├── SingleQueryAsync()              # 单次查询示例
├── SessionModeAsync()              # 会话模式示例
├── FileOperationTaskAsync()        # 文件操作示例
├── TerminalTaskAsync()             # 终端命令示例
├── BatchTasksAsync()               # 批量处理示例
└── ShowComparisonInfo()            # 对比信息
```

## 🎯 使用场景

### 适合使用进程调用

- ✅ 本地脚本自动化
- ✅ 离线任务处理
- ✅ 开发调试
- ✅ 简单工具集成

### 不适合使用进程调用

- ❌ 高并发 Web 应用
- ❌ 需要流式响应
- ❌ 实时交互应用
- ❌ 微服务架构

> 对于上述场景，建议使用 [HTTP API 集成方案](docs/http-api-integration.md)

## 📊 性能参考

| 操作 | 延迟 | 说明 |
|------|------|------|
| 首次调用 | ~2-3 秒 | 启动新进程 |
| 单次查询 | ~2-3 秒 | 每次都启动新进程 |
| 会话模式 | ~100-500ms | 复用已启动进程 |
| 内存占用 | ~50MB/进程 | 每个 Hermes 进程 |

## ⚙️ 配置说明

### appsettings.json

```json
{
  "HermesAgent": {
    "ProcessClient": {
      "HermesPath": "hermes",           // Hermes 路径
      "WorkingDirectory": ".",          // 工作目录
      "TimeoutSeconds": 300             // 超时时间
    }
  }
}
```

### 环境变量

```bash
export HERMES_PATH=/home/fzf003/.local/bin/hermes
```

## 🆘 常见问题

### Q: 找不到 hermes 命令？
```bash
which hermes
# 或设置完整路径
export HERMES_PATH=/home/fzf003/.local/bin/hermes
```

### Q: 命令执行超时？
```json
{
  "HermesAgent": {
    "ProcessClient": {
      "TimeoutSeconds": 600
    }
  }
}
```

### Q: 如何保持会话？
```csharp
var sessionId = "my-session";
await client.ExecuteCommandAsync("问题 1", sessionId);
await client.ExecuteCommandAsync("问题 2", sessionId);
```

## 🔗 相关链接

- [Hermes Agent 官方文档](https://hermes-agent.nousresearch.com/docs/)
- [.NET 文档](https://docs.microsoft.com/dotnet/)
- [进程调用集成指南](docs/PROCESS-INTEGRATION-GUIDE.md)
- [HTTP API 集成方案](docs/http-api-integration.md)

## 📞 项目位置

- **Windows 路径**: `D:\Hermes\Projects\CSharp-Hermes-Integration\`
- **WSL 路径**: `/mnt/d/Hermes/Projects/CSharp-Hermes-Integration/`

---

**最后更新**: 2025-04-16
**版本**: 1.0.0
