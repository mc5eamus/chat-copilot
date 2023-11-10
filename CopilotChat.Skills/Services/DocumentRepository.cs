using CopilotChat.Skills.Contracts;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.AI.Embeddings;
using Azure.Search.Documents;
using Microsoft.Extensions.Options;
using CopilotChat.Skills.Config;
using Azure;
using Azure.Search.Documents.Models;
using CopilotChat.Skills.Models;

namespace CopilotChat.Skills.Services;
public class DocumentRepository : IDocumentRepository
{
    private readonly ITextEmbeddingGeneration textEmbedding;
    private readonly DocumentRepositoryConfig config;
    private readonly ILogger logger;
    private readonly SearchClient client;

    public DocumentRepository(
        ITextEmbeddingGeneration textEmbedding,
        IOptions<DocumentRepositoryConfig> options,
        ILoggerFactory loggerFactory)
    {
        this.textEmbedding = textEmbedding;
        this.config = options.Value;
        this.logger = loggerFactory.CreateLogger<DocumentRepository>();
        this.client = new SearchClient(
            new Uri(this.config.Endpoint),
            this.config.IndexName,
            new AzureKeyCredential(this.config.APIKey));
    }

    public async Task<string?> LookupDocument(string titleQuery)
    {

        var searchOptions = new SearchOptions
        {
            Select = { DocumentSearchRecord.TitleField, DocumentSearchRecord.DocumentIdField },
            Size = 1,
            SearchFields = { DocumentSearchRecord.TitleField },
            QueryType = SearchQueryType.Full,
        };

        var response = await client.SearchAsync<DocumentSearchRecord>(titleQuery, searchOptions);

        if (response == null) { return null; }

        // get the first result from the response
        var result = response.Value.GetResults().FirstOrDefault();
        return result?.Document?.Title;

        //return await Task.FromResult($"{documentId} : {query} for {maxResults}, {string.Join(";", queryEmbedding.ToArray().Take(10))}");
    }

    public async Task<string> QueryDocument(string documentTitle, string query, int maxResults)
    {
        var queryEmbedding = await this.textEmbedding.GenerateEmbeddingAsync(query);

        RawVectorQuery rawVectorQuery = new()
        {
            KNearestNeighborsCount = maxResults,
            Fields = { DocumentSearchRecord.VectorField },
            Vector = queryEmbedding.ToArray()
        };

        var searchOptions = new SearchOptions
        {
            Skip = 0,
            Filter = $"title eq '{documentTitle}'",
            Select = { DocumentSearchRecord.ChunkField },
            Size = maxResults,
            SearchFields = { DocumentSearchRecord.TitleField },
            QueryType = SearchQueryType.Full,
            VectorQueries = { rawVectorQuery }
        };

        var response = await client.SearchAsync<DocumentSearchRecord>(query, searchOptions);

        if (response == null) { return null; }

        return string.Join("\n", response.Value.GetResults().Select(_ => _.Document.Chunk));
    }
}
