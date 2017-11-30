using System;
namespace Players7Server.Data
{
    public interface IDatabase
    {
        void Load();
        void OnClosing();
    }
}
