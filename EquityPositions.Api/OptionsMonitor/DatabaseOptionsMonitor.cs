using EquityPositions.Api.Repositories.Abstract;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Reflection;
using System.Text.Json;

namespace EquityPositions.Api.OptionsMonitor
{
    public class DatabaseOptionsMonitor<TOptions> : IOptionsMonitor<TOptions>, IDisposable
    where TOptions : class, new()
    {
        private readonly IConfigurationRepository _repository;
        private readonly ILogger<DatabaseOptionsMonitor<TOptions>> _logger;
        private readonly Timer _timer;
        private readonly string? _sectionName;
        private readonly ConcurrentDictionary<string, TOptions> _cache;
        private readonly ConcurrentDictionary<string, List<Action<TOptions, string>>> _listeners;
        private DateTime _lastModified;

        public DatabaseOptionsMonitor(
            IConfigurationRepository repository,
            ILogger<DatabaseOptionsMonitor<TOptions>> logger,
            string sectionName = "")
        {
            _repository = repository;
            _logger = logger;
            _sectionName = string.IsNullOrEmpty(sectionName) ? typeof(TOptions).Name : sectionName;
            _cache = new ConcurrentDictionary<string, TOptions>();
            _listeners = new ConcurrentDictionary<string, List<Action<TOptions, string>>>();
            
            // Check for updates every 30 seconds
            _timer = new Timer(CheckForUpdates, null, TimeSpan.Zero, TimeSpan.FromSeconds(30));
        }

        public TOptions CurrentValue => Get(_sectionName);

        public TOptions Get(string name)
        {
            name ??= string.Empty;

            if (_cache.TryGetValue(name, out var cachedValue))
            {
                return cachedValue;
            }

            return LoadConfiguration(name);
        }

        public IDisposable OnChange(Action<TOptions, string> listener)
        {
            var key = Guid.NewGuid().ToString();
            var listenersList = _listeners.GetOrAdd(string.Empty, _ => new List<Action<TOptions, string>>());

            lock (listenersList)
            {
                listenersList.Add(listener);
            }

            return new OptionsChangeDisposable(key, () => RemoveListener(string.Empty, listener));
        }

        private TOptions LoadConfiguration(string name)
        {
            try
            {
                var configData = _repository.GetConfigurationAsync(_sectionName).GetAwaiter().GetResult();
                var options = new TOptions();

                // Use reflection to map configuration values to object properties
                MapConfigurationToObject(configData, options);

                _cache.AddOrUpdate(name, options, (_, _) => options);
                _lastModified = _repository.GetLastModifiedAsync(_sectionName).GetAwaiter().GetResult();

                return options;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load configuration for section: {Section}", _sectionName);
                return new TOptions();
            }
        }

        private void MapConfigurationToObject(Dictionary<string, string> configData, TOptions options)
        {
            var properties = typeof(TOptions).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanWrite);

            foreach (var property in properties)
            {
                var key = property.Name;
                if (configData.TryGetValue(key, out var value))
                {
                    try
                    {
                        var convertedValue = ConvertValue(value, property.PropertyType);
                        property.SetValue(options, convertedValue);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to convert value '{Value}' for property '{Property}'",
                            value, property.Name);
                    }
                }
            }
        }

        private static object? ConvertValue(string value, Type targetType)
        {
            if (targetType == typeof(string))
                return value;

            if (targetType == typeof(int))
                return int.Parse(value);

            if (targetType == typeof(bool))
                return bool.Parse(value);

            if (targetType == typeof(DateTime))
                return DateTime.Parse(value);

            if (targetType == typeof(TimeSpan))
                return TimeSpan.Parse(value);

            if (targetType.IsEnum)
                return Enum.Parse(targetType, value);

            // Handle Dictionary<string, string>
            if (targetType == typeof(Dictionary<string, string>))
            {
                return JsonSerializer.Deserialize<Dictionary<string, string>>(value) ?? new Dictionary<string, string>();
            }

            return Convert.ChangeType(value, targetType);
        }

        private async void CheckForUpdates(object? state)
        {
            try
            {
                var lastModified = await _repository.GetLastModifiedAsync(_sectionName);

                if (lastModified > _lastModified)
                {
                    _logger.LogInformation("Configuration changes detected for section: {Section}", _sectionName);

                    // Clear cache and reload
                    _cache.Clear();
                    var newOptions = LoadConfiguration(string.Empty);

                    // Notify listeners
                    NotifyListeners(newOptions, string.Empty);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking for configuration updates");
            }
        }

        private void NotifyListeners(TOptions options, string name)
        {
            if (_listeners.TryGetValue(name, out var listenersList))
            {
                lock (listenersList)
                {
                    foreach (var listener in listenersList)
                    {
                        try
                        {
                            listener(options, name);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error notifying configuration change listener");
                        }
                    }
                }
            }
        }

        private void RemoveListener(string name, Action<TOptions, string> listener)
        {
            if (_listeners.TryGetValue(name, out var listenersList))
            {
                lock (listenersList)
                {
                    listenersList.Remove(listener);
                }
            }
        }

        public void Dispose()
        {
            _timer?.Dispose();
            _cache.Clear();
            _listeners.Clear();
        }

        private class OptionsChangeDisposable : IDisposable
        {
            private readonly string _key;
            private readonly Action _dispose;

            public OptionsChangeDisposable(string key, Action dispose)
            {
                _key = key;
                _dispose = dispose;
            }

            public void Dispose() => _dispose();
        }
    }
}
