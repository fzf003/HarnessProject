using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using HermesAgent.Client.Models;

namespace HermesAgent.Client
{
    /// <summary>
    /// Hermes Agent 客户端接口
    /// </summary>
    public interface IHermesAgentClient : IDisposable
    {
        /// <summary>
        /// 发送聊天消息
        /// </summary>
        /// <param name="message">消息内容</param>
        /// <param name="sessionId">会话ID</param>
        /// <param name="systemPrompt">系统提示</param>
        /// <param name="temperature">温度参数</param>
        /// <param name="maxTokens">最大token数</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>聊天响应</returns>
        Task<ChatResponse> ChatAsync(
            string message,
            string? sessionId = null,
            string? systemPrompt = null,
            double? temperature = null,
            int? maxTokens = null,
            CancellationToken cancellationToken = default);

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
        IAsyncEnumerable<string> ChatStreamAsync(
            string message,
            string? sessionId = null,
            string? systemPrompt = null,
            double? temperature = null,
            int? maxTokens = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// 创建新会话
        /// </summary>
        /// <param name="initialMessage">初始消息</param>
        /// <param name="systemPrompt">系统提示</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>会话ID</returns>
        Task<string> CreateSessionAsync(
            string? initialMessage = null,
            string? systemPrompt = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// 获取可用模型列表
        /// </summary>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>模型列表</returns>
        Task<IReadOnlyList<string>> GetModelsAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// 健康检查
        /// </summary>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>是否健康</returns>
        Task<bool> HealthCheckAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// 执行命令（进程调用模式）
        /// </summary>
        /// <param name="command">命令</param>
        /// <param name="sessionId">会话ID</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>命令输出</returns>
        Task<string> ExecuteCommandAsync(
            string command,
            string? sessionId = null,
            CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Hermes Webhook 客户端接口
    /// </summary>
    public interface IHermesWebhookClient : IDisposable
    {
        /// <summary>
        /// 发送 Webhook 事件
        /// </summary>
        /// <param name="event">事件数据</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>是否成功</returns>
        Task<bool> SendEventAsync(WebhookEvent @event, CancellationToken cancellationToken = default);

        /// <summary>
        /// 验证 Webhook 签名
        /// </summary>
        /// <param name="payload">载荷</param>
        /// <param name="signature">签名</param>
        /// <param name="secret">密钥</param>
        /// <returns>是否验证通过</returns>
        bool VerifySignature(string payload, string signature, string secret);
    }
}