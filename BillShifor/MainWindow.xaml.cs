using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace WpCalculator
{
    public partial class MainWindow : Window
    {
        private WpEngine wpEngine;

        public MainWindow()
        {
            InitializeComponent();
            wpEngine = new WpEngine();
        }

        private void CalculateWp_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string program = ProgramTextBox.Text;
                string postCondition = PostConditionTextBox.Text;
                string postDescription = PostDescriptionTextBox.Text;

                var result = wpEngine.CalculateWp(program, postCondition, postDescription);

                // Отображаем результаты
                StepTraceTextBox.Text = result.StepTrace;
                FinalPreconditionTextBox.Text = result.FinalPrecondition;
                FinalDescriptionTextBox.Text = result.FinalDescription;
                HoareTripleTextBox.Text = result.HoareTriple;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка расчета",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadMaxExample(object sender, RoutedEventArgs e)
        {
            ProgramTextBox.Text = "if (x1 >= x2)\n    max := x1;\nelse\n    max := x2;";
            PostConditionTextBox.Text = "max > 100";
            PostDescriptionTextBox.Text = "Максимальное значение больше 100";
        }

        private void LoadQuadraticExample(object sender, RoutedEventArgs e)
        {
            ProgramTextBox.Text = "if (D >= 0)\n    root := (-b + sqrt(D)) / (2*a);\nelse\n    root := -999;";
            PostConditionTextBox.Text = "(root >= -999) && (D >= 0 -> root*root*a + root*b + c == 0)";
            PostDescriptionTextBox.Text = "Корень либо вычислен корректно, либо установлен в специальное значение";
        }

        private void LoadSequenceExample(object sender, RoutedEventArgs e)
        {
            ProgramTextBox.Text = "x := x + 10;\ny := x + 1;";
            PostConditionTextBox.Text = "y == x - 9 && x > 15";
            PostDescriptionTextBox.Text = "y равно x-9 и x больше 15";
        }
    }
}