namespace HermesAgent.Examples.WebApi.Configuration
{
    /// <summary>
    /// Swagger 配置选项
    /// </summary>
    public class SwaggerOptions
    {
        /// <summary>
        /// 是否启用 Swagger
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Swagger UI 路由前缀
        /// </summary>
        public string RoutePrefix { get; set; } = "swagger";

        /// <summary>
        /// 文档标题
        /// </summary>
        public string DocumentTitle { get; set; } = "Hermes Agent API";

        /// <summary>
        /// 文档版本
        /// </summary>
        public string DocumentVersion { get; set; } = "v1";

        /// <summary>
        /// 是否包含安全配置
        /// </summary>
        public bool IncludeSecurity { get; set; } = true;

        /// <summary>
        /// 是否启用"Try it out"功能
        /// </summary>
        public bool EnableTryItOut { get; set; } = true;

        /// <summary>
        /// 是否启用深度链接
        /// </summary>
        public bool EnableDeepLinking { get; set; } = true;

        /// <summary>
        /// 是否在开发环境中强制启用（即使配置为禁用）
        /// </summary>
        public bool ForceEnableInDevelopment { get; set; } = true;
    }
}