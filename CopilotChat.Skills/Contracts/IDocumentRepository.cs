namespace CopilotChat.Skills.Contracts;
public interface IDocumentRepository
{
    public Task<string?> LookupDocument(string titleQuery);
    public Task<string> QueryDocument(string documentId, string query, int maxResults);
}
