using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace BillShifor.WpCalculator
{
    public class CryptoWpStep
    {
        public string Description { get; set; } = "";
        public string Expression { get; set; } = "";
        public string SecurityCheck { get; set; } = "";
    }

    public class CryptoWpResult
    {
        public List<CryptoWpStep> Steps { get; set; } = new List<CryptoWpStep>();
        public string FinalPrecondition { get; set; } = "";
        public string SecurityAssessment { get; set; } = "";
        public string HoareTriple { get; set; } = "";
        public bool IsSecure { get; set; }
    }

    public static class CryptoWpEngine
    {
        private static readonly Dictionary<string, string> CryptoRules = new Dictionary<string, string>
        {
            { "key_length", "key.Length >= 8" },
            { "non_repeating", "∀i,j: i≠j ⇒ key[i] ≠ key[j]" },
            { "alphabet_bound", "∀c∈text: c ∈ alphabet" },
            { "shift_range", "shift >= 1 ∧ shift <= 32" },
            { "no_division_by_zero", "denominator ≠ 0" },
            { "positive_modulus", "modulus > 1" },
            { "prime_check", "IsPrime(p) ∧ IsPrime(q)" }
        };

        public static CryptoWpResult AnalyzeCryptoAlgorithm(string algorithm, string code, string securityPostcondition)
        {
            var result = new CryptoWpResult();
            string currentCondition = securityPostcondition.Trim();

            result.Steps.Add(new CryptoWpStep 
            { 
                Description = $"🔐 Начальное условие безопасности для {algorithm}",
                Expression = currentCondition,
                SecurityCheck = "Инициализация анализа"
            });

            var lines = ParseCode(code);
            var securityRules = GetSecurityRulesForAlgorithm(algorithm);
            
            foreach (var rule in securityRules)
            {
                currentCondition = $"{rule} ∧ ({currentCondition})";
                
                result.Steps.Add(new CryptoWpStep 
                { 
                    Description = $"🛡️ Добавлено правило безопасности",
                    Expression = rule,
                    SecurityCheck = "Криптографическая проверка"
                });
            }

            for (int i = lines.Count - 1; i >= 0; i--)
            {
                var line = lines[i].Trim();
                if (string.IsNullOrEmpty(line)) continue;

                if (line.StartsWith("if", StringComparison.OrdinalIgnoreCase))
                {
                    currentCondition = ProcessCryptoIfStatement(line, currentCondition, algorithm, result);
                }
                else if (line.Contains(":="))
                {
                    currentCondition = ProcessCryptoAssignment(line, currentCondition, algorithm, result);
                }
                else if (line.Contains("encrypt") || line.Contains("decrypt"))
                {
                    currentCondition = ProcessCryptoOperation(line, currentCondition, result);
                }
            }

            result.FinalPrecondition = currentCondition;
            result.IsSecure = EvaluateSecurity(currentCondition);
            result.SecurityAssessment = GenerateSecurityAssessment(result.IsSecure, algorithm);
            result.HoareTriple = GenerateCryptoHoareTriple(result.FinalPrecondition, algorithm, code, securityPostcondition);
            
            return result;
        }

        private static List<string> GetSecurityRulesForAlgorithm(string algorithm)
        {
            var rules = new List<string>();

            switch (algorithm.ToLower())
            {
                case "цезарь":
                    rules.Add(CryptoRules["shift_range"]);
                    rules.Add("alphabet.Length == 33");
                    break;
                    
                case "виженер":
                    rules.Add(CryptoRules["key_length"]);
                    rules.Add(CryptoRules["non_repeating"]);
                    rules.Add(CryptoRules["alphabet_bound"]);
                    break;
                    
                case "rsa":
                    rules.Add(CryptoRules["positive_modulus"]);
                    rules.Add(CryptoRules["prime_check"]);
                    rules.Add("p ≠ q");
                    rules.Add("gcd(e, φ(n)) == 1");
                    break;
                    
                case "плейфер":
                    rules.Add("matrix.IsValid()");
                    rules.Add("∀row: row.Distinct().Count() == 6");
                    rules.Add(CryptoRules["key_length"]);
                    break;
                    
                case "вернам":
                    rules.Add("key.Length >= text.Length");
                    rules.Add("key.IsRandom");
                    rules.Add("∀c∈key: c ∈ alphabet");
                    break;
            }

            return rules;
        }

        private static string ProcessCryptoAssignment(string assignment, string condition, string algorithm, CryptoWpResult result)
        {
            var parts = assignment.Split(new[] { ":=" }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2) return condition;

            string variable = parts[0].Trim();
            string expression = parts[1].Trim().TrimEnd(';');

            var cryptoChecks = GenerateCryptoSafetyChecks(expression, algorithm);
            
            string newCondition = ReplaceVariableInCondition(condition, variable, expression);
            
            if (!string.IsNullOrEmpty(cryptoChecks))
            {
                newCondition = $"{cryptoChecks} ∧ ({newCondition})";
            }

            result.Steps.Add(new CryptoWpStep 
            { 
                Description = $"🔒 После криптографического присваивания: {assignment}",
                Expression = newCondition,
                SecurityCheck = cryptoChecks
            });

            return newCondition;
        }

        private static string ProcessCryptoIfStatement(string ifLine, string condition, string algorithm, CryptoWpResult result)
        {
            var match = Regex.Match(ifLine, @"if\s*\((.*?)\)", RegexOptions.IgnoreCase);
            if (!match.Success) return condition;

            string conditionB = match.Groups[1].Value.Trim();
            
            string newCondition = $"( {conditionB} ∧ wp(secure_branch, {condition}) ) ∨ ( ¬{conditionB} ∧ wp(fallback_branch, {condition}) )";

            result.Steps.Add(new CryptoWpStep 
            { 
                Description = $"🔄 Анализ ветвления безопасности",
                Expression = newCondition,
                SecurityCheck = $"Проверка условия: {conditionB}"
            });

            return newCondition;
        }

        private static string ProcessCryptoOperation(string operation, string condition, CryptoWpResult result)
        {
            string newCondition = condition;
            
            if (operation.Contains("encrypt"))
            {
                newCondition = $"IsValidEncryption(text, key) ∧ ({condition})";
                
                result.Steps.Add(new CryptoWpStep 
                { 
                    Description = "🔏 Анализ операции шифрования",
                    Expression = newCondition,
                    SecurityCheck = "Проверка корректности шифрования"
                });
            }
            else if (operation.Contains("decrypt"))
            {
                newCondition = $"IsValidDecryption(ciphertext, key) ∧ ({condition})";
                
                result.Steps.Add(new CryptoWpStep 
                { 
                    Description = "🔓 Анализ операции дешифрования",
                    Expression = newCondition,
                    SecurityCheck = "Проверка корректности дешифрования"
                });
            }

            return newCondition;
        }

        private static string GenerateCryptoSafetyChecks(string expression, string algorithm)
        {
            var checks = new List<string>();

            if (expression.Contains("/"))
            {
                checks.Add("denominator ≠ 0");
            }

            if (expression.Contains("sqrt("))
            {
                checks.Add("sqrt_argument ≥ 0");
            }

            if (expression.Contains("mod") || expression.Contains("%"))
            {
                checks.Add("modulus > 0");
            }

            switch (algorithm.ToLower())
            {
                case "цезарь":
                    if (expression.Contains("shift"))
                    {
                        checks.Add("1 ≤ shift ≤ 32");
                    }
                    break;
                    
                case "rsa":
                    if (expression.Contains("p * q"))
                    {
                        checks.Add("p > 1 ∧ q > 1 ∧ p ≠ q");
                    }
                    if (expression.Contains("ModPow"))
                    {
                        checks.Add("base ≥ 0 ∧ exponent ≥ 0 ∧ modulus > 1");
                    }
                    break;
            }

            return checks.Count > 0 ? string.Join(" ∧ ", checks) : "";
        }

        private static bool EvaluateSecurity(string condition)
        {
            return !condition.Contains("0") && 
                   !condition.Contains("undefined") && 
                   !condition.ToLower().Contains("false");
        }

        private static string GenerateSecurityAssessment(bool isSecure, string algorithm)
        {
            if (isSecure)
            {
                return $"✅ Алгоритм '{algorithm}' прошел проверку безопасности. Все предусловия удовлетворяют требованиям криптографической стойкости.";
            }
            else
            {
                return $"⚠️ Алгоритм '{algorithm}' имеет потенциальные уязвимости. Рекомендуется провести дополнительный аудит безопасности.";
            }
        }

        private static string GenerateCryptoHoareTriple(string precondition, string algorithm, string code, string postcondition)
        {
            return $"{{ БЕЗОПАСНОСТЬ: {precondition} }}\n" +
                   $"АЛГОРИТМ: {algorithm}\n" +
                   $"КОД: {code}\n" +
                   $"{{ ГАРАНТИЯ: {postcondition} }}";
        }

        private static List<string> ParseCode(string code)
        {
            return code.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                      .Select(line => line.Trim())
                      .Where(line => !string.IsNullOrEmpty(line))
                      .ToList();
        }

        private static string ReplaceVariableInCondition(string condition, string variable, string expression)
        {
            string pattern = $@"\b{variable}\b";
            return Regex.Replace(condition, pattern, $"({expression})");
        }
    }
}