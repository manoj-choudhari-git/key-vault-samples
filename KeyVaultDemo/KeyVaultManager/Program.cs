using System;
using System.Text;
using System.Threading.Tasks;

using Azure.Identity;
using Azure.Security.KeyVault.Keys;
using Azure.Security.KeyVault.Keys.Cryptography;

namespace KeyVaultManager
{
    class Program
    {
        static string keyVaultUrl = "https://<<your-key-vault>>.vault.azure.net";
        static string clientId = "{ClientID}";
        static string tenantId = "{TenantID}";
        static string clientSecret = "{ClientSecret}";
        static async Task Main(string[] args)
        {

            // Create a new key client using the default credential from Azure.Identity using environment variables previously set,
            // including AZURE_CLIENT_ID, AZURE_CLIENT_SECRET, and AZURE_TENANT_ID.
            var client = new KeyClient(vaultUri: new Uri(keyVaultUrl), credential: new ClientSecretCredential(tenantId, clientId, clientSecret));

            // next two lines are just to recover key in case we stop program after deleting and before recovering / purging
            //var recoverOperation1 = await client.StartRecoverDeletedKeyAsync("rsa-key-name");
            //await recoverOperation1.WaitForCompletionAsync();

            // Create a software RSA key
            var rsaCreateKey = new CreateRsaKeyOptions("rsa-key-name", hardwareProtected: false);
            KeyVaultKey rsaKey = await client.CreateRsaKeyAsync(rsaCreateKey);

            Console.WriteLine("Created the key....");
            Console.WriteLine($"rsaKey.Name: {rsaKey.Name}");
            Console.WriteLine($"rsaKey.KeyType: {rsaKey.KeyType}");
            Console.WriteLine("==================================================");
            Console.WriteLine();

            // Retrieve
            KeyVaultKey key = await client.GetKeyAsync("rsa-key-name");
            Console.WriteLine("Retrieve the key");
            Console.WriteLine($"key.Name: {key.Name}");
            Console.WriteLine($"key.KeyType: {key.KeyType}");
            Console.WriteLine("==================================================");
            Console.WriteLine();


            // Update
            KeyVaultKey updateKey = await client.CreateKeyAsync("rsa-key-name", KeyType.Rsa);

            // You can specify additional application-specific metadata in the form of tags.
            updateKey.Properties.Tags["foo"] = "updated tag";

            KeyVaultKey updatedKey = await client.UpdateKeyPropertiesAsync(updateKey.Properties);
            Console.WriteLine("Update Initiated.");
            Console.WriteLine($"updatedKey.Name: {updatedKey.Name}");
            Console.WriteLine($"updatedKey.Properties.Version: {updatedKey.Properties.Version}");
            Console.WriteLine($"updatedKey.Properties.UpdatedOn: {updatedKey.Properties.UpdatedOn}");
            Console.WriteLine("==================================================");
            Console.WriteLine();

            /// Delete
            DeleteKeyOperation operation = await client.StartDeleteKeyAsync("rsa-key-name");

            DeletedKey deletedKey = operation.Value;
            Console.WriteLine("Delete operation initialted.");
            Console.WriteLine($"deletedKey.Name: {deletedKey.Name}");
            Console.WriteLine($"deletedKey.DeletedOn: {deletedKey.DeletedOn}");
            Console.WriteLine("==================================================");
            Console.WriteLine();

            // Wait for deletion to complete
            await operation.WaitForCompletionAsync();

            // Recover deleted key
            var recoverOperation = await client.StartRecoverDeletedKeyAsync("rsa-key-name");
            await recoverOperation.WaitForCompletionAsync();
            Console.WriteLine("Recovery completed");
            Console.WriteLine("==================================================");
            Console.WriteLine();

            // Create crypto client and demo of encryption / decryption
            var cryptoClient = new CryptographyClient(keyId: key.Id, credential: new ClientSecretCredential(tenantId, clientId, clientSecret));
            byte[] plaintext = Encoding.UTF8.GetBytes("If you can dream it, you can do it.");

            // encrypt the data using the algorithm RSAOAEP
            EncryptResult encryptResult = await cryptoClient.EncryptAsync(EncryptionAlgorithm.RsaOaep, plaintext);
            Console.WriteLine("Encryption demo.");
            Console.WriteLine("Encrypted Base64: " + Convert.ToBase64String(encryptResult.Ciphertext));
            Console.WriteLine("==================================================");
            Console.WriteLine();

            // decrypt the encrypted data.
            DecryptResult decryptResult = await cryptoClient.DecryptAsync(EncryptionAlgorithm.RsaOaep, encryptResult.Ciphertext);
            Console.WriteLine("Decryption demo.");
            Console.WriteLine("Decrypted: " + Encoding.UTF8.GetString(decryptResult.Plaintext));
            Console.WriteLine("==================================================");
            Console.WriteLine();

            // Purge 
            DeleteKeyOperation deleteOperation = await client.StartDeleteKeyAsync("rsa-key-name");
            await deleteOperation.WaitForCompletionAsync();

            DeletedKey purgekey = deleteOperation.Value;
            await client.PurgeDeletedKeyAsync(purgekey.Name);

            Console.WriteLine("Purge Initiated.");
            Console.WriteLine($"purgekey.Name: {purgekey.Name}");
            Console.WriteLine("==================================================");
            Console.WriteLine();            
        }
    }
}
