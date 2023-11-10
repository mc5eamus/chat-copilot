// Copyright (c) Microsoft. All rights reserved.

using System.Text.Json.Serialization;

namespace CopilotChat.Skills.Models;

public class DocumentSearchRecord
{
    public const string DocumentIdField = "parent_id";
    public const string TitleField = "title";
    public const string ChunkIdField = "chunk_id";
    public const string ChunkField = "chunk";
    public const string VectorField = "vector";

    [JsonPropertyName(DocumentIdField)]
    public string DocumentId { get; set; }

    [JsonPropertyName(TitleField)]
    public string Title { get; set; }

    [JsonPropertyName(ChunkIdField)]
    public string ChunkId { get; set; }

    [JsonPropertyName(ChunkField)]
    public string Chunk { get; set; }

}
