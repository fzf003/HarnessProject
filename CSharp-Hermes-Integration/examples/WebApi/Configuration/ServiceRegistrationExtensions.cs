using HermesAgent.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerUI;

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

            // 注册 Swagger 配置
            var swaggerSection = configuration.GetSection("Swagger");
            if (swaggerSection.Exists())
            {
                services.Configure<SwaggerOptions>(swaggerSection);
            }
            else
            {
                // 如果没有配置，使用默认值
                services.Configure<SwaggerOptions>(options => { });
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

            // 获取 Swagger 配置
            var swaggerOptions = app.Services.GetService<IOptions<SwaggerOptions>>()?.Value 
                ?? new SwaggerOptions();

            // 判断是否应该启用 Swagger
            bool shouldEnableSwagger = ShouldEnableSwagger(app, swaggerOptions);

            if (shouldEnableSwagger)
            {
                app.UseSwagger();
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/swagger/v1/swagger.json", swaggerOptions.DocumentTitle);
                    options.RoutePrefix = swaggerOptions.RoutePrefix;
                    
                    // 配置 Swagger UI 选项
                    if (!swaggerOptions.EnableTryItOut)
                    {
                        options.DefaultModelsExpandDepth(-1);
                        options.DefaultModelExpandDepth(0);
                        options.DisplayRequestDuration();
                        options.DocExpansion(DocExpansion.None);
                    }
                    
                    if (swaggerOptions.EnableDeepLinking)
                    {
                        options.EnableDeepLinking();
                    }
                });
            }

            return app;
        }

        /// <summary>
        /// 判断是否应该启用 Swagger
        /// </summary>
        private static bool ShouldEnableSwagger(WebApplication app, SwaggerOptions options)
        {
            // 如果配置明确启用，则启用
            if (options.Enabled)
            {
                return true;
            }

            // 如果配置禁用，但在开发环境中且强制启用，则启用
            if (!options.Enabled && app.Environment.IsDevelopment() && options.ForceEnableInDevelopment)
            {
                return true;
            }

            // 其他情况不启用
            return false;
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
