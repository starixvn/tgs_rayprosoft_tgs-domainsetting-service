using k8s;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using OpenTelemetry.Exporter;
using OpenTelemetry.Instrumentation.AspNetCore;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Tgs.AgentDomainSettingService;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("/config/appsettings.json", true)
    .AddEnvironmentVariables();

builder.Logging
    .AddOpenTelemetry(options =>
    {
        options.IncludeFormattedMessage = true;
        options.ParseStateValues = true;
        _ = options.AddFluentdForwardExporter(fluentOptions =>
        {
            var nodeIp = builder.Configuration.GetValue<string>("NODE_IP");
            fluentOptions.Host = nodeIp;
            fluentOptions.Tag = builder.Configuration.GetValue<string>("OTEL_SERVICE_NAME");
            fluentOptions.UseMessagePack();
        });
    });

builder.Services
    .AddRouting(options => options.LowercaseUrls = true)
    .AddControllers();

builder.Services.AddHealthChecks();

builder.Services
    .AddEndpointsApiExplorer()
    .AddSwaggerGen(options => options.SwaggerDoc(
        "v1",
        new OpenApiInfo
        {
            Title = "AgentDomainSetting Api",
            Version = "v1"
        }));

builder.Services
    .AddTransient<HttpClientLoggingHandler>()
    .AddBrandCertificateClient((options, sp) =>
    {
        var config = sp.GetRequiredService<IConfiguration>();

        options.BrandCertificateBaseUrl = config.GetValue<Uri>("DomainSetting:BrandCertificateBaseUrl");
    })
    .AddAgentDomainSetting()
    .ConfigureKubernetesClientConfiguration(
        sp =>
        {
            var config = sp.GetRequiredService<IConfiguration>();

            var isInCluster = config.GetValue<bool>("DomainSetting:KubernetesInCluster");

            return isInCluster
                ? KubernetesClientConfiguration.InClusterConfig()
                : KubernetesClientConfiguration.BuildConfigFromConfigFile();
        },
        sp => new[] { sp.GetRequiredService<HttpClientLoggingHandler>() })
    .ConfigureCloudflareOptions((options, sp) =>
    {
        var config = sp.GetRequiredService<IConfiguration>();

        config.GetSection("Cloudflare").Bind(options);
    })
    .ConfigureCloudflareClient(builder => builder.AddHttpMessageHandler<HttpClientLoggingHandler>())
    .Services
    .AddAgentDomainSettingRepositories((sp, optionsBuilder) =>
    {
        var config = sp.GetRequiredService<IConfiguration>();

        _ = optionsBuilder.UseNpgsql(config.GetConnectionString("Tgs"));
    });

builder.Services
    .AddSingleton<IConfigureOptions<OtlpExporterOptions>>(
        sp => new ConfigureNamedOptions<OtlpExporterOptions>(
            Microsoft.Extensions.Options.Options.DefaultName,
            exporterOptions =>
            {
                var config = sp.GetRequiredService<IConfiguration>();
                var nodeIp = config.GetValue<string>("NODE_IP");

                if (!string.IsNullOrWhiteSpace(nodeIp))
                    exporterOptions.Endpoint = new Uri($"http://{nodeIp}:4317");

                config.GetSection("Opentelemetry:Tracing").Bind(exporterOptions);
            }))
    .AddOpenTelemetry()
    .WithTracing(
        tracingBuilder => tracingBuilder
            .SetResourceBuilder(ResourceBuilder.CreateDefault()
                .AddEnvironmentVariableDetector())
            .AddSource("Tgs.*")
            .AddSource("TGS.*")
            .AddAspNetCoreInstrumentation()
            .AddEntityFrameworkCoreInstrumentation(options => options.SetDbStatementForText = true)
            .AddHttpClientInstrumentation()
            .AddProfileViewExporter()
            .AddOtlpExporter())
    .Services
    .Configure<AspNetCoreInstrumentationOptions>(
        options => options.Filter = httpContext => !httpContext.Request.Path.HasValue
            || !httpContext.Request.Path.StartsWithSegments("/swagger")
            && !httpContext.Request.Path.StartsWithSegments("/healthz")
            && !httpContext.Request.Path.StartsWithSegments("/metrics")
            && !httpContext.Request.Path.StartsWithSegments("/profiler/")
            && !new[] { "html", "ico", "png", "jpg" }.Any(
                ext => httpContext.Request.Path.Value.EndsWith($".{ext}")))
    .AddProfileViewer();

var app = builder.Build();

app.MapHealthChecks("/healthz");
app.MapControllers();
app.MapSwagger();
app.MapProfileViewer();

app.UseSwaggerUI(options => options.SwaggerEndpoint("/swagger/v1/swagger.yaml", "v1"));
app.UseReDoc();

app.Run();
