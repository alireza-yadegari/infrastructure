namespace Common.SecretManagement;

public interface ISecretManagementService
{
    Task<string> GetSecretAsync(string key);
}