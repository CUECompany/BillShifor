using System;
using System.Collections.Generic;
using System.Linq;

namespace Ciphers
{
    public static class Alphabet
    {
        public static readonly char[] alphabet = {
            'а','б','в','г','д','е','ё','ж','з','и','й',
            'к','л','м','н','о','п','р','с','т','у','ф',
            'х','ц','ч','ш','щ','ъ','ы','ь','э','ю','я'
        };

        public static int Length => alphabet.Length;
        public static int IndexOf(char c) => Array.IndexOf(alphabet, c);
        public static bool Contains(char c) => IndexOf(c) != -1;
    }

    public static class Code
    {
        public static string Caesar(string text, int key)
        {
            text = text.ToLower();
            return new string(text.Select(c =>
            {
                int idx = Alphabet.IndexOf(c);
                return idx == -1 ? c : Alphabet.alphabet[(idx + key + Alphabet.Length) % Alphabet.Length];
            }).ToArray());
        }

        public static string Vigenere(string text, string key)
        {
            text = text.ToLower();
            key = new string((key ?? "").ToLower().Where(Alphabet.Contains).ToArray());
            if (key.Length == 0) return text;

            int kpos = 0;
            return new string(text.Select(c =>
            {
                int idx = Alphabet.IndexOf(c);
                if (idx == -1) return c;
                int shift = Alphabet.IndexOf(key[kpos++ % key.Length]);
                return Alphabet.alphabet[(idx + shift) % Alphabet.Length];
            }).ToArray());
        }

        public static string Atbash(string text)
        {
            text = text.ToLower();
            return new string(text.Select(c =>
            {
                int idx = Alphabet.IndexOf(c);
                return idx == -1 ? c : Alphabet.alphabet[Alphabet.Length - 1 - idx];
            }).ToArray());
        }

        public static string Playfair(string text, string key)
        {
            var matrix = BuildMatrix(key);
            var clean = text.ToLower().Where(Alphabet.Contains).ToList();

            for (int i = 0; i < clean.Count - 1; i += 2)
                if (clean[i] == clean[i + 1])
                    clean.Insert(i + 1, 'ъ');

            if (clean.Count % 2 != 0)
                clean.Add('ъ');

            var result = new List<char>();
            for (int i = 0; i < clean.Count; i += 2)
            {
                (int r1, int c1) = Find(matrix, clean[i]);
                (int r2, int c2) = Find(matrix, clean[i + 1]);

                if (r1 == r2)
                {
                    result.Add(matrix[r1, (c1 + 1) % 6]);
                    result.Add(matrix[r2, (c2 + 1) % 6]);
                }
                else if (c1 == c2)
                {
                    result.Add(matrix[(r1 + 1) % 6, c1]);
                    result.Add(matrix[(r2 + 1) % 6, c2]);
                }
                else
                {
                    result.Add(matrix[r1, c2]);
                    result.Add(matrix[r2, c1]);
                }
            }

            return new string(result.ToArray());
        }

        public static string Vernam(string text, string key)
        {
            text = text.ToLower();
            key = (key ?? "").ToLower();
            if (key.Length == 0) return text;

            int kpos = 0;
            return new string(text.Select(c =>
            {
                int idx = Alphabet.IndexOf(c);
                if (idx == -1) return c;
                int shift = Alphabet.IndexOf(key[kpos++ % key.Length]);
                return Alphabet.alphabet[(idx + shift) % Alphabet.Length];
            }).ToArray());
        }

        public static string DES(string text, string key)
        {
            text = text.ToLower();
            key = (key ?? "").ToLower();

            int keySum = key.Where(Alphabet.Contains).Sum(Alphabet.IndexOf) % Alphabet.Length;
            return new string(text.Select((c, i) =>
            {
                int idx = Alphabet.IndexOf(c);
                if (idx == -1) return c;
                int shift = (keySum + i) % Alphabet.Length;
                return Alphabet.alphabet[(idx + shift) % Alphabet.Length];
            }).ToArray());
        }

        public static (string encrypted, int e, int d, int n) RSA(string text, int p = 61, int q = 53)
        {
            text = text ?? "";
            int n = p * q;
            int phi = (p - 1) * (q - 1);
            int e = 17;
            int d = ModInverse(e, phi);

            var enc = new List<long>();
            foreach (char ch in text)
            {
                int m = (int)ch; 
                long ciph = ModPow(m, e, n);
                enc.Add(ciph);
            }

            return (string.Join(",", enc), e, d, n);
        }

        private static char[,] BuildMatrix(string key)
        {
            var matrixChars = new List<char>();
            var used = new HashSet<char>();

            foreach (var c in (key ?? "").ToLower())
                if (Alphabet.Contains(c) && used.Add(c))
                    matrixChars.Add(c);

            foreach (var c in Alphabet.alphabet)
                if (used.Add(c))
                    matrixChars.Add(c);

            char filler = '1';
            while (matrixChars.Count < 36)
            {
                if (!used.Contains(filler))
                {
                    matrixChars.Add(filler);
                    used.Add(filler);
                }
                filler++;
                if (filler > '9' && matrixChars.Count < 36)
                    filler = 'A';
            }

            var matrix = new char[6, 6];
            for (int i = 0, k = 0; i < 6; i++)
                for (int j = 0; j < 6; j++)
                    matrix[i, j] = matrixChars[k++];

            return matrix;
        }

        private static (int, int) Find(char[,] matrix, char c)
        {
            for (int i = 0; i < 6; i++)
                for (int j = 0; j < 6; j++)
                    if (matrix[i, j] == c)
                        return (i, j);
            return (-1, -1);
        }

        private static long ModPow(long baseVal, long exp, long mod)
        {
            long result = 1;
            baseVal = baseVal % mod;
            if (baseVal < 0) baseVal += mod;
            while (exp > 0)
            {
                if ((exp & 1) == 1)
                    result = (result * baseVal) % mod;
                exp >>= 1;
                baseVal = (baseVal * baseVal) % mod;
            }
            return result;
        }

        private static int ModInverse(int a, int m)
        {
            long m0 = m, x0 = 0, x1 = 1;
            long aa = a;
            if (m == 1) return 0;
            while (aa > 1)
            {
                long q = aa / m;
                long t = m;
                m = (int)(aa % m);
                aa = t;
                t = x0;
                x0 = x1 - q * x0;
                x1 = t;
            }
            if (x1 < 0) x1 += m0;
            return (int)x1;
        }
    }
    
    public static class DeCode
    {
        public static string Caesar(string text, int key) => Code.Caesar(text, -key);

        public static string Vigenere(string text, string key)
        {
            text = text.ToLower();
            key = new string((key ?? "").ToLower().Where(Alphabet.Contains).ToArray());
            if (key.Length == 0) return text;

            int kpos = 0;
            return new string(text.Select(c =>
            {
                int idx = Alphabet.IndexOf(c);
                if (idx == -1) return c;
                int shift = Alphabet.IndexOf(key[kpos++ % key.Length]);
                return Alphabet.alphabet[(idx - shift + Alphabet.Length) % Alphabet.Length];
            }).ToArray());
        }

        public static string Atbash(string text) => Code.Atbash(text);

        public static string Playfair(string text, string key)
        {
            var matrix = BuildMatrix(key);
            var clean = text.ToLower().Where(Alphabet.Contains).ToArray();
            if (clean.Length % 2 != 0)
                throw new ArgumentException("Encrypted text must have even length.");

            var result = new List<char>();
            for (int i = 0; i < clean.Length; i += 2)
            {
                (int r1, int c1) = Find(matrix, clean[i]);
                (int r2, int c2) = Find(matrix, clean[i + 1]);

                if (r1 == r2)
                {
                    result.Add(matrix[r1, (c1 + 5) % 6]);
                    result.Add(matrix[r2, (c2 + 5) % 6]);
                }
                else if (c1 == c2)
                {
                    result.Add(matrix[(r1 + 5) % 6, c1]);
                    result.Add(matrix[(r2 + 5) % 6, c2]);
                }
                else
                {
                    result.Add(matrix[r1, c2]);
                    result.Add(matrix[r2, c1]);
                }
            }

            return new string(result.ToArray()).Replace("ъ", "");
        }

        public static string Vernam(string text, string key)
        {
            text = text.ToLower();
            key = (key ?? "").ToLower();
            if (key.Length == 0) return text;

            int kpos = 0;
            return new string(text.Select(c =>
            {
                int idx = Alphabet.IndexOf(c);
                if (idx == -1) return c;
                int shift = Alphabet.IndexOf(key[kpos++ % key.Length]);
                return Alphabet.alphabet[(idx - shift + Alphabet.Length) % Alphabet.Length];
            }).ToArray());
        }

        public static string DES(string text, string key)
        {
            text = text.ToLower();
            key = (key ?? "").ToLower();
            int keySum = key.Where(Alphabet.Contains).Sum(Alphabet.IndexOf) % Alphabet.Length;

            return new string(text.Select((c, i) =>
            {
                int idx = Alphabet.IndexOf(c);
                if (idx == -1) return c;
                int shift = (keySum + i) % Alphabet.Length;
                return Alphabet.alphabet[(idx - shift + Alphabet.Length) % Alphabet.Length];
            }).ToArray());
        }

        public static string RSA(string encrypted, int d, int n)
        {
            if (string.IsNullOrEmpty(encrypted)) return "";

            var parts = encrypted.Split(',', StringSplitOptions.RemoveEmptyEntries);
            var result = new List<char>();
            foreach (var p in parts)
            {
                if (!long.TryParse(p.Trim(), out long ciph))
                {
                    result.Add('?');
                    continue;
                }
                long m = ModPow(ciph, d, n);
                result.Add((char)m);
            }
            return new string(result.ToArray());
        }

        private static char[,] BuildMatrix(string key)
        {
            var matrixChars = new List<char>();
            var used = new HashSet<char>();

            foreach (var c in (key ?? "").ToLower())
                if (Alphabet.Contains(c) && used.Add(c))
                    matrixChars.Add(c);

            foreach (var c in Alphabet.alphabet)
                if (used.Add(c))
                    matrixChars.Add(c);

            char filler = '1';
            while (matrixChars.Count < 36)
            {
                if (!used.Contains(filler))
                {
                    matrixChars.Add(filler);
                    used.Add(filler);
                }
                filler++;
                if (filler > '9' && matrixChars.Count < 36)
                    filler = 'A';
            }

            var matrix = new char[6, 6];
            for (int i = 0, k = 0; i < 6; i++)
                for (int j = 0; j < 6; j++)
                    matrix[i, j] = matrixChars[k++];

            return matrix;
        }

        private static (int, int) Find(char[,] matrix, char c)
        {
            for (int i = 0; i < 6; i++)
                for (int j = 0; j < 6; j++)
                    if (matrix[i, j] == c)
                        return (i, j);
            return (-1, -1);
        }

        private static long ModPow(long baseVal, long exp, long mod)
        {
            long result = 1;
            baseVal %= mod;
            if (baseVal < 0) baseVal += mod;
            while (exp > 0)
            {
                if ((exp & 1) == 1) result = (result * baseVal) % mod;
                exp >>= 1;
                baseVal = (baseVal * baseVal) % mod;
            }
            return result;
        }
    }
}
