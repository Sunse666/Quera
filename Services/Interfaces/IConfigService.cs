namespace Quera.Services;

public interface IConfigService
{
    ConfigData Current { get; }
    void Load();
    void Reload();
    string ExpandPath(string path);
}
