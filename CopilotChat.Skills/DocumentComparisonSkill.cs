// Copyright (c) Microsoft. All rights reserved.

using CopilotChat.Skills.Contracts;
using Microsoft.SemanticKernel.SkillDefinition;
using System.ComponentModel;

namespace CopilotChat.Skills;

public class DocumentComparisonSkill
{
    private readonly IComparisonOrchestrator orchestrator;
    public DocumentComparisonSkill(IComparisonOrchestrator orchestrator)
    {
        this.orchestrator = orchestrator;
    }

    [SKFunction, Description("Compares 2 external cell line protocols identified by keyword related to its filename. Only use this skill if comparison is explicitly requested, do not use it for anything else. The result contains the complete comparison that can be passed over to the user.")]
    public async Task<string> CompareDocument(
        [Description("First protocol keyword")]
            string documentId1,
        [Description("Second protocol keyword")]
            string documentId2,
        [Description("What exactly needs to be evaluated")]
            string whatToLookFor)
    {
        return await this.orchestrator.CompareAsync(documentId1, documentId2, whatToLookFor);
    }

    /*
    [SKFunction, Description("Looks up a cell line protocol by a keyword. Returns the full file name of the protocol or null if none has been found. Only use it if protocol lookup is explicitly requested, do not use it for anything else.")]
    public async Task<string?> LookupDocument(
    [Description("Protocol keyword")]
            string documentKey)
    {
        return await this.orchestrator.LookupAsync(documentKey);
    }
    */

}
