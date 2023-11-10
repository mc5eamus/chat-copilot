using CopilotChat.Skills.Config;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using CopilotChat.Skills.Contracts;
using CopilotChat.Skills.Services;
using static System.Collections.Specialized.BitVector32;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.AI.ChatCompletion;
using Microsoft.SemanticKernel;

namespace CopilotChat.Skills.Extensions;
public static class ConfigurationExtensions
{
    public static void AddDocumentRepository(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<DocumentRepositoryConfig>()
            .Bind(configuration.GetSection(nameof(DocumentRepository)));

        services.AddTransient<IDocumentRepository, DocumentRepository>();
    }

    public static void WithComparisonSkills(this IKernel kernel, IServiceProvider sp)
    {
        var repo = sp.GetRequiredService<IDocumentRepository>();
        var chatCompletion = kernel.GetService<IChatCompletion>();
        var comparisonOrchestrator = new ComparisonOrchestrator(repo, chatCompletion, kernel.LoggerFactory);
        var comparisonSkill = new DocumentComparisonSkill(comparisonOrchestrator);
        kernel.ImportSkill(comparisonSkill, nameof(DocumentComparisonSkill));
    }

}
