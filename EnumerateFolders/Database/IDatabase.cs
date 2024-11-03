
namespace EnumerateFolders.Database
{
    interface IDatabase
    {
        void SetConnectionString(string connectionString);
        void CreateDatabase(string path);
    }
}
