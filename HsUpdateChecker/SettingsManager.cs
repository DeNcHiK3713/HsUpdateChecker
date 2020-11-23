using HsUpdateChecker.Serializers;
using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;

namespace HsUpdateChecker
{
    public class SettingsManager
    {
        #region Fields

        private string configPath;
        private ISerializer provider;
        private HashSet<object> sections = new HashSet<object>();

        #endregion Fields

        public SettingsManager(string _configPath = null)
        {
            if (string.IsNullOrEmpty(_configPath))
            {
                _configPath = "appsettings.json";
            }
            if (!File.Exists(_configPath))
            {
                var ex = new FileNotFoundException(nameof(_configPath));
                throw ex;
            }
            provider = new JsonSerializer();
            configPath = _configPath;
            AppDomain.CurrentDomain.ProcessExit += (s, e) => SaveSettings();
            LoadSettings();
        }

        public T GetSection<T>() where T : new()
        {
            var s = sections.OfType<T>().SingleOrDefault();
            if (s == null)
            {
                s = provider.GetSection<T>() ?? new T();
                sections.Add(s);
            }
            return s;
        }

        private void SetSection(object data)
        {
            provider.SetSection(data);
        }
        public void SetSection<T>(T data) where T : new()
        {
            var s = sections.OfType<T>().SingleOrDefault();
            if (s != null)
            {
                sections.Remove(s);
            }
            sections.Add(data);
            provider.SetSection<T>(data);
        }

        private void LoadSettings()
        {
            provider.Load(configPath);
        }

        public void SaveSettings()
        {
            foreach (var s in sections)
            {
                SetSection(s);
            }
            provider.Save(configPath);
        }
    }
}