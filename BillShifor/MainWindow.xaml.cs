using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using Microsoft.Win32;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using BillShifor.WpCalculator;
using BillShifor.ViewModels;
using BillShifor.Models;

namespace BillShifor
{
    public partial class MainWindow : Window
    {
        private string currentCipher = "Не выбрана";
        private int rsaE = 0;
        private int rsaN = 0;
        private int rsaD = 0;
        private string importedText = "";

        public MainWindow()
        {
            InitializeComponent();
            InitializeCycleAnalysis();
            InitializeLogicalAnalysis();
            InitializeCaesarVerification();
        }

        #region Методы выбора шифров

        private void CaesarMenu_Click(object sender, RoutedEventArgs e)
        {
            currentCipher = "Цезарь";
            UpdateCipherDisplay();
        }

        private void VigenereMenu_Click(object sender, RoutedEventArgs e)
        {
            currentCipher = "Виженер";
            UpdateCipherDisplay();
        }

        private void AtbashMenu_Click(object sender, RoutedEventArgs e)
        {
            currentCipher = "Атбаш";
            UpdateCipherDisplay();
        }

        private void PlayfairMenu_Click(object sender, RoutedEventArgs e)
        {
            currentCipher = "Плейфер";
            UpdateCipherDisplay();
        }

        private void VernamMenu_Click(object sender, RoutedEventArgs e)
        {
            currentCipher = "Вернам";
            UpdateCipherDisplay();
        }

        private void RSAMenu_Click(object sender, RoutedEventArgs e)
        {
            currentCipher = "RSA";
            UpdateCipherDisplay();
        }

        private void DESMenu_Click(object sender, RoutedEventArgs e)
        {
            currentCipher = "DES";
            UpdateCipherDisplay();
        }

        private void UpdateCipherDisplay()
        {
            Label_Cipher.Content = $"Шифровка: {currentCipher}";
            ListBox_Display.Items.Add($"{currentCipher}");
        }

        #endregion

        #region Основные методы шифрования/дешифрования

        private void EncryptButton_Click(object sender, RoutedEventArgs e)
        {
            string inputText = GetInputText();
            string key = TextBox_Key.Text;

            if (string.IsNullOrWhiteSpace(inputText))
            {
                MessageBox.Show("Введите текст для шифрования или импортируйте файл!");
                return;
            }

            try
            {
                string result = "";

                switch (currentCipher)
                {
                    case "Цезарь":
                        if (!int.TryParse(key, out int shift))
                        {
                            MessageBox.Show("Для шифра Цезаря ключ должен быть числом!");
                            return;
                        }
                        result = Ciphers.Code.Caesar(inputText, shift);
                        break;

                    case "Виженер":
                        if (string.IsNullOrWhiteSpace(key))
                        {
                            MessageBox.Show("Для шифра Виженера нужен ключ!");
                            return;
                        }
                        result = Ciphers.Code.Vigenere(inputText, key);
                        break;

                    case "Атбаш":
                        result = Ciphers.Code.Atbash(inputText);
                        break;

                    case "Плейфер":
                        if (string.IsNullOrWhiteSpace(key))
                        {
                            MessageBox.Show("Для шифра Плейфера нужен ключ!");
                            return;
                        }
                        result = Ciphers.Code.Playfair(inputText, key);
                        break;

                    case "Вернам":
                        if (string.IsNullOrWhiteSpace(key))
                        {
                            MessageBox.Show("Для шифра Вернама нужен ключ!");
                            return;
                        }
                        result = Ciphers.Code.Vernam(inputText, key);
                        break;

                    case "RSA":
                        var rsaResult = Ciphers.Code.RSA(inputText);
                        result = rsaResult.encrypted;
                        rsaE = rsaResult.e;
                        rsaN = rsaResult.n;
                        rsaD = ModInverse(rsaE, (61 - 1) * (53 - 1));
                        ListBox_Display.Items.Add($"RSA параметры: e={rsaE}, n={rsaN}, d={rsaD}");
                        break;

                    case "DES":
                        if (string.IsNullOrWhiteSpace(key))
                        {
                            MessageBox.Show("Для DES нужен ключ!");
                            return;
                        }
                        result = Ciphers.Code.DES(inputText, key);
                        break;

                    default:
                        MessageBox.Show("Выберите алгоритм шифрования!");
                        return;
                }

                ListBox_Result.Items.Add($"[{DateTime.Now:HH:mm:ss}] Зашифровано: {result}");
                if (string.IsNullOrEmpty(importedText))
                {
                    TextBox_Input.Text = result;
                }
                else
                {
                    importedText = result; 
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при шифровании: {ex.Message}");
            }
        }

        private void DecryptButton_Click(object sender, RoutedEventArgs e)
        {
            string inputText = GetInputText();
            string key = TextBox_Key.Text;

            if (string.IsNullOrWhiteSpace(inputText))
            {
                MessageBox.Show("Введите текст для расшифровки или импортируйте файл!");
                return;
            }

            try
            {
                string result = "";

                switch (currentCipher)
                {
                    case "Цезарь":
                        if (!int.TryParse(key, out int shift))
                        {
                            MessageBox.Show("Для шифра Цезаря ключ должен быть числом!");
                            return;
                        }
                        result = Ciphers.DeCode.Caesar(inputText, shift);
                        break;

                    case "Виженер":
                        if (string.IsNullOrWhiteSpace(key))
                        {
                            MessageBox.Show("Для шифра Виженера нужен ключ!");
                            return;
                        }
                        result = Ciphers.DeCode.Vigenere(inputText, key);
                        break;

                    case "Атбаш":
                        result = Ciphers.DeCode.Atbash(inputText);
                        break;

                    case "Плейфер":
                        if (string.IsNullOrWhiteSpace(key))
                        {
                            MessageBox.Show("Для шифра Плейфера нужен ключ!");
                            return;
                        }
                        result = Ciphers.DeCode.Playfair(inputText, key);
                        break;

                    case "Вернам":
                        if (string.IsNullOrWhiteSpace(key))
                        {
                            MessageBox.Show("Для шифра Вернама нужен ключ!");
                            return;
                        }
                        result = Ciphers.DeCode.Vernam(inputText, key);
                        break;

                    case "RSA":
                        if (rsaD == 0 || rsaN == 0)
                        {
                            MessageBox.Show("Сначала выполните шифрование RSA!");
                            return;
                        }
                        result = Ciphers.DeCode.RSA(inputText, rsaD, rsaN);
                        break;

                    case "DES":
                        if (string.IsNullOrWhiteSpace(key))
                        {
                            MessageBox.Show("Для DES нужен ключ!");
                            return;
                        }
                        result = Ciphers.DeCode.DES(inputText, key);
                        break;

                    default:
                        MessageBox.Show("Выберите алгоритм шифрования!");
                        return;
                }

                ListBox_Result.Items.Add($"[{DateTime.Now:HH:mm:ss}] Расшифровано: {result}");
                if (string.IsNullOrEmpty(importedText))
                {
                    TextBox_Input.Text = result;
                }
                else
                {
                    importedText = result; 
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при расшифровке: {ex.Message}");
            }
        }

        private string GetInputText()
        {
            if (!string.IsNullOrEmpty(importedText))
            {
                return importedText;
            }
            return TextBox_Input.Text;
        }

        private int ModInverse(int a, int m)
        {
            for (int x = 1; x < m; x++)
                if ((a * x) % m == 1)
                    return x;
            return 1;
        }

        #endregion

        #region Крипто-Анализатор методы

        private void AnalyzeSecurity_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var algorithmItem = CryptoAlgorithmCombo.SelectedItem as ComboBoxItem;
                string algorithm = algorithmItem?.Tag?.ToString() ?? "цезарь";
                string code = CryptoCodeInput.Text;
                string postcondition = SecurityPostCondition.Text;

                if (string.IsNullOrWhiteSpace(code))
                {
                    MessageBox.Show("Введите код криптографического алгоритма!");
                    return;
                }

                var result = CryptoWpEngine.AnalyzeCryptoAlgorithm(algorithm, code, postcondition);

                UpdateSecurityUI(result);

                MessageBox.Show($"Анализ безопасности завершен!\nСтатус: {(result.IsSecure ? "БЕЗОПАСНО" : "ТРЕБУЕТСЯ АУДИТ")}",
                              "Анализ завершен", MessageBoxButton.OK,
                              result.IsSecure ? MessageBoxImage.Information : MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при анализе безопасности: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateSecurityUI(CryptoWpResult result)
        {
            SecurityStatusText.Text = result.SecurityAssessment;
            SecurityStatusBorder.Background = result.IsSecure ?
                new SolidColorBrush(Color.FromRgb(45, 106, 79)) :
                new SolidColorBrush(Color.FromRgb(179, 57, 57)); 

            CryptoStepsList.ItemsSource = result.Steps;

            FinalSecurityPrecondition.Text = result.FinalPrecondition;
            CryptoHoareTripleText.Text = result.HoareTriple;

            SecurityRecommendations.Text = GenerateRecommendations(result);

            SecurityDetailsGroup.Visibility = Visibility.Visible;
            RecommendationsGroup.Visibility = Visibility.Visible;
        }

        private void ShowCryptoHoareTriple_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(CryptoHoareTripleText.Text))
            {
                MessageBox.Show("Сначала выполните анализ безопасности!");
                return;
            }

            MessageBox.Show(CryptoHoareTripleText.Text, "Триада Хоара для крипто-алгоритма",
                          MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void CryptoExample1_Click(object sender, RoutedEventArgs e)
        {
            CryptoAlgorithmCombo.SelectedIndex = 0; // Цезарь
            CryptoCodeInput.Text = @"function encrypt(text, key) {
    shift := key % 32;
    result := """";  
    foreach char in text {
        if (char in alphabet) {
            idx := alphabet.indexOf(char);
            new_idx := (idx + shift) % alphabet.length;
            result := result + alphabet[new_idx];
        } else {
            result := result + char;
        }
    }
    return result;
}";

            SecurityPostCondition.Text = "decrypt(encrypt(text, key), key) == text";
        }

        private void CryptoExample2_Click(object sender, RoutedEventArgs e)
        {
            CryptoAlgorithmCombo.SelectedIndex = 2; // RSA
            CryptoCodeInput.Text = @"function rsa_encrypt(message, e, n) {
    encrypted := [];
    foreach char in message {
        m := char_to_int(char);
        c := mod_pow(m, e, n);
        encrypted.append(c);
    }
    return encrypted;
}

function mod_pow(base, exponent, modulus) {
    result := 1;
    base := base % modulus;
    while (exponent > 0) {
        if (exponent % 2 == 1) {
            result := (result * base) % modulus;
        }
        exponent := exponent / 2;
        base := (base * base) % modulus;
    }
    return result;
}";

            SecurityPostCondition.Text = "∀m: decrypt(encrypt(m, e, n), d, n) == m";
        }

        private void CryptoExample3_Click(object sender, RoutedEventArgs e)
        {
            CryptoAlgorithmCombo.SelectedIndex = 1; // Виженер
            CryptoCodeInput.Text = @"function vigenere_encrypt(text, key) {
    result := """";
    key_index := 0;
    foreach char in text {
        if (char in alphabet) {
            shift := alphabet.indexOf(key[key_index]);
            idx := alphabet.indexOf(char);
            new_idx := (idx + shift) % alphabet.length;
            result := result + alphabet[new_idx];
            key_index := (key_index + 1) % key.length;
        } else {
            result := result + char;
        }
    }
    return result;
}";

            SecurityPostCondition.Text = "IsReversible(encrypt, decrypt) ∧ KeyDependent(output)";
        }

        private string GenerateRecommendations(CryptoWpResult result)
        {
            var recommendations = new List<string>();

            if (result.IsSecure)
            {
                recommendations.Add("✅ Алгоритм прошел базовую проверку безопасности");
                recommendations.Add("✅ Все предусловия удовлетворяют криптографическим требованиям");
                recommendations.Add("✅ Рекомендуется провести дополнительное тестирование на стойкость");
            }
            else
            {
                recommendations.Add("⚠️ Обнаружены потенциальные уязвимости");
                recommendations.Add("⚠️ Проверьте граничные условия и обработку ошибок");
                recommendations.Add("⚠️ Рекомендуется криптографический аудит");
                recommendations.Add("⚠️ Убедитесь в отсутствии временных атак");
            }

            recommendations.Add("💡 Всегда используйте проверенные криптографические библиотеки");
            recommendations.Add("💡 Регулярно обновляйте алгоритмы и ключи");

            return string.Join("\n• ", recommendations);
        }


        private void AnalyzeCurrentCipher_Click(object sender, RoutedEventArgs e)
        {
            string currentAlgorithm = currentCipher;
            string pseudoCode = GeneratePseudoCodeForAlgorithm(currentAlgorithm);

            CryptoAlgorithmCombo.SelectedItem = CryptoAlgorithmCombo.Items
                .Cast<ComboBoxItem>()
                .FirstOrDefault(item => item.Tag?.ToString()?.ToLower() == currentAlgorithm.ToLower());

            CryptoCodeInput.Text = pseudoCode;

            var tabControl = (TabControl)this.Content;
            tabControl.SelectedIndex = 2; 

            MessageBox.Show($"Загружена псевдокодовая реализация алгоритма {currentAlgorithm} для анализа",
                          "Анализ запущен", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private string GeneratePseudoCodeForAlgorithm(string algorithm)
        {
            switch (algorithm.ToLower())
            {
                case "цезарь":
                    return @"function caesar_encrypt(text, shift) {
    result := """";
    foreach char in text {
        if (char in alphabet) {
            idx := alphabet.indexOf(char);
            new_idx := (idx + shift) % alphabet.length;
            result := result + alphabet[new_idx];
        } else {
            result := result + char;
        }
    }
    return result;
}";
                default:
                    return "// Псевдокод для анализа безопасности алгоритма";
            }
        }
        #endregion

        #region Существующие методы

        private void MenuItem_Click(object sender, RoutedEventArgs e) { }
        private void Button_Click(object sender, RoutedEventArgs e) { this.Close(); }
        private void MenuItem_Click_1(object sender, RoutedEventArgs e) { }
        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e) { }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
            saveFileDialog.Title = "Сохранить результаты";

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    StringBuilder content = new StringBuilder();
                    content.AppendLine("=== РЕЗУЛЬТАТЫ ШИФРОВАНИЯ ===");
                    content.AppendLine($"Дата: {DateTime.Now}");
                    content.AppendLine($"Алгоритм: {currentCipher}");
                    content.AppendLine();
                    content.AppendLine("История операций:");

                    foreach (var item in ListBox_Result.Items)
                    {
                        content.AppendLine(item.ToString());
                    }

                    File.WriteAllText(saveFileDialog.FileName, content.ToString(), Encoding.UTF8);
                    MessageBox.Show("Результаты успешно экспортированы!", "Экспорт завершен",
                                  MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при сохранении файла:\n{ex.Message}",
                                  "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void MenuItem_Import(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
            openFileDialog.Title = "Выберите текстовый файл для импорта";

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    string fileContent = File.ReadAllText(openFileDialog.FileName, Encoding.UTF8);

                    importedText = fileContent;

                    ListBox_Display.Items.Clear();
                    string[] lines = fileContent.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

                    foreach (string line in lines)
                    {
                        if (!string.IsNullOrWhiteSpace(line))
                        {
                            ListBox_Display.Items.Add(line);
                        }
                    }

                    if (lines.Length > 0 && !string.IsNullOrWhiteSpace(lines[0]))
                    {
                        TextBox_Input.Text = lines[0] + (lines.Length > 1 ? "..." : "");
                    }

                    MessageBox.Show($"Файл успешно импортирован!\nДобавлено строк: {ListBox_Display.Items.Count}\nТеперь можно шифровать содержимое файла.",
                                  "Импорт завершен", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при чтении файла:\n{ex.Message}",
                                  "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ClearFields_Click(object sender, RoutedEventArgs e)
        {
            TextBox_Input.Text = "";
            TextBox_Key.Text = "";
            ListBox_Display.Items.Clear();
            ListBox_Result.Items.Clear();
            importedText = "";
            rsaE = rsaN = rsaD = 0;
            MessageBox.Show("Все поля очищены. Импортированный текст удален.", "Очистка завершена");
        }

        private void Help_Button(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("📋 ОСНОВНОЕ МЕНЮ:\r\n• Шифровки - выбор алгоритма (Цезарь, Виженер, RSA и др.)\r\n• Действия - экспорт, импорт, очистка полей\r\n• Справка - это окно\r\n\r\n🎯 КАК РАБОТАТЬ:\r\n1. Выберите алгоритм в меню 'Шифровки'\r\n2. Введите текст ИЛИ импортируйте файл\r\n3. Введите ключ в поле 'Ключ'\r\n4. Нажмите 'Шифровать' или 'Расшифровать'\r\n5. Результат появится в истории операций\r\n\r\n🔑 КЛЮЧИ:\r\n• Цезарь: число (сдвиг)\r\n• Виженер: слово\r\n• Атбаш: не нужен\r\n• RSA: автоматическая генерация\r\n\r\n💡 СОВЕТЫ:\r\n• Сохраняйте ключи!\r\n• Используйте экспорт для результатов\r\n• Очищайте поля перед новой операцией");
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            Window imageWindow = new Window
            {
                Title = "Изображение",
                Width = 600,
                Height = 500,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this
            };

            Image image = new Image();
            try
            {
                image.Source = new BitmapImage(new Uri("Images\\photo_2025-09-28_23-05-28.png"));
                image.Stretch = Stretch.Uniform;

                ScrollViewer scrollViewer = new ScrollViewer
                {
                    Content = image,
                    HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                    VerticalScrollBarVisibility = ScrollBarVisibility.Auto
                };

                imageWindow.Content = scrollViewer;
                imageWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки изображения: {ex.Message}");
            }
        }

        #endregion

        #region Анализатор Циклов

        private CycleAnalysisViewModel cycleVM = new CycleAnalysisViewModel();
        private System.Windows.Threading.DispatcherTimer executionTimer;

        private void InitializeCycleAnalysis()
        {
            // Установка DataContext для анализатора циклов
            CycleAnalysisTab.DataContext = cycleVM;

            // Инициализация таймера для автоматического выполнения
            executionTimer = new System.Windows.Threading.DispatcherTimer();
            executionTimer.Interval = TimeSpan.FromMilliseconds(1000);
            executionTimer.Tick += ExecutionTimer_Tick;
        }

        // Обработчики кнопок анализатора циклов
        private void StepButton_Click(object sender, RoutedEventArgs e)
        {
            cycleVM.ExecuteStep();
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            executionTimer.Start();
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            executionTimer.Stop();
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            executionTimer.Stop();
            cycleVM.ResetAnalysis();
        }

        private void GenerateArrayButton_Click(object sender, RoutedEventArgs e)
        {
            string[] sampleTexts = { "привет", "шифр", "текст", "анализ" };
            Random rnd = new Random();
            cycleVM.CurrentText = sampleTexts[rnd.Next(sampleTexts.Length)];
        }

        private void EditArrayButton_Click(object sender, RoutedEventArgs e)
        {
            string newText = Microsoft.VisualBasic.Interaction.InputBox("Введите новый текст:", "Редактирование текста", cycleVM.CurrentText);
            if (!string.IsNullOrEmpty(newText))
            {
                cycleVM.CurrentText = newText;
            }
        }

        private void AlgorithmComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cycleVM != null && AlgorithmComboBox.SelectedIndex >= 0)
            {
                cycleVM.CurrentMode = (AnalysisMode)AlgorithmComboBox.SelectedIndex;
            }
        }

        private void ExecutionTimer_Tick(object sender, EventArgs e)
        {
            if (cycleVM.CurrentIndex < cycleVM.CharArray.Count)
            {
                cycleVM.ExecuteStep();
            }
            else
            {
                executionTimer.Stop();
            }
        }

        #endregion

        #region Логический Анализ

        private LogicalAnalysisViewModel logicalVM = new LogicalAnalysisViewModel();

        private void InitializeLogicalAnalysis()
        {
            LogicalAnalysisTab.DataContext = logicalVM;
        }

        private void GenerateTruthTable_Click(object sender, RoutedEventArgs e)
        {
            logicalVM.GenerateTruthTable();
        }

        private void AnalyzeFormula_Click(object sender, RoutedEventArgs e)
        {
            logicalVM.AnalyzeFormula();
        }

        private void CompareFormulas_Click(object sender, RoutedEventArgs e)
        {
            logicalVM.CompareFormulas();
        }

        private void ClearLogicalAnalysis_Click(object sender, RoutedEventArgs e)
        {
            logicalVM.ClearAnalysis();
        }

        #endregion

        #region Верификация Цезаря
        private void InitializeCaesarVerification()
        {
            // Инициализация значений по умолчанию с проверкой на null
            if (CaesarEncryptText != null && CaesarEncryptKey != null)
            {
                CheckCaesarEncryptPreconditions();
            }

            if (CaesarDecryptText != null && CaesarDecryptKey != null)
            {
                CheckCaesarDecryptPreconditions();
            }
        }

        private const string RUSSIAN_ALPHABET = "абвгдеёжзийклмнопрстуфхцчшщъыьэюя";

        private void CaesarEncryptKeySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (CaesarEncryptKey != null)
            {
                CaesarEncryptKey.Text = ((int)e.NewValue).ToString();
                CheckCaesarEncryptPreconditions();
            }
        }

        private void CaesarDecryptKeySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (CaesarDecryptKey != null)
            {
                CaesarDecryptKey.Text = ((int)e.NewValue).ToString();
                CheckCaesarDecryptPreconditions();
            }
        }

        private void CaesarEncryptButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (CaesarEncryptText == null || CaesarEncryptKey == null || CaesarResultText == null)
                    return;

                if (!CheckCaesarEncryptPreconditions())
                {
                    CaesarResultText.Text = "❌ Pre-условия не выполнены!";
                    return;
                }

                string text = CaesarEncryptText.Text.ToLower();
                int key = int.Parse(CaesarEncryptKey.Text);

                string encrypted = CaesarEncrypt(text, key);

                if (CaesarDecryptText != null)
                    CaesarDecryptText.Text = encrypted;

                if (CaesarEncryptPostIndicator != null && CaesarEncryptPostStatus != null)
                {
                    CaesarEncryptPostIndicator.Fill = new SolidColorBrush(Colors.Green);
                    CaesarEncryptPostStatus.Text = "Выполнено";
                }

                CaesarResultText.Text = $"✅ Шифрование выполнено!\nИсходный текст: {text}\nЗашифрованный текст: {encrypted}\nКлюч: {key}";

                // Автоматически проверяем pre-условия для дешифрования
                CheckCaesarDecryptPreconditions();
            }
            catch (Exception ex)
            {
                if (CaesarResultText != null)
                    CaesarResultText.Text = $"❌ Ошибка при шифровании: {ex.Message}";
            }
        }

        private void CaesarDecryptButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (CaesarDecryptText == null || CaesarDecryptKey == null || CaesarResultText == null)
                    return;

                if (!CheckCaesarDecryptPreconditions())
                {
                    CaesarResultText.Text = "❌ Pre-условия не выполнены!";
                    return;
                }

                string text = CaesarDecryptText.Text.ToLower();
                int key = int.Parse(CaesarDecryptKey.Text);

                string decrypted = CaesarDecrypt(text, key);

                if (CaesarDecryptPostIndicator != null && CaesarDecryptPostStatus != null)
                {
                    CaesarDecryptPostIndicator.Fill = new SolidColorBrush(Colors.Green);
                    CaesarDecryptPostStatus.Text = "Выполнено";
                }

                CaesarResultText.Text = $"✅ Дешифрование выполнено!\nЗашифрованный текст: {text}\nРасшифрованный текст: {decrypted}\nКлюч: {key}";
            }
            catch (Exception ex)
            {
                if (CaesarResultText != null)
                    CaesarResultText.Text = $"❌ Ошибка при дешифровании: {ex.Message}";
            }
        }

        private bool CheckCaesarEncryptPreconditions()
        {
            try
            {
                if (CaesarEncryptText == null || CaesarEncryptKey == null ||
                    CaesarEncryptPreIndicator == null || CaesarEncryptPreStatus == null)
                    return false;

                // Pre-условия для шифрования
                bool hasText = !string.IsNullOrWhiteSpace(CaesarEncryptText.Text);
                bool validKey = int.TryParse(CaesarEncryptKey.Text, out int key) && key >= 1 && key <= 32;
                bool textInAlphabet = CaesarEncryptText.Text.ToLower().All(c => RUSSIAN_ALPHABET.Contains(c) || char.IsWhiteSpace(c));

                bool preconditionsMet = hasText && validKey && textInAlphabet;

                CaesarEncryptPreIndicator.Fill = preconditionsMet ?
                    new SolidColorBrush(Colors.Green) : new SolidColorBrush(Colors.Red);
                CaesarEncryptPreStatus.Text = preconditionsMet ? "Выполнены" : "Не выполнены";

                return preconditionsMet;
            }
            catch
            {
                if (CaesarEncryptPreIndicator != null && CaesarEncryptPreStatus != null)
                {
                    CaesarEncryptPreIndicator.Fill = new SolidColorBrush(Colors.Red);
                    CaesarEncryptPreStatus.Text = "Ошибка проверки";
                }
                return false;
            }
        }

        private bool CheckCaesarDecryptPreconditions()
        {
            try
            {
                if (CaesarDecryptText == null || CaesarDecryptKey == null ||
                    CaesarDecryptPreIndicator == null || CaesarDecryptPreStatus == null)
                    return false;

                // Pre-условия для дешифрования
                bool hasText = !string.IsNullOrWhiteSpace(CaesarDecryptText.Text);
                bool validKey = int.TryParse(CaesarDecryptKey.Text, out int key) && key >= 1 && key <= 32;
                bool textInAlphabet = CaesarDecryptText.Text.ToLower().All(c => RUSSIAN_ALPHABET.Contains(c) || char.IsWhiteSpace(c));

                bool preconditionsMet = hasText && validKey && textInAlphabet;

                CaesarDecryptPreIndicator.Fill = preconditionsMet ?
                    new SolidColorBrush(Colors.Green) : new SolidColorBrush(Colors.Red);
                CaesarDecryptPreStatus.Text = preconditionsMet ? "Выполнены" : "Не выполнены";

                return preconditionsMet;
            }
            catch
            {
                if (CaesarDecryptPreIndicator != null && CaesarDecryptPreStatus != null)
                {
                    CaesarDecryptPreIndicator.Fill = new SolidColorBrush(Colors.Red);
                    CaesarDecryptPreStatus.Text = "Ошибка проверки";
                }
                return false;
            }
        }

        private void CaesarEncryptContractButton_Click(object sender, RoutedEventArgs e)
        {
            string contract = @"📜 КОНТРАКТ ШИФРОВАНИЯ ЦЕЗАРЯ

Pre-условия:
• Текст не должен быть пустым
• Ключ должен быть целым числом от 1 до 32
• Текст должен содержать только русские буквы и пробелы

Post-условия:
• Длина зашифрованного текста = длине исходного текста
• ∀i: encrypted[i] = alphabet[(alphabet.indexof(text[i]) + key) % 32]
• decrypt(encrypt(text, key), key) = text

Инвариант:
• Сохраняется порядок символов, не входящих в алфавит
• Мощность алфавита = 32";

            MessageBox.Show(contract, "Контракт шифрования", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void CaesarDecryptContractButton_Click(object sender, RoutedEventArgs e)
        {
            string contract = @"📜 КОНТРАКТ ДЕШИФРОВАНИЯ ЦЕЗАРЯ

Pre-условия:
• Текст не должен быть пустым
• Ключ должен быть целым числом от 1 до 32
• Текст должен содержать только русские буквы и пробелы

Post-условия:
• Длина расшифрованного текста = длине зашифрованного текста
• ∀i: decrypted[i] = alphabet[(alphabet.indexof(text[i]) - key + 32) % 32]
• encrypt(decrypt(text, key), key) = text

Инвариант:
• Сохраняется порядок символов, не входящих в алфавит
• Мощность алфавита = 32";

            MessageBox.Show(contract, "Контракт дешифрования", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private string CaesarEncrypt(string text, int key)
        {
            return new string(text.Select(c =>
            {
                if (char.IsWhiteSpace(c)) return c;

                int index = RUSSIAN_ALPHABET.IndexOf(c);
                if (index == -1) return c;

                int newIndex = (index + key) % RUSSIAN_ALPHABET.Length;
                return RUSSIAN_ALPHABET[newIndex];
            }).ToArray());
        }

        private string CaesarDecrypt(string text, int key)
        {
            return new string(text.Select(c =>
            {
                if (char.IsWhiteSpace(c)) return c;

                int index = RUSSIAN_ALPHABET.IndexOf(c);
                if (index == -1) return c;

                int newIndex = (index - key + RUSSIAN_ALPHABET.Length) % RUSSIAN_ALPHABET.Length;
                return RUSSIAN_ALPHABET[newIndex];
            }).ToArray());
        }
        #endregion
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
        private void CaesarEncryptText_TextChanged(object sender, TextChangedEventArgs e)
        {
            CheckCaesarEncryptPreconditions();
        }

        private void CaesarDecryptText_TextChanged(object sender, TextChangedEventArgs e)
        {
            CheckCaesarDecryptPreconditions();
        }

        private void CaesarEncryptKey_TextChanged(object sender, TextChangedEventArgs e)
        {
            CheckCaesarEncryptPreconditions();
        }

        private void CaesarDecryptKey_TextChanged(object sender, TextChangedEventArgs e)
        {
            CheckCaesarDecryptPreconditions();
        }

    }
}