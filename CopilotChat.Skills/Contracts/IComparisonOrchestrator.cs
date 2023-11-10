namespace CopilotChat.Skills.Contracts;
public interface IComparisonOrchestrator
{
    Task<string?> LookupAsync(string documentId);
    Task<string> CompareAsync(string documentIdLeft, string documentIdRight, string query);
}
