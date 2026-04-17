using HermesAgent.Client;
using Microsoft.Extensions.Logging;

namespace HermesAgent.Examples.ConsoleApp
{
    /// <summary>
    /// Hermes Agent C# 客户端示例程序
    /// 演示如何使用Hermes Agent客户端库进行各种操作
    /// </summary>
    internal class Program
    {
        // 配置常数
        private const string DefaultBaseUrl = "http://localhost:8642";
        private const string DefaultApiKey = "hermes-csharp-integration-key-20250416";
        private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(30);

        // 应用标题
        private const string ApplicationTitle = "Hermes Agent C# 客户端示例";
        private const string Separator = "==========================================================";

        /// <summary>
        /// 应用程序入口点
        /// </summary>
        static async Task Main(string[] args)
        {
            try
            {
                PrintApplicationHeader();

                // 从配置或命令行参数获取设置
                var (baseUrl, apiKey) = ParseArguments(args);

                // 创建客户端
                Console.WriteLine($"正在连接到 Hermes Agent: {baseUrl}");
                using var client = new HermesHttpClient(
                    baseUrl: baseUrl,
                    apiKey: apiKey,
                    timeout: DefaultTimeout
                );

                // 运行示例
                await RunExamples(client);

                PrintApplicationFooter(success: true);
            }
            catch (HttpRequestException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"连接错误: {ex.Message}");
                Console.ResetColor();
                Console.WriteLine($"请确保 Hermes Agent 正在运行:");
                Console.WriteLine($"  命令: hermes gateway run");
                Console.WriteLine($"  地址: {DefaultBaseUrl}");
                PrintApplicationFooter(success: false);
                Environment.Exit(1);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"发生错误: {ex.Message}");
                if (!string.IsNullOrEmpty(ex.InnerException?.Message))
                {
                    Console.WriteLine($"详情: {ex.InnerException.Message}");
                }
                Console.ResetColor();
                PrintApplicationFooter(success: false);
                Environment.Exit(1);
            }
        }

        /// <summary>
        /// 解析命令行参数
        /// </summary>
        private static (string baseUrl, string apiKey) ParseArguments(string[] args)
        {
            var baseUrl = DefaultBaseUrl;
            var apiKey = DefaultApiKey;

            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i].ToLower())
                {
                    case "--url":
                    case "-u":
                        if (i + 1 < args.Length)
                            baseUrl = args[++i];
                        break;

                    case "--key":
                    case "-k":
                        if (i + 1 < args.Length)
                            apiKey = args[++i];
                        break;

                    case "--help":
                    case "-h":
                        PrintHelpMessage();
                        Environment.Exit(0);
                        break;
                }
            }

            return (baseUrl, apiKey);
        }

        /// <summary>
        /// 打印应用程序标题
        /// </summary>
        private static void PrintApplicationHeader()
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(Separator);
            Console.WriteLine($"  {ApplicationTitle}");
            Console.WriteLine(Separator);
            Console.ResetColor();
            Console.WriteLine();
        }

        /// <summary>
        /// 打印应用程序页脚
        /// </summary>
        private static void PrintApplicationFooter(bool success)
        {
            Console.WriteLine();
            Console.ForegroundColor = success ? ConsoleColor.Green : ConsoleColor.Yellow;
            Console.WriteLine(Separator);
            Console.WriteLine(success ? "  示例执行完成" : "  示例执行失败");
            Console.WriteLine(Separator);
            Console.ResetColor();
            Console.WriteLine();
        }

        /// <summary>
        /// 打印帮助信息
        /// </summary>
        private static void PrintHelpMessage()
        {
            Console.WriteLine("用法: HermesAgent.Examples.ConsoleApp [选项]");
            Console.WriteLine();
            Console.WriteLine("选项:");
            Console.WriteLine("  -u, --url <URL>      Hermes Agent API 地址 (默认: http://localhost:8642)");
            Console.WriteLine("  -k, --key <KEY>      API 密钥");
            Console.WriteLine("  -h, --help           显示此帮助信息");
            Console.WriteLine();
        }

        /// <summary>
        /// 运行所有示例
        /// </summary>
        private static async Task RunExamples(IHermesAgentClient client)
        {
            // 1. 健康检查
            await RunHealthCheck(client);

            // 2. 获取模型列表
            await RunGetModels(client);

            // 3. 创建会话
            var sessionId = await RunCreateSession(client);

            if (string.IsNullOrEmpty(sessionId))
            {
                return;
            }

            // 4. 聊天
            await RunChat(client, sessionId);

            // 5. 流式聊天
            await RunStreamingChat(client, sessionId);

            // 6. 批量聊天
            await RunBatchChat(client, sessionId);
        }

        /// <summary>
        /// 运行健康检查示例
        /// </summary>
        private static async Task RunHealthCheck(IHermesAgentClient client)
        {
            PrintSectionHeader("1. 健康检查");

            try
            {
                var isHealthy = await client.HealthCheckAsync();
                PrintResult($"状态: {(isHealthy ? "✓ 健康" : "✗ 异常")}", isHealthy);
            }
            catch (Exception ex)
            {
                PrintError($"健康检查失败: {ex.Message}");
            }

            Console.WriteLine();
        }

        /// <summary>
        /// 运行获取模型列表示例
        /// </summary>
        private static async Task RunGetModels(IHermesAgentClient client)
        {
            PrintSectionHeader("2. 获取模型列表");

            try
            {
                var models = await client.GetModelsAsync();
                if (models.Any())
                {
                    PrintResult($"可用模型: {string.Join(", ", models)}");
                }
                else
                {
                    PrintWarning("未找到任何可用模型");
                }
            }
            catch (Exception ex)
            {
                PrintError($"获取模型失败: {ex.Message}");
            }

            Console.WriteLine();
        }

        /// <summary>
        /// 运行创建会话示例
        /// </summary>
        private static async Task<string?> RunCreateSession(IHermesAgentClient client)
        {
            PrintSectionHeader("3. 创建会话");

            try
            {
                var sessionId = await client.CreateSessionAsync(initialMessage: "创建新会话");
                PrintResult($"会话ID: {sessionId}");
                return sessionId;
            }
            catch (Exception ex)
            {
                PrintError($"创建会话失败: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 运行聊天示例
        /// </summary>
        private static async Task RunChat(IHermesAgentClient client, string sessionId)
        {
            PrintSectionHeader("4. 聊天");

            var message = "你好,请介绍一下你自己";

            try
            {
                Console.WriteLine($"发送消息: {message}");
                var (session, response) = await client.ChatAsync(message, sessionId: sessionId);
                PrintResult($"响应:\n{response}");
            }
            catch (Exception ex)
            {
                PrintError($"聊天失败: {ex.Message}");
            }

            Console.WriteLine();
        }

        /// <summary>
        /// 运行流式聊天示例
        /// </summary>
        private static async Task RunStreamingChat(IHermesAgentClient client, string sessionId)
        {
            PrintSectionHeader("5. 流式聊天");

            var message = "请用三句话总结你的功能";

            try
            {
                Console.WriteLine($"发送消息: {message}");
                Console.WriteLine("流式响应:");
                Console.WriteLine(new string('-', 50));

                var chunkCount = 0;
                await foreach (var chunk in client.ChatStreamAsync(message, sessionId: sessionId))
                {
                    Console.Write(chunk);
                    chunkCount++;
                }

                Console.WriteLine();
                Console.WriteLine(new string('-', 50));
                PrintResult($"流式传输完成 (接收{chunkCount}个数据块)");
            }
            catch (Exception ex)
            {
                PrintError($"流式聊天失败: {ex.Message}");
            }

            Console.WriteLine();
        }

        /// <summary>
        /// 运行批量聊天示例
        /// </summary>
        private static async Task RunBatchChat(IHermesAgentClient client, string sessionId)
        {
            PrintSectionHeader("6. 批量聊天");

            var messages = new List<string>
            {
                "你是谁？",
                "你能做什么？",
                "你的工作原理是什么？"
            };

            try
            {
                Console.WriteLine($"发送{messages.Count}条消息...");
                var responses = new List<string>();

                for (int i = 0; i < messages.Count; i++)
                {
                    try
                    {
                        Console.WriteLine($"  消息{i + 1}: {messages[i]}");
                        var (session, response) = await client.ChatAsync(messages[i], sessionId: sessionId);
                        responses.Add(response);
                        var preview = response.Length > 50 ? response.Substring(0, 50) : response;
                        Console.WriteLine($"  响应: {preview}...");
                    }
                    catch (Exception ex)
                    {
                        PrintWarning($"  消息{i + 1}失败: {ex.Message}");
                    }
                }

                PrintResult($"成功处理{responses.Count}条消息");
            }
            catch (Exception ex)
            {
                PrintError($"批量聊天失败: {ex.Message}");
            }

            Console.WriteLine();
        }

        /// <summary>
        /// 打印分区标题
        /// </summary>
        private static void PrintSectionHeader(string title)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(title);
            Console.WriteLine(new string('-', title.Length));
            Console.ResetColor();
        }

        /// <summary>
        /// 打印成功结果
        /// </summary>
        private static void PrintResult(string message, bool? success = null)
        {
            var prefix = success == false ? "✗" : success == true ? "✓" : "ℹ";
            Console.ForegroundColor = success == false ? ConsoleColor.Red : success == true ? ConsoleColor.Green : ConsoleColor.Cyan;
            Console.WriteLine($"{prefix} {message}");
            Console.ResetColor();
        }

        /// <summary>
        /// 打印警告
        /// </summary>
        private static void PrintWarning(string message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"⚠ {message}");
            Console.ResetColor();
        }

        /// <summary>
        /// 打印错误
        /// </summary>
        private static void PrintError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"✗ {message}");
            Console.ResetColor();
        }
    }
}
