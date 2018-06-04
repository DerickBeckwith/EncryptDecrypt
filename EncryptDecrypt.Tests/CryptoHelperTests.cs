using NUnit.Framework;

namespace EncryptDecrypt.Tests
{
    [TestFixture]
    public class CryptoHelperTests
    {
        private const string SecretMessage = "Secret Message";
        private const string Password = "SuperSecretPassword";
        private const string ShortPassword = "Pass";

        [Test]
        public void Password_MeetsMinimumLengthRequirement()
        {
            // Assert
            Assert.GreaterOrEqual(Password.Length, CryptoHelper.MinPasswordLength);
        }

        [Test]
        public void ShortPassword_FailsMinimumLengthRequirement()
        {
            // Assert
            Assert.Less(ShortPassword.Length, CryptoHelper.MinPasswordLength);
        }

        [Test]
        public void EncryptWithPassword_ThrowsArgumentException_WhenSecretMessageIsEmptyString()
        {
            // Act and Assert
            Assert.That(() => CryptoHelper.EncryptWithPassword(string.Empty, Password), Throws.ArgumentException);
        }

        [Test]
        public void EncryptWithPassword_ThrowsArgumentException_WhenPasswordLengthIsLessThanMinimum()
        {
            // Act and Assert
            Assert.That(() => CryptoHelper.EncryptWithPassword(SecretMessage, ShortPassword), Throws.ArgumentException);
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
            // Act and Assert
            Assert.That(() => CryptoHelper.DecryptWithPassword(string.Empty, Password), Throws.ArgumentException);
        }

        [Test]
        public void DecryptWithPassword_ThrowsArgumentException_WhenPasswordLengthIsLessThanMinimum()
        {
            // Arrange
            var cipher = CryptoHelper.EncryptWithPassword(SecretMessage, Password);

            // Act and Assert
            Assert.That(() => CryptoHelper.DecryptWithPassword(cipher, ShortPassword), Throws.ArgumentException);
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
