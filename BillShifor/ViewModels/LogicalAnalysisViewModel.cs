
using BillShifor.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;

namespace BillShifor.ViewModels
{
    public class LogicalAnalysisViewModel : BaseViewModel
    {
        private string selectedCipher = "Вернам (XOR)";
        private int variableCount = 2;
        private int functionNumber = 6; // 0110 - XOR
        private string formulaText = "A ^ B";
        private string formula1 = "A ^ B";
        private string formula2 = "(A & !B) | (!A & B)";

        public ObservableCollection<string> CipherOperations { get; } = new ObservableCollection<string>
        {
            "Вернам (XOR)",
            "Цезарь (сложение mod 2)",
            "Атбаш (инверсия)",
            "Виженер (сложение)",
            "Плейфер (правила замены)"
        };

        public ObservableCollection<TruthTableRow> TruthTable { get; } = new ObservableCollection<TruthTableRow>();
        public ObservableCollection<string> AnalysisHistory { get; } = new ObservableCollection<string>();

        public string SelectedCipher
        {
            get => selectedCipher;
            set { selectedCipher = value; OnPropertyChanged(); UpdatePreset(); }
        }

        public int VariableCount
        {
            get => variableCount;
            set { variableCount = value; OnPropertyChanged(); }
        }

        public int FunctionNumber
        {
            get => functionNumber;
            set { functionNumber = value; OnPropertyChanged(); }
        }

        public string FormulaText
        {
            get => formulaText;
            set { formulaText = value; OnPropertyChanged(); }
        }

        public string Formula1
        {
            get => formula1;
            set { formula1 = value; OnPropertyChanged(); }
        }

        public string Formula2
        {
            get => formula2;
            set { formula2 = value; OnPropertyChanged(); }
        }

        public string DNFResult { get; set; } = "";
        public string KNFResult { get; set; } = "";
        public string CostResult { get; set; } = "";
        public string ComparisonResult { get; set; } = "";

        private void UpdatePreset()
        {
            switch (SelectedCipher)
            {
                case "Вернам (XOR)":
                    FormulaText = "A ^ B";
                    VariableCount = 2;
                    FunctionNumber = 6; // 0110
                    break;
                case "Цезарь (сложение mod 2)":
                    FormulaText = "(A + B) % 2";
                    VariableCount = 2;
                    FunctionNumber = 6; // 0110 - XOR эквивалент
                    break;
                case "Атбаш (инверсия)":
                    FormulaText = "!A";
                    VariableCount = 1;
                    FunctionNumber = 1; // 01 - NOT
                    break;
                case "Виженер (сложение)":
                    FormulaText = "(A + B) >= 2 ? 1 : 0";
                    VariableCount = 2;
                    FunctionNumber = 8; // 1000 - AND
                    break;
            }
            OnPropertyChanged(nameof(FormulaText));
            OnPropertyChanged(nameof(VariableCount));
            OnPropertyChanged(nameof(FunctionNumber));
        }

        public void GenerateTruthTable()
        {
            TruthTable.Clear();
            AnalysisHistory.Add($"[{DateTime.Now:HH:mm:ss}] Генерация таблицы для {SelectedCipher}");

            try
            {
                int rowCount = (int)Math.Pow(2, VariableCount);

                for (int i = 0; i < rowCount; i++)
                {
                    var row = new TruthTableRow();

                    // Заполняем входы
                    for (int j = VariableCount - 1; j >= 0; j--)
                    {
                        row.Inputs.Add(((i >> j) & 1) == 1);
                    }

                    // Вычисляем выход на основе номера функции
                    bool output = ((FunctionNumber >> i) & 1) == 1;
                    row.Output = output;

                    TruthTable.Add(row);
                }

                AnalysisHistory.Add($"✓ Таблица построена: {rowCount} строк");
            }
            catch (Exception ex)
            {
                AnalysisHistory.Add($"✗ Ошибка: {ex.Message}");
            }
        }

        public void AnalyzeFormula()
        {
            AnalysisHistory.Add($"[{DateTime.Now:HH:mm:ss}] Анализ формулы: {FormulaText}");

            try
            {
                // Определяем переменные в формуле
                var variables = ExtractVariables(FormulaText);
                VariableCount = variables.Count;
                
                // Строим таблицу истинности для формулы
                TruthTable.Clear();
                int rowCount = (int)Math.Pow(2, VariableCount);
                
                for (int i = 0; i < rowCount; i++)
                {
                    var row = new TruthTableRow();
                    var values = new Dictionary<char, bool>();
                    
                    // Заполняем входы
                    for (int j = 0; j < VariableCount; j++)
                    {
                        bool value = ((i >> (VariableCount - 1 - j)) & 1) == 1;
                        row.Inputs.Add(value);
                        values[variables[j]] = value;
                    }
                    
                    // Вычисляем выход формулы
                    row.Output = EvaluateFormula(FormulaText, values);
                    TruthTable.Add(row);
                }
                
                // Генерация DNF и KNF на основе таблицы истинности
                var dnfTerms = new List<string>();
                var knfTerms = new List<string>();

                foreach (var row in TruthTable)
                {
                    if (row.Output) // Для DNF (где f=1)
                    {
                        var term = new List<string>();
                        for (int i = 0; i < row.Inputs.Count; i++)
                        {
                            char varName = variables[i];
                            term.Add(row.Inputs[i] ? varName.ToString() : $"!{varName}");
                        }
                        dnfTerms.Add($"({string.Join(" & ", term)})");
                    }
                    else // Для KNF (где f=0)
                    {
                        var term = new List<string>();
                        for (int i = 0; i < row.Inputs.Count; i++)
                        {
                            char varName = variables[i];
                            term.Add(!row.Inputs[i] ? varName.ToString() : $"!{varName}");
                        }
                        knfTerms.Add($"({string.Join(" | ", term)})");
                    }
                }

                DNFResult = dnfTerms.Count > 0 ? string.Join(" | ", dnfTerms) : "0";
                KNFResult = knfTerms.Count > 0 ? string.Join(" & ", knfTerms) : "1";

                // Расчет стоимости
                int literalCost = DNFResult.Count(c => char.IsLetter(c));
                int conjunctCost = DNFResult.Count(c => c == '&');
                int disjunctCost = DNFResult.Count(c => c == '|');

                CostResult = $"Литералы: {literalCost}, Конъюнкции: {conjunctCost}, Дизъюнкции: {disjunctCost}";

                AnalysisHistory.Add($"✓ Таблица истинности построена: {rowCount} строк");
                AnalysisHistory.Add($"✓ СДНФ: {DNFResult}");
                AnalysisHistory.Add($"✓ СКНФ: {KNFResult}");
                AnalysisHistory.Add($"✓ Стоимость: {CostResult}");

                OnPropertyChanged(nameof(DNFResult));
                OnPropertyChanged(nameof(KNFResult));
                OnPropertyChanged(nameof(CostResult));
            }
            catch (Exception ex)
            {
                AnalysisHistory.Add($"✗ Ошибка анализа: {ex.Message}");
                MessageBox.Show($"Ошибка при анализе формулы: {ex.Message}\n\nПримеры корректных формул:\n- A ^ B\n- (A & !B) | (!A & B)\n- A | B\n- !A & B", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void CompareFormulas()
        {
            AnalysisHistory.Add($"[{DateTime.Now:HH:mm:ss}] Сравнение: '{Formula1}' и '{Formula2}'");

            try
            {
                // Извлекаем все переменные из обеих формул
                var vars1 = ExtractVariables(Formula1);
                var vars2 = ExtractVariables(Formula2);
                var allVars = vars1.Union(vars2).OrderBy(c => c).ToList();
                
                if (allVars.Count == 0)
                {
                    ComparisonResult = "❌ Ошибка: формулы не содержат переменных";
                    AnalysisHistory.Add("✗ Формулы не содержат переменных");
                    OnPropertyChanged(nameof(ComparisonResult));
                    return;
                }
                
                // Строим таблицы истинности для обеих формул
                int rowCount = (int)Math.Pow(2, allVars.Count);
                bool equivalent = true;
                string counterExample = "";
                
                for (int i = 0; i < rowCount; i++)
                {
                    var values = new Dictionary<char, bool>();
                    
                    // Заполняем значения переменных
                    for (int j = 0; j < allVars.Count; j++)
                    {
                        bool value = ((i >> (allVars.Count - 1 - j)) & 1) == 1;
                        values[allVars[j]] = value;
                    }
                    
                    // Вычисляем обе формулы
                    bool result1 = EvaluateFormula(Formula1, values);
                    bool result2 = EvaluateFormula(Formula2, values);
                    
                    // Проверяем эквивалентность
                    if (result1 != result2)
                    {
                        equivalent = false;
                        // Формируем контр-пример
                        var counterParts = allVars.Select(v => $"{v}={(values[v] ? 1 : 0)}");
                        counterExample = string.Join(", ", counterParts) + 
                                       $" → F1={(result1 ? 1 : 0)}, F2={(result2 ? 1 : 0)}";
                        break;
                    }
                }

                if (equivalent)
                {
                    ComparisonResult = "✅ Формулы ЭКВИВАЛЕНТНЫ";
                    AnalysisHistory.Add("✓ Формулы эквивалентны на всех наборах значений");
                }
                else
                {
                    ComparisonResult = $"❌ Формулы НЕ эквивалентны\n\nКонтр-пример:\n{counterExample}";
                    AnalysisHistory.Add("✗ Формулы не эквивалентны");
                    AnalysisHistory.Add($"Контр-пример: {counterExample}");
                }

                OnPropertyChanged(nameof(ComparisonResult));
            }
            catch (Exception ex)
            {
                AnalysisHistory.Add($"✗ Ошибка сравнения: {ex.Message}");
                MessageBox.Show($"Ошибка при сравнении формул: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void ClearAnalysis()
        {
            TruthTable.Clear();
            AnalysisHistory.Clear();
            DNFResult = "";
            KNFResult = "";
            CostResult = "";
            ComparisonResult = "";

            OnPropertyChanged(nameof(DNFResult));
            OnPropertyChanged(nameof(KNFResult));
            OnPropertyChanged(nameof(CostResult));
            OnPropertyChanged(nameof(ComparisonResult));

            AnalysisHistory.Add($"[{DateTime.Now:HH:mm:ss}] Анализ очищен");
        }
        
        // ===== ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ =====
        
        private List<char> ExtractVariables(string formula)
        {
            var variables = new HashSet<char>();
            foreach (char c in formula)
            {
                if (char.IsUpper(c) && c >= 'A' && c <= 'Z')
                {
                    variables.Add(c);
                }
            }
            return variables.OrderBy(c => c).ToList();
        }
        
        private bool EvaluateFormula(string formula, Dictionary<char, bool> values)
        {
            // Простой парсер и вычислитель логических формул
            // Поддерживаемые операции: !, &, |, ^
            
            // Заменяем переменные на их значения
            string expr = formula;
            foreach (var kvp in values)
            {
                expr = expr.Replace(kvp.Key.ToString(), kvp.Value ? "1" : "0");
            }
            
            // Убираем пробелы
            expr = expr.Replace(" ", "");
            
            // Вычисляем выражение
            return EvaluateExpression(expr);
        }
        
        private bool EvaluateExpression(string expr)
        {
            // Рекурсивный парсер с приоритетами операций
            // Приоритет (от высокого к низкому): !, &, ^, |
            
            expr = expr.Trim();
            
            if (expr == "1" || expr == "true") return true;
            if (expr == "0" || expr == "false") return false;
            
            // Обработка скобок
            if (expr.StartsWith("(") && expr.EndsWith(")"))
            {
                // Проверяем, что это действительно парные скобки
                int depth = 0;
                bool validPair = true;
                for (int i = 0; i < expr.Length - 1; i++)
                {
                    if (expr[i] == '(') depth++;
                    else if (expr[i] == ')') depth--;
                    if (depth == 0)
                    {
                        validPair = false;
                        break;
                    }
                }
                if (validPair)
                {
                    return EvaluateExpression(expr.Substring(1, expr.Length - 2));
                }
            }
            
            // Обработка NOT (!)
            if (expr.StartsWith("!"))
            {
                return !EvaluateExpression(expr.Substring(1));
            }
            
            // Ищем операторы с наименьшим приоритетом (справа налево)
            // Порядок: | (OR), ^ (XOR), & (AND)
            
            // OR (|)
            int orPos = FindOperatorOutsideParentheses(expr, '|');
            if (orPos >= 0)
            {
                bool left = EvaluateExpression(expr.Substring(0, orPos));
                bool right = EvaluateExpression(expr.Substring(orPos + 1));
                return left || right;
            }
            
            // XOR (^)
            int xorPos = FindOperatorOutsideParentheses(expr, '^');
            if (xorPos >= 0)
            {
                bool left = EvaluateExpression(expr.Substring(0, xorPos));
                bool right = EvaluateExpression(expr.Substring(xorPos + 1));
                return left ^ right;
            }
            
            // AND (&)
            int andPos = FindOperatorOutsideParentheses(expr, '&');
            if (andPos >= 0)
            {
                bool left = EvaluateExpression(expr.Substring(0, andPos));
                bool right = EvaluateExpression(expr.Substring(andPos + 1));
                return left && right;
            }
            
            throw new Exception($"Не удалось распарсить выражение: {expr}");
        }
        
        private int FindOperatorOutsideParentheses(string expr, char op)
        {
            int depth = 0;
            // Ищем справа налево для правильной ассоциативности
            for (int i = expr.Length - 1; i >= 0; i--)
            {
                if (expr[i] == ')') depth++;
                else if (expr[i] == '(') depth--;
                else if (expr[i] == op && depth == 0)
                {
                    return i;
                }
            }
            return -1;
        }
    }
}
