using NUnit.Framework;

namespace EncryptDecrypt.Tests
{
    [TestFixture]
    public class CryptoHelperTests
    {
        private const string Secret = "Secret Message to Encrypt";
        private const string Password = "SuperSecretPassword";

        [Test]
        public void DecryptReturnsNonNullString()
        {
            // Act
            var cipher = CryptoHelper.EncryptWithPassword(Secret, Password);

            // Assert
            Assert.IsNotNull(cipher);
        }

        [Test]
        public void DecryptReturnsNonEmptyString()
        {
            // Act
            var cipher = CryptoHelper.EncryptWithPassword(Secret, Password);

            // Assert
            Assert.IsNotEmpty(cipher);
        }
    }
}
