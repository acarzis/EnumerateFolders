
namespace EnumerateFolders.Database
{
    interface IDatabase
    {
        string GetConnectionString();
        void SetConnectionString(string path);
    }
}
