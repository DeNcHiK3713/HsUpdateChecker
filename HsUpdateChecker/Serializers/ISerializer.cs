namespace HsUpdateChecker.Serializers
{
    public interface ISerializer
    {
        void Load(string path);

        void Save(string path);

        T GetSection<T>() where T : new();
        void SetSection(object data);

        void SetSection<T>(T data) where T : new();
    }
}