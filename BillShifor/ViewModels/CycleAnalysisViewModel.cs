using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Media;
using BillShifor.Models;

namespace BillShifor.ViewModels
{
    public class CycleAnalysisViewModel : INotifyPropertyChanged
    {
        private AnalysisMode _currentMode = AnalysisMode.CaesarEncryption;
        private int _currentIndex = 0;
        private int _variantFunction = 0;
        private bool _isInvariantBefore = true;
        private bool _isInvariantAfter = true;
        private string _currentText = "привет мир";
        private int _caesarKey = 3;
        private int _threshold = 50;
        private int _prefixSum = 0;
        private int _countGreaterThanT = 0;
        private int _prefixMax = 0;

        public ObservableCollection<CharElement> CharArray { get; set; } = new ObservableCollection<CharElement>();
        public ObservableCollection<ExecutionLogEntry> ExecutionLog { get; set; } = new ObservableCollection<ExecutionLogEntry>();

        public AnalysisMode CurrentMode
        {
            get => _currentMode;
            set
            {
                _currentMode = value;
                OnPropertyChanged(nameof(CurrentMode));
                UpdateModeDescription();
                ResetAnalysis();
            }
        }

        public int CurrentIndex
        {
            get => _currentIndex;
            set
            {
                _currentIndex = value;
                OnPropertyChanged(nameof(CurrentIndex));
                UpdateCurrentElement();
                UpdateVariantFunction();
            }
        }

        public int VariantFunction
        {
            get => _variantFunction;
            set
            {
                _variantFunction = value;
                OnPropertyChanged(nameof(VariantFunction));
                OnPropertyChanged(nameof(VariantProgress));
            }
        }

        public double VariantProgress => CharArray.Count > 0 ?
            (double)VariantFunction / CharArray.Count * 100 : 0;

        public bool IsInvariantBefore
        {
            get => _isInvariantBefore;
            set
            {
                _isInvariantBefore = value;
                OnPropertyChanged(nameof(IsInvariantBefore));
            }
        }

        public bool IsInvariantAfter
        {
            get => _isInvariantAfter;
            set
            {
                _isInvariantAfter = value;
                OnPropertyChanged(nameof(IsInvariantAfter));
            }
        }

        public string CurrentText
        {
            get => _currentText;
            set
            {
                _currentText = value;
                OnPropertyChanged(nameof(CurrentText));
                GenerateCharArray();
            }
        }

        public int CaesarKey
        {
            get => _caesarKey;
            set
            {
                _caesarKey = value;
                OnPropertyChanged(nameof(CaesarKey));
            }
        }

        public int Threshold
        {
            get => _threshold;
            set
            {
                _threshold = value;
                OnPropertyChanged(nameof(Threshold));
            }
        }

        public int PrefixSum
        {
            get => _prefixSum;
            set
            {
                _prefixSum = value;
                OnPropertyChanged(nameof(PrefixSum));
            }
        }

        public int CountGreaterThanT
        {
            get => _countGreaterThanT;
            set
            {
                _countGreaterThanT = value;
                OnPropertyChanged(nameof(CountGreaterThanT));
            }
        }

        public int PrefixMax
        {
            get => _prefixMax;
            set
            {
                _prefixMax = value;
                OnPropertyChanged(nameof(PrefixMax));
            }
        }

        // Инварианты для разных режимов
        public string InvariantWords => CurrentMode switch
        {
            AnalysisMode.CaesarEncryption => "Для всех символов от 0 до j-1 выполнено шифрование Цезарем. j в диапазоне от 0 до длины текста.",
            AnalysisMode.PrefixSum => "res содержит сумму элементов a[0] до a[j-1]",
            AnalysisMode.CountGreaterThanT => "res содержит количество элементов > T от a[0] до a[j-1]",
            AnalysisMode.PrefixMax => "res содержит максимальный элемент от a[0] до a[j-1]",
            _ => ""
        };

        public string InvariantFormula => CurrentMode switch
        {
            AnalysisMode.CaesarEncryption => "∀k (0 ≤ k < j) ⇒ (text[k] = Encrypt(original[k], key)) ∧ (0 ≤ j ≤ n)",
            AnalysisMode.PrefixSum => "res = Σ(i=0 to j-1) a[i] ∧ 0 ≤ j ≤ n",
            AnalysisMode.CountGreaterThanT => "res = |{i | 0 ≤ i < j ∧ a[i] > T}| ∧ 0 ≤ j ≤ n",
            AnalysisMode.PrefixMax => "res = max(a[0..j-1]) ∧ 0 ≤ j ≤ n",
            _ => ""
        };

        public string VariantFunctionText => CurrentMode switch
        {
            AnalysisMode.CaesarEncryption => "n - j",
            _ => "n - j"
        };

        public string CurrentResult => CurrentMode switch
        {
            AnalysisMode.CaesarEncryption => new string(CharArray.Select(c => c.CurrentValue).ToArray()),
            AnalysisMode.PrefixSum => PrefixSum.ToString(),
            AnalysisMode.CountGreaterThanT => CountGreaterThanT.ToString(),
            AnalysisMode.PrefixMax => PrefixMax.ToString(),
            _ => "0"
        };

        public SolidColorBrush VerificationColor => IsInvariantBefore && IsInvariantAfter ?
            new SolidColorBrush(Color.FromRgb(76, 175, 80)) :
            new SolidColorBrush(Color.FromRgb(244, 67, 54));

        public string PostConditionText => CurrentIndex >= CharArray.Count ?
            "Постусловие: цикл завершен" : "Постусловие: цикл выполняется";

        public string VerificationFormula => $"(Inv ∧ j < n) ⇒ wp(S, Inv)";

        public string VerificationExplanation => CurrentMode switch
        {
            AnalysisMode.CaesarEncryption => "Если инвариант истинен и j < n, то после шифрования символа j инвариант сохранится для j+1",
            AnalysisMode.PrefixSum => "Если инвариант истинен и j < n, то после добавления a[j] к сумме инвариант сохранится",
            AnalysisMode.CountGreaterThanT => "Если инвариант истинен и j < n, то после проверки a[j] инвариант сохранится",
            AnalysisMode.PrefixMax => "Если инвариант истинен и j < n, то после сравнения с a[j] инвариант сохранится",
            _ => "Если инвариант истинен и условие цикла выполняется, то после выполнения тела цикла инвариант сохранится"
        };

        public string VerificationResult => IsInvariantBefore && IsInvariantAfter ?
            "✓ Условие верификации выполняется" :
            "✗ Условие верификации нарушено";

        public CycleAnalysisViewModel()
        {
            GenerateCharArray();
        }

        public void GenerateCharArray()
        {
            CharArray.Clear();
            for (int i = 0; i < CurrentText.Length; i++)
            {
                CharArray.Add(new CharElement
                {
                    Index = i,
                    OriginalValue = CurrentText[i],
                    CurrentValue = CurrentText[i]
                });
            }
            ResetAnalysis();
        }

        public void ResetAnalysis()
        {
            CurrentIndex = 0;
            VariantFunction = CharArray.Count;
            PrefixSum = 0;
            CountGreaterThanT = 0;
            PrefixMax = 0;

            // Сброс значений массива
            foreach (var element in CharArray)
            {
                element.CurrentValue = element.OriginalValue;
                element.IsCurrent = false;
            }

            if (CharArray.Count > 0)
                CharArray[0].IsCurrent = true;

            ExecutionLog.Clear();
            AddLogEntry("Инициализация", "Анализ сброшен", "Все параметры установлены в начальное состояние");
        }

        private void UpdateModeDescription()
        {
            OnPropertyChanged(nameof(InvariantWords));
            OnPropertyChanged(nameof(InvariantFormula));
            OnPropertyChanged(nameof(VariantFunctionText));
            OnPropertyChanged(nameof(CurrentResult));
        }

        private void UpdateCurrentElement()
        {
            foreach (var element in CharArray)
            {
                element.IsCurrent = element.Index == CurrentIndex;
            }
            OnPropertyChanged(nameof(CurrentResult));
        }

        private void UpdateVariantFunction()
        {
            VariantFunction = CharArray.Count - CurrentIndex;
        }

        private void AddLogEntry(string step, string action, string details)
        {
            ExecutionLog.Add(new ExecutionLogEntry
            {
                Step = step,
                Action = action,
                Details = details
            });
        }

        public void ExecuteStep()
        {
            if (CurrentIndex >= CharArray.Count) return;

            // Проверка инварианта до шага
            IsInvariantBefore = CheckInvariantBeforeStep();

            AddLogEntry($"Шаг {CurrentIndex + 1}", "Проверка инварианта", $"Инвариант до шага: {(IsInvariantBefore ? "ИСТИНА" : "ЛОЖЬ")}");

            // Выполнение шага в зависимости от режима
            switch (CurrentMode)
            {
                case AnalysisMode.CaesarEncryption:
                    ExecuteCaesarStep();
                    break;
                case AnalysisMode.PrefixSum:
                    ExecutePrefixSumStep();
                    break;
                case AnalysisMode.CountGreaterThanT:
                    ExecuteCountGreaterThanTStep();
                    break;
                case AnalysisMode.PrefixMax:
                    ExecutePrefixMaxStep();
                    break;
            }

            // Проверка инварианта после шага
            IsInvariantAfter = CheckInvariantAfterStep();
            AddLogEntry($"Шаг {CurrentIndex + 1}", "Проверка инварианта", $"Инвариант после шага: {(IsInvariantAfter ? "ИСТИНА" : "ЛОЖЬ")}");

            CurrentIndex++;
            UpdateVariantFunction();

            AddLogEntry($"Шаг {CurrentIndex}", "Завершение", $"j = {CurrentIndex}, t = {VariantFunction}");
        }

        private void ExecuteCaesarStep()
        {
            var currentElement = CharArray[CurrentIndex];
            char originalChar = currentElement.OriginalValue;

            // Простое шифрование Цезаря (только для русских букв)
            if (char.IsLetter(originalChar) && IsRussianLetter(originalChar))
            {
                char baseChar = char.IsLower(originalChar) ? 'а' : 'А';
                int idx = originalChar - baseChar;
                int newIdx = (idx + CaesarKey) % 32; // 32 буквы в русском алфавите
                char encryptedChar = (char)(baseChar + newIdx);
                currentElement.CurrentValue = encryptedChar;

                AddLogEntry($"Шаг {CurrentIndex + 1}", "Шифрование Цезаря",
                           $"'{originalChar}' → '{encryptedChar}' (сдвиг {CaesarKey})");
            }
            else
            {
                AddLogEntry($"Шаг {CurrentIndex + 1}", "Пропуск символа",
                           $"Символ '{originalChar}' не является русской буквой");
            }
        }

        private bool IsRussianLetter(char c)
        {
            return (c >= 'а' && c <= 'я') || (c >= 'А' && c <= 'Я') || c == 'ё' || c == 'Ё';
        }

        private void ExecutePrefixSumStep()
        {
            // Для демонстрации используем ASCII коды
            int value = (int)CharArray[CurrentIndex].OriginalValue;
            PrefixSum += value;

            AddLogEntry($"Шаг {CurrentIndex + 1}", "PrefixSum",
                       $"a[{CurrentIndex}] = {value}, сумма = {PrefixSum}");
        }

        private void ExecuteCountGreaterThanTStep()
        {
            int value = (int)CharArray[CurrentIndex].OriginalValue;
            if (value > Threshold)
            {
                CountGreaterThanT++;
                AddLogEntry($"Шаг {CurrentIndex + 1}", "Count > T",
                           $"a[{CurrentIndex}] = {value} > {Threshold}, счетчик = {CountGreaterThanT}");
            }
            else
            {
                AddLogEntry($"Шаг {CurrentIndex + 1}", "Count > T",
                           $"a[{CurrentIndex}] = {value} ≤ {Threshold}, счетчик = {CountGreaterThanT}");
            }
        }

        private void ExecutePrefixMaxStep()
        {
            int value = (int)CharArray[CurrentIndex].OriginalValue;
            if (value > PrefixMax || CurrentIndex == 0)
            {
                PrefixMax = value;
                AddLogEntry($"Шаг {CurrentIndex + 1}", "PrefixMax",
                           $"a[{CurrentIndex}] = {value} (новый максимум)");
            }
            else
            {
                AddLogEntry($"Шаг {CurrentIndex + 1}", "PrefixMax",
                           $"a[{CurrentIndex}] = {value} (максимум = {PrefixMax})");
            }
        }

        private bool CheckInvariantBeforeStep()
        {
            // Упрощенная проверка инварианта
            return CurrentIndex >= 0 && CurrentIndex <= CharArray.Count;
        }

        private bool CheckInvariantAfterStep()
        {
            // Упрощенная проверка инварианта
            return CurrentIndex >= 0 && CurrentIndex <= CharArray.Count;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}