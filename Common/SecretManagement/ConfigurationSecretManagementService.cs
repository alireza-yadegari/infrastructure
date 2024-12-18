
using Microsoft.Extensions.Configuration;

namespace Common.SecretManagement;

public class ConfigurationSecretManagementService : ISecretManagementService
{
    private readonly IConfiguration _configuration;

    public ConfigurationSecretManagementService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<string> GetSecretAsync(string key)
    {
        return _configuration[$"Secrets:{key}"] ?? throw new KeyNotFoundException($"Secret with key '{key}' not found.");
    }
}
