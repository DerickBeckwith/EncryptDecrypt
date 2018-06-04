﻿/*
* This work(Modern Encryption of a String C#, by James Tuley), 
* identified by James Tuley, is free of known copyright restrictions.
* https://gist.github.com/4336842
* http://creativecommons.org/publicdomain/mark/1.0/ 
*/

using System;
using System.IO;
using System.Text;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

namespace EncryptDecrypt
{
    public static class CryptoHelper
    {
        private static readonly SecureRandom Random = new SecureRandom();

        // Preconfigured Encryption Parameters
        public static readonly int NonceBitSize = 128;
        public static readonly int MacBitSize = 128;
        public static readonly int KeyBitSize = 256;

        // Preconfigured Password Key Derivation Parameters
        public static readonly int SaltBitSize = 128;
        public static readonly int Iterations = 10000;
        public static readonly int MinPasswordLength = 12;

        /// <summary>
        /// Encryption And Authentication (AES-GCM) of a UTF8 string using key derived from a password (PBKDF2).
        /// </summary>
        /// <param name="secretMessage">The secret message.</param>
        /// <param name="password">The password.</param>
        /// <param name="nonSecretPayload">The non secret payload.</param>
        /// <returns>
        /// Encrypted Message
        /// </returns>
        /// <remarks>
        /// Significantly less secure than using random binary keys.
        /// Adds additional non secret payload for key generation parameters.
        /// </remarks>
        public static string EncryptWithPassword(string secretMessage, string password, byte[] nonSecretPayload = null)
        {
            if (string.IsNullOrWhiteSpace(secretMessage))
            {
                throw new ArgumentException("Secret Message Required!", nameof(secretMessage));
            }

            if (string.IsNullOrWhiteSpace(password) || password.Length < MinPasswordLength)
            {
                throw new ArgumentException($"Must have a password of at least {MinPasswordLength} characters!", nameof(password));
            }

            var plainText = Encoding.UTF8.GetBytes(secretMessage);
            var cipherText = EncryptWithPassword(plainText, password, nonSecretPayload);

            return Convert.ToBase64String(cipherText);
        }

        /// <summary>
        /// Simple Decryption and Authentication (AES-GCM) of a UTF8 message using a key derived from a password (PBKDF2)
        /// </summary>
        /// <param name="encryptedMessage">The encrypted message.</param>
        /// <param name="password">The password.</param>
        /// <param name="nonSecretPayloadLength">Length of the non secret payload.</param>
        /// <returns>
        /// Decrypted Message
        /// </returns>
        /// <exception cref="ArgumentException">Encrypted Message Required!;encryptedMessage</exception>
        /// <remarks>
        /// Significantly less secure than using random binary keys.
        /// </remarks>
        public static string DecryptWithPassword(string encryptedMessage, string password, int nonSecretPayloadLength = 0)
        {
            if (string.IsNullOrWhiteSpace(encryptedMessage))
            {
                throw new ArgumentException("Encrypted Message Required!", nameof(encryptedMessage));
            }

            if (string.IsNullOrWhiteSpace(password) || password.Length < MinPasswordLength)
            {
                throw new ArgumentException($"Must have a password of at least {MinPasswordLength} characters!", nameof(password));
            }

            var cipherText = Convert.FromBase64String(encryptedMessage);
            var plainText = DecryptWithPassword(cipherText, password, nonSecretPayloadLength);

            return plainText == null ? null : Encoding.UTF8.GetString(plainText);
        }

        /// <summary>
        /// Encryption And Authentication (AES-GCM) of a UTF8 string using key derived from a password.
        /// </summary>
        /// <param name="secretMessage">The secret message.</param>
        /// <param name="password">The password.</param>
        /// <param name="nonSecretPayload">The non secret payload.</param>
        /// <returns>
        /// Encrypted Message
        /// </returns>
        /// <exception cref="ArgumentException">Must have a password of minimum length;password</exception>
        /// <remarks>
        /// Significantly less secure than using random binary keys.
        /// Adds additional non secret payload for key generation parameters.
        /// </remarks>
        private static byte[] EncryptWithPassword(byte[] secretMessage, string password, byte[] nonSecretPayload = null)
        {
            nonSecretPayload = nonSecretPayload ?? new byte[] { };

            var generator = new Pkcs5S2ParametersGenerator();

            // Use Random Salt to minimize pre-generated weak password attacks.
            var salt = new byte[SaltBitSize / 8];
            Random.NextBytes(salt);

            generator.Init(
                PbeParametersGenerator.Pkcs5PasswordToBytes(password.ToCharArray()),
                salt,
                Iterations);

            // Generate Key
            var key = (KeyParameter)generator.GenerateDerivedMacParameters(KeyBitSize);

            // Create Full Non Secret Payload
            var payload = new byte[salt.Length + nonSecretPayload.Length];
            Array.Copy(nonSecretPayload, payload, nonSecretPayload.Length);
            Array.Copy(salt, 0, payload, nonSecretPayload.Length, salt.Length);

            return Encrypt(secretMessage, key.GetKey(), payload);
        }

        /// <summary>
        /// Simple Decryption and Authentication of a UTF8 message using a key derived from a password
        /// </summary>
        /// <param name="encryptedMessage">The encrypted message.</param>
        /// <param name="password">The password.</param>
        /// <param name="nonSecretPayloadLength">Length of the non secret payload.</param>
        /// <returns>
        /// Decrypted Message
        /// </returns>
        /// <exception cref="ArgumentException">Must have a password of minimum length;password</exception>
        /// <remarks>
        /// Significantly less secure than using random binary keys.
        /// </remarks>
        private static byte[] DecryptWithPassword(byte[] encryptedMessage, string password, int nonSecretPayloadLength = 0)
        {
            var generator = new Pkcs5S2ParametersGenerator();

            // Grab Salt from Payload
            var salt = new byte[SaltBitSize / 8];
            Array.Copy(encryptedMessage, nonSecretPayloadLength, salt, 0, salt.Length);

            generator.Init(
                PbeParametersGenerator.Pkcs5PasswordToBytes(password.ToCharArray()),
                salt,
                Iterations);

            // Generate Key
            var key = (KeyParameter)generator.GenerateDerivedMacParameters(KeyBitSize);

            return Decrypt(encryptedMessage, key.GetKey(), salt.Length + nonSecretPayloadLength);
        }

        /// <summary>
        /// Encryption And Authentication (AES-GCM) of a UTF8 string.
        /// </summary>
        /// <param name="secretMessage">The secret message.</param>
        /// <param name="key">The key.</param>
        /// <param name="nonSecretPayload">Optional non-secret payload.</param>
        /// <returns>Encrypted Message</returns>
        /// <remarks>
        /// Adds overhead of (Optional-Payload + BlockSize(16) + Message +  HMac-Tag(16)) * 1.33 Base64
        /// </remarks>
        private static byte[] Encrypt(byte[] secretMessage, byte[] key, byte[] nonSecretPayload = null)
        {
            // Key size check
            if (key == null || key.Length != KeyBitSize / 8)
            {
                throw new ArgumentException($"Key needs to be {KeyBitSize} bit!", nameof(key));
            }

            nonSecretPayload = nonSecretPayload ?? new byte[] { };

            // Using random nonce large enough not to repeat
            var nonce = new byte[NonceBitSize / 8];
            Random.NextBytes(nonce, 0, nonce.Length);

            var cipher = new GcmBlockCipher(new AesEngine());
            var parameters = new AeadParameters(new KeyParameter(key), MacBitSize, nonce, nonSecretPayload);
            cipher.Init(true, parameters);

            // Generate Cipher Text With Auth Tag
            var cipherText = new byte[cipher.GetOutputSize(secretMessage.Length)];
            var len = cipher.ProcessBytes(secretMessage, 0, secretMessage.Length, cipherText, 0);
            cipher.DoFinal(cipherText, len);

            // Assemble Message
            using (var combinedStream = new MemoryStream())
            {
                using (var binaryWriter = new BinaryWriter(combinedStream))
                {
                    // Prepend Authenticated Payload
                    binaryWriter.Write(nonSecretPayload);

                    // Prepend Nonce
                    binaryWriter.Write(nonce);

                    // Write Cipher Text
                    binaryWriter.Write(cipherText);
                }

                return combinedStream.ToArray();
            }
        }

        /// <summary>
        /// Decryption & Authentication (AES-GCM) of a UTF8 Message
        /// </summary>
        /// <param name="encryptedMessage">The encrypted message.</param>
        /// <param name="key">The key.</param>
        /// <param name="nonSecretPayloadLength">Length of the optional non-secret payload.</param>
        /// <returns>Decrypted Message</returns>
        private static byte[] Decrypt(byte[] encryptedMessage, byte[] key, int nonSecretPayloadLength = 0)
        {
            // Key size check
            if (key == null || key.Length != KeyBitSize / 8)
            {
                throw new ArgumentException($"Key needs to be {KeyBitSize} bit!", nameof(key));
            }

            using (var cipherStream = new MemoryStream(encryptedMessage))
            {
                using (var cipherReader = new BinaryReader(cipherStream))
                {
                    // Grab Payload
                    var nonSecretPayload = cipherReader.ReadBytes(nonSecretPayloadLength);

                    // Grab Nonce
                    var nonce = cipherReader.ReadBytes(NonceBitSize / 8);

                    var cipher = new GcmBlockCipher(new AesEngine());
                    var parameters = new AeadParameters(new KeyParameter(key), MacBitSize, nonce, nonSecretPayload);
                    cipher.Init(false, parameters);

                    // Decrypt Cipher Text
                    var cipherText = cipherReader.ReadBytes(encryptedMessage.Length - nonSecretPayloadLength - nonce.Length);
                    var plainText = new byte[cipher.GetOutputSize(cipherText.Length)];

                    try
                    {
                        var len = cipher.ProcessBytes(cipherText, 0, cipherText.Length, plainText, 0);
                        cipher.DoFinal(plainText, len);

                    }
                    catch (InvalidCipherTextException)
                    {
                        // Return null if it doesn't authenticate
                        return null;
                    }

                    return plainText;
                }
            }
        }
    }
}
