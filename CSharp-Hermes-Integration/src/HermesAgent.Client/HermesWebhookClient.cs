using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;

using HermesAgent.Client.Models;
using HermesAgent.Client.Exceptions;

using static HermesAgent.Client.HermesWebhookClient;

namespace HermesAgent.Client
{
    /// <summary>
    /// Hermes Agent Webhook 客户端
    /// </summary>
    public class HermesWebhookClient : IDisposable, IHermesWebhookClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _webhookUrl;
        private readonly string? _secret;
        private readonly JsonSerializerOptions _jsonOptions;

        /// <summary>
        /// 初始化 Webhook 客户端
        /// </summary>
        /// <param name="baseUrl">Hermes Agent Webhook 地址，默认 http://localhost:8644</param>
        /// <param name="route">Webhook 路由名称</param>
        /// <param name="secret">HMAC 密钥</param>
        /// <param name="timeout">超时时间</param>
        public HermesWebhookClient(
            string baseUrl = "http://localhost:8644", 
            string route = "dotnet-webhook", 
            string? secret = null,
            TimeSpan? timeout = null)
        {
            _webhookUrl = $"{baseUrl.TrimEnd('/')}/webhooks/{route}";
            _secret = secret;
            _httpClient = new HttpClient();
            
            if (timeout.HasValue)
            {
                _httpClient.Timeout = timeout.Value;
            }
            
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                WriteIndented = false
            };
        }

        /// <summary>
        /// 初始化 Webhook 客户端（用于 DI typed client）
        /// </summary>
        /// <param name="httpClient">注入的 HttpClient</param>
        /// <param name="options">Hermes Agent 配置</param>
        public HermesWebhookClient(HttpClient httpClient, IOptions<HermesAgentOptions> options)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            var opts = options?.Value ?? new HermesAgentOptions();
            var baseUrl = httpClient.BaseAddress?.ToString().TrimEnd('/') ?? opts.BaseUrl.TrimEnd('/');
            _webhookUrl = $"{baseUrl}/webhooks/{opts.WebhookRoute}";
            _secret = opts.WebhookSecret;

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                WriteIndented = false
            };
        }

        /// <summary>
        /// 发送 Webhook 事件
        /// </summary>
        /// <param name="eventType">事件类型</param>
        /// <param name="payload">事件数据</param>
        /// <param name="headers">自定义头部</param>
        /// <returns>是否成功</returns>
        public async Task<bool> SendEventAsync(
            string eventType, 
            object payload, 
            Dictionary<string, string>? headers = null)
        {
            var payloadJson = JsonSerializer.Serialize(payload, _jsonOptions);
            var content = new StringContent(payloadJson, Encoding.UTF8, "application/json");
            
            // 添加事件类型头
            content.Headers.Add("X-Event-Type", eventType);
            
            // 添加自定义头部
            if (headers != null)
            {
                foreach (var header in headers)
                {
                    content.Headers.Add(header.Key, header.Value);
                }
            }
            
            // 如果配置了密钥，添加签名
            if (!string.IsNullOrEmpty(_secret))
            {
                var signature = ComputeHmacSha256(payloadJson, _secret);
                content.Headers.Add("X-Signature", signature);
            }
            
            try
            {
                var response = await _httpClient.PostAsync(_webhookUrl, content);
                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException ex)
            {
                throw new HermesApiException(500, $"Webhook 请求失败: {ex.Message}", null);
            }
        }

        /// <summary>
        /// 发送 Webhook 事件（接口实现）
        /// </summary>
        /// <param name="event">事件数据</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>是否成功</returns>
        public async Task<bool> SendEventAsync(WebhookEvent @event, CancellationToken cancellationToken = default)
        {
            var payloadJson = JsonSerializer.Serialize(@event.Data, _jsonOptions);
            var content = new StringContent(payloadJson, Encoding.UTF8, "application/json");
            
            // 添加事件类型头
            content.Headers.Add("X-Event-Type", @event.Event);
            
            // 如果事件包含签名，使用它
            if (!string.IsNullOrEmpty(@event.Signature))
            {
                content.Headers.Add("X-Signature", @event.Signature);
            }
            // 否则，如果配置了密钥，计算签名
            else if (!string.IsNullOrEmpty(_secret))
            {
                var signature = ComputeHmacSha256(payloadJson, _secret);
                content.Headers.Add("X-Signature", signature);
            }
            
            try
            {
                var response = await _httpClient.PostAsync(_webhookUrl, content, cancellationToken);
                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException ex)
            {
                throw new HermesApiException(500, $"Webhook 请求失败: {ex.Message}", null);
            }
        }
        public bool ValidateSignature(string payload, string signature)
        {
            if (string.IsNullOrEmpty(_secret) || string.IsNullOrEmpty(signature))
                return false;
            
            var expectedSignature = CalculateHmac(payload, _secret);
            
            // 移除可能的 "sha256=" 前缀
            var cleanSignature = signature.Replace("sha256=", "", StringComparison.OrdinalIgnoreCase);
            
            return string.Equals(expectedSignature, cleanSignature, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 验证 Webhook 签名（接口实现）
        /// </summary>
        /// <param name="payload">载荷</param>
        /// <param name="signature">签名</param>
        /// <param name="secret">密钥</param>
        /// <returns>是否验证通过</returns>
        public bool VerifySignature(string payload, string signature, string secret)
        {
            if (string.IsNullOrEmpty(secret) || string.IsNullOrEmpty(signature))
                return false;
            
            var expectedSignature = CalculateHmac(payload, secret);
            
            // 移除可能的 "sha256=" 前缀
            var cleanSignature = signature.Replace("sha256=", "", StringComparison.OrdinalIgnoreCase);
            
            return string.Equals(expectedSignature, cleanSignature, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 发送 GitHub 推送事件
        /// </summary>
        /// <param name="repository">仓库信息</param>
        /// <param name="commits">提交列表</param>
        /// <param name="sender">发送者</param>
        /// <returns>是否成功</returns>
        public async Task<bool> SendGitHubPushEventAsync(
            GitHubRepository repository,
            List<GitHubCommit> commits,
            GitHubUser? sender = null)
        {
            var payload = new GitHubPushEvent
            {
                Ref = "refs/heads/main",
                Before = "abc123",
                After = "def456",
                Repository = repository,
                Pusher = new GitHubUser { Name = "pusher", Email = "pusher@example.com" },
                Sender = sender,
                Commits = commits
            };
            
            return await SendEventAsync("github.push", payload);
        }

        /// <summary>
        /// 发送 .NET 构建事件
        /// </summary>
        /// <param name="project">项目名称</param>
        /// <param name="status">构建状态</param>
        /// <param name="duration">构建时长（秒）</param>
        /// <param name="output">构建输出</param>
        /// <returns>是否成功</returns>
        public async Task<bool> SendDotNetBuildEventAsync(
            string project,
            BuildStatus status,
            int duration,
            string? output = null)
        {
            var payload = new DotNetBuildEvent
            {
                Project = project,
                Status = status.ToString().ToLower(),
                Duration = duration,
                Output = output,
                Timestamp = DateTime.UtcNow
            };
            
            return await SendEventAsync("dotnet.build", payload);
        }

        /// <summary>
        /// 计算 HMAC-SHA256 签名
        /// </summary>
        private string CalculateHmac(string payload, string secret)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            _httpClient?.Dispose();
            GC.SuppressFinalize(this);
        }

        #region 事件模型类

        public class GitHubPushEvent
        {
            [JsonPropertyName("ref")]
            public string? Ref { get; set; }
            
            [JsonPropertyName("before")]
            public string? Before { get; set; }
            
            [JsonPropertyName("after")]
            public string? After { get; set; }
            
            [JsonPropertyName("repository")]
            public GitHubRepository? Repository { get; set; }
            
            [JsonPropertyName("pusher")]
            public GitHubUser? Pusher { get; set; }
            
            [JsonPropertyName("sender")]
            public GitHubUser? Sender { get; set; }
            
            [JsonPropertyName("commits")]
            public List<GitHubCommit> Commits { get; set; } = new();
        }

        public class GitHubRepository
        {
            [JsonPropertyName("id")]
            public long Id { get; set; }
            
            [JsonPropertyName("name")]
            public string? Name { get; set; }
            
            [JsonPropertyName("full_name")]
            public string? FullName { get; set; }
            
            [JsonPropertyName("html_url")]
            public string? HtmlUrl { get; set; }
            
            [JsonPropertyName("description")]
            public string? Description { get; set; }
        }

        public class GitHubUser
        {
            [JsonPropertyName("id")]
            public long Id { get; set; }
            
            [JsonPropertyName("login")]
            public string? Login { get; set; }
            
            [JsonPropertyName("name")]
            public string? Name { get; set; }
            
            [JsonPropertyName("email")]
            public string? Email { get; set; }
            
            [JsonPropertyName("avatar_url")]
            public string? AvatarUrl { get; set; }
        }

        public class GitHubCommit
        {
            [JsonPropertyName("id")]
            public string? Id { get; set; }
            
            [JsonPropertyName("message")]
            public string? Message { get; set; }
            
            [JsonPropertyName("timestamp")]
            public DateTime Timestamp { get; set; }
            
            [JsonPropertyName("author")]
            public GitHubUser? Author { get; set; }
            
            [JsonPropertyName("committer")]
            public GitHubUser? Committer { get; set; }
            
            [JsonPropertyName("added")]
            public List<string> Added { get; set; } = new();
            
            [JsonPropertyName("removed")]
            public List<string> Removed { get; set; } = new();
            
            [JsonPropertyName("modified")]
            public List<string> Modified { get; set; } = new();
        }

        public class DotNetBuildEvent
        {
            [JsonPropertyName("project")]
            public string? Project { get; set; }
            
            [JsonPropertyName("status")]
            public string? Status { get; set; } // "success", "failure", "cancelled"
            
            [JsonPropertyName("duration")]
            public int Duration { get; set; } // 秒
            
            [JsonPropertyName("output")]
            public string? Output { get; set; }
            
            [JsonPropertyName("timestamp")]
            public DateTime Timestamp { get; set; }
            
            [JsonPropertyName("framework")]
            public string? Framework { get; set; } = "net8.0";
            
            [JsonPropertyName("configuration")]
            public string? Configuration { get; set; } = "Release";
        }

        public enum BuildStatus
        {
            Success,
            Failure,
            Cancelled
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 计算 HMAC-SHA256 签名
        /// </summary>
        /// <param name="data">要签名的数据</param>
        /// <param name="key">密钥</param>
        /// <returns>十六进制格式的签名</returns>
        private static string ComputeHmacSha256(string data, string key)
        {
            using var hmac = new System.Security.Cryptography.HMACSHA256(System.Text.Encoding.UTF8.GetBytes(key));
            var hash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(data));
            return Convert.ToHexString(hash).ToLowerInvariant();
        }

        #endregion
    }
}