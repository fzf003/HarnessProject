using HermesAgent.Client;

namespace HermesAgent.Examples.ConsoleApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("=== Hermes Agent C# 客户端示例 ===");
            Console.WriteLine();

            // 创建客户端
            using var client = new HermesHttpClient(
                baseUrl: "http://localhost:8642",
                apiKey: "hermes-csharp-integration-key-20250416",
                timeout: TimeSpan.FromSeconds(30)
            );

            try
            {
                // 1. 健康检查
                Console.WriteLine("1. 健康检查...");
                var isHealthy = await client.HealthCheckAsync();
                Console.WriteLine($"   状态: {(isHealthy ? "健康" : "异常")}");
                Console.WriteLine();

                // 2. 获取模型列表
                Console.WriteLine("2. 获取模型列表...");
                var models = await client.GetModelsAsync();
                Console.WriteLine($"   可用模型: {string.Join(", ", models.Select(m => m.Id))}");
                Console.WriteLine();

                // 3. 创建会话
                Console.WriteLine("3. 创建会话...");
                var sessionId = await client.CreateSessionAsync(initialMessage: "创建会会话");
                Console.WriteLine($"   会话ID: {sessionId}");
                Console.WriteLine();

                // 4. 聊天
                Console.WriteLine("4. 聊天...");
                var message = "你好,首次访问冒昧了,请问你是谁?";
                var response = await client.ChatAsync(message, sessionId: sessionId);
                Console.WriteLine($"   响应: {response}");
                Console.WriteLine();

                Console.WriteLine("5. 流式聊天...");
                Console.WriteLine($"流式响应:");
                await foreach (
                    var responsemessage in client.ChatStreamAsync(message, sessionId: sessionId)
                )
                {
                    Console.Write($"{responsemessage}");
                }
                Console.WriteLine();

                // 6. 批量聊天
                /* Console.WriteLine("6. 批量聊天...");
                 var messages = new List<string> { "今天天气怎么样？", "你能介绍一下自己吗？" };
                 var responses = await client.BatchChatAsync(messages, sessionId);
                 for (int i = 0; i < messages.Count; i++)
                 {
                     Console.WriteLine($"   响应{i + 1}: {responses[i]}");
                 }*/

                Console.WriteLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"错误: {ex.Message}");
                Console.WriteLine($"确保 Hermes Agent 正在运行: hermes gateway run");
                Console.WriteLine($"API地址: http://localhost:8642");
            }

            Console.WriteLine();
            Console.WriteLine("=== 示例结束 ===");
        }
    }
}
