// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Text;
using CopilotChat.Skills.Contracts;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.AI.ChatCompletion;

namespace CopilotChat.Skills.Services;
public class ComparisonOrchestrator : IComparisonOrchestrator
{
    private readonly IDocumentRepository repo;
    private readonly IChatCompletion chatCompletion;
    private readonly ILogger logger;
    public ComparisonOrchestrator(
        IDocumentRepository repo,
        IChatCompletion chatCompletion,
        ILoggerFactory loggerFactory)
    {
        this.repo = repo;
        this.chatCompletion = chatCompletion;
        this.logger = loggerFactory.CreateLogger<ComparisonOrchestrator>();
    }

    public async Task<string?> LookupAsync(string documentId) {
        var lookupResult = await this.repo.LookupDocument(documentId);
        if (lookupResult == null)
        {
            return null;
        }

        return lookupResult;
    }

    public async Task<string> CompareAsync(string documentIdLeft, string documentIdRight, string query)
    {
        var lookups = await Task.WhenAll(
            this.repo.LookupDocument(documentIdLeft),
            this.repo.LookupDocument(documentIdRight));

        // if either lookup failed, return an error message
        if (lookups[0] == null || lookups[1] == null)
        {
            // identify which lookups failed and add the response to a stringbuffer
            var responseSb = new StringBuilder();
            if (lookups[0] == null)
            {
                responseSb.AppendLine($"Could not find the protocol referred to as '{documentIdLeft}'");
            }
            if (lookups[1] == null)
            {
                responseSb.AppendLine($"Could not find the protocol referred to as '{documentIdRight}'");
            }

            return responseSb.ToString();
        }

        var candidates = await Task.WhenAll(
            this.repo.QueryDocument(lookups[0], query, 3),
            this.repo.QueryDocument(lookups[1], query, 3));

        ChatHistory history = new();
        history.AddAssistantMessage("Your are an assistant assisting with protocol comparison. Only provide comparison for quoted content, do not invent anything. Only respond to the scope of comparison stated by the user.");

        var response = new StringBuilder();
        for (int i = 0; i < candidates.Length; i++)
        {
            response.AppendLine($"--- Protocol {i + 1} ---");
            response.AppendLine(candidates[i]);
            response.AppendLine($"--- End of Protocol {i + 1} ---");
        }

        history.AddSystemMessage(response.ToString());
        history.AddUserMessage("Compare the protocols quoted above and identify differences with regards to " + query);


        var result = await this.chatCompletion.GenerateMessageAsync(history);
        return result;
    }
}
