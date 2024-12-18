namespace Common.SecretManagement;

using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Configuration;

public class AzureSecretManagementService : ISecretManagementService
{
  private readonly SecretClient secretClient;

  public AzureSecretManagementService(IConfiguration configuration)
  {
    string keyVaultName = configuration["Secret_Vault_Name"];
    var kvUri = "https://" + keyVaultName + ".vault.azure.net";
    secretClient = new SecretClient(new Uri(kvUri), new DefaultAzureCredential());;
  }

  public async Task<string> GetSecretAsync(string key)
  {
    try
    {
      var secret = await secretClient.GetSecretAsync(key);
      return secret.Value.Value;
    }
    catch (Exception ex)
    {
      throw new KeyNotFoundException($"Failed to retrieve secret '{key}' from Azure Key Vault.", ex);
    }
  }
}
