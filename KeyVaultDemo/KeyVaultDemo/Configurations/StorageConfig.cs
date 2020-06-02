using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KeyVaultDemo.Configurations
{
    public class StorageConfig
    {
        private readonly IConfiguration configuration;

        public StorageConfig(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public string AccountName
        {
            get { return this.configuration["AccountName"]; }
        }

        public string ContainerName
        {
            get { return this.configuration["ContainerName"]; }
        }

        public string Key
        {
            get { return this.configuration["Key"]; }
        }
    }
}
