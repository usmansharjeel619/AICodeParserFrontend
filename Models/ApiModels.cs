using System.Collections.Generic;
using Newtonsoft.Json;

namespace AICodeParser.Models
{
    // Request Models
    public class DebugRequest
    {
        [JsonProperty("code")]
        public string Code { get; set; } = string.Empty;

        [JsonProperty("filename")]
        public string Filename { get; set; } = "temp.c";
    }

    public class NlpRequest
    {
        [JsonProperty("specifications")]
        public string Specifications { get; set; } = string.Empty;
    }

    public class FormalRequest
    {
        [JsonProperty("code")]
        public string Code { get; set; } = string.Empty;

        [JsonProperty("function_name")]
        public string FunctionName { get; set; } = string.Empty;
    }

    // Response Models
    public class ApiResponse<T>
    {
        [JsonProperty("status")]
        public string Status { get; set; } = string.Empty;

        [JsonProperty("message")]
        public string Message { get; set; } = string.Empty;

        [JsonProperty("data")]
        public T? Data { get; set; }

        [JsonProperty("error")]
        public string? Error { get; set; }
    }

    public class DebugResponse
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("issues")]
        public List<DebugIssue> Issues { get; set; } = new();

        [JsonProperty("suggestions")]
        public List<string> Suggestions { get; set; } = new();

        [JsonProperty("output")]
        public string Output { get; set; } = string.Empty;
    }

    public class DebugIssue
    {
        [JsonProperty("line")]
        public int Line { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; } = string.Empty;

        [JsonProperty("description")]
        public string Description { get; set; } = string.Empty;
    }

    public class StatusResponse
    {
        [JsonProperty("status")]
        public string Status { get; set; } = string.Empty;

        [JsonProperty("chatdbg_available")]
        public bool? ChatDbgAvailable { get; set; }

        [JsonProperty("llmdebugger_available")]
        public bool? LlmDebuggerAvailable { get; set; }

        [JsonProperty("gdb_available")]
        public bool? GdbAvailable { get; set; }

        [JsonProperty("device")]
        public string? Device { get; set; }

        [JsonProperty("models_loaded")]
        public bool? ModelsLoaded { get; set; }

        [JsonProperty("functions_discovered")]
        public int? FunctionsDiscovered { get; set; }

        [JsonProperty("framac_available")]
        public bool? FramacAvailable { get; set; }

        [JsonProperty("llm_available")]
        public bool? LlmAvailable { get; set; }
    }

    public class NlpResponse
    {
        [JsonProperty("generated_tests")]
        public List<GeneratedTest> GeneratedTests { get; set; } = new();

        [JsonProperty("summary")]
        public string Summary { get; set; } = string.Empty;
    }

    public class GeneratedTest
    {
        [JsonProperty("test_name")]
        public string TestName { get; set; } = string.Empty;

        [JsonProperty("test_code")]
        public string TestCode { get; set; } = string.Empty;

        [JsonProperty("description")]
        public string Description { get; set; } = string.Empty;
    }

    public class FunctionInfo
    {
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("signature")]
        public string Signature { get; set; } = string.Empty;

        [JsonProperty("file")]
        public string File { get; set; } = string.Empty;
    }

    public class FunctionsResponse
    {
        [JsonProperty("functions")]
        public List<FunctionInfo> Functions { get; set; } = new();

        [JsonProperty("count")]
        public int Count { get; set; }
    }

    public class FormalResponse
    {
        [JsonProperty("verified")]
        public bool Verified { get; set; }

        [JsonProperty("contracts")]
        public List<string> Contracts { get; set; } = new();

        [JsonProperty("violations")]
        public List<string> Violations { get; set; } = new();

        [JsonProperty("report")]
        public string Report { get; set; } = string.Empty;
    }

    public class HealthResponse
    {
        [JsonProperty("status")]
        public string Status { get; set; } = string.Empty;

        [JsonProperty("message")]
        public string Message { get; set; } = string.Empty;

        [JsonProperty("version")]
        public string Version { get; set; } = string.Empty;
    }

    // Enums
    public enum ModuleType
    {
        Debugging,
        NLP,
        FormalVerification
    }
}