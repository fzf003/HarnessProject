using HermesAgent.Client;
using HermesAgent.Examples.WebApi.Configuration;
using HermesAgent.Examples.WebApi.Middleware;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerUI;
using System.Reflection;

namespace HermesAgent.Examples.WebApi
{
    /// <summary>
    /// 应用程序启动类
    /// 配置依赖注入、中间件和路由
    /// </summary>
    public class Program
    {
        /// <summary>
        /// 应用程序入口点
        /// </summary>
        /// <param name="args">命令行参数</param>
        public static void Main(string[] args)
        {

            var builder = CreateHostBuilder(args);
            var app = builder.Build();

            // 获取日志记录器
            var logger = app.Services.GetRequiredService<ILogger<Program>>();

            // 记录启动信息
            StartupLogger.LogStartup(logger, app.Configuration, app.Environment);

            // 配置中间件
            ConfigureMiddleware(app, logger);

            // 记录服务和中间件配置状态
            LogConfigurationStatus(logger);

            app.Run();
        }

        /// <summary>
        /// 创建主机生成器
        /// </summary>
        private static WebApplicationBuilder CreateHostBuilder(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // 配置日志
            ConfigureLogging(builder);

            // 注册服务
            ConfigureServices(builder.Services, builder.Configuration);

            return builder;
        }

        /// <summary>
        /// 配置日志
        /// </summary>
        private static void ConfigureLogging(WebApplicationBuilder builder)
        {
            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();
            builder.Logging.AddDebug();

            // 配置日志级别
            builder.Logging.AddFilter("Microsoft", LogLevel.Warning);
            builder.Logging.AddFilter("Microsoft.AspNetCore", LogLevel.Information);
            builder.Logging.AddFilter("HermesAgent", LogLevel.Debug);
        }

        /// <summary>
        /// 配置应用程序服务
        /// </summary>
        private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            // 添加应用程序服务
            services.AddApplicationServices(configuration);

            // 添加 Swagger 文档
            AddSwaggerDocumentation(services);

            // 添加健康检查
            services.AddHealthChecks();

            // 添加CORS（如需要）
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", builder =>
                {
                    builder.AllowAnyOrigin()
                           .AllowAnyMethod()
                           .AllowAnyHeader();
                });
            });
        }

        /// <summary>
        /// 添加 Swagger/OpenAPI 文档配置
        /// </summary>
        private static void AddSwaggerDocumentation(IServiceCollection services)
        {
             services.AddSwaggerGen(options =>
            {
                var xmlFile = Path.Combine(AppContext.BaseDirectory, "HermesAgent.Examples.WebApi.xml");
                options.IncludeXmlComments(xmlFile);
                options.SchemaGeneratorOptions.SchemaIdSelector = (type) => type.FullName;
            });
        }

        /// <summary>
        /// 配置应用程序中间件管道
        /// </summary>
        private static void ConfigureMiddleware(WebApplication app, ILogger logger)
        {
            // 异常处理中间件必须首先注册
            app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
            logger.LogDebug("Middleware configured: Global Exception Handler");

            // 请求日志中间件
            app.UseMiddleware<RequestLoggingMiddleware>();
            logger.LogDebug("Middleware configured: Request Logging");

            // 配置 Swagger/OpenAPI
            app.ConfigureApplicationMiddleware();
            logger.LogDebug("Middleware configured: Swagger/OpenAPI");

            // HTTPS重定向（在生产环境中启用）
            if (!app.Environment.IsDevelopment())
            {
                app.UseHsts();
                //app.UseHttpsRedirection();
            }

            // 启用CORS
            app.UseCors("AllowAll");
            logger.LogDebug("Middleware configured: CORS");

            // 配置路由
            app.ConfigureRouting();
            logger.LogDebug("Middleware configured: Routing");
        }

        /// <summary>
        /// 记录配置状态
        /// </summary>
        private static void LogConfigurationStatus(ILogger logger)
        {
            // 记录服务注册状态
            var servicesRegistered = new Dictionary<string, bool>
            {
                { "Controllers", true },
                { "Swagger", true },
                { "Health Checks", true },
                { "CORS", true }
            };
            StartupLogger.LogServicesRegistered(logger, servicesRegistered);

            // 记录中间件配置状态
            var middlewareConfigured = new Dictionary<string, bool>
            {
                { "Global Exception Handler", true },
                { "Request Logging", true },
                { "Swagger UI", true },
                { "CORS", true },
                { "Routing", true }
            };
            StartupLogger.LogMiddlewareConfigured(logger, middlewareConfigured);
        }
    }
}