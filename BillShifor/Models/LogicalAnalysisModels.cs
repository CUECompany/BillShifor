
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace BillShifor.Models
{
    public class TruthTableRow
    {
        public List<bool> Inputs { get; set; } = new List<bool>();
        public bool Output { get; set; }
        public string InputString => string.Join("", Inputs.Select(b => b ? "1" : "0"));
        public string OutputString => Output ? "1" : "0";
    }

    public class LogicalFunction
    {
        public string Name { get; set; } = "";
        public List<TruthTableRow> TruthTable { get; set; } = new List<TruthTableRow>();
        public string DNF { get; set; } = "";
        public string KNF { get; set; } = "";
        public int LiteralCost { get; set; }
        public int ConjunctCost { get; set; }
        public int DisjunctCost { get; set; }
    }

    public class ComparisonResult
    {
        public bool AreEquivalent { get; set; }
        public string Message { get; set; } = "";
        public string CounterExample { get; set; } = "";
    }
}