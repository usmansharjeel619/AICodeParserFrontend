using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AICodeParser.Services;
using AICodeParser.Models;
using Microsoft.Win32;

namespace AICodeParser.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly IApiClient _apiClient;

        [ObservableProperty]
        private string _codeInput = string.Empty;

        [ObservableProperty]
        private string _outputText = string.Empty;

        [ObservableProperty]
        private string _statusText = "Ready";

        [ObservableProperty]
        private bool _isProcessing = false;

        [ObservableProperty]
        private ModuleType _selectedModule = ModuleType.Debugging;

        [ObservableProperty]
        private string _functionName = string.Empty;

        [ObservableProperty]
        private string _specifications = string.Empty;

        [ObservableProperty]
        private string _currentFilePath = string.Empty;

        [ObservableProperty]
        private bool _isApiConnected = false;

        public MainViewModel(IApiClient apiClient)
        {
            _apiClient = apiClient;
            _ = CheckApiConnectionAsync();
        }

        [RelayCommand]
        private async Task OpenFileAsync()
        {
            try
            {
                var openFileDialog = new OpenFileDialog
                {
                    Filter = "C Files (*.c)|*.c|C++ Files (*.cpp)|*.cpp|All Files (*.*)|*.*",
                    Title = "Select a C/C++ File"
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    CurrentFilePath = openFileDialog.FileName;
                    CodeInput = await File.ReadAllTextAsync(CurrentFilePath);
                    StatusText = $"Loaded: {Path.GetFileName(CurrentFilePath)}";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening file: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task SaveFileAsync()
        {
            try
            {
                var saveFileDialog = new SaveFileDialog
                {
                    Filter = "C Files (*.c)|*.c|C++ Files (*.cpp)|*.cpp|All Files (*.*)|*.*",
                    Title = "Save C/C++ File",
                    FileName = string.IsNullOrEmpty(CurrentFilePath)
                        ? "code.c"
                        : Path.GetFileName(CurrentFilePath)
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    await File.WriteAllTextAsync(saveFileDialog.FileName, CodeInput);
                    CurrentFilePath = saveFileDialog.FileName;
                    StatusText = $"Saved: {Path.GetFileName(CurrentFilePath)}";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving file: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task ProcessCodeAsync()
        {
            if (string.IsNullOrWhiteSpace(CodeInput))
            {
                MessageBox.Show("Please enter or load some code first.", "No Code",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            IsProcessing = true;
            OutputText = "";

            try
            {
                switch (SelectedModule)
                {
                    case ModuleType.Debugging:
                        await DebugCodeAsync();
                        break;
                    case ModuleType.NLP:
                        await AnalyzeCodeAsync();
                        break;
                    case ModuleType.FormalVerification:
                        await VerifyCodeAsync();
                        break;
                }
            }
            catch (Exception ex)
            {
                OutputText = $"Error: {ex.Message}";
                StatusText = "Error occurred";
            }
            finally
            {
                IsProcessing = false;
            }
        }

        private async Task DebugCodeAsync()
        {
            StatusText = "Debugging code...";

            var filename = string.IsNullOrWhiteSpace(CurrentFilePath)
                ? "temp.c"
                : Path.GetFileName(CurrentFilePath);

            var response = await _apiClient.DebugCodeAsync(CodeInput, filename);

            OutputText = FormatDebugResponse(response);
            StatusText = response.Status == "Analysis complete"
                ? "Debugging completed successfully"
                : "Analysis completed";
        }

        private async Task AnalyzeCodeAsync()
        {
            StatusText = "Analyzing code and generating tests...";

            var response = await _apiClient.AnalyzeCodeAsync(CodeInput);

            OutputText = FormatNlpResponse(response);
            StatusText = response.Status == "Test generation complete"
                ? "Analysis completed successfully"
                : "Analysis completed";
        }

        private async Task VerifyCodeAsync()
        {
            if (string.IsNullOrWhiteSpace(FunctionName))
            {
                MessageBox.Show("Please enter a function name for verification.", "Function Name Required",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            StatusText = "Verifying code...";

            var response = await _apiClient.VerifyCodeAsync(CodeInput, FunctionName);

            OutputText = FormatFormalResponse(response);
            StatusText = response.Validation?.Valid == true
                ? "Verification successful"
                : "Verification completed";
        }

        [RelayCommand]
        private void ClearAll()
        {
            CodeInput = string.Empty;
            OutputText = string.Empty;
            FunctionName = string.Empty;
            Specifications = string.Empty;
            CurrentFilePath = string.Empty;
            StatusText = "Ready";
        }

        private async Task CheckApiConnectionAsync()
        {
            try
            {
                IsApiConnected = await _apiClient.CheckHealthAsync();
                StatusText = IsApiConnected
                    ? "Connected to API"
                    : "API not available - Please start the Flask server";
            }
            catch
            {
                IsApiConnected = false;
                StatusText = "API not available - Please start the Flask server";
            }
        }

        // ==================== FORMATTING METHODS ====================

        private string FormatDebugResponse(DebugResponse response)
        {
            var output = "═══════════════════════════════════════════════════════\n";
            output += "                 DEBUGGING RESULTS\n";
            output += "═══════════════════════════════════════════════════════\n\n";

            output += $"Status: {response.Status}\n\n";

            // Compilation Analysis
            output += "─────────────────────────────────────────────────────────\n";
            output += "COMPILATION ANALYSIS\n";
            output += "─────────────────────────────────────────────────────────\n";
            output += $"Success: {(response.CompilationAnalysis.CompilationSuccessful ? "✓ Yes" : "✗ No")}\n";

            if (!string.IsNullOrWhiteSpace(response.CompilationAnalysis.Errors))
            {
                output += $"\nErrors:\n{response.CompilationAnalysis.Errors}\n";
            }
            output += "\n";

            // Static Analysis
            output += "─────────────────────────────────────────────────────────\n";
            output += "STATIC ANALYSIS\n";
            output += "─────────────────────────────────────────────────────────\n";
            output += $"Lines of Code: {response.StaticAnalysis.LineCount}\n";
            output += $"Functions: {response.StaticAnalysis.FunctionCount}\n";
            output += $"Cyclomatic Complexity: {response.StaticAnalysis.CodeMetrics.CyclomaticComplexity}\n";
            output += $"Comment Ratio: {response.StaticAnalysis.CodeMetrics.CommentRatio:P1}\n";
            output += $"Avg Function Length: {response.StaticAnalysis.CodeMetrics.AvgFunctionLength:F1} lines\n";

            if (response.StaticAnalysis.PotentialIssues.Count > 0)
            {
                output += "\nPotential Issues:\n";
                foreach (var issue in response.StaticAnalysis.PotentialIssues)
                {
                    output += $"  • {issue}\n";
                }
            }
            output += "\n";

            // AI Analysis
            if (!string.IsNullOrWhiteSpace(response.AiAnalysis.RootCause))
            {
                output += "─────────────────────────────────────────────────────────\n";
                output += "AI ANALYSIS\n";
                output += "─────────────────────────────────────────────────────────\n";
                output += $"Confidence: {response.AiAnalysis.Confidence:P0}\n\n";
                output += $"Root Cause:\n{response.AiAnalysis.RootCause}\n\n";

                if (response.AiAnalysis.FixSuggestions.Count > 0)
                {
                    output += "Fix Suggestions:\n";
                    for (int i = 0; i < response.AiAnalysis.FixSuggestions.Count; i++)
                    {
                        output += $"  {i + 1}. {response.AiAnalysis.FixSuggestions[i]}\n";
                    }
                    output += "\n";
                }

                if (response.AiAnalysis.Prevention.Count > 0)
                {
                    output += "Prevention Tips:\n";
                    foreach (var tip in response.AiAnalysis.Prevention)
                    {
                        output += $"  • {tip}\n";
                    }
                    output += "\n";
                }
            }

            // Recommendations
            if (response.Recommendations.Count > 0)
            {
                output += "─────────────────────────────────────────────────────────\n";
                output += "RECOMMENDATIONS\n";
                output += "─────────────────────────────────────────────────────────\n";

                var highPriority = response.Recommendations.Where(r => r.Priority.ToLower() == "high").ToList();
                var mediumPriority = response.Recommendations.Where(r => r.Priority.ToLower() == "medium").ToList();
                var lowPriority = response.Recommendations.Where(r => r.Priority.ToLower() == "low").ToList();

                if (highPriority.Count > 0)
                {
                    output += "\n🔴 HIGH PRIORITY:\n";
                    foreach (var rec in highPriority)
                    {
                        output += $"  [{rec.Category}] {rec.Description}\n";
                    }
                }

                if (mediumPriority.Count > 0)
                {
                    output += "\n🟡 MEDIUM PRIORITY:\n";
                    foreach (var rec in mediumPriority)
                    {
                        output += $"  [{rec.Category}] {rec.Description}\n";
                    }
                }

                if (lowPriority.Count > 0)
                {
                    output += "\n🟢 LOW PRIORITY:\n";
                    foreach (var rec in lowPriority)
                    {
                        output += $"  [{rec.Category}] {rec.Description}\n";
                    }
                }
            }

            output += "\n═══════════════════════════════════════════════════════\n";
            return output;
        }

        private string FormatNlpResponse(NlpResponse response)
        {
            var output = "═══════════════════════════════════════════════════════\n";
            output += "            CODE ANALYSIS & TEST GENERATION\n";
            output += "═══════════════════════════════════════════════════════\n\n";

            output += $"Status: {response.Status}\n";
            output += $"Functions Found: {response.FunctionsFound}\n";
            output += $"Total Tests Generated: {response.TotalTests}\n\n";

            // Functions Details
            if (response.FunctionsDetails.Count > 0)
            {
                output += "─────────────────────────────────────────────────────────\n";
                output += "DISCOVERED FUNCTIONS\n";
                output += "─────────────────────────────────────────────────────────\n";

                foreach (var func in response.FunctionsDetails)
                {
                    output += $"\nFunction: {func.Name}\n";
                    output += $"Line: {func.LineNumber}\n";
                    output += $"Signature: {func.Signature}\n";
                    output += $"Returns: {func.ReturnType}\n";

                    if (func.Parameters.Count > 0)
                    {
                        output += "Parameters:\n";
                        foreach (var param in func.Parameters)
                        {
                            output += $"  • {param.Type} {param.Name}\n";
                        }
                    }
                }
                output += "\n";
            }

            // Test Cases Summary
            if (response.TestCases.Count > 0)
            {
                output += "─────────────────────────────────────────────────────────\n";
                output += "TEST CASES SUMMARY\n";
                output += "─────────────────────────────────────────────────────────\n\n";

                foreach (var testGroup in response.TestCases)
                {
                    output += $"Tests for: {testGroup.Key}\n";

                    if (testGroup.Value.FunctionalTests.Count > 0)
                        output += $"  • Functional Tests: {testGroup.Value.FunctionalTests.Count}\n";

                    if (testGroup.Value.BoundaryTests.Count > 0)
                        output += $"  • Boundary Tests: {testGroup.Value.BoundaryTests.Count}\n";

                    if (testGroup.Value.ErrorTests.Count > 0)
                        output += $"  • Error Tests: {testGroup.Value.ErrorTests.Count}\n";

                    if (testGroup.Value.PerformanceTests.Count > 0)
                        output += $"  • Performance Tests: {testGroup.Value.PerformanceTests.Count}\n";

                    output += "\n";
                }
            }

            // Unit Test Code
            if (!string.IsNullOrWhiteSpace(response.UnitTestCode))
            {
                output += "─────────────────────────────────────────────────────────\n";
                output += "GENERATED UNIT TEST CODE\n";
                output += "─────────────────────────────────────────────────────────\n\n";
                output += response.UnitTestCode;
                output += "\n";
            }

            output += "═══════════════════════════════════════════════════════\n";
            return output;
        }

        private string FormatFormalResponse(FormalResponse response)
        {
            var output = "═══════════════════════════════════════════════════════\n";
            output += "           FORMAL VERIFICATION RESULTS\n";
            output += "═══════════════════════════════════════════════════════\n\n";

            output += $"Function: {response.FunctionName}\n";
            output += $"Valid: {(response.Validation?.Valid == true ? "✓ Yes" : "✗ No")}\n\n";

            // Contracts
            output += "─────────────────────────────────────────────────────────\n";
            output += "ACSL CONTRACTS\n";
            output += "─────────────────────────────────────────────────────────\n\n";

            if (response.Contracts.Preconditions.Count > 0)
            {
                output += "Preconditions (requires):\n";
                foreach (var pre in response.Contracts.Preconditions)
                {
                    output += $"  @ {pre}\n";
                }
                output += "\n";
            }

            if (response.Contracts.Postconditions.Count > 0)
            {
                output += "Postconditions (ensures):\n";
                foreach (var post in response.Contracts.Postconditions)
                {
                    output += $"  @ {post}\n";
                }
                output += "\n";
            }

            // Full ACSL Code
            if (!string.IsNullOrWhiteSpace(response.AcslCode))
            {
                output += "─────────────────────────────────────────────────────────\n";
                output += "COMPLETE ACSL SPECIFICATION\n";
                output += "─────────────────────────────────────────────────────────\n\n";
                output += response.AcslCode;
                output += "\n\n";
            }

            // Validation Details
            if (response.Validation?.Warnings.Count > 0)
            {
                output += "─────────────────────────────────────────────────────────\n";
                output += "WARNINGS\n";
                output += "─────────────────────────────────────────────────────────\n";
                foreach (var warning in response.Validation.Warnings)
                {
                    output += $"  ⚠ {warning}\n";
                }
                output += "\n";
            }

            if (response.Validation?.Errors.Count > 0)
            {
                output += "─────────────────────────────────────────────────────────\n";
                output += "ERRORS\n";
                output += "─────────────────────────────────────────────────────────\n";
                foreach (var error in response.Validation.Errors)
                {
                    output += $"  ✗ {error}\n";
                }
                output += "\n";
            }

            // Metadata
            if (response.Metadata != null)
            {
                output += "─────────────────────────────────────────────────────────\n";
                output += "METADATA\n";
                output += "─────────────────────────────────────────────────────────\n";
                output += $"Template Used: {(response.Metadata.TemplateUsed ? "Yes" : "No")}\n";
                output += $"AI Enhanced: {(response.Metadata.AiEnhanced ? "Yes" : "No")}\n";

                if (!string.IsNullOrWhiteSpace(response.Metadata.Category))
                {
                    output += $"Category: {response.Metadata.Category}\n";
                }
            }

            output += "\n═══════════════════════════════════════════════════════\n";
            return output;
        }
    }
}