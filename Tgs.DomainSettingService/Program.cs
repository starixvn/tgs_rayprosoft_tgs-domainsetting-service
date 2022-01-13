using k8s;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Tgs.AgentDomainSettingService;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("/config/appsettings.json", true);

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

        options.BrandCertificateBaseUrl = config.GetValue<Uri>("AgentDomain:BrandCertificateBaseUrl");
    })
    .AddAgentDomainSetting()
    .ConfigureKubernetesClientConfiguration(
        sp =>
        {
            var config = sp.GetRequiredService<IConfiguration>();

            var isInCluster = config.GetValue<bool>("AgentDomain:KubernetesInCluster");

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

var app = builder.Build();

app.MapHealthChecks("/healthz");
app.MapControllers();
app.MapSwagger();

app.UseSwaggerUI(options => options.SwaggerEndpoint("/swagger/v1/swagger.yaml", "v1"));
app.UseReDoc();

app.Run();
