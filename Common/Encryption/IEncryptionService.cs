namespace Common.Encryption;
  public interface IEncryptionService
  {
    Task<string> EncryptAsync(string clearText, string passphrase);
    Task<string> DecryptAsync(string encrypted, string passphrase);
  }
