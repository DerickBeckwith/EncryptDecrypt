using NUnit.Framework;

namespace EncryptDecrypt.Tests
{
    [TestFixture]
    public class CryptoHelperTests
    {
        private const string SecretMessage = "Secret Message";
        private const string Password = "SuperSecretPassword";

        [Test]
        public void EncryptWithPassword_ThrowsArgumentException_WhenSecretMessageIsNull()
        {
            Assert.That(() => CryptoHelper.EncryptWithPassword(null, Password), Throws.ArgumentException);
        }

        [Test]
        public void EncryptWithPassword_ReturnsNonNullString()
        {
            // Act
            var cipher = CryptoHelper.EncryptWithPassword(SecretMessage, Password);

            // Assert
            Assert.IsNotNull(cipher);
        }

        [Test]
        public void EncryptWithPassword_ReturnsNonEmptyString()
        {
            // Act
            var cipher = CryptoHelper.EncryptWithPassword(SecretMessage, Password);

            // Assert
            Assert.IsNotEmpty(cipher);
        }

        [Test]
        public void EncryptWithPassword_DoesNotReturnPlainText()
        {
            // Act
            var cipher = CryptoHelper.EncryptWithPassword(SecretMessage, Password);

            // Assert
            Assert.AreNotEqual(SecretMessage, cipher);
        }

        [Test]
        public void DecryptWithPassword_ThrowsArgumentException_WhenEncryptedMessageIsNull()
        {
            Assert.That(() => CryptoHelper.DecryptWithPassword(null, Password), Throws.ArgumentException);
        }

        [Test]
        public void DecryptWithPassword_ReturnsNonNullString()
        {
            // Arrange
            var cipher = CryptoHelper.EncryptWithPassword(SecretMessage, Password);

            // Act
            var decrypted = CryptoHelper.DecryptWithPassword(cipher, Password);

            // Assert
            Assert.IsNotNull(decrypted);
        }

        [Test]
        public void DecryptWithPassword_ReturnsNonEmptyString()
        {
            // Arrange
            var cipher = CryptoHelper.EncryptWithPassword(SecretMessage, Password);

            // Act
            var decrypted = CryptoHelper.DecryptWithPassword(cipher, Password);

            // Assert
            Assert.IsNotEmpty(decrypted);
        }

        [Test]
        public void DecryptWithPassword_ReturnsOriginalPlainText()
        {
            // Arrange
            var cipher = CryptoHelper.EncryptWithPassword(SecretMessage, Password);

            // Act
            var decrypted = CryptoHelper.DecryptWithPassword(cipher, Password);

            // Assert
            Assert.AreEqual(SecretMessage, decrypted);
        }

        [Test]
        public void DecryptWithPassword_DoesNotReturnCipherText()
        {
            // Arrange
            var cipher = CryptoHelper.EncryptWithPassword(SecretMessage, Password);

            // Act
            var decrypted = CryptoHelper.DecryptWithPassword(cipher, Password);

            // Assert
            Assert.AreNotEqual(cipher, decrypted);
        }
    }
}
