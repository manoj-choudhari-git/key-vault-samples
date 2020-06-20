using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureKeyVault;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace KeyVaultDemo
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, config) =>
                {
                    // User assigned managed identity to access key vault (works only from app service, not from VS)
                    var userAssignedManagedIdentityConnString = "RunAs=App;AppId={User_Assigned_Managed_Identity_ID};";

                    // Service principal with client secret (Works from both VS and App Service)
                    var clientSecretConnectionString = "RunAs=App;AppId={ClientID};TenantId={TenantID};Appkey={ClientSecret};";

                    // Service principal with certificate thumbprint (Works from both VS and App Service)
                    var certThumbprintConnectionString = "RunAs=App;AppId={ClientID};TenantId={TenantID};CertificateThumbprint={Thumbprint}; CertificateStoreLocation ={CurrentUser or LocalMachine}";

                    // Service Principal with certificate subject (Works from both VS and App Service)
                    var certSubjectConnectionString = "RunAs=App;AppId={ClientID};TenantId={TenantID};CertificateThumbprint={Subject}; CertificateStoreLocation ={CurrentUser or LocalMachine}";

                    var keyVaultEndpoint = GetKeyVaultEndpoint();
                    if (!string.IsNullOrEmpty(keyVaultEndpoint))
                    {
                        // Pass appropriate connection string 
                        var azureServiceTokenProvider = new AzureServiceTokenProvider(certThumbprintConnectionString);
                        var keyVaultClient = new KeyVaultClient(
                            new KeyVaultClient.AuthenticationCallback(
                                azureServiceTokenProvider.KeyVaultTokenCallback));
                        config.AddAzureKeyVault(keyVaultEndpoint, keyVaultClient, new DefaultKeyVaultSecretManager());
                    }
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });

        private static string GetKeyVaultEndpoint() => "https://<<key-vault-name>>.vault.azure.net";

    }
}
