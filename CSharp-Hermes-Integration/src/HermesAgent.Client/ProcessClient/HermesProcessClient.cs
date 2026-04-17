using System.Diagnostics;
using System.Text;

namespace HermesAgent.Client;

/// <summary>
/// Hermes Agent 进程调用客户端
/// 通过直接调用 hermes CLI 命令与 Agent 交互
/// </summary>
public class HermesProcessClient : IDisposable
{
    private readonly string _hermesPath;
    private readonly string _workingDirectory;
    private readonly TimeSpan _timeout;
    private readonly Dictionary<string, ProcessSession> _activeSessions = new();
    private bool _disposed;

    /// <summary>
    /// 创建进程客户端
    /// </summary>
    /// <param name="hermesPath">Hermes 可执行文件路径</param>
    /// <param name="workingDirectory">工作目录</param>
    /// <param name="timeout">命令超时时间</param>
    public HermesProcessClient(
        string hermesPath = "hermes",
        string workingDirectory = null,
        TimeSpan? timeout = null)
    {
        _hermesPath = hermesPath;
        _workingDirectory = workingDirectory ?? Directory.GetCurrentDirectory();
        _timeout = timeout ?? TimeSpan.FromMinutes(5);
    }

    /// <summary>
    /// 执行命令（非交互式，单次查询）
    /// </summary>
    /// <param name="command">要执行的命令/问题</param>
    /// <param name="sessionId">可选的会话 ID（用于保持上下文）</param>
    /// <returns>Hermes 的响应文本</returns>
    public async Task<string> ExecuteCommandAsync(string command, string sessionId = null)
    {
        ThrowIfDisposed();

        var arguments = new StringBuilder();
        arguments.Append("chat -q");

        if (!string.IsNullOrEmpty(sessionId))
        {
            arguments.Append($" --resume {sessionId}");
        }

        // 转义命令中的特殊字符
        var escapedCommand = command.Replace("\"", "\\\"").Replace("\\", "\\\\");
        arguments.Append($" \"{escapedCommand}\"");

        return await ExecuteProcessAsync(arguments.ToString());
    }

    /// <summary>
    /// 启动交互式会话
    /// </summary>
    /// <param name="sessionId">会话 ID（为空则自动生成）</param>
    /// <returns>会话 ID</returns>
    public async Task<string> StartInteractiveSessionAsync(string sessionId = null)
    {
        ThrowIfDisposed();

        sessionId ??= $"session_{DateTime.Now:yyyyMMdd_HHmmss}_{Guid.NewGuid():N8}";

        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = _hermesPath,
                Arguments = $"--resume {sessionId}",
                WorkingDirectory = _workingDirectory,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8
            }
        };

        process.Start();

        _activeSessions[sessionId] = new ProcessSession
        {
            Process = process,
            SessionId = sessionId,
            StartTime = DateTime.Now
        };

        // 等待 Hermes 启动
        await Task.Delay(2000);

        return sessionId;
    }

    /// <summary>
    /// 向会话发送消息
    /// </summary>
    /// <param name="sessionId">会话 ID</param>
    /// <param name="message">消息内容</param>
    /// <returns>Hermes 的响应</returns>
    public async Task<string> SendMessageAsync(string sessionId, string message)
    {
        ThrowIfDisposed();

        if (!_activeSessions.TryGetValue(sessionId, out var session))
        {
            throw new InvalidOperationException($"会话 {sessionId} 不存在");
        }

        // 发送消息
        await session.Process.StandardInput.WriteLineAsync(message);
        await session.Process.StandardInput.FlushAsync();

        // 读取响应（简化实现，实际可能需要更复杂的逻辑）
        var output = await session.Process.StandardOutput.ReadLineAsync();
        return output ?? string.Empty;
    }

    /// <summary>
    /// 结束会话
    /// </summary>
    /// <param name="sessionId">会话 ID</param>
    public async Task<bool> StopSessionAsync(string sessionId)
    {
        ThrowIfDisposed();

        if (!_activeSessions.TryGetValue(sessionId, out var session))
        {
            return false;
        }

        try
        {
            // 发送退出命令
            await session.Process.StandardInput.WriteLineAsync("/quit");
            await session.Process.StandardInput.FlushAsync();

            // 等待进程退出
            await session.Process.WaitForExitAsync();
           // if (!exited)
            {
                session.Process.Kill();
            }

            _activeSessions.Remove(sessionId);
            return true;
        }
        catch
        {
            session.Process.Kill();
            _activeSessions.Remove(sessionId);
            return false;
        }
    }

    /// <summary>
    /// 获取活动会话列表
    /// </summary>
    public IReadOnlyList<ProcessSession> GetActiveSessions()
    {
        ThrowIfDisposed();
        return _activeSessions.Values.ToList().AsReadOnly();
    }

    /// <summary>
    /// 执行进程并获取输出
    /// </summary>
    private async Task<string> ExecuteProcessAsync(string arguments)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = _hermesPath,
                Arguments = arguments,
                WorkingDirectory = _workingDirectory,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8
            }
        };

        var output = new StringBuilder();
        var error = new StringBuilder();

        process.OutputDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
                output.AppendLine(e.Data);
        };

        process.ErrorDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
                error.AppendLine(e.Data);
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        // 等待完成或超时
     await process.WaitForExitAsync();
      //  if (!completed)
        {
            process.Kill();
            throw new TimeoutException($"Hermes 命令执行超时（{_timeout.TotalSeconds}秒）");
        }

        if (process.ExitCode != 0)
        {
            throw new HermesProcessException(
                $"Hermes 进程退出码：{process.ExitCode}",
                error.ToString(),
                process.ExitCode);
        }

        return output.ToString().TrimEnd();
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(HermesProcessClient));
        }
    }

    public void Dispose()
    {
        if (_disposed) return;

        // 清理所有活动会话
        foreach (var session in _activeSessions.Values)
        {
            try
            {
                if (!session.Process.HasExited)
                {
                    session.Process.Kill();
                }
                session.Process.Dispose();
            }
            catch
            {
                // 忽略清理错误
            }
        }

        _activeSessions.Clear();
        _disposed = true;
    }
}

/// <summary>
/// 进程会话信息
/// </summary>
public class ProcessSession
{
    public Process Process { get; set; }
    public string SessionId { get; set; }
    public DateTime StartTime { get; set; }
    public TimeSpan Duration => DateTime.Now - StartTime;
}

/// <summary>
/// Hermes 进程异常
/// </summary>
public class HermesProcessException : Exception
{
    public int ExitCode { get; }
    public string ErrorOutput { get; }

    public HermesProcessException(string message, string errorOutput, int exitCode)
        : base(message)
    {
        ErrorOutput = errorOutput;
        ExitCode = exitCode;
    }
}
