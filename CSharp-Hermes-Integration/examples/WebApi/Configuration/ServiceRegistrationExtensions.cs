namespace HermesAgent.Examples.WebApi.Configuration
{
    /// <summary>
    /// 服务注册扩展方法
    /// </summary>
    public static class ServiceRegistrationExtensions
    {
        /// <summary>
        /// 注册应用程序服务
        /// </summary>
        public static IServiceCollection AddApplicationServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(configuration);

            // 注册 Hermes Agent 客户端和相关服务
            var section = configuration.GetSection("HermesAgent");
            if (section.Exists())
            {
                services.Configure<HermesAgentOptions>(section);
                services.AddHttpClient<IHermesAgentClient, HermesHttpClient>();
            }

            // 注册控制器
            services.AddControllers();

            // 注册 API 探索
            services.AddEndpointsApiExplorer();

            return services;
        }

        /// <summary>
        /// 配置应用程序中间件
        /// </summary>
        public static WebApplication ConfigureApplicationMiddleware(this WebApplication app)
        {
            ArgumentNullException.ThrowIfNull(app);

            // 在开发环境中启用 Swagger
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Hermes Agent API v1");
                    options.RoutePrefix = string.Empty;
                });
            }

            return app;
        }

        /// <summary>
        /// 配置路由和端点
        /// </summary>
        public static WebApplication ConfigureRouting(this WebApplication app)
        {
            ArgumentNullException.ThrowIfNull(app);

            app.MapControllers();

            // 添加健康检查端点
            app.MapHealthChecks("/health");

            return app;
        }
    }
}
