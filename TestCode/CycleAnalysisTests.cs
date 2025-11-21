using Xunit;
using BillShifor.ViewModels;
using BillShifor.Models;
using System.Linq;

namespace TestCode
{
    /// <summary>
    /// Тесты для методов Анализатора Циклов (TabItem 3)
    /// </summary>
    public class CycleAnalysisTests
    {
        [Fact]
        public void GenerateCharArray_ValidText_CreatesCorrectArray()
        {
            // Arrange
            var viewModel = new CycleAnalysisViewModel();
            string testText = "тест";

            // Act
            viewModel.CurrentText = testText;

            // Assert
            Assert.Equal(testText.Length, viewModel.CharArray.Count);
            for (int i = 0; i < testText.Length; i++)
            {
                Assert.Equal(i, viewModel.CharArray[i].Index);
                Assert.Equal(testText[i], viewModel.CharArray[i].OriginalValue);
                Assert.Equal(testText[i], viewModel.CharArray[i].CurrentValue);
            }
        }

        [Fact]
        public void ExecuteCaesarStep_RussianLetter_EncryptsCorrectly()
        {
            // Arrange
            var viewModel = new CycleAnalysisViewModel
            {
                CurrentText = "а",
                CaesarKey = 1,
                CurrentMode = AnalysisMode.CaesarEncryption
            };
            viewModel.GenerateCharArray();

            // Act
            viewModel.ExecuteStep();

            // Assert
            Assert.Equal('б', viewModel.CharArray[0].CurrentValue);
            Assert.True(viewModel.CharArray[0].IsEncrypted);
        }

        [Fact]
        public void ExecuteCaesarStep_NonRussianLetter_RemainsUnchanged()
        {
            // Arrange
            var viewModel = new CycleAnalysisViewModel
            {
                CurrentText = "1",
                CaesarKey = 5,
                CurrentMode = AnalysisMode.CaesarEncryption
            };
            viewModel.GenerateCharArray();

            // Act
            viewModel.ExecuteStep();

            // Assert
            Assert.Equal('1', viewModel.CharArray[0].CurrentValue);
            Assert.False(viewModel.CharArray[0].IsEncrypted);
        }

        [Fact]
        public void ExecutePrefixSumStep_AddsValueCorrectly()
        {
            // Arrange
            var viewModel = new CycleAnalysisViewModel
            {
                CurrentText = "abc",
                CurrentMode = AnalysisMode.PrefixSum
            };
            viewModel.GenerateCharArray();

            // Act
            viewModel.ExecuteStep(); // Первый символ 'a'

            // Assert
            int expectedSum = (int)'a';
            Assert.Equal(expectedSum, viewModel.PrefixSum);
        }

        [Fact]
        public void ExecuteCountGreaterThanTStep_CountsCorrectly()
        {
            // Arrange
            var viewModel = new CycleAnalysisViewModel
            {
                CurrentText = "ABC", // ASCII коды: A=65, B=66, C=67
                Threshold = 66,
                CurrentMode = AnalysisMode.CountGreaterThanT
            };
            viewModel.GenerateCharArray();

            // Act
            viewModel.ExecuteStep(); // A (65) <= 66
            viewModel.ExecuteStep(); // B (66) <= 66
            viewModel.ExecuteStep(); // C (67) > 66

            // Assert
            Assert.Equal(1, viewModel.CountGreaterThanT);
        }

        [Fact]
        public void ExecutePrefixMaxStep_FindsMaximum()
        {
            // Arrange
            var viewModel = new CycleAnalysisViewModel
            {
                CurrentText = "abc", // ASCII: a=97, b=98, c=99
                CurrentMode = AnalysisMode.PrefixMax
            };
            viewModel.GenerateCharArray();

            // Act
            viewModel.ExecuteStep(); // a = 97
            viewModel.ExecuteStep(); // b = 98
            viewModel.ExecuteStep(); // c = 99

            // Assert
            Assert.Equal((int)'c', viewModel.PrefixMax);
        }

        [Fact]
        public void ResetAnalysis_ResetsAllParameters()
        {
            // Arrange
            var viewModel = new CycleAnalysisViewModel
            {
                CurrentText = "test",
                CaesarKey = 5,
                CurrentMode = AnalysisMode.CaesarEncryption
            };
            viewModel.GenerateCharArray();
            viewModel.ExecuteStep();
            viewModel.ExecuteStep();

            // Act
            viewModel.ResetAnalysis();

            // Assert
            Assert.Equal(0, viewModel.CurrentIndex);
            Assert.Equal(viewModel.CharArray.Count, viewModel.VariantFunction);
            Assert.Equal(0, viewModel.PrefixSum);
            Assert.Equal(0, viewModel.CountGreaterThanT);
            Assert.Equal(0, viewModel.PrefixMax);
            
            // Проверка, что все символы вернулись к исходному состоянию
            foreach (var element in viewModel.CharArray)
            {
                Assert.Equal(element.OriginalValue, element.CurrentValue);
            }
        }

        [Fact]
        public void CheckInvariantBeforeStep_ReturnsTrue_WhenIndexInRange()
        {
            // Arrange
            var viewModel = new CycleAnalysisViewModel
            {
                CurrentText = "test"
            };
            viewModel.GenerateCharArray();

            // Act
            viewModel.ExecuteStep();

            // Assert
            Assert.True(viewModel.IsInvariantBefore);
        }

        [Fact]
        public void CheckInvariantAfterStep_ReturnsTrue_WhenIndexInRange()
        {
            // Arrange
            var viewModel = new CycleAnalysisViewModel
            {
                CurrentText = "test"
            };
            viewModel.GenerateCharArray();

            // Act
            viewModel.ExecuteStep();

            // Assert
            Assert.True(viewModel.IsInvariantAfter);
        }

        [Fact]
        public void VariantFunction_DecreasesAfterEachStep()
        {
            // Arrange
            var viewModel = new CycleAnalysisViewModel
            {
                CurrentText = "test"
            };
            viewModel.GenerateCharArray();
            int initialVariant = viewModel.VariantFunction;

            // Act
            viewModel.ExecuteStep();
            int afterFirstStep = viewModel.VariantFunction;
            viewModel.ExecuteStep();
            int afterSecondStep = viewModel.VariantFunction;

            // Assert
            Assert.Equal(4, initialVariant);
            Assert.Equal(3, afterFirstStep);
            Assert.Equal(2, afterSecondStep);
            Assert.True(initialVariant > afterFirstStep);
            Assert.True(afterFirstStep > afterSecondStep);
        }

        [Fact]
        public void ExecutionLog_RecordsSteps()
        {
            // Arrange
            var viewModel = new CycleAnalysisViewModel
            {
                CurrentText = "ab"
            };
            viewModel.GenerateCharArray();

            // Act
            viewModel.ExecuteStep();

            // Assert
            Assert.True(viewModel.ExecutionLog.Count > 0);
            Assert.Contains(viewModel.ExecutionLog, log => log.Step.Contains("Шаг"));
        }

        [Theory]
        [InlineData(AnalysisMode.CaesarEncryption)]
        [InlineData(AnalysisMode.PrefixSum)]
        [InlineData(AnalysisMode.CountGreaterThanT)]
        [InlineData(AnalysisMode.PrefixMax)]
        public void ExecuteStep_DifferentModes_AllExecuteWithoutError(AnalysisMode mode)
        {
            // Arrange
            var viewModel = new CycleAnalysisViewModel
            {
                CurrentText = "test",
                CurrentMode = mode,
                Threshold = 100
            };
            viewModel.GenerateCharArray();

            // Act & Assert
            var exception = Record.Exception(() => viewModel.ExecuteStep());
            Assert.Null(exception);
        }

        [Fact]
        public void InvariantFormula_ChangesWithMode()
        {
            // Arrange
            var viewModel = new CycleAnalysisViewModel();

            // Act & Assert - Цезарь
            viewModel.CurrentMode = AnalysisMode.CaesarEncryption;
            Assert.Contains("Encrypt", viewModel.InvariantFormula);

            // Act & Assert - PrefixSum
            viewModel.CurrentMode = AnalysisMode.PrefixSum;
            Assert.Contains("Σ", viewModel.InvariantFormula);

            // Act & Assert - CountGreaterThanT
            viewModel.CurrentMode = AnalysisMode.CountGreaterThanT;
            Assert.Contains(">", viewModel.InvariantFormula);

            // Act & Assert - PrefixMax
            viewModel.CurrentMode = AnalysisMode.PrefixMax;
            Assert.Contains("max", viewModel.InvariantFormula);
        }

        [Fact]
        public void CurrentResult_CaesarMode_ReturnsEncryptedString()
        {
            // Arrange
            var viewModel = new CycleAnalysisViewModel
            {
                CurrentText = "abc",
                CaesarKey = 1,
                CurrentMode = AnalysisMode.CaesarEncryption
            };
            viewModel.GenerateCharArray();

            // Act
            viewModel.ExecuteStep();
            viewModel.ExecuteStep();
            viewModel.ExecuteStep();

            // Assert
            string result = viewModel.CurrentResult;
            Assert.NotEmpty(result);
            Assert.Equal(3, result.Length);
        }

        [Fact]
        public void VariantProgress_CalculatesCorrectPercentage()
        {
            // Arrange
            var viewModel = new CycleAnalysisViewModel
            {
                CurrentText = "test"
            };
            viewModel.GenerateCharArray();

            // Act
            double initialProgress = viewModel.VariantProgress;
            viewModel.ExecuteStep();
            double afterOneStep = viewModel.VariantProgress;
            viewModel.ExecuteStep();
            double afterTwoSteps = viewModel.VariantProgress;

            // Assert
            Assert.Equal(100.0, initialProgress);
            Assert.Equal(75.0, afterOneStep);
            Assert.Equal(50.0, afterTwoSteps);
        }

        [Fact]
        public void ExecuteStep_BeyondArrayLength_DoesNotExecute()
        {
            // Arrange
            var viewModel = new CycleAnalysisViewModel
            {
                CurrentText = "a"
            };
            viewModel.GenerateCharArray();

            // Act
            viewModel.ExecuteStep(); // Выполняем один шаг
            int indexAfterFirstStep = viewModel.CurrentIndex;
            viewModel.ExecuteStep(); // Пытаемся выполнить еще один шаг

            // Assert
            Assert.Equal(1, indexAfterFirstStep); // После первого шага индекс = 1
            Assert.Equal(1, viewModel.CurrentIndex); // После второго шага индекс не изменился
        }
    }
}
