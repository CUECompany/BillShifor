namespace Ciphers
{

    public class Code
    {
        private static readonly char[] russianAlphabetLower = {
    'а', 'б', 'в', 'г', 'д', 'е', 'ё', 'ж', 'з', 'и', 'й',
    'к', 'л', 'м', 'н', 'о', 'п', 'р', 'с', 'т', 'у', 'ф',
    'х', 'ц', 'ч', 'ш', 'щ', 'ъ', 'ы', 'ь', 'э', 'ю', 'я'
};
        public static string Caesar(string str, int key)
        {
            str = str.ToLower();
            char[] charArray = str.ToCharArray();
            int lngth = charArray.Length;
            for (int i = 0; i < lngth; i++)
            {
                char sim = charArray[i];
                if (sim == ' ' || Array.IndexOf(russianAlphabetLower, sim) == -1)
                {
                    continue;
                }
                int ind = Array.IndexOf(russianAlphabetLower, sim);
                ind = (ind + key) % russianAlphabetLower.Length;
                charArray[i] = russianAlphabetLower[ind];
            }
            return new string(charArray);
        }
        public static string Vijener(string str, string key)
        {
            string newString = "";
            int lnght = str.Length;
            int cntKey = key.Length;
            char[] charArray = str.ToCharArray();
            for (int i = 0; i < lnght; i++)
            {
                char sim = str[i];
                int pozStr = Array.IndexOf(russianAlphabetLower, sim);
                sim = key[i % (cntKey)];
                int posKey = Array.IndexOf(russianAlphabetLower, sim);
                pozStr = (pozStr + posKey) % russianAlphabetLower.Length;
                charArray[i] = russianAlphabetLower[(int)pozStr % russianAlphabetLower.Length];
            }
            return new string(charArray);
        }
        public static string Atbash(string str)
        {
            str = str.ToLower();
            char[] result = new char[str.Length];
            for (int i = 0; i < str.Length; i++)
            {
                char c = str[i];
                int idx = Array.IndexOf(russianAlphabetLower, c);
                if (idx == -1)
                {
                    result[i] = c; // оставляем как есть (пробелы, знаки)
                }
                else
                {
                    result[i] = russianAlphabetLower[russianAlphabetLower.Length - 1 - idx];
                }
            }
            return new string(result);
        }

        // === Шифр Плейфера (Playfair) для русского алфавита ===
        public static string Playfair(string str, string key)
        {
            // Подготовка ключа: удаляем дубликаты и строим матрицу 6x6 (36 ячеек, но у нас 33 буквы → добавим 3 символа или упростим)
            // Для упрощения: будем использовать 33 буквы → матрица 6x6 = 36, оставим 3 ячейки пустыми или заменим "ё" на "е"
            // Но в твоём алфавите "ё" есть → используем 33 буквы → матрица 6x6, последние 3 ячейки игнорируем

            // Удалим "ё" для упрощения до 32 букв? Нет — оставим 33 и сделаем матрицу 6x6 (36), заполним по порядку
            // Но Playfair требует чётное число → добавим "ъ" как разделитель или удвоим последнюю букву

            // Упрощённая реализация:
            string cleanKey = new string(key.ToLower().Where(c => Array.IndexOf(russianAlphabetLower, c) != -1).ToArray());
            var seen = new HashSet<char>();
            var matrixChars = new List<char>();

            // Добавляем уникальные символы из ключа
            foreach (char c in cleanKey)
            {
                if (!seen.Contains(c))
                {
                    seen.Add(c);
                    matrixChars.Add(c);
                }
            }
            // Добавляем остальной алфавит
            foreach (char c in russianAlphabetLower)
            {
                if (!seen.Contains(c))
                {
                    matrixChars.Add(c);
                }
            }

            // Матрица 6x6 (36), но у нас 33 буквы → дополним до 36 символами (например, '1','2','3') или просто игнорируем
            // Для простоты: используем только первые 33, а при шифровании будем обрабатывать пары

            char[,] matrix = new char[6, 6];
            int idx = 0;
            for (int i = 0; i < 6; i++)
            {
                for (int j = 0; j < 6; j++)
                {
                    if (idx < matrixChars.Count)
                    {
                        matrix[i, j] = matrixChars[idx++];
                    }
                    else
                    {
                        matrix[i, j] = '\0'; // пусто
                    }
                }
            }

            // Функция поиска позиции
            (int row, int col) FindPos(char c)
            {
                for (int i = 0; i < 6; i++)
                    for (int j = 0; j < 6; j++)
                        if (matrix[i, j] == c)
                            return (i, j);
                return (-1, -1);
            }

            // Подготовка текста: удаляем неалфавитные символы, разбиваем на биграммы
            var cleanText = str.ToLower().Where(c => Array.IndexOf(russianAlphabetLower, c) != -1).ToArray();
            var list = new List<char>(cleanText);

            // Обработка одинаковых подряд идущих букв
            for (int i = 0; i < list.Count - 1; i += 2)
            {
                if (list[i] == list[i + 1])
                {
                    list.Insert(i + 1, 'ъ'); // вставляем разделитель
                }
            }

            // Если нечётное — добавляем 'ъ'
            if (list.Count % 2 == 1)
                list.Add('ъ');

            var result = new char[list.Count];
            for (int i = 0; i < list.Count; i += 2)
            {
                char a = list[i], b = list[i + 1];
                var (row1, col1) = FindPos(a);
                var (row2, col2) = FindPos(b);

                if (row1 == row2)
                {
                    result[i] = matrix[row1, (col1 + 1) % 6];
                    result[i + 1] = matrix[row2, (col2 + 1) % 6];
                }
                else if (col1 == col2)
                {
                    result[i] = matrix[(row1 + 1) % 6, col1];
                    result[i + 1] = matrix[(row2 + 1) % 6, col2];
                }
                else
                {
                    result[i] = matrix[row1, col2];
                    result[i + 1] = matrix[row2, col1];
                }
            }

            return new string(result);
        }

        // === Шифр Вернама (XOR с ключом, но для букв) ===
        public static string Vernam(string str, string key)
        {
            str = str.ToLower();
            key = key.ToLower();

            var result = new char[str.Length];
            for (int i = 0; i < str.Length; i++)
            {
                char c = str[i];
                int idx = Array.IndexOf(russianAlphabetLower, c);
                if (idx == -1)
                {
                    result[i] = c;
                    continue;
                }

                char k = key[i % key.Length];
                int keyIdx = Array.IndexOf(russianAlphabetLower, k);
                if (keyIdx == -1) keyIdx = 0;

                int encryptedIdx = (idx + keyIdx) % russianAlphabetLower.Length;
                result[i] = russianAlphabetLower[encryptedIdx];
            }
            return new string(result);
        }

        // === Упрощённый RSA (ТОЛЬКО ДЛЯ ОБУЧЕНИЯ!) ===
        public static (string encrypted, int e, int n) RSA(string str, int p = 61, int q = 53)
        {
            // p и q — простые числа (по умолчанию 61 и 53 → n = 3233, φ = 3120)
            int n = p * q;
            int phi = (p - 1) * (q - 1);
            int e = 17; // должно быть взаимно просто с phi
            int d = ModInverse(e, phi); // закрытый ключ (не возвращаем)

            var numbers = str.ToLower()
                .Where(c => Array.IndexOf(russianAlphabetLower, c) != -1)
                .Select(c => Array.IndexOf(russianAlphabetLower, c))
                .ToArray();

            var encrypted = numbers.Select(num => ModPow(num, e, n)).ToArray();
            string encryptedStr = string.Join(",", encrypted);
            return (encryptedStr, e, n);
        }

        // Вспомогательные функции для RSA
        private static long ModPow(long baseValue, long exp, long mod)
        {
            long result = 1;
            baseValue = baseValue % mod;
            while (exp > 0)
            {
                if (exp % 2 == 1)
                    result = (result * baseValue) % mod;
                exp = exp >> 1;
                baseValue = (baseValue * baseValue) % mod;
            }
            return result;
        }

        private static int ModInverse(int a, int m)
        {
            for (int x = 1; x < m; x++)
                if ((a * x) % m == 1)
                    return x;
            throw new Exception("Modular inverse does not exist");
        }

        // === Упрощённый DES (на самом деле — учебная подстановка, НЕ настоящий DES!) ===
        public static string DES(string str, string key)
        {
            // Настоящий DES — блочный шифр с 64-битными блоками и сложной структурой.
            // Здесь — имитация: используем ключ для сдвига по Цезарю с изменяющимся шагом
            str = str.ToLower();
            key = key.ToLower();

            var result = new char[str.Length];
            int keySum = key.Sum(c => (int)c) % russianAlphabetLower.Length;

            for (int i = 0; i < str.Length; i++)
            {
                char c = str[i];
                int idx = Array.IndexOf(russianAlphabetLower, c);
                if (idx == -1)
                {
                    result[i] = c;
                    continue;
                }

                int shift = (keySum + i) % russianAlphabetLower.Length;
                int encryptedIdx = (idx + shift) % russianAlphabetLower.Length;
                result[i] = russianAlphabetLower[encryptedIdx];
            }
            return new string(result);
        }
    }
    public class DeCode
    {
        private static readonly char[] russianAlphabetLower = {
    'а', 'б', 'в', 'г', 'д', 'е', 'ё', 'ж', 'з', 'и', 'й',
    'к', 'л', 'м', 'н', 'о', 'п', 'р', 'с', 'т', 'у', 'ф',
    'х', 'ц', 'ч', 'ш', 'щ', 'ъ', 'ы', 'ь', 'э', 'ю', 'я'
};
        public static string Caesar(string str, int key)
        {
            str = str.ToLower();
            char[] charArray = str.ToCharArray();
            int lngth = charArray.Length;
            int alphabetLength = russianAlphabetLower.Length;

            for (int i = 0; i < lngth; i++)
            {
                char sim = charArray[i];

                int ind = Array.IndexOf(russianAlphabetLower, sim);

                if (ind == -1)
                {
                    continue;
                }
                ind = (ind - key) % alphabetLength;
                if (ind < 0)
                {
                    ind += alphabetLength;
                }

                charArray[i] = russianAlphabetLower[ind];
            }
            return new string(charArray);
        }
        public static string DeVijener(string str, string key)
        {
            string newString = "";
            int lnght = str.Length;
            int cntKey = key.Length;
            char[] charArray = str.ToCharArray();

            for (int i = 0; i < lnght; i++)
            {
                char sim = str[i];
                int pozStr = Array.IndexOf(russianAlphabetLower, sim);
                sim = key[i % cntKey];
                int posKey = Array.IndexOf(russianAlphabetLower, sim);

                // Основное отличие: вычитаем позицию ключа вместо сложения
                pozStr = (pozStr - posKey + russianAlphabetLower.Length) % russianAlphabetLower.Length;
                charArray[i] = russianAlphabetLower[pozStr];
            }

            return new string(charArray);
        }
        // === Атбаш — сам себе дешифратор ===
        public static string DeAtbash(string str) => Code.Atbash(str);

        // === Дешифровка Плейфера ===
        public static string DePlayfair(string str, string key)
        {
            // Аналогично шифрованию, но с обратными сдвигами
            string cleanKey = new string(key.ToLower().Where(c => Array.IndexOf(russianAlphabetLower, c) != -1).ToArray());
            var seen = new HashSet<char>();
            var matrixChars = new List<char>();

            foreach (char c in cleanKey)
            {
                if (!seen.Contains(c))
                {
                    seen.Add(c);
                    matrixChars.Add(c);
                }
            }
            foreach (char c in russianAlphabetLower)
            {
                if (!seen.Contains(c))
                {
                    matrixChars.Add(c);
                }
            }

            char[,] matrix = new char[6, 6];
            int idx = 0;
            for (int i = 0; i < 6; i++)
            {
                for (int j = 0; j < 6; j++)
                {
                    if (idx < matrixChars.Count)
                    {
                        matrix[i, j] = matrixChars[idx++];
                    }
                    else
                    {
                        matrix[i, j] = '\0';
                    }
                }
            }

            (int row, int col) FindPos(char c)
            {
                for (int i = 0; i < 6; i++)
                    for (int j = 0; j < 6; j++)
                        if (matrix[i, j] == c)
                            return (i, j);
                return (-1, -1);
            }

            var cleanText = str.ToLower().Where(c => Array.IndexOf(russianAlphabetLower, c) != -1).ToArray();
            if (cleanText.Length % 2 != 0)
                throw new ArgumentException("Encrypted text must have even length");

            var result = new char[cleanText.Length];
            for (int i = 0; i < cleanText.Length; i += 2)
            {
                char a = cleanText[i], b = cleanText[i + 1];
                var (row1, col1) = FindPos(a);
                var (row2, col2) = FindPos(b);

                if (row1 == row2)
                {
                    result[i] = matrix[row1, (col1 + 5) % 6]; // -1 mod 6 = +5
                    result[i + 1] = matrix[row2, (col2 + 5) % 6];
                }
                else if (col1 == col2)
                {
                    result[i] = matrix[(row1 + 5) % 6, col1];
                    result[i + 1] = matrix[(row2 + 5) % 6, col2];
                }
                else
                {
                    result[i] = matrix[row1, col2];
                    result[i + 1] = matrix[row2, col1];
                }
            }

            // Удаляем разделители 'ъ' в конце или между одинаковыми буквами
            return new string(result).Replace("ъ", "");
        }

        // === Дешифровка Вернама ===
        public static string DeVernam(string str, string key)
        {
            str = str.ToLower();
            key = key.ToLower();

            var result = new char[str.Length];
            for (int i = 0; i < str.Length; i++)
            {
                char c = str[i];
                int idx = Array.IndexOf(russianAlphabetLower, c);
                if (idx == -1)
                {
                    result[i] = c;
                    continue;
                }

                char k = key[i % key.Length];
                int keyIdx = Array.IndexOf(russianAlphabetLower, k);
                if (keyIdx == -1) keyIdx = 0;

                int decryptedIdx = (idx - keyIdx + russianAlphabetLower.Length) % russianAlphabetLower.Length;
                result[i] = russianAlphabetLower[decryptedIdx];
            }
            return new string(result);
        }

        // === Дешифровка RSA ===
        public static string DeRSA(string encryptedNumbers, int d, int n)
        {
            var numbers = encryptedNumbers.Split(',').Select(long.Parse).ToArray();
            var decrypted = numbers.Select(num => ModPow(num, d, n)).ToArray();

            var result = new char[decrypted.Length];
            for (int i = 0; i < decrypted.Length; i++)
            {
                long val = decrypted[i];
                if (val < 0 || val >= russianAlphabetLower.Length)
                    result[i] = '?';
                else
                    result[i] = russianAlphabetLower[val];
            }
            return new string(result);
        }

        // Вспомогательная функция (скопируй из Code)
        private static long ModPow(long baseValue, long exp, long mod)
        {
            long result = 1;
            baseValue = baseValue % mod;
            while (exp > 0)
            {
                if (exp % 2 == 1)
                    result = (result * baseValue) % mod;
                exp = exp >> 1;
                baseValue = (baseValue * baseValue) % mod;
            }
            return result;
        }

        // === Дешифровка DES (учебная) ===
        public static string DeDES(string str, string key)
        {
            str = str.ToLower();
            key = key.ToLower();

            var result = new char[str.Length];
            int keySum = key.Sum(c => (int)c) % russianAlphabetLower.Length;

            for (int i = 0; i < str.Length; i++)
            {
                char c = str[i];
                int idx = Array.IndexOf(russianAlphabetLower, c);
                if (idx == -1)
                {
                    result[i] = c;
                    continue;
                }

                int shift = (keySum + i) % russianAlphabetLower.Length;
                int decryptedIdx = (idx - shift + russianAlphabetLower.Length) % russianAlphabetLower.Length;
                result[i] = russianAlphabetLower[decryptedIdx];
            }
            return new string(result);
        }
    }
}
