using EnumerateFolders.Entities;
using EnumerateFolders.Utils;
using Microsoft.EntityFrameworkCore;
using System;
using System.Configuration;


namespace EnumerateFolders.Database
{
    public class SqlSrvCtx : DbContext, IDatabase
    {
        // passed to this class from FolderInfoRepository.SetAssemblyLocation
        static string exeConfigPath = string.Empty;


        // This code creates the DB objects:
        public DbSet<Category> Categories { get; set; }
        public DbSet<Folder> Folders { get; set; }
        public DbSet<Entities.File> Files { get; set; }
        public DbSet<Drive> Drives { get; set; }
        public DbSet<ToScanQueue> ToScanQueue { get; set; }
        string connectionstring = string.Empty;  

        public SqlSrvCtx()
        {
            // read config file & get connection string
            Configuration config = null;

            if (exeConfigPath == string.Empty)
                return;

            try
            {
                config = ConfigurationManager.OpenExeConfiguration(exeConfigPath);
            }
            catch (Exception ex)
            {
                // TO DO: handle error here
                return;
            }

            if (config != null)
            {
                connectionstring = GetAppSetting(config, "connectionstring");
            }
        }

        // called from FolderInfoRepository.SetAssemblyLocation
        public void SetAssemblyLocation(string servicepath)
        {
            exeConfigPath = servicepath;
            Configuration config = null;

            try
            {
                config = ConfigurationManager.OpenExeConfiguration(exeConfigPath);
            }
            catch (Exception)
            {
                // TO DO: handle error here
                return;
            }

            if (config != null)
            {
                connectionstring = GetAppSetting(config, "connectionstring");
            }
        }

        public string GetServiceAssemblyLocation()
        {
            return exeConfigPath;
        }


        // IDatabase methods
        public string GetConnectionString()
        {
            return connectionstring;
        }

        public void SetConnectionString(string fullDbFilePath)
        {
            connectionstring = "DataSource=" + fullDbFilePath;
            Generic.AddOrUpdateAppSettings(exeConfigPath, "connectionstring", connectionstring);
        }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (connectionstring == string.Empty)
                return;

            SQLitePCL.raw.SetProvider(new SQLitePCL.SQLite3Provider_e_sqlite3());

            // optionsBuilder.UseSqlServer(connectionstring);
            optionsBuilder.UseSqlite(connectionstring);
        }

        string GetAppSetting(Configuration config, string key)
        {
            KeyValueConfigurationElement element = config.AppSettings.Settings[key];
            if (element != null)
            {
                string value = element.Value;
                if (!string.IsNullOrEmpty(value))
                    return value;
            }
            return string.Empty;
        }
    }
}
