using System.Text;

namespace JaguarCipher
{
    public static class RussianCipher
    {
        // Русский алфавит с Ё
        private const string Alphabet =
            "АБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯ";


        #region 1. ШИФР ЦЕЗАРЯ

        public static string CaesarEncrypt(string text, int shift)
        {
            return Caesar(text, shift);
        }

        public static string CaesarDecrypt(string text, int shift)
        {
            return Caesar(text, -shift);
        }

        private static string Caesar(string text, int shift)
        {
            StringBuilder result = new StringBuilder();

            shift %= Alphabet.Length;

            foreach (char originalChar in text)
            {
                char upperChar = char.ToUpper(originalChar);

                int index = Alphabet.IndexOf(upperChar);

                // Если символ не является русской буквой —
                // оставляем его без изменений
                if (index == -1)
                {
                    result.Append(originalChar);
                    continue;
                }

                int newIndex = (index + shift) % Alphabet.Length;

                if (newIndex < 0)
                    newIndex += Alphabet.Length;

                char newChar = Alphabet[newIndex];

                // Сохраняем регистр
                if (char.IsLower(originalChar))
                    newChar = char.ToLower(newChar);

                result.Append(newChar);
            }

            return result.ToString();
        }

        #endregion

        #region 2. ШИФР ВИЖЕНЕРА

        public static string VigenereEncrypt(string text, string password)
        {
            return Vigenere(text, password, false);
        }

        public static string VigenereDecrypt(string text, string password)
        {
            return Vigenere(text, password, true);
        }

        private static string Vigenere(
            string text,
            string password,
            bool decrypt)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Пароль не может быть пустым.");

            // Оставляем из пароля только русские буквы
            string key = "";

            foreach (char c in password.ToUpper())
            {
                if (Alphabet.Contains(c))
                    key += c;
            }

            if (key.Length == 0)
                throw new ArgumentException(
                    "Пароль должен содержать русские буквы.");

            StringBuilder result = new StringBuilder();

            int keyPosition = 0;

            foreach (char originalChar in text)
            {
                char upperChar = char.ToUpper(originalChar);

                int textIndex = Alphabet.IndexOf(upperChar);

                // Пробелы, цифры и знаки препинания
                // не шифруем и не двигаем ключ
                if (textIndex == -1)
                {
                    result.Append(originalChar);
                    continue;
                }

                char keyChar = key[keyPosition % key.Length];

                int keyIndex = Alphabet.IndexOf(keyChar);

                int newIndex;

                if (decrypt)
                    newIndex = textIndex - keyIndex;
                else
                    newIndex = textIndex + keyIndex;

                newIndex %= Alphabet.Length;

                if (newIndex < 0)
                    newIndex += Alphabet.Length;

                char newChar = Alphabet[newIndex];

                if (char.IsLower(originalChar))
                    newChar = char.ToLower(newChar);

                result.Append(newChar);

                keyPosition++;
            }

            return result.ToString();
        }

        #endregion

        #region 3. ШИФР АТБАШ

        public static string AtbashEncrypt(string text)
        {
            return Atbash(text);
        }

        public static string AtbashDecrypt(string text)
        {
            // Атбаш является симметричным:
            // повторное применение возвращает исходный текст
            return Atbash(text);
        }

        private static string Atbash(string text)
        {
            StringBuilder result = new StringBuilder();

            foreach (char originalChar in text)
            {
                char upperChar = char.ToUpper(originalChar);

                int index = Alphabet.IndexOf(upperChar);

                if (index == -1)
                {
                    result.Append(originalChar);
                    continue;
                }

                // Первый символ становится последним,
                // второй — предпоследним и т.д.
                int newIndex = Alphabet.Length - 1 - index;

                char newChar = Alphabet[newIndex];

                if (char.IsLower(originalChar))
                    newChar = char.ToLower(newChar);

                result.Append(newChar);
            }

            return result.ToString();
        }

        #endregion

        #region 4. ШИФР БОФОРА

        public static string BeaufortEncrypt(
            string text,
            string password)
        {
            return Beaufort(text, password);
        }

        public static string BeaufortDecrypt(
            string text,
            string password)
        {
            return Beaufort(text, password);
        }

        private static string Beaufort(
            string text,
            string password)
        {
            string key = GetRussianKey(password);

            StringBuilder result = new StringBuilder();

            int keyPosition = 0;

            foreach (char originalChar in text)
            {
                char upperChar = char.ToUpper(originalChar);

                int textIndex = Alphabet.IndexOf(upperChar);

                if (textIndex == -1)
                {
                    result.Append(originalChar);
                    continue;
                }

                int keyIndex =
                    Alphabet.IndexOf(
                        key[keyPosition % key.Length]);

                int newIndex =
                    Mod(keyIndex - textIndex, Alphabet.Length);

                char newChar = Alphabet[newIndex];

                if (char.IsLower(originalChar))
                    newChar = char.ToLower(newChar);

                result.Append(newChar);

                keyPosition++;
            }

            return result.ToString();
        }

        #endregion

        #region 5. ШИФР ТРИТЕМИЯ

        public static string TrithemiusEncrypt(
            string text,
            int startShift)
        {
            return Trithemius(text, startShift, false);
        }

        public static string TrithemiusDecrypt(
            string text,
            int startShift)
        {
            return Trithemius(text, startShift, true);
        }

        private static string Trithemius(
            string text,
            int startShift,
            bool decrypt)
        {
            StringBuilder result = new StringBuilder();

            int position = 0;

            foreach (char originalChar in text)
            {
                char upperChar = char.ToUpper(originalChar);

                int index = Alphabet.IndexOf(upperChar);

                if (index == -1)
                {
                    result.Append(originalChar);
                    continue;
                }

                int shift = startShift + position;

                if (decrypt)
                    shift = -shift;

                int newIndex = Mod(
                    index + shift,
                    Alphabet.Length);

                char newChar = Alphabet[newIndex];

                if (char.IsLower(originalChar))
                    newChar = char.ToLower(newChar);

                result.Append(newChar);

                position++;
            }

            return result.ToString();
        }

        #endregion

        #region 6. ШИФР ВЕРНАМА

        public static string VernamEncrypt(
    string text,
    string key)
        {
            return VernamProcess(
                text,
                key,
                true);
        }

        public static string VernamDecrypt(
            string text,
            string key)
        {
            return VernamProcess(
                text,
                key,
                false);
        }

        private static string VernamProcess(
            string text,
            string key,
            bool encrypt)
        {
            string preparedKey =
                new string(
                    key
                        .ToUpperInvariant()
                        .Where(c =>
                            Alphabet.Contains(c))
                        .ToArray());

            if (preparedKey.Length == 0)
            {
                throw new Exception(
                    "Для Вернама нужен ключ " +
                    "из русских букв.");
            }

            StringBuilder result =
                new StringBuilder();

            int keyPosition = 0;

            foreach (char original in text)
            {
                char upper =
                    char.ToUpperInvariant(original);

                int textIndex =
                    Alphabet.IndexOf(upper);

                if (textIndex < 0)
                {
                    result.Append(original);
                    continue;
                }

                int keyIndex =
                    Alphabet.IndexOf(
                        preparedKey[
                            keyPosition %
                            preparedKey.Length]);

                int newIndex;

                if (encrypt)
                {
                    newIndex =
                        (textIndex + keyIndex) % 33;
                }
                else
                {
                    newIndex =
                        (textIndex - keyIndex + 33) % 33;
                }

                char resultChar =
                    Alphabet[newIndex];

                if (char.IsLower(original))
                {
                    resultChar =
                        char.ToLowerInvariant(resultChar);
                }

                result.Append(resultChar);

                keyPosition++;
            }

            return result.ToString();
        }

        #endregion

        private static string GetRussianKey(string password)
        {
            StringBuilder key = new StringBuilder();

            foreach (char c in password.ToUpper())
            {
                if (Alphabet.Contains(c))
                    key.Append(c);
            }

            if (key.Length == 0)
                throw new ArgumentException(
                    "Ключ должен содержать русские буквы.");

            return key.ToString();
        }

        private static int Mod(int value, int modulo)
        {
            return (value % modulo + modulo) % modulo;
        }

        private static int Gcd(int a, int b)
        {
            while (b != 0)
            {
                int temp = b;
                b = a % b;
                a = temp;
            }

            return Math.Abs(a);
        }

        private static int ModularInverse(
            int value,
            int modulo)
        {
            value = Mod(value, modulo);

            for (int i = 1; i < modulo; i++)
            {
                if ((value * i) % modulo == 1)
                    return i;
            }

            throw new ArgumentException(
                "Обратного элемента не существует.");
        }
    }
}
