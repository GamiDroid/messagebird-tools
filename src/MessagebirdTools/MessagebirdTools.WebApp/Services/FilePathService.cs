using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace MessagebirdTools.WebApp.Services;

public interface IFilePathService
{
    string? CurrentPath { get; }
    bool HasValidPath { get; }
    event Action? OnPathChanged;
    Task InitializeAsync();
    Task SetPathAsync(string path);
}

public class FilePathService : IFilePathService
{
    private readonly ProtectedLocalStorage _localStorage;
    private const string ExcelPathKey = "excel-file-path";
    private string? _currentPath;

    public FilePathService(ProtectedLocalStorage localStorage)
    {
        _localStorage = localStorage;
    }

    public event Action? OnPathChanged;

    public string? CurrentPath 
    { 
        get => _currentPath;
        private set
        {
            _currentPath = value;
            OnPathChanged?.Invoke();
        }
    }

    public bool HasValidPath => !string.IsNullOrEmpty(CurrentPath) && File.Exists(CurrentPath);

    public async Task InitializeAsync()
    {
        try
        {
            var result = await _localStorage.GetAsync<string>(ExcelPathKey);
            if (result.Success && !string.IsNullOrEmpty(result.Value))
            {
                CurrentPath = result.Value;
            }
        }
        catch
        {
            // Handle first-time use or storage exceptions
            CurrentPath = null;
        }
    }

    public async Task SetPathAsync(string path)
    {
        CurrentPath = path;
        await _localStorage.SetAsync(ExcelPathKey, path);
    }
}