using k8s;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("/config/appsettings.json", true);

builder.Services.AddControllers();

builder.Services.AddHealthChecks();

builder.Services
    .AddEndpointsApiExplorer()
    .AddSwaggerGen(options =>
    {
        options.SwaggerDoc(
            "v1",
            new OpenApiInfo
            {
                Title = "AgentDomainSetting Api",
                Version = "v1"
            });
    });

builder.Services
    .AddBrandCertificateClient((options, sp) =>
    {
        var config = sp.GetRequiredService<IConfiguration>();

        options.BrandCertificateBaseUrl = config.GetValue<Uri>("AgentDomain:BrandCertificateBaseUrl");
    })
    .AddAgentDomainSetting(
        sp =>
        {
            var config = sp.GetRequiredService<IConfiguration>();

            var isInCluster = config.GetValue<bool>("AgentDomain:KubernetesInCluster");

            if (isInCluster)
                return KubernetesClientConfiguration.InClusterConfig();
            else
                return KubernetesClientConfiguration.BuildConfigFromConfigFile();
        })
    .AddAgentDomainSettingRepositories((sp, optionsBuilder) =>
    {
        var config = sp.GetRequiredService<IConfiguration>();

        optionsBuilder.UseNpgsql(config.GetConnectionString("Tgs"));
    });

var app = builder.Build();

app.MapHealthChecks("/healthz");
app.MapControllers();
app.MapSwagger();

app.UseSwaggerUI(options => options.SwaggerEndpoint("/swagger/v1/swagger.yaml", "v1"));

app.Run();
