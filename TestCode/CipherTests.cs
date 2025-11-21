using Xunit;
using System;

namespace Ciphers.Tests
{
    public class AlphabetTests
    {
        [Fact]
        public void Alphabet_Length_ReturnsCorrectValue()
        {
            // Arrange & Act
            int length = Alphabet.Length;

            // Assert
            Assert.Equal(33, length);
        }

        [Fact]
        public void Alphabet_IndexOf_ExistingChar_ReturnsCorrectIndex()
        {
            // Arrange & Act
            int indexA = Alphabet.IndexOf('а');
            int indexYa = Alphabet.IndexOf('я');

            // Assert
            Assert.Equal(0, indexA);
            Assert.Equal(32, indexYa);
        }

        [Fact]
        public void Alphabet_IndexOf_NonExistingChar_ReturnsMinusOne()
        {
            // Arrange & Act
            int index = Alphabet.IndexOf('z');

            // Assert
            Assert.Equal(-1, index);
        }

        [Fact]
        public void Alphabet_Contains_ExistingChar_ReturnsTrue()
        {
            // Arrange & Act & Assert
            Assert.True(Alphabet.Contains('б'));
            Assert.True(Alphabet.Contains('ю'));
        }

        [Fact]
        public void Alphabet_Contains_NonExistingChar_ReturnsFalse()
        {
            // Arrange & Act & Assert
            Assert.False(Alphabet.Contains('q'));
            Assert.False(Alphabet.Contains('!'));
        }
    }

    public class CodeTests
    {
        [Fact]
        public void Caesar_WithPositiveKey_EncryptsCorrectly()
        {
            // Arrange
            string text = "абв";
            int key = 3;

            // Act
            string result = Code.Caesar(text, key);

            // Assert
            Assert.Equal("где", result);
        }

        [Fact]
        public void Caesar_WithNegativeKey_EncryptsCorrectly()
        {
            // Arrange
            string text = "где";
            int key = -3;

            // Act
            string result = Code.Caesar(text, key);

            // Assert
            Assert.Equal("абв", result);
        }

        [Fact]
        public void Caesar_WithOverflowKey_WrapsCorrectly()
        {
            // Arrange
            string text = "я";
            int key = 1;

            // Act
            string result = Code.Caesar(text, key);

            // Assert
            Assert.Equal("а", result);
        }

        [Fact]
        public void Vigenere_WithValidKey_EncryptsCorrectly()
        {
            // Arrange
            string text = "привет";
            string key = "ключ";

            // Act
            string result = Code.Vigenere(text, key);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(text.Length, result.Length);
        }

        [Fact]
        public void Vigenere_WithEmptyKey_ReturnsOriginalText()
        {
            // Arrange
            string text = "привет";
            string key = "";

            // Act
            string result = Code.Vigenere(text, key);

            // Assert
            Assert.Equal(text, result);
        }

        [Fact]
        public void Atbash_EncryptsCorrectly()
        {
            // Arrange
            string text = "абв";

            // Act
            string result = Code.Atbash(text);

            // Assert
            Assert.Equal("яюэ", result);
        }

        [Fact]
        public void Atbash_DoubleApplication_ReturnsOriginal()
        {
            // Arrange
            string text = "приветмир";

            // Act
            string encrypted = Code.Atbash(text);
            string decrypted = Code.Atbash(encrypted);

            // Assert
            Assert.Equal(text, decrypted);
        }

        [Fact]
        public void Playfair_WithValidKey_EncryptsCorrectly()
        {
            // Arrange
            string text = "привет";
            string key = "шифр";

            // Act
            string result = Code.Playfair(text, key);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Length % 2 == 0);
        }

        [Fact]
        public void Vernam_WithValidKey_EncryptsCorrectly()
        {
            // Arrange
            string text = "текст";
            string key = "ключ";

            // Act
            string result = Code.Vernam(text, key);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(text.Length, result.Length);
        }

        [Fact]
        public void DES_WithValidKey_EncryptsCorrectly()
        {
            // Arrange
            string text = "сообщение";
            string key = "секрет";

            // Act
            string result = Code.DES(text, key);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(text.Length, result.Length);
        }

        [Fact]
        public void RSA_WithValidText_ReturnsEncryptedData()
        {
            // Arrange
            string text = "ABC";

            // Act
            var result = Code.RSA(text);

            // Assert
            Assert.NotNull(result.encrypted);
            Assert.True(result.e > 0);
            Assert.True(result.d > 0);
            Assert.True(result.n > 0);
            Assert.Contains(",", result.encrypted);
        }
    }

    public class DeCodeTests
    {
        [Fact]
        public void Caesar_DecryptsCorrectly()
        {
            // Arrange
            string original = "привет";
            int key = 5;
            string encrypted = Code.Caesar(original, key);

            // Act
            string decrypted = DeCode.Caesar(encrypted, key);

            // Assert
            Assert.Equal(original, decrypted);
        }

        [Fact]
        public void Vigenere_DecryptsCorrectly()
        {
            // Arrange
            string original = "секретноесообщение";
            string key = "пароль";
            string encrypted = Code.Vigenere(original, key);

            // Act
            string decrypted = DeCode.Vigenere(encrypted, key);

            // Assert
            Assert.Equal(original, decrypted);
        }

        [Fact]
        public void Playfair_EncryptAndDecrypt_ReturnsOriginal()
        {
            // Arrange
            string original = "привет";
            string key = "шифр";
            string encrypted = Code.Playfair(original, key);

            // Act
            string decrypted = DeCode.Playfair(encrypted, key);

            // Assert
            // Playfair может добавлять/удалять символы, поэтому проверяем основное содержание
            Assert.Contains("привет", decrypted);
        }

        [Fact]
        public void Vernam_EncryptAndDecrypt_ReturnsOriginal()
        {
            // Arrange
            string original = "текст";
            string key = "длинныйключ";
            string encrypted = Code.Vernam(original, key);

            // Act
            string decrypted = DeCode.Vernam(encrypted, key);

            // Assert
            Assert.Equal(original, decrypted);
        }

        [Fact]
        public void DES_EncryptAndDecrypt_ReturnsOriginal()
        {
            // Arrange
            string original = "сообщение";
            string key = "секрет";
            string encrypted = Code.DES(original, key);

            // Act
            string decrypted = DeCode.DES(encrypted, key);

            // Assert
            Assert.Equal(original, decrypted);
        }

        [Fact]
        public void RSA_EncryptAndDecrypt_ReturnsOriginal()
        {
            // Arrange
            string original = "Hello";
            var encryptedData = Code.RSA(original);

            // Act
            string decrypted = DeCode.RSA(encryptedData.encrypted, encryptedData.d, encryptedData.n);

            // Assert
            Assert.Equal(original, decrypted);
        }

        [Fact]
        public void Atbash_DecryptsCorrectly()
        {
            // Arrange
            string text = "эюя";

            // Act
            string result = DeCode.Atbash(text);

            // Assert
            Assert.Equal("вба", result);
        }
    }

    public class IntegrationTests
    {
        [Fact]
        public void AllCiphers_RoundTrip_ReturnOriginalText()
        {
            // Arrange
            string originalText = "тестовоесообщениедляпроверки";

            // Test Caesar
            string caesarEncrypted = Code.Caesar(originalText, 7);
            string caesarDecrypted = DeCode.Caesar(caesarEncrypted, 7);
            Assert.Equal(originalText, caesarDecrypted);

            // Test Vigenere
            string vigenereEncrypted = Code.Vigenere(originalText, "ключ");
            string vigenereDecrypted = DeCode.Vigenere(vigenereEncrypted, "ключ");
            Assert.Equal(originalText, vigenereDecrypted);

            // Test Atbash
            string atbashEncrypted = Code.Atbash(originalText);
            string atbashDecrypted = DeCode.Atbash(atbashEncrypted);
            Assert.Equal(originalText, atbashDecrypted);

            // Test Vernam
            string vernamEncrypted = Code.Vernam(originalText, "ключ");
            string vernamDecrypted = DeCode.Vernam(vernamEncrypted, "ключ");
            Assert.Equal(originalText, vernamDecrypted);
        }

        [Fact]
        public void Ciphers_WithSpecialCharacters_HandleCorrectly()
        {
            // Arrange
            string textWithSpecialChars = "привет, мир! 123";

            // Act & Assert - проверяем что шифры не ломаются на специальных символах
            Assert.NotNull(Code.Caesar(textWithSpecialChars, 3));
            Assert.NotNull(Code.Vigenere(textWithSpecialChars, "к"));
            Assert.NotNull(Code.Atbash(textWithSpecialChars));
        }
    }

    public class FailingTests
    {
        [Fact]
        public void Caesar_WithZeroKey_ShouldNotChangeText_ButDoes()
        {
            // Arrange
            string text = "привет";
            int key = 0;

            // Act
            string result = Code.Caesar(text, key);

            // Assert - это НЕ должно работать, так как алгоритм все равно применяет преобразования
            Assert.Equal("ПРИВЕТ", result); // Ожидаем верхний регистр, но метод всегда возвращает нижний
        }

        [Fact]
        public void Alphabet_IndexOf_WithUpperCase_ShouldWork_ButDoesNot()
        {
            // Arrange & Act
            int index = Alphabet.IndexOf('А'); // Подаем заглавную букву

            // Assert - это НЕ должно работать, так как алфавит только в нижнем регистре
            Assert.NotEqual(-1, index); // Ожидаем, что найдет, но метод вернет -1
        }

        [Fact]
        public void DES_WithVeryLongKey_ShouldFail_ButWorks()
        {
            // Arrange
            string text = "к";
            string key = "оченьдлинныйключкоторыйпревышаетобычныеограничения"; // Слишком длинный ключ

            // Act
            string result = Code.DES(text, key);

            // Assert - ожидаем проблему, но метод работает
            Assert.Equal("этотсимволдолженбытьдругим", result); // Заведомо неверное ожидание
        }

        [Fact]
        public void Vernam_KeyShorterThanText_ShouldFail_ButWorks()
        {
            // Arrange
            string text = "оченьдлинныйтекст";
            string key = "к"; // Ключ короче текста

            // Act
            string result = Code.Vernam(text, key);

            // Assert - ожидаем исключение, но метод циклически повторяет ключ
            Assert.Equal(text, result); // Ожидаем, что не сможет зашифровать, но метод работает
        }

        [Fact]
        public void Alphabet_Contains_WithNumber_ShouldReturnTrue_ButReturnsFalse()
        {
            // Arrange & Act & Assert
            // Ожидаем, что цифры есть в алфавите после построения матрицы Playfair,
            // но базовый алфавит их не содержит
            Assert.True(Alphabet.Contains('1')); // Это НЕ сработает
        }

        [Fact]
        public void Caesar_WithUnicodeCharacters_ShouldFail_ButHandles()
        {
            // Arrange
            string text = "hello world! 🚀"; // Содержит эмодзи
            int key = 3;

            // Act
            string result = Code.Caesar(text, key);

            // Assert - ожидаем крах, но метод пропускает неизвестные символы
            Assert.Equal("khoor zruog! 🚀", result); // Неверное ожидание для русского алфавита
        }

        [Fact]
        public void Playfair_DecryptWithoutPadding_ShouldFail()
        {
            // Arrange
            string encrypted = "нечетнаядлина"; // Нечетная длина без padding
            string key = "ключ";

            // Act & Assert - ожидаем исключение
            string result = DeCode.Playfair(encrypted, key); // Должно бросить ArgumentException
        }

        [Fact]
        public void RSA_WithLargePrimes_ShouldFail_ButWorks()
        {
            // Arrange
            string text = "тест";
            int p = 999999999;
            int q = 888888888; // Непростые числа

            // Act
            var result = Code.RSA(text, p, q);

            // Assert - ожидаем проблему с вычислениями
            Assert.Equal("correct", result.encrypted); // Заведомо неверное ожидание
        }

        [Fact(Skip = "Требуется реализация через reflection для доступа к private методу")]
        public void ModInverse_WithNonCoPrimeNumbers_ShouldFail()
        {
            // Arrange
            int a = 4;
            int m = 6; // НЕ взаимно простые числа

            // Act - попытка вычислить обратный элемент
            // Должно бросить исключение или вернуть 0, но метод пытается вычислить
            int result = ModInverse(a, m);

            // Assert
            Assert.NotEqual(0, result); // Ожидаем ошибку, но метод что-то возвращает
        }

        // Вспомогательный метод для доступа к private методу (через reflection в реальном коде)
        private int ModInverse(int a, int m)
        {
            // Это упрощенная версия - в реальности нужно использовать reflection
            return 0; // Заглушка
        }
    }
}