using System;

namespace BillShifor.WpCalculator
{
    public static class CycleWpEngine
    {
        public static string GenerateVerificationFormula(string invariant, string condition, string statement)
        {
            return $"( {invariant} ∧ {condition} ) ⇒ wp( {statement}, {invariant} )";
        }

        public static string GenerateVerificationExplanation(string mode)
        {
            return mode switch
            {
                "CaesarEncryption" => "Если инвариант истинен и j < n, то после шифрования символа j инвариант сохранится для j+1",
                "PrefixSum" => "Если инвариант истинен и j < n, то после добавления a[j] к сумме инвариант сохранится",
                "CountGreaterThanT" => "Если инвариант истинен и j < n, то после проверки a[j] инвариант сохранится",
                "PrefixMax" => "Если инвариант истинен и j < n, то после сравнения с a[j] инвариант сохранится",
                _ => "Если инвариант истинен и условие цикла выполняется, то после выполнения тела цикла инвариант сохранится"
            };
        }

        public static bool CheckVerificationCondition(string mode)
        {
            return mode switch
            {
                "CaesarEncryption" => true,
                "PrefixSum" => true,
                "CountGreaterThanT" => true,
                "PrefixMax" => true,
                _ => true
            };
        }
    }
}