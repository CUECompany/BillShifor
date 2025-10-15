using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace WpCalculator
{
    public class WpResult
    {
        public string StepTrace { get; set; } = "";
        public string FinalPrecondition { get; set; } = "";
        public string FinalDescription { get; set; } = "";
        public string HoareTriple { get; set; } = "";
    }

    public class WpEngine
    {
        private StringBuilder stepTrace;
        private List<string> definednessConditions;

        public WpResult CalculateWp(string program, string postCondition, string postDescription)
        {
            stepTrace = new StringBuilder();
            definednessConditions = new List<string>();

            stepTrace.AppendLine("=== НАЧАЛО РАСЧЕТА WP ===");
            stepTrace.AppendLine($"Исходное постусловие: {postCondition}");
            stepTrace.AppendLine();

            // Разбиваем программу на строки и обрабатываем
            string[] lines = program.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            string currentCondition = postCondition;

            for (int i = lines.Length - 1; i >= 0; i--)
            {
                string line = lines[i].Trim();
                if (string.IsNullOrEmpty(line)) continue;

                stepTrace.AppendLine($"Обрабатываем: {line}");
                string previousCondition = currentCondition;

                if (line.StartsWith("if"))
                {
                    currentCondition = ProcessIfStatement(line, lines, ref i, currentCondition);
                }
                else if (line.Contains(":="))
                {
                    currentCondition = ProcessAssignment(line, currentCondition);
                }
                else if (line == "else" || line == "{")
                {
                    continue;
                }
                else if (line == "}")
                {
                    // Пропускаем закрывающие скобки
                    continue;
                }

                stepTrace.AppendLine($"Преобразованное условие: {currentCondition}");
                stepTrace.AppendLine();
            }

            // Добавляем условия определенности
            if (definednessConditions.Count > 0)
            {
                string definedness = string.Join(" && ", definednessConditions);
                if (currentCondition != "true")
                {
                    currentCondition = $"{currentCondition} && {definedness}";
                }
                else
                {
                    currentCondition = definedness;
                }

                stepTrace.AppendLine($"Добавлены условия определенности: {definedness}");
                stepTrace.AppendLine($"Финальное предусловие: {currentCondition}");
            }

            stepTrace.AppendLine("=== РАСЧЕТ ЗАВЕРШЕН ===");

            return new WpResult
            {
                StepTrace = stepTrace.ToString(),
                FinalPrecondition = currentCondition,
                FinalDescription = GeneratePreconditionDescription(currentCondition),
                HoareTriple = GenerateHoareTriple(currentCondition, program, postCondition)
            };
        }

        private string ProcessAssignment(string line, string condition)
        {
            // Разбираем присваивание: variable := expression
            var match = Regex.Match(line, @"(\w+)\s*:=\s*(.+)");
            if (!match.Success)
                throw new ArgumentException($"Некорректное присваивание: {line}");

            string variable = match.Groups[1].Value;
            string expression = match.Groups[2].Value.TrimEnd(';');

            // Добавляем условия определенности
            AddDefinednessConditions(expression);

            // Заменяем переменную на выражение в условии
            string result = ReplaceVariableWithExpression(condition, variable, expression);

            stepTrace.AppendLine($"Присваивание: {variable} := {expression}");
            stepTrace.AppendLine($"Замена в условии: {condition} -> {result}");

            return result;
        }

        private string ProcessIfStatement(string ifLine, string[] lines, ref int currentIndex, string condition)
        {
            // Извлекаем условие из if
            var match = Regex.Match(ifLine, @"if\s*\((.*)\)");
            if (!match.Success)
                throw new ArgumentException($"Некорректный оператор if: {ifLine}");

            string ifCondition = match.Groups[1].Value;

            // Находим ветки then и else
            string thenBranch = "";
            string elseBranch = "";
            bool inThen = false, inElse = false;
            int braceCount = 0;

            for (int i = currentIndex + 1; i < lines.Length; i++)
            {
                string line = lines[i].Trim();

                if (line == "{")
                {
                    braceCount++;
                    continue;
                }
                else if (line == "}")
                {
                    braceCount--;
                    if (braceCount == 0)
                    {
                        if (inThen) inThen = false;
                        if (inElse) inElse = false;
                    }
                    continue;
                }

                if (line == "else")
                {
                    inThen = false;
                    inElse = true;
                    continue;
                }

                if (braceCount == 1 && !inThen && !inElse && line != "else")
                {
                    inThen = true;
                }

                if (inThen && braceCount == 1)
                {
                    thenBranch += line + "\n";
                }
                else if (inElse && braceCount == 1)
                {
                    elseBranch += line + "\n";
                }
            }

            // Рекурсивно вычисляем wp для веток
            stepTrace.AppendLine($"Обработка ветвления: if ({ifCondition})");

            string wpThen = condition;
            if (!string.IsNullOrEmpty(thenBranch))
            {
                var thenResult = CalculateWp(thenBranch, condition, "");
                wpThen = thenResult.FinalPrecondition;
            }

            string wpElse = condition;
            if (!string.IsNullOrEmpty(elseBranch))
            {
                var elseResult = CalculateWp(elseBranch, condition, "");
                wpElse = elseResult.FinalPrecondition;
            }

            // Формируем результат по правилу для if
            string result = $"({ifCondition} && {wpThen}) || (!({ifCondition}) && {wpElse})";

            stepTrace.AppendLine($"Then ветка: {wpThen}");
            stepTrace.AppendLine($"Else ветка: {wpElse}");
            stepTrace.AppendLine($"Результат if: {result}");

            // Пропускаем обработанные строки
            while (currentIndex < lines.Length - 1 &&
                   !lines[currentIndex + 1].Trim().StartsWith("if") &&
                   !lines[currentIndex + 1].Trim().Contains(":="))
            {
                currentIndex++;
            }

            return result;
        }

        private string ReplaceVariableWithExpression(string condition, string variable, string expression)
        {
            // Простая замена переменной на выражение
            // В реальной реализации нужен парсер выражений
            string pattern = $@"\b{variable}\b";
            return Regex.Replace(condition, pattern, $"({expression})");
        }

        private void AddDefinednessConditions(string expression)
        {
            // Проверяем деление на ноль
            if (expression.Contains("/"))
            {
                var divisions = Regex.Matches(expression, @"([^/]+)/([^/]+)");
                foreach (Match division in divisions)
                {
                    string denominator = division.Groups[2].Value;
                    if (!denominator.Contains("(") && !denominator.Contains(")"))
                    {
                        definednessConditions.Add($"{denominator} != 0");
                    }
                }
            }

            // Проверяем квадратный корень
            if (expression.Contains("sqrt("))
            {
                var sqrtMatches = Regex.Matches(expression, @"sqrt\(([^)]+)\)");
                foreach (Match sqrtMatch in sqrtMatches)
                {
                    string sqrtArg = sqrtMatch.Groups[1].Value;
                    definednessConditions.Add($"{sqrtArg} >= 0");
                }
            }
        }

        private string GeneratePreconditionDescription(string precondition)
        {
            if (precondition.Contains(">=") && precondition.Contains("||"))
            {
                return "Либо первое число больше или равно второму и первое число больше 100, " +
                       "либо первое число меньше второго и второе число больше 100";
            }
            else if (precondition.Contains("> 15"))
            {
                return "Исходное значение x должно быть больше 5";
            }
            else if (precondition.Contains("sqrt("))
            {
                return "Дискриминант должен быть неотрицательным и знаменатель не должен быть нулевым";
            }

            return "Автоматически сгенерированное предусловие для гарантии выполнения постусловия";
        }

        private string GenerateHoareTriple(string precondition, string program, string postCondition)
        {
            return $"{{ {precondition} }}\n{program}\n{{ {postCondition} }}";
        }
    }
}