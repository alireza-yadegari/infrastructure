
// using System.Security.Cryptography;
// using System.Text;

// namespace Auth.Service;

// internal class EncryptionService : IEncryptionService
// {
//     private byte[] IV =
//     {
//             0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08
//     };

//     private byte[] DeriveKeyFromPassword(string password)
//     {
//         var emptySalt = Array.Empty<byte>();
//         var iterations = 1000;
//         var desiredKeyLength = 16; // 16 bytes equal 128 bits.
//         var hashMethod = HashAlgorithmName.SHA512;
//         return Rfc2898DeriveBytes.Pbkdf2(Encoding.Unicode.GetBytes(password),
//                                          emptySalt,
//                                          iterations,
//                                          hashMethod,
//                                          desiredKeyLength);
//     }

//     public async Task<string> EncryptAsync(string clearText, string passphrase)
//     {
//         byte[] encrypted;

//         // Create a TripleDES object with the specified key and IV.
//         using (TripleDES tripleDES = TripleDES.Create())
//         {
//             tripleDES.Key = DeriveKeyFromPassword(passphrase);
//             tripleDES.IV = IV;

//             // Create a new MemoryStream object to contain the encrypted bytes.
//             using (MemoryStream memoryStream = new MemoryStream())
//             {
//                 // Create a CryptoStream object to perform the encryption.
//                 using (CryptoStream cryptoStream = new CryptoStream(memoryStream, tripleDES.CreateEncryptor(), CryptoStreamMode.Write))
//                 {
//                     // Encrypt the clearText.
//                     using (StreamWriter streamWriter = new StreamWriter(cryptoStream))
//                     {
//                         streamWriter.Write(clearText.Trim());
//                     }

//                     encrypted = memoryStream.ToArray();
//                 }
//             }
//         }

//         return Convert.ToBase64String(encrypted);
//     }


//     public async Task<string> DecryptAsync(string encrypted, string passphrase)
//     {
//         string decrypted;

//         // Create a TripleDES object with the specified key and IV.
//         using (TripleDES tripleDES = TripleDES.Create())
//         {
//             tripleDES.Key = DeriveKeyFromPassword(passphrase);
//             tripleDES.IV = IV;

//             // Create a new MemoryStream object to contain the decrypted bytes.
//             using (MemoryStream memoryStream = new MemoryStream(Convert.FromBase64String(encrypted)))
//             {
//                 // Create a CryptoStream object to perform the decryption.
//                 using (CryptoStream cryptoStream = new CryptoStream(memoryStream, tripleDES.CreateDecryptor(), CryptoStreamMode.Read))
//                 {
//                     // Decrypt the ciphertext.
//                     using (StreamReader streamReader = new StreamReader(cryptoStream))
//                     {
//                         decrypted = streamReader.ReadToEnd();
//                     }
//                 }
//             }
//         }

//         return decrypted;
//     }
// }
