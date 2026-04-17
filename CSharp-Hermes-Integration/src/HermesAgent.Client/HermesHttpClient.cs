using System.Data.Common;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using static HermesAgent.Client.HermesHttpClient;

namespace HermesAgent.Client
{
    /// <summary>
    /// Hermes Agent HTTP 客户端
    /// </summary>
    public class HermesHttpClient : IHermesAgentClient, IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private readonly JsonSerializerOptions _jsonOptions;

        public HermesHttpClient()
            : this(
                baseUrl: "http://localhost:8642",
                apiKey: string.Empty,
                timeout: TimeSpan.FromSeconds(30)
            ) { }

        /// <summary>
        /// 初始化 Hermes HTTP 客户端（用于 DI typed client）
        /// </summary>
        /// <param name="httpClient">注入的 HttpClient</param>
        public HermesHttpClient(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _baseUrl = httpClient.BaseAddress?.ToString().TrimEnd('/') ?? "http://localhost:8642";

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                WriteIndented = false
            };
            _jsonOptions.Converters.Add(new JsonStringEnumConverter());
        }

        /// <summary>
        /// 初始化 Hermes HTTP 客户端
        /// </summary>
        /// <param name="baseUrl">Hermes Agent API 地址，默认 http://localhost:8642</param>
        /// <param name="apiKey">API 密钥</param>
        /// <param name="timeout">超时时间，默认 30 秒</param>
        public HermesHttpClient(
            string baseUrl = "http://localhost:8642",
            string apiKey = null,
            TimeSpan? timeout = null
        )
        {
            _baseUrl = baseUrl.TrimEnd('/');
            _httpClient = new HttpClient();

            if (timeout.HasValue)
            {
                _httpClient.Timeout = timeout.Value;
            }

            if (!string.IsNullOrEmpty(apiKey))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                    "Bearer",
                    apiKey
                );
            }

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                WriteIndented = false
            };

            // 添加自定义转换器
            _jsonOptions.Converters.Add(new JsonStringEnumConverter());
        }

        /// <summary>
        /// 发送聊天消息
        /// </summary>
        /// <param name="message">用户消息</param>
        /// <param name="model">模型名称</param>
        /// <param name="sessionId">会话ID，用于保持会话状态</param>
        /// <param name="systemPrompt">系统提示</param>
        /// <param name="temperature">温度参数，0-2，默认0.7</param>
        /// <param name="maxTokens">最大token数</param>
        /// <returns>助手回复</returns>
        public async Task<(string SessionId, string Content)> ChatAsync(
            string message,
            string model = "deepseek-chat",
            string sessionId = null,
            string systemPrompt = null,
            double? temperature = null,
            int? maxTokens = null
        )
        {
            var messages = new List<ChatMessage>();

            if (!string.IsNullOrEmpty(systemPrompt))
            {
                messages.Add(new ChatMessage { Role = "system", Content = systemPrompt });
            }

            messages.Add(new ChatMessage { Role = "user", Content = message });

            var request = new ChatCompletionRequest
            {
                Model = "deepseek-chat", //"hermes-agent",
                Messages = messages,
                Stream = false,
                Temperature = temperature,
                MaxTokens = maxTokens
            };

            var json = JsonSerializer.Serialize(request, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            if (!string.IsNullOrEmpty(sessionId))
            {
                content.Headers.Add("X-Session-ID", sessionId);
            }

            var response = await _httpClient.PostAsync($"{_baseUrl}/v1/chat/completions", content);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync();

            // Console.WriteLine(responseJson);

            var result = JsonSerializer.Deserialize<ChatCompletionResponse>(
                responseJson,
                _jsonOptions
            );

            return (
                result?.Id,
                result?.Choices?.FirstOrDefault()?.Message?.Content ?? string.Empty
            );
        }

        /// <summary>
        /// 流式聊天（Server-Sent Events）
        /// </summary>
        /// <param name="message">用户消息</param>
        /// <param name="model">模型名称</param>
        /// <param name="sessionId">会话ID</param>
        /// <param name="systemPrompt">系统提示</param>
        /// <param name="temperature">温度参数</param>
        /// <param name="maxTokens">最大token数</param>
        /// <returns>流式响应块</returns>
        public async IAsyncEnumerable<string> ChatStreamAsync(
            string message,
            string model = "deepseek-chat",
            string sessionId = null,
            string systemPrompt = null,
            double? temperature = null,
            int? maxTokens = null
        )
        {
            var messages = new List<ChatMessage>();

            if (!string.IsNullOrEmpty(systemPrompt))
            {
                messages.Add(new ChatMessage { Role = "system", Content = systemPrompt });
            }

            messages.Add(new ChatMessage { Role = "user", Content = message });

            var request = new ChatCompletionRequest
            {
                Model = model,
                Messages = messages,
                Stream = true,
                Temperature = temperature,
                MaxTokens = maxTokens
            };

            var json = JsonSerializer.Serialize(request, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            if (!string.IsNullOrEmpty(sessionId))
            {
                content.Headers.Add("X-Session-ID", sessionId);
            }

            using var requestMessage = new HttpRequestMessage(
                HttpMethod.Post,
                $"{_baseUrl}/v1/chat/completions"
            )
            {
                Content = content
            };

            using var response = await _httpClient.SendAsync(
                requestMessage,
                HttpCompletionOption.ResponseHeadersRead
            );
            response.EnsureSuccessStatusCode();

            using var stream = await response.Content.ReadAsStreamAsync();
            using var reader = new StreamReader(stream);

            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();
                if (string.IsNullOrEmpty(line) || !line.StartsWith("data: "))
                    continue;

                var data = line.Substring(6);
                if (data == "[DONE]")
                    yield break;

                var chunk = JsonSerializer.Deserialize<ChatCompletionChunk>(data, _jsonOptions);
                var contentChunk = chunk?.Choices?.FirstOrDefault()?.Delta?.Content;
                if (!string.IsNullOrEmpty(contentChunk))
                    yield return contentChunk;
            }
        }

        /// <summary>
        /// 批量处理消息
        /// </summary>
        /// <param name="messages">消息列表</param>
        /// <param name="model">模型名称</param>
        /// <param name="sessionId">会话ID</param>
        /// <param name="systemPrompt">系统提示</param>
        /// <returns>回复列表</returns>
        public async Task<List<string>> BatchChatAsync(
            IEnumerable<string> messages,
            string model = "deepseek-chat",
            string sessionId = null,
            string systemPrompt = null
        )
        {
            var results = new List<string>();

            foreach (var message in messages)
            {
                var (session, content) = await ChatAsync(message, sessionId, systemPrompt);
                results.Add(content);
            }

            return results;
        }

        /// <summary>
        /// 获取可用模型列表
        /// </summary>
        /// <returns>模型信息列表</returns>
        public async Task<List<ModelInfo>> GetModelsAsync()
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/v1/models");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ModelListResponse>(json, _jsonOptions);

            return result?.Data ?? new List<ModelInfo>();
        }

        /// <summary>
        /// 健康检查
        /// </summary>
        /// <returns>是否健康</returns>
        public async Task<bool> HealthCheckAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/health");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 创建新会话
        /// </summary>
        /// <param name="initialMessage">初始消息</param>
        /// <param name="systemPrompt">系统提示</param>
        /// <returns>会话ID</returns>
        public async Task<string> CreateSessionAsync(
            string initialMessage = null,
            string model = "deepseek-chat",
            string systemPrompt = null
        )
        {
            var sessionId = string.Empty;

            if (!string.IsNullOrEmpty(initialMessage))
            {
               var (SessionId, content) = await ChatAsync(initialMessage, model, systemPrompt);

               sessionId = SessionId;
            }

            return sessionId;
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            _httpClient?.Dispose();
            GC.SuppressFinalize(this);
        }

        #region 内部模型类


        public class ChatCompletionRequest
        {
            [JsonPropertyName("model")]
            public string Model { get; set; } = "hermes-agent";

            [JsonPropertyName("messages")]
            public List<ChatMessage> Messages { get; set; } = new();

            [JsonPropertyName("stream")]
            public bool Stream { get; set; } = false;

            [JsonPropertyName("temperature")]
            public double? Temperature { get; set; } = 0.7;

            [JsonPropertyName("max_tokens")]
            public int? MaxTokens { get; set; }
        }

        public class ChatMessage
        {
            [JsonPropertyName("role")]
            public string Role { get; set; } // "system", "user", "assistant"

            [JsonPropertyName("content")]
            public string Content { get; set; }
        }

        public class ChatCompletionResponse
        {
            [JsonPropertyName("id")]
            public string Id { get; set; }

            [JsonPropertyName("object")]
            public string Object { get; set; }

            [JsonPropertyName("created")]
            public long Created { get; set; }

            [JsonPropertyName("model")]
            public string Model { get; set; }

            [JsonPropertyName("choices")]
            public List<ChatChoice> Choices { get; set; }

            [JsonPropertyName("usage")]
            public TokenUsage Usage { get; set; }
        }

        public class ChatChoice
        {
            [JsonPropertyName("index")]
            public int Index { get; set; }

            [JsonPropertyName("message")]
            public ChatMessage Message { get; set; }

            [JsonPropertyName("finish_reason")]
            public string FinishReason { get; set; }
        }

        public class ChatCompletionChunk
        {
            [JsonPropertyName("id")]
            public string Id { get; set; }

            [JsonPropertyName("object")]
            public string Object { get; set; }

            [JsonPropertyName("choices")]
            public List<ChatChoiceChunk> Choices { get; set; }
        }

        public class ChatChoiceChunk
        {
            [JsonPropertyName("index")]
            public int Index { get; set; }

            [JsonPropertyName("delta")]
            public ChatDelta Delta { get; set; }

            [JsonPropertyName("finish_reason")]
            public string FinishReason { get; set; }
        }

        public class ChatDelta
        {
            [JsonPropertyName("role")]
            public string Role { get; set; }

            [JsonPropertyName("content")]
            public string Content { get; set; }
        }

        public class TokenUsage
        {
            [JsonPropertyName("prompt_tokens")]
            public int PromptTokens { get; set; }

            [JsonPropertyName("completion_tokens")]
            public int CompletionTokens { get; set; }

            [JsonPropertyName("total_tokens")]
            public int TotalTokens { get; set; }
        }

        public class ModelListResponse
        {
            [JsonPropertyName("object")]
            public string Object { get; set; }

            [JsonPropertyName("data")]
            public List<ModelInfo> Data { get; set; }
        }

        public class ModelInfo
        {
            [JsonPropertyName("id")]
            public string Id { get; set; }

            [JsonPropertyName("object")]
            public string Object { get; set; }

            [JsonPropertyName("created")]
            public long Created { get; set; }

            [JsonPropertyName("owned_by")]
            public string OwnedBy { get; set; }
        }

        #endregion
    }

    /// <summary>
    /// Hermes Agent 客户端接口
    /// </summary>
    public interface IHermesAgentClient : IDisposable
    {
        /// <summary>
        /// 发送聊天消息
        /// </summary>
        Task<(string SessionId, string Content)> ChatAsync(
            string message,
            string model = "deepseek-chat",
            string sessionId = null,
            string systemPrompt = null,
            double? temperature = null,
            int? maxTokens = null
        );

        /// <summary>
        /// 流式聊天
        /// </summary>
        IAsyncEnumerable<string> ChatStreamAsync(
            string message,
            string model = "deepseek-chat",
            string sessionId = null,
            string systemPrompt = null,
            double? temperature = null,
            int? maxTokens = null
        );

        /// <summary>
        /// 批量处理消息
        /// </summary>
        Task<List<string>> BatchChatAsync(
            IEnumerable<string> messages,
            string model = "deepseek-chat",
            string sessionId = null,
            string systemPrompt = null
        );

        /// <summary>
        /// 获取可用模型列表
        /// </summary>
        Task<List<ModelInfo>> GetModelsAsync();

        /// <summary>
        /// 健康检查
        /// </summary>
        Task<bool> HealthCheckAsync();

        /// <summary>
        /// 创建新会话
        /// </summary>
        Task<string> CreateSessionAsync(
            string initialMessage = null,
            string model = "deepseek-chat",
            string systemPrompt = null
        );
    }
}
