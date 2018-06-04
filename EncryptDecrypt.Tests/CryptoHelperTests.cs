using NUnit.Framework;

namespace EncryptDecrypt.Tests
{
    [TestFixture]
    public class CryptoHelperTests
    {
        private const string SecretMessage = "Secret Message";
        private const string Password = "SuperSecretPassword";

        [Test]
        public void EncryptWithPassword_ThrowsArgumentException_WhenSecretMessageIsEmptyString()
        {
            Assert.That(() => CryptoHelper.EncryptWithPassword(string.Empty, Password), Throws.ArgumentException);
        }

        [Test]
        public void EncryptWithPassword_ReturnsNotNullOrEmptyString()
        {
            // Act
            var cipher = CryptoHelper.EncryptWithPassword(SecretMessage, Password);

            // Assert
            Assert.IsNotNull(cipher);
            Assert.IsNotEmpty(cipher);
        }

        [Test]
        public void EncryptWithPassword_ReturnsCipherText()
        {
            // Act
            var cipher = CryptoHelper.EncryptWithPassword(SecretMessage, Password);

            // Assert
            Assert.AreNotEqual(SecretMessage, cipher);
        }

        [Test]
        public void DecryptWithPassword_ThrowsArgumentException_WhenEncryptedMessageIsEmptyString()
        {
            Assert.That(() => CryptoHelper.DecryptWithPassword(string.Empty, Password), Throws.ArgumentException);
        }

        [Test]
        public void DecryptWithPassword_ReturnsNotNullOrEmptyString()
        {
            // Arrange
            var cipher = CryptoHelper.EncryptWithPassword(SecretMessage, Password);

            // Act
            var decrypted = CryptoHelper.DecryptWithPassword(cipher, Password);

            // Assert
            Assert.IsNotNull(decrypted);
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
    }
}
