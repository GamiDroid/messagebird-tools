using System.Text.Json;
using MessagebirdTools.WebApp.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace MessagebirdTools.WebApp.Services;

/// <summary>
/// Service for managing application settings stored in a JSON file on the server.
/// </summary>
public interface ISettingsService
{
    /// <summary>
    /// Indicates whether settings have been loaded.
    /// </summary>
    bool IsInitialized { get; }

    /// <summary>
    /// Indicates whether API credentials are configured.
    /// </summary>
    bool HasValidSettings { get; }

    /// <summary>
    /// Event triggered when settings change.
    /// </summary>
    event Action? OnSettingsChanged;

    /// <summary>
    /// Initializes and loads settings from storage.
    /// </summary>
    Task InitializeAsync();

    /// <summary>
    /// Gets the current settings.
    /// </summary>
    AppSettings GetSettings();

    /// <summary>
    /// Updates and persists settings.
    /// </summary>
    Task UpdateSettingsAsync(AppSettings settings);
}

/// <summary>
/// Implementation of ISettingsService that stores settings in a JSON file on the server.
/// </summary>
public class SettingsService : ISettingsService
{
    private readonly string _settingsFilePath;
    private readonly ILogger<SettingsService> _logger;
    private readonly SemaphoreSlim _fileLock = new(1, 1);
    
    private AppSettings _settings = new();
    private bool _isInitialized = false;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true
    };

    public SettingsService(IWebHostEnvironment environment, ILogger<SettingsService> logger)
    {
        _logger = logger;
        
        // Store settings in the app's data directory
        var dataDirectory = Path.Combine(environment.ContentRootPath, "App_Data");
        if (!Directory.Exists(dataDirectory))
        {
            Directory.CreateDirectory(dataDirectory);
        }
        
        _settingsFilePath = Path.Combine(dataDirectory, "settings.json");
    }

    public bool IsInitialized => _isInitialized;

    public bool HasValidSettings => 
        !string.IsNullOrEmpty(_settings.ApiKey) &&
        !string.IsNullOrEmpty(_settings.DatabaseKey) &&
        !string.IsNullOrEmpty(_settings.DatabaseRecordKey);

    public event Action? OnSettingsChanged;

    public async Task InitializeAsync()
    {
        if (_isInitialized)
            return;

        await _fileLock.WaitAsync();
        try
        {
            if (File.Exists(_settingsFilePath))
            {
                var json = await File.ReadAllTextAsync(_settingsFilePath);
                var settings = JsonSerializer.Deserialize<AppSettings>(json);
                if (settings is not null)
                {
                    _settings = settings;
                    _logger.LogInformation("Settings loaded from {Path}", _settingsFilePath);
                }
            }
            else
            {
                _logger.LogInformation("No settings file found at {Path}, using defaults", _settingsFilePath);
                _settings = new AppSettings();
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error loading settings from {Path}, using defaults", _settingsFilePath);
            _settings = new AppSettings();
        }
        finally
        {
            _fileLock.Release();
        }

        _isInitialized = true;
    }

    public AppSettings GetSettings()
    {
        return new AppSettings
        {
            ApiKey = _settings.ApiKey,
            DatabaseKey = _settings.DatabaseKey,
            DatabaseRecordKey = _settings.DatabaseRecordKey
        };
    }

    public async Task UpdateSettingsAsync(AppSettings settings)
    {
        await _fileLock.WaitAsync();
        try
        {
            _settings = new AppSettings
            {
                ApiKey = settings.ApiKey,
                DatabaseKey = settings.DatabaseKey,
                DatabaseRecordKey = settings.DatabaseRecordKey
            };

            var json = JsonSerializer.Serialize(_settings, _jsonOptions);
            await File.WriteAllTextAsync(_settingsFilePath, json);
            
            _logger.LogInformation("Settings saved to {Path}", _settingsFilePath);
        }
        finally
        {
            _fileLock.Release();
        }
        
        OnSettingsChanged?.Invoke();
    }
}
