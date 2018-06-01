using System;
using System.Configuration;

namespace EncryptDecrypt
{
    internal class Program
    {
        private static readonly string Password = ConfigurationManager.AppSettings["CryptoKey"];

        private static void Main(string[] args)
        {
            var secret = "The secret message";
            if (args.Length > 0 && !string.IsNullOrWhiteSpace(args[0])) secret = args[0];
            Console.WriteLine("Secret message to encrypt: {0}", secret);
            Console.WriteLine();

            Console.WriteLine("Password used to encrypt/decrypt secret message: {0}", Password);
            Console.WriteLine();

            var cipher = CryptoHelper.EncryptWithPassword(secret, Password);
            Console.WriteLine("Encrypted cipher text: {0}", cipher);
            Console.WriteLine();

            var plain = CryptoHelper.DecryptWithPassword(cipher, Password);
            Console.WriteLine("Decrypted plain text: {0}", plain);
            Console.ReadLine();
        }
    }
}
