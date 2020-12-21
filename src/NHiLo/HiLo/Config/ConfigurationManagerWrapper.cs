﻿using Microsoft.Extensions.Configuration;
using NHiLo.Common.Config;
using System.Configuration;
using System.Linq;

namespace NHiLo.HiLo.Config
{
    /// <summary>
    /// Converts the underlying config model from the .NET framework to the nHilo's config model.
    /// </summary>
    public class ConfigurationManagerWrapper : IConfigurationManager
    {
        /*
         * {
         *   "ConnectionStrings": {
         *       "NHiLo": {
         *          "ConnectionString": "Data Source=|DataDirectory|\\Database1.sdf;Persist Security Info=False;",
         *              "ProviderName": "System.Data.SqlServerCe.4.0"
         *       }
         *   },
         *   "NHilo": {
         *      "ConnectionStringId": "",
         *      "ProviderName": "",
         *      "CreateHiLoStructureIfNotExists": true,
         *      "DefaultMaxLo": 100,
         *      "TableName": "",
         *      "NextHiColumnName": "",
         *      "EntityColumnName": "",
         *      "StorageType": "",
         *      "ObjectPrefix": "",
         *      "Entities": [
         *          { "name": "", "maxLo" : 10 }   
         *      ]
         *   }
         * }
         */

        private readonly IConfiguration _configuration;

        public ConfigurationManagerWrapper(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IHiLoConfiguration GetKeyGeneratorConfigurationSection()
        {
            var configuration = new HiLoConfigElement
            {
                ConnectionStringId = _configuration.GetValue<string>("NHilo:ConnectionStringId", string.Empty),
                ProviderName = _configuration.GetValue<string>("NHilo:ProviderName", string.Empty),
                CreateHiLoStructureIfNotExists = _configuration.GetValue("NHilo:CreateHiLoStructureIfNotExists", true),
                DefaultMaxLo = _configuration.GetValue("NHilo:DefaultMaxLo", 100),
                TableName = _configuration.GetValue("NHilo:TableName", "NHILO"),
                NextHiColumnName = _configuration.GetValue("NHilo:NextHiColumnName", "NEXT_HI"),
                EntityColumnName = _configuration.GetValue("NHilo:EntityColumnName", "ENTITY"),
                StorageType = _configuration.GetValue("NHilo:StorageType", HiLoStorageType.Table),
                ObjectPrefix = _configuration.GetValue("NHilo:ObjectPrefix", string.Empty),
                Entities = _configuration.GetSection("NHilo:ObjectPrefix").GetChildren().Select(v =>
                    (IEntityConfiguration)new EntityConfigElement()
                    {
                        Name = v.GetValue<string>("Name"),
                        MaxLo = v.GetValue("MaxLo", 10)
                    }
            ).ToList()
            };
            return configuration;
        }

        public ConnectionStringsSection GetConnectionStringsSection()
        {
            var connectionStringsSection = new ConnectionStringsSection();
            var connectionStrings = _configuration.GetSection("ConnectionStrings");
            foreach (var connectionString in connectionStrings.GetChildren())
            {
                var children = connectionString.GetChildren();
                var connectionStringSetting = new ConnectionStringSettings()
                {
                    Name = connectionString.Key,
                    ConnectionString = children.SingleOrDefault(x => x.Key.ToLower() == "connectionstring")?.Value ?? connectionString.Value,
                    ProviderName = children.SingleOrDefault(x => x.Key.ToLower() == "providername")?.Value ?? _configuration.GetValue<string>("NHilo:ProviderName", string.Empty)
                };
                connectionStringsSection.ConnectionStrings.Add(connectionStringSetting);
            }
            return connectionStringsSection;
        }
    }
}
