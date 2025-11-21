using Xunit;
using BillShifor.WpCalculator;
using System.Linq;

namespace TestCode
{
    /// <summary>
    /// Тесты для методов Крипто-Анализатора (TabItem 2)
    /// </summary>
    public class CryptoAnalyzerTests
    {
        [Fact]
        public void AnalyzeCryptoAlgorithm_CaesarCipher_ReturnsValidResult()
        {
            // Arrange
            string algorithm = "цезарь";
            string code = @"function encrypt(text, key) {
    shift := key % 32;
    result := """";
    foreach char in text {
        if (char in alphabet) {
            idx := alphabet.indexOf(char);
            new_idx := (idx + shift) % alphabet.length;
            result := result + alphabet[new_idx];
        }
    }
    return result;
}";
            string postcondition = "decrypt(encrypt(text, key), key) == text";

            // Act
            var result = CryptoWpEngine.AnalyzeCryptoAlgorithm(algorithm, code, postcondition);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result.Steps);
            Assert.NotNull(result.FinalPrecondition);
            Assert.NotNull(result.SecurityAssessment);
            Assert.NotNull(result.HoareTriple);
        }

        [Fact]
        public void AnalyzeCryptoAlgorithm_RSACipher_ContainsSecurityRules()
        {
            // Arrange
            string algorithm = "rsa";
            string code = @"function rsa_encrypt(m, e, n) {
    c := mod_pow(m, e, n);
    return c;
}";
            string postcondition = "decrypt(encrypt(m, e, n), d, n) == m";

            // Act
            var result = CryptoWpEngine.AnalyzeCryptoAlgorithm(algorithm, code, postcondition);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Steps.Count > 0);
            
            // Проверка, что добавлены правила безопасности для RSA
            var securitySteps = result.Steps.Where(s => s.Description.Contains("правило безопасности")).ToList();
            Assert.True(securitySteps.Count >= 4, "Должно быть минимум 4 правила безопасности для RSA");
        }

        [Fact]
        public void AnalyzeCryptoAlgorithm_VigenereCipher_ReturnsCorrectHoareTriple()
        {
            // Arrange
            string algorithm = "виженер";
            string code = "result := encrypt(text, key);";
            string postcondition = "IsReversible(encrypt, decrypt)";

            // Act
            var result = CryptoWpEngine.AnalyzeCryptoAlgorithm(algorithm, code, postcondition);

            // Assert
            Assert.NotNull(result.HoareTriple);
            Assert.Contains(algorithm, result.HoareTriple);
            Assert.Contains(postcondition, result.HoareTriple);
        }

        [Fact]
        public void AnalyzeCryptoAlgorithm_WithDivision_AddsSafetyCheck()
        {
            // Arrange
            string algorithm = "цезарь";
            string code = @"result := value / divisor;";
            string postcondition = "result > 0";

            // Act
            var result = CryptoWpEngine.AnalyzeCryptoAlgorithm(algorithm, code, postcondition);

            // Assert
            Assert.NotNull(result.FinalPrecondition);
            // Проверка, что добавлена проверка деления на ноль
            Assert.Contains("denominator", result.FinalPrecondition);
        }

        [Fact]
        public void AnalyzeCryptoAlgorithm_EmptyCode_ReturnsBasicAnalysis()
        {
            // Arrange
            string algorithm = "цезарь";
            string code = "";
            string postcondition = "true";

            // Act
            var result = CryptoWpEngine.AnalyzeCryptoAlgorithm(algorithm, code, postcondition);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Steps.Count > 0); // Должен быть хотя бы начальный шаг
        }

        [Fact]
        public void AnalyzeCryptoAlgorithm_PlayfairCipher_HasMatrixValidation()
        {
            // Arrange
            string algorithm = "плейфер";
            string code = "matrix := createMatrix(key);";
            string postcondition = "IsValidMatrix(matrix)";

            // Act
            var result = CryptoWpEngine.AnalyzeCryptoAlgorithm(algorithm, code, postcondition);

            // Assert
            Assert.NotNull(result);
            var securitySteps = result.Steps.Where(s => s.Expression.Contains("matrix")).ToList();
            Assert.NotEmpty(securitySteps);
        }

        [Fact]
        public void AnalyzeCryptoAlgorithm_VernamCipher_ChecksKeyLength()
        {
            // Arrange
            string algorithm = "вернам";
            string code = "encrypted := xor(text, key);";
            string postcondition = "decrypt(encrypted, key) == text";

            // Act
            var result = CryptoWpEngine.AnalyzeCryptoAlgorithm(algorithm, code, postcondition);

            // Assert
            Assert.NotNull(result);
            // Проверка, что есть правило о длине ключа
            var keyLengthCheck = result.Steps.Any(s => s.Expression.Contains("key.Length"));
            Assert.True(keyLengthCheck, "Должна быть проверка длины ключа для шифра Вернама");
        }

        [Fact]
        public void AnalyzeCryptoAlgorithm_SecureAlgorithm_IsSecureIsTrue()
        {
            // Arrange
            string algorithm = "цезарь";
            string code = "result := caesar(text, key);";
            string postcondition = "result != null";

            // Act
            var result = CryptoWpEngine.AnalyzeCryptoAlgorithm(algorithm, code, postcondition);

            // Assert
            // Проверка, что алгоритм считается безопасным
            Assert.Contains("прошел проверку", result.SecurityAssessment, StringComparison.OrdinalIgnoreCase);
        }

        [Theory]
        [InlineData("цезарь")]
        [InlineData("виженер")]
        [InlineData("rsa")]
        [InlineData("плейфер")]
        [InlineData("вернам")]
        public void AnalyzeCryptoAlgorithm_DifferentAlgorithms_AllReturnResults(string algorithm)
        {
            // Arrange
            string code = "encrypted := encrypt(text, key);";
            string postcondition = "IsValid(encrypted)";

            // Act
            var result = CryptoWpEngine.AnalyzeCryptoAlgorithm(algorithm, code, postcondition);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result.Steps);
            Assert.NotNull(result.FinalPrecondition);
            Assert.NotNull(result.SecurityAssessment);
            Assert.Contains(algorithm, result.HoareTriple);
        }
    }
}
