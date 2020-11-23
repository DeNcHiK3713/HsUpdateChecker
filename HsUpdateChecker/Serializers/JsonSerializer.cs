using Newtonsoft.Json.Linq;
using System.IO;

namespace HsUpdateChecker.Serializers
{
    public class JsonSerializer : ISerializer
    {
        private JObject jobject;

        void ISerializer.Load(string path)
        {
            jobject = JObject.Parse(File.ReadAllText(path));
        }

        void ISerializer.Save(string path)
        {
            File.WriteAllText(path, jobject.ToString());
        }

        T ISerializer.GetSection<T>()
        {
            var section = jobject["Settings"][typeof(T).Name];
            return section == null ? default : section.ToObject<T>();
        }

        void ISerializer.SetSection<T>(T data)
        {
            jobject["Settings"][typeof(T).Name] = JToken.FromObject(data);
        }

        void ISerializer.SetSection(object data)
        {
            jobject["Settings"][data.GetType().Name] = JToken.FromObject(data);
        }
    }
}