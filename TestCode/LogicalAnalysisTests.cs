using Xunit;
using BillShifor.ViewModels;
using BillShifor.Models;
using System.Linq;

namespace TestCode
{
    /// <summary>
    /// Тесты для методов Логического Анализа (TabItem 4)
    /// </summary>
    public class LogicalAnalysisTests
    {
        [Fact]
        public void GenerateTruthTable_TwoVariables_CreatesCorrectTable()
        {
            // Arrange
            var viewModel = new LogicalAnalysisViewModel
            {
                VariableCount = 2,
                FunctionNumber = 6 // XOR: 0110
            };

            // Act
            viewModel.GenerateTruthTable();

            // Assert
            Assert.Equal(4, viewModel.TruthTable.Count); // 2^2 = 4 строки
            
            // Проверка XOR функции
            Assert.False(viewModel.TruthTable[0].Output); // 0 XOR 0 = 0
            Assert.True(viewModel.TruthTable[1].Output);  // 0 XOR 1 = 1
            Assert.True(viewModel.TruthTable[2].Output);  // 1 XOR 0 = 1
            Assert.False(viewModel.TruthTable[3].Output); // 1 XOR 1 = 0
        }

        [Fact]
        public void GenerateTruthTable_ThreeVariables_CreatesCorrectTable()
        {
            // Arrange
            var viewModel = new LogicalAnalysisViewModel
            {
                VariableCount = 3,
                FunctionNumber = 128 // Любая функция для трех переменных
            };

            // Act
            viewModel.GenerateTruthTable();

            // Assert
            Assert.Equal(8, viewModel.TruthTable.Count); // 2^3 = 8 строк
            
            // Проверка, что каждая строка имеет правильное количество входов
            foreach (var row in viewModel.TruthTable)
            {
                Assert.Equal(3, row.Inputs.Count);
            }
        }

        [Fact]
        public void AnalyzeFormula_XORFunction_GeneratesCorrectDNF()
        {
            // Arrange
            var viewModel = new LogicalAnalysisViewModel
            {
                VariableCount = 2,
                FunctionNumber = 6, // XOR
                FormulaText = "A ^ B"
            };
            viewModel.GenerateTruthTable();

            // Act
            viewModel.AnalyzeFormula();

            // Assert
            Assert.NotEmpty(viewModel.DNFResult);
            // Для XOR должно быть 2 терма: (!A & B) | (A & !B)
            Assert.Contains("&", viewModel.DNFResult);
            Assert.Contains("|", viewModel.DNFResult);
        }

        [Fact]
        public void AnalyzeFormula_GeneratesCorrectKNF()
        {
            // Arrange
            var viewModel = new LogicalAnalysisViewModel
            {
                VariableCount = 2,
                FunctionNumber = 6 // XOR
            };
            viewModel.GenerateTruthTable();

            // Act
            viewModel.AnalyzeFormula();

            // Assert
            Assert.NotEmpty(viewModel.KNFResult);
            Assert.Contains("&", viewModel.KNFResult);
            Assert.Contains("|", viewModel.KNFResult);
        }

        [Fact]
        public void AnalyzeFormula_CalculatesCost()
        {
            // Arrange
            var viewModel = new LogicalAnalysisViewModel
            {
                VariableCount = 2,
                FunctionNumber = 6
            };
            viewModel.GenerateTruthTable();

            // Act
            viewModel.AnalyzeFormula();

            // Assert
            Assert.NotEmpty(viewModel.CostResult);
            Assert.Contains("Литералы", viewModel.CostResult);
            Assert.Contains("Конъюнкты", viewModel.CostResult);
            Assert.Contains("Дизъюнкты", viewModel.CostResult);
        }

        [Fact]
        public void CompareFormulas_IdenticalFormulas_ReturnsEquivalent()
        {
            // Arrange
            var viewModel = new LogicalAnalysisViewModel
            {
                Formula1 = "A ^ B",
                Formula2 = "A ^ B"
            };

            // Act
            viewModel.CompareFormulas();

            // Assert
            Assert.Contains("ЭКВИВАЛЕНТНЫ", viewModel.ComparisonResult);
        }

        [Fact]
        public void CompareFormulas_DifferentFormulas_ReturnsNotEquivalent()
        {
            // Arrange
            var viewModel = new LogicalAnalysisViewModel
            {
                Formula1 = "A & B",
                Formula2 = "A | B"
            };

            // Act
            viewModel.CompareFormulas();

            // Assert
            Assert.Contains("НЕ эквивалентны", viewModel.ComparisonResult);
            Assert.Contains("Контр-пример", viewModel.ComparisonResult);
        }

        [Fact]
        public void ClearAnalysis_ClearsAllData()
        {
            // Arrange
            var viewModel = new LogicalAnalysisViewModel
            {
                VariableCount = 2,
                FunctionNumber = 6
            };
            viewModel.GenerateTruthTable();
            viewModel.AnalyzeFormula();

            // Act
            viewModel.ClearAnalysis();

            // Assert
            Assert.Empty(viewModel.TruthTable);
            Assert.Empty(viewModel.DNFResult);
            Assert.Empty(viewModel.KNFResult);
            Assert.Empty(viewModel.CostResult);
            Assert.Empty(viewModel.ComparisonResult);
        }

        [Fact]
        public void SelectedCipher_VernamXOR_UpdatesPreset()
        {
            // Arrange
            var viewModel = new LogicalAnalysisViewModel();

            // Act
            viewModel.SelectedCipher = "Вернам (XOR)";

            // Assert
            Assert.Equal("A ^ B", viewModel.FormulaText);
            Assert.Equal(2, viewModel.VariableCount);
            Assert.Equal(6, viewModel.FunctionNumber);
        }

        [Fact]
        public void SelectedCipher_AtbashInversion_UpdatesPreset()
        {
            // Arrange
            var viewModel = new LogicalAnalysisViewModel();

            // Act
            viewModel.SelectedCipher = "Атбаш (инверсия)";

            // Assert
            Assert.Equal("!A", viewModel.FormulaText);
            Assert.Equal(1, viewModel.VariableCount);
            Assert.Equal(1, viewModel.FunctionNumber);
        }

        [Theory]
        [InlineData(0, 0)] // Константа 0
        [InlineData(3, 2)] // OR
        [InlineData(1, 1)] // AND
        [InlineData(15, 4)] // Константа 1
        public void GenerateTruthTable_DifferentFunctions_CreatesValidTables(int functionNumber, int expectedTrueCount)
        {
            // Arrange
            var viewModel = new LogicalAnalysisViewModel
            {
                VariableCount = 2,
                FunctionNumber = functionNumber
            };

            // Act
            viewModel.GenerateTruthTable();

            // Assert
            Assert.Equal(4, viewModel.TruthTable.Count);
            int trueCount = viewModel.TruthTable.Count(row => row.Output);
            Assert.Equal(expectedTrueCount, trueCount);
        }

        [Fact]
        public void TruthTableRow_InputString_FormatsCorrectly()
        {
            // Arrange
            var row = new TruthTableRow();
            row.Inputs.Add(true);
            row.Inputs.Add(false);
            row.Inputs.Add(true);

            // Act
            string inputString = row.InputString;

            // Assert
            Assert.Equal("101", inputString);
        }

        [Fact]
        public void TruthTableRow_OutputString_FormatsCorrectly()
        {
            // Arrange
            var row1 = new TruthTableRow { Output = true };
            var row2 = new TruthTableRow { Output = false };

            // Act & Assert
            Assert.Equal("1", row1.OutputString);
            Assert.Equal("0", row2.OutputString);
        }

        [Fact]
        public void AnalyzeFormula_EmptyTruthTable_HandlesGracefully()
        {
            // Arrange
            var viewModel = new LogicalAnalysisViewModel
            {
                FormulaText = "A & B"
            };
            // Не вызываем GenerateTruthTable

            // Act
            viewModel.AnalyzeFormula();

            // Assert
            Assert.Equal("0", viewModel.DNFResult);
            Assert.Equal("1", viewModel.KNFResult);
        }

        [Fact]
        public void GenerateTruthTable_AddsToAnalysisHistory()
        {
            // Arrange
            var viewModel = new LogicalAnalysisViewModel
            {
                VariableCount = 2,
                FunctionNumber = 6
            };

            // Act
            viewModel.GenerateTruthTable();

            // Assert
            Assert.True(viewModel.AnalysisHistory.Count > 0);
            Assert.Contains(viewModel.AnalysisHistory, entry => entry.Contains("Генерация"));
        }

        [Fact]
        public void AnalyzeFormula_AddsToAnalysisHistory()
        {
            // Arrange
            var viewModel = new LogicalAnalysisViewModel
            {
                VariableCount = 2,
                FunctionNumber = 6,
                FormulaText = "A ^ B"
            };
            viewModel.GenerateTruthTable();

            // Act
            viewModel.AnalyzeFormula();

            // Assert
            Assert.Contains(viewModel.AnalysisHistory, entry => entry.Contains("DNF"));
            Assert.Contains(viewModel.AnalysisHistory, entry => entry.Contains("KNF"));
            Assert.Contains(viewModel.AnalysisHistory, entry => entry.Contains("Стоимость"));
        }

        [Fact]
        public void CompareFormulas_AddsToAnalysisHistory()
        {
            // Arrange
            var viewModel = new LogicalAnalysisViewModel
            {
                Formula1 = "A & B",
                Formula2 = "A | B"
            };

            // Act
            viewModel.CompareFormulas();

            // Assert
            Assert.Contains(viewModel.AnalysisHistory, entry => entry.Contains("Сравнение"));
        }

        [Theory]
        [InlineData("Вернам (XOR)")]
        [InlineData("Цезарь (сложение mod 2)")]
        [InlineData("Атбаш (инверсия)")]
        [InlineData("Виженер (сложение)")]
        [InlineData("Плейфер (правила замены)")]
        public void SelectedCipher_AllCiphers_UpdatesCorrectly(string cipher)
        {
            // Arrange
            var viewModel = new LogicalAnalysisViewModel();

            // Act
            viewModel.SelectedCipher = cipher;

            // Assert
            Assert.NotEmpty(viewModel.FormulaText);
            Assert.True(viewModel.VariableCount > 0);
            Assert.True(viewModel.FunctionNumber >= 0);
        }

        [Fact]
        public void AnalyzeFormula_AllOnesFunction_DNFIsOne()
        {
            // Arrange
            var viewModel = new LogicalAnalysisViewModel
            {
                VariableCount = 2,
                FunctionNumber = 15 // 1111 - все единицы
            };
            viewModel.GenerateTruthTable();

            // Act
            viewModel.AnalyzeFormula();

            // Assert
            // Для функции всех единиц, DNF должна содержать все 4 терма
            int termCount = viewModel.DNFResult.Split('|').Length;
            Assert.True(termCount >= 4, "DNF должна содержать все термы для функции всех единиц");
        }

        [Fact]
        public void AnalyzeFormula_AllZerosFunction_DNFIsZero()
        {
            // Arrange
            var viewModel = new LogicalAnalysisViewModel
            {
                VariableCount = 2,
                FunctionNumber = 0 // 0000 - все нули
            };
            viewModel.GenerateTruthTable();

            // Act
            viewModel.AnalyzeFormula();

            // Assert
            Assert.Equal("0", viewModel.DNFResult);
        }
    }
}
