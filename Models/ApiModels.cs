using System.Collections.Generic;
using Newtonsoft.Json;

namespace AICodeParser.Models
{
    public enum ModuleType
    {
        Debugging,
        NLP,
        FormalVerification
    }

    // ==================== DEBUG API MODELS ====================

    public class DebugRequest
    {
        [JsonProperty("code")]
        public string Code { get; set; } = string.Empty;

        [JsonProperty("filename")]
        public string Filename { get; set; } = "test.c";
    }

    public class DebugResponse
    {
        [JsonProperty("status")]
        public string Status { get; set; } = string.Empty;

        [JsonProperty("file_path")]
        public string FilePath { get; set; } = string.Empty;

        [JsonProperty("compilation_analysis")]
        public CompilationAnalysis CompilationAnalysis { get; set; } = new();

        [JsonProperty("static_analysis")]
        public StaticAnalysis StaticAnalysis { get; set; } = new();

        [JsonProperty("runtime_analysis")]
        public RuntimeAnalysis RuntimeAnalysis { get; set; } = new();

        [JsonProperty("ai_analysis")]
        public AiAnalysis AiAnalysis { get; set; } = new();

        [JsonProperty("recommendations")]
        public List<Recommendation> Recommendations { get; set; } = new();
    }

    public class CompilationAnalysis
    {
        [JsonProperty("compilation_successful")]
        public bool CompilationSuccessful { get; set; }

        [JsonProperty("command")]
        public string Command { get; set; } = string.Empty;

        [JsonProperty("errors")]
        public string Errors { get; set; } = string.Empty;
    }

    public class StaticAnalysis
    {
        [JsonProperty("line_count")]
        public int LineCount { get; set; }

        [JsonProperty("function_count")]
        public int FunctionCount { get; set; }

        [JsonProperty("code_metrics")]
        public CodeMetrics CodeMetrics { get; set; } = new();

        [JsonProperty("potential_issues")]
        public List<string> PotentialIssues { get; set; } = new();
    }

    public class CodeMetrics
    {
        [JsonProperty("cyclomatic_complexity")]
        public int CyclomaticComplexity { get; set; }

        [JsonProperty("comment_ratio")]
        public double CommentRatio { get; set; }

        [JsonProperty("avg_function_length")]
        public double AvgFunctionLength { get; set; }
    }

    public class RuntimeAnalysis
    {
        // Add properties as needed
    }

    public class AiAnalysis
    {
        [JsonProperty("root_cause")]
        public string RootCause { get; set; } = string.Empty;

        [JsonProperty("fix_suggestions")]
        public List<string> FixSuggestions { get; set; } = new();

        [JsonProperty("prevention")]
        public List<string> Prevention { get; set; } = new();

        [JsonProperty("confidence")]
        public double Confidence { get; set; }
    }

    public class Recommendation
    {
        [JsonProperty("type")]
        public string Type { get; set; } = string.Empty;

        [JsonProperty("category")]
        public string Category { get; set; } = string.Empty;

        [JsonProperty("description")]
        public string Description { get; set; } = string.Empty;

        [JsonProperty("priority")]
        public string Priority { get; set; } = string.Empty;
    }

    // ==================== NLP API MODELS ====================

    public class NlpRequest
    {
        [JsonProperty("code")]
        public string Code { get; set; } = string.Empty;
    }

    public class NlpResponse
    {
        [JsonProperty("status")]
        public string Status { get; set; } = string.Empty;

        [JsonProperty("analyzed_code")]
        public string AnalyzedCode { get; set; } = string.Empty;

        [JsonProperty("functions_found")]
        public int FunctionsFound { get; set; }

        [JsonProperty("discovered_functions")]
        public List<string> DiscoveredFunctions { get; set; } = new();

        [JsonProperty("functions_details")]
        public List<FunctionDetail> FunctionsDetails { get; set; } = new();

        [JsonProperty("test_cases")]
        public Dictionary<string, TestCaseGroup> TestCases { get; set; } = new();

        [JsonProperty("total_tests")]
        public int TotalTests { get; set; }

        [JsonProperty("unit_test_code")]
        public string UnitTestCode { get; set; } = string.Empty;
    }

    public class FunctionDetail
    {
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("signature")]
        public string Signature { get; set; } = string.Empty;

        [JsonProperty("return_type")]
        public string ReturnType { get; set; } = string.Empty;

        [JsonProperty("parameters")]
        public List<Parameter> Parameters { get; set; } = new();

        [JsonProperty("line_number")]
        public int LineNumber { get; set; }
    }

    public class Parameter
    {
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("type")]
        public string Type { get; set; } = string.Empty;
    }

    public class TestCaseGroup
    {
        [JsonProperty("functional_tests")]
        public List<TestCase> FunctionalTests { get; set; } = new();

        [JsonProperty("boundary_tests")]
        public List<TestCase> BoundaryTests { get; set; } = new();

        [JsonProperty("error_tests")]
        public List<TestCase> ErrorTests { get; set; } = new();

        [JsonProperty("performance_tests")]
        public List<TestCase> PerformanceTests { get; set; } = new();

        [JsonProperty("all_tests")]
        public List<TestCase> AllTests { get; set; } = new();
    }

    public class TestCase
    {
        [JsonProperty("description")]
        public string Description { get; set; } = string.Empty;

        [JsonProperty("input")]
        public string Input { get; set; } = string.Empty;

        [JsonProperty("expected_output")]
        public string ExpectedOutput { get; set; } = string.Empty;

        [JsonProperty("test_code")]
        public string TestCode { get; set; } = string.Empty;
    }

    // ==================== FORMAL VERIFICATION API MODELS ====================

    public class FormalRequest
    {
        [JsonProperty("code")]
        public string Code { get; set; } = string.Empty;

        [JsonProperty("function_name")]
        public string FunctionName { get; set; } = string.Empty;
    }

    public class FormalResponse
    {
        [JsonProperty("function_name")]
        public string FunctionName { get; set; } = string.Empty;

        [JsonProperty("contracts")]
        public Contracts Contracts { get; set; } = new();

        [JsonProperty("acsl_code")]
        public string AcslCode { get; set; } = string.Empty;

        [JsonProperty("validation")]
        public Validation Validation { get; set; } = new();

        [JsonProperty("metadata")]
        public Metadata Metadata { get; set; } = new();
    }

    public class Contracts
    {
        [JsonProperty("preconditions")]
        public List<string> Preconditions { get; set; } = new();

        [JsonProperty("postconditions")]
        public List<string> Postconditions { get; set; } = new();
    }

    public class Validation
    {
        [JsonProperty("valid")]
        public bool Valid { get; set; }

        [JsonProperty("warnings")]
        public List<string> Warnings { get; set; } = new();

        [JsonProperty("errors")]
        public List<string> Errors { get; set; } = new();
    }

    public class Metadata
    {
        [JsonProperty("template_used")]
        public bool TemplateUsed { get; set; }

        [JsonProperty("ai_enhanced")]
        public bool AiEnhanced { get; set; }

        [JsonProperty("category")]
        public string Category { get; set; } = string.Empty;
    }

    // ==================== HEALTH CHECK ====================

    public class HealthResponse
    {
        [JsonProperty("status")]
        public string Status { get; set; } = string.Empty;

        [JsonProperty("timestamp")]
        public string Timestamp { get; set; } = string.Empty;
    }
}