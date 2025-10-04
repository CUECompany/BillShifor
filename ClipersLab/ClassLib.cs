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
    }
}
