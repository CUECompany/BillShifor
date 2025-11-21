using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Media;

namespace BillShifor.Models
{
    // Элемент массива символов для отображения в таблице
    public class CharElement : INotifyPropertyChanged
    {
        private bool _isCurrent;
        private char _currentValue;

        public int Index { get; set; }

        public char OriginalValue { get; set; }

        public char CurrentValue
        {
            get => _currentValue;
            set
            {
                _currentValue = value;
                OnPropertyChanged(nameof(CurrentValue));
                OnPropertyChanged(nameof(IsEncrypted));
            }
        }

        public bool IsEncrypted => CurrentValue != OriginalValue;

        public bool IsCurrent
        {
            get => _isCurrent;
            set
            {
                _isCurrent = value;
                OnPropertyChanged(nameof(IsCurrent));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // Запись в логе выполнения
    public class ExecutionLogEntry
    {
        public string Step { get; set; } = "";
        public string Action { get; set; } = "";
        public string Details { get; set; } = "";
    }

    // Режим анализа
    public enum AnalysisMode
    {
        CaesarEncryption,
        PrefixSum,
        CountGreaterThanT,
        PrefixMax
    }
}