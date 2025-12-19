using System;
using System.IO;
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
                    Title = "Save File"
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
            OutputText = string.Empty;

            try
            {
                switch (SelectedModule)
                {
                    case ModuleType.Debugging:
                        await DebugCodeAsync();
                        break;
                    case ModuleType.NLP:
                        await GenerateTestsAsync();
                        break;
                    case ModuleType.FormalVerification:
                        await VerifyCodeAsync();
                        break;
                }
            }
            catch (Exception ex)
            {
                OutputText = $"Error: {ex.Message}\n\nPlease ensure the Flask API is running.";
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsProcessing = false;
            }
        }

        private async Task DebugCodeAsync()
        {
            StatusText = "Debugging code...";

            var filename = string.IsNullOrEmpty(CurrentFilePath)
                ? "temp.c"
                : Path.GetFileName(CurrentFilePath);

            var response = await _apiClient.DebugCodeAsync(CodeInput, filename);

            OutputText = FormatDebugResponse(response);
            StatusText = response.Success ? "Debugging completed successfully" : "Issues found";
        }

        private async Task GenerateTestsAsync()
        {
            StatusText = "Generating test cases...";

            var specs = string.IsNullOrWhiteSpace(Specifications)
                ? CodeInput
                : Specifications;

            var response = await _apiClient.GenerateTestsAsync(specs);

            OutputText = FormatNlpResponse(response);
            StatusText = "Test generation completed";
        }

        private async Task VerifyCodeAsync()
        {
            StatusText = "Verifying code...";

            var response = await _apiClient.VerifyCodeAsync(CodeInput, FunctionName);

            OutputText = FormatFormalResponse(response);
            StatusText = response.Verified
                ? "Verification successful"
                : "Verification completed with issues";
        }

        [RelayCommand]
        private async Task CheckStatusAsync()
        {
            IsProcessing = true;
            try
            {
                StatusResponse status;

                switch (SelectedModule)
                {
                    case ModuleType.Debugging:
                        status = await _apiClient.GetDebugStatusAsync();
                        OutputText = FormatDebugStatus(status);
                        break;
                    case ModuleType.NLP:
                        status = await _apiClient.GetNlpStatusAsync();
                        OutputText = FormatNlpStatus(status);
                        break;
                    case ModuleType.FormalVerification:
                        status = await _apiClient.GetFormalStatusAsync();
                        OutputText = FormatFormalStatus(status);
                        break;
                }

                StatusText = "Status check completed";
            }
            catch (Exception ex)
            {
                OutputText = $"Error checking status: {ex.Message}";
            }
            finally
            {
                IsProcessing = false;
            }
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

        // Formatting methods
        private string FormatDebugResponse(DebugResponse response)
        {
            var output = $"=== Debugging Results ===\n\n";
            output += $"Status: {(response.Success ? "Success" : "Issues Found")}\n\n";

            if (response.Issues.Count > 0)
            {
                output += "Issues:\n";
                foreach (var issue in response.Issues)
                {
                    output += $"  Line {issue.Line} [{issue.Type}]: {issue.Description}\n";
                }
                output += "\n";
            }

            if (response.Suggestions.Count > 0)
            {
                output += "Suggestions:\n";
                foreach (var suggestion in response.Suggestions)
                {
                    output += $"  • {suggestion}\n";
                }
                output += "\n";
            }

            if (!string.IsNullOrEmpty(response.Output))
            {
                output += $"Output:\n{response.Output}\n";
            }

            return output;
        }

        private string FormatNlpResponse(NlpResponse response)
        {
            var output = $"=== Generated Test Cases ===\n\n";

            if (!string.IsNullOrEmpty(response.Summary))
            {
                output += $"Summary: {response.Summary}\n\n";
            }

            if (response.GeneratedTests.Count > 0)
            {
                output += $"Generated {response.GeneratedTests.Count} test(s):\n\n";

                foreach (var test in response.GeneratedTests)
                {
                    output += $"--- {test.TestName} ---\n";
                    output += $"Description: {test.Description}\n\n";
                    output += $"Code:\n{test.TestCode}\n\n";
                }
            }
            else
            {
                output += "No tests generated.\n";
            }

            return output;
        }

        private string FormatFormalResponse(FormalResponse response)
        {
            var output = $"=== Formal Verification Results ===\n\n";
            output += $"Verified: {(response.Verified ? "Yes" : "No")}\n\n";

            if (response.Contracts.Count > 0)
            {
                output += "Generated Contracts:\n";
                foreach (var contract in response.Contracts)
                {
                    output += $"  {contract}\n";
                }
                output += "\n";
            }

            if (response.Violations.Count > 0)
            {
                output += "Violations Found:\n";
                foreach (var violation in response.Violations)
                {
                    output += $"  • {violation}\n";
                }
                output += "\n";
            }

            if (!string.IsNullOrEmpty(response.Report))
            {
                output += $"Report:\n{response.Report}\n";
            }

            return output;
        }

        private string FormatDebugStatus(StatusResponse status)
        {
            var output = "=== Debugging Module Status ===\n\n";
            output += $"Status: {status.Status}\n";
            output += $"ChatDBG Available: {status.ChatDbgAvailable}\n";
            output += $"LLM Debugger Available: {status.LlmDebuggerAvailable}\n";
            output += $"GDB Available: {status.GdbAvailable}\n";
            return output;
        }

        private string FormatNlpStatus(StatusResponse status)
        {
            var output = "=== NLP Module Status ===\n\n";
            output += $"Status: {status.Status}\n";
            output += $"Device: {status.Device}\n";
            output += $"Models Loaded: {status.ModelsLoaded}\n";
            output += $"Functions Discovered: {status.FunctionsDiscovered}\n";
            return output;
        }

        private string FormatFormalStatus(StatusResponse status)
        {
            var output = "=== Formal Verification Module Status ===\n\n";
            output += $"Status: {status.Status}\n";
            output += $"Frama-C Available: {status.FramacAvailable}\n";
            output += $"LLM Available: {status.LlmAvailable}\n";
            return output;
        }
    }
}