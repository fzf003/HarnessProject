using System.Data.Common;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using static HermesAgent.Client.HermesHttpClient;

using HermesAgent.Client.Models;

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
        /// 流式聊天（Server-Sent Events）
        /// </summary>
        /// <param name="message">用户消息</param>
        /// <param name="model">模型名称</param>
        /// <param name="sessionId">会话ID</param>
        /// <param name="systemPrompt">系统提示</param>
        /// <param name="temperature">温度参数</param>
        /// <param name="maxTokens">最大token数</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>流式响应块</returns>
        public async IAsyncEnumerable<string> ChatStreamAsync(
            string message,
            string model = "deepseek-chat",
            string? sessionId = null,
            string? systemPrompt = null,
            double? temperature = null,
            int? maxTokens = null,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
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
        /// 发送聊天消息（带模型参数）
        /// </summary>
        /// <param name="message">消息内容</param>
        /// <param name="model">模型名称</param>
        /// <param name="sessionId">会话ID</param>
        /// <param name="systemPrompt">系统提示</param>
        /// <param name="temperature">温度参数，0-2，默认0.7</param>
        /// <param name="maxTokens">最大token数</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>助手回复</returns>
        public async Task<(string? SessionId, string Content)> ChatWithModelAsync(
            string message,
            string model = "deepseek-chat",
            string? sessionId = null,
            string? systemPrompt = null,
            double? temperature = null,
            int? maxTokens = null,
            CancellationToken cancellationToken = default)
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

            using var requestMessage = new HttpRequestMessage(
                HttpMethod.Post,
                $"{_baseUrl}/v1/chat/completions"
            )
            {
                Content = content
            };

            using var response = await _httpClient.SendAsync(
                requestMessage,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken
            );
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync();
            var completion = JsonSerializer.Deserialize<ChatCompletionResponse>(responseJson, _jsonOptions);

            if (completion == null)
                throw new HermesApiException("API响应解析失败");

            var sessionIdFromResponse = response.Headers.TryGetValues("X-Session-ID", out var values)
                ? values.FirstOrDefault()
                : null;

            return (sessionIdFromResponse, completion.Choices?.FirstOrDefault()?.Message?.Content ?? string.Empty);
        }

        /// <summary>
        /// 发送聊天消息（接口实现）
        /// </summary>
        /// <param name="message">消息内容</param>
        /// <param name="sessionId">会话ID</param>
        /// <param name="systemPrompt">系统提示</param>
        /// <param name="temperature">温度参数</param>
        /// <param name="maxTokens">最大token数</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>聊天响应</returns>
        public async Task<ChatResponse> ChatAsync(
            string message,
            string? sessionId = null,
            string? systemPrompt = null,
            double? temperature = null,
            int? maxTokens = null,
            CancellationToken cancellationToken = default)
        {
            var (session, content) = await ChatWithModelAsync(message, "deepseek-chat", sessionId, systemPrompt, temperature, maxTokens, cancellationToken);
            
            return new ChatResponse
            {
                Content = content,
                SessionId = session ?? string.Empty,
                Model = "deepseek-chat"
            };
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
            string? sessionId = null,
            string? systemPrompt = null
        )
        {
            var results = new List<string>();

            foreach (var message in messages)
            {
                var (session, content) = await ChatAsync(message, model, sessionId, systemPrompt);
                results.Add(content);
            }

            return results;
        }



        /// <summary>
        /// 健康检查（接口实现）
        /// </summary>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>是否健康</returns>
        public async Task<bool> HealthCheckAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/health", cancellationToken);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 创建新会话（接口实现）
        /// </summary>
        /// <param name="initialMessage">初始消息</param>
        /// <param name="systemPrompt">系统提示</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>会话ID</returns>
        public async Task<string> CreateSessionAsync(
            string? initialMessage = null,
            string? systemPrompt = null,
            CancellationToken cancellationToken = default)
        {
            var sessionId = string.Empty;

            if (!string.IsNullOrEmpty(initialMessage))
            {
                var (SessionId, content) = await ChatWithModelAsync(initialMessage, "deepseek-chat", sessionId: null, systemPrompt: systemPrompt, cancellationToken: cancellationToken);
                sessionId = SessionId ?? string.Empty;
            }

            return sessionId;
        }

        /// <summary>
        /// 执行命令（进程调用模式）
        /// </summary>
        /// <param name="command">命令</param>
        /// <param name="sessionId">会话ID</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>命令输出</returns>
        public async Task<string> ExecuteCommandAsync(
            string command,
            string? sessionId = null,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(command))
                throw new ArgumentException("命令不能为空", nameof(command));

        }
        /// <summary>
        /// 获取可用模型列表
        /// </summary>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>模型列表</returns>
        public async Task<IReadOnlyList<string>> GetModelsAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/api/models", cancellationToken);
                response.EnsureSuccessStatusCode();

                var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
                var models = JsonSerializer.Deserialize<List<ModelInfo>>(responseJson, _jsonOptions);

                return models?.Select(m => m.Name).ToList().AsReadOnly() ?? new List<string>().AsReadOnly();
            }
            catch (HttpRequestException ex)
            {
                throw new HermesApiException((int?)ex.StatusCode ?? 500, "获取模型列表失败", ex.Message);
            }
            catch (Exception ex)
            {
                throw new HermesAgentException("MODELS_FETCH_ERROR", "获取模型列表时发生错误", ex);
            }
        }

        /// <summary>
        /// 流式发送聊天消息
        /// </summary>
        /// <param name="message">消息内容</param>
        /// <param name="sessionId">会话ID</param>
        /// <param name="systemPrompt">系统提示</param>
        /// <param name="temperature">温度参数</param>
        /// <param name="maxTokens">最大token数</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>流式响应枚举</returns>
        public async IAsyncEnumerable<string> ChatStreamAsync(
            string message,
            string? sessionId = null,
            string? systemPrompt = null,
            double? temperature = null,
            int? maxTokens = null,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentException("消息不能为空", nameof(message));

            var request = new ChatCompletionRequest
            {
                Model = "hermes-agent",
                Messages = new List<ChatMessage>
                {
                    new ChatMessage { Role = "user", Content = message }
                },
                Stream = true,
                Temperature = temperature,
                MaxTokens = maxTokens
            };

            if (!string.IsNullOrWhiteSpace(systemPrompt))
            {
                request.Messages.Insert(0, new ChatMessage { Role = "system", Content = systemPrompt });
            }

            var jsonContent = JsonSerializer.Serialize(request, _jsonOptions);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(
                $"{_baseUrl}/api/chat/completions",
                content,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);

            response.EnsureSuccessStatusCode();

            using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var reader = new StreamReader(stream);

            while (!cancellationToken.IsCancellationRequested)
            {
                var line = await reader.ReadLineAsync(cancellationToken);
                if (line == null) break;

                if (line.StartsWith("data: "))
                {
                    var data = line.Substring(6);
                    if (data == "[DONE]") break;

                    try
                    {
                        var chunk = JsonSerializer.Deserialize<ChatCompletionChunk>(data, _jsonOptions);
                        if (chunk?.Choices?.FirstOrDefault()?.Delta?.Content is string contentChunk)
                        {
                            yield return contentChunk;
                        }
                    }
                    catch (JsonException)
                    {
                        // 忽略解析错误，继续读取下一行
                    }
                }
            }
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


        public class ExecuteCommandResponse
        {
            [JsonPropertyName("output")]
            public string Output { get; set; } = string.Empty;

            [JsonPropertyName("exit_code")]
            public int ExitCode { get; set; }

            [JsonPropertyName("success")]
            public bool Success { get; set; }

            [JsonPropertyName("error")]
            public string? Error { get; set; }
        }

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
            public string? Role { get; set; } // "system", "user", "assistant"

            [JsonPropertyName("content")]
            public string? Content { get; set; }
        }

        public class ChatCompletionResponse
        {
            [JsonPropertyName("id")]
            public string? Id { get; set; }

            [JsonPropertyName("object")]
            public string? Object { get; set; }

            [JsonPropertyName("created")]
            public long Created { get; set; }

            [JsonPropertyName("model")]
            public string? Model { get; set; }

            [JsonPropertyName("choices")]
            public List<ChatChoice>? Choices { get; set; }

            [JsonPropertyName("usage")]
            public TokenUsage? Usage { get; set; }
        }

        public class ChatChoice
        {
            [JsonPropertyName("index")]
            public int Index { get; set; }

            [JsonPropertyName("message")]
            public ChatMessage? Message { get; set; }

            [JsonPropertyName("finish_reason")]
            public string? FinishReason { get; set; }
        }

        public class ChatCompletionChunk
        {
            [JsonPropertyName("id")]
            public string? Id { get; set; }

            [JsonPropertyName("object")]
            public string? Object { get; set; }

            [JsonPropertyName("choices")]
            public List<ChatChoiceChunk>? Choices { get; set; }
        }

        public class ChatChoiceChunk
        {
            [JsonPropertyName("index")]
            public int Index { get; set; }

            [JsonPropertyName("delta")]
            public ChatDelta? Delta { get; set; }

            [JsonPropertyName("finish_reason")]
            public string? FinishReason { get; set; }
        }

        public class ChatDelta
        {
            [JsonPropertyName("role")]
            public string? Role { get; set; }

            [JsonPropertyName("content")]
            public string? Content { get; set; }
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
            public string? Object { get; set; }

            [JsonPropertyName("data")]
            public List<ModelInfo>? Data { get; set; }
        }

        public class ModelInfo
        {
            [JsonPropertyName("id")]
            public string? Id { get; set; }

            [JsonPropertyName("object")]
            public string? Object { get; set; }

            [JsonPropertyName("created")]
            public long Created { get; set; }

            [JsonPropertyName("owned_by")]
            public string? OwnedBy { get; set; }
        }

        #endregion
    }
}
