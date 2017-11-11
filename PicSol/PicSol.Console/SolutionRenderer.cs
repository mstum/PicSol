using System.Linq;
using System.Text;

namespace PicSol
{
    public static class SolutionRenderer
    {
        private const char Filled = '■';
        private const char Empty = '∙';

        public static string RenderToString(Solution solution)
        { 
            var sb = new StringBuilder();
            sb.AppendLine($"Nonogram '{solution.Nonogram.Name}' Result: {solution.Result}.");
            var state = solution.SolverState;
            sb.AppendLine($"Performance: Time taken: {solution.TimeTaken.TotalMilliseconds}ms, Steps: {state.StepCount}, Initial Permutations: {state.InitialRowPermutationsCount} Row, {state.InitialColumnPermutationsCount} Column, {state.InitialRowPermutationsCount + state.InitialColumnPermutationsCount} Total permutations.");
            sb.AppendLine();

            int rowHintMax = MaxArrayLength(solution.Nonogram.RowHints);
            int colHintMax = MaxArrayLength(solution.Nonogram.ColumnHints);
            int rowGutterSize = rowHintMax * 2;
            int colGutterSize = colHintMax * 2;

            for (int row = 0; row < colHintMax; row++)
            {
                sb.Append(" ".PadLeft(rowGutterSize + 1));
                for (int col = 0; col < solution.ColumnCount; col++)
                {
                    if (solution.Nonogram.ColumnHints[col].Length > row)
                    {
                        sb.Append(solution.Nonogram.ColumnHints[col][row].ToString().PadLeft(2, ' '));
                    }
                    else
                    {
                        sb.Append("  ");
                    }
                    sb.Append(" ");
                }
                sb.AppendLine();
            }

            for (int row = 0; row < solution.RowCount; row++)
            {
                var rowHint = solution.Nonogram.RowHints[row];

                var rowGutter = string.Join(' ', rowHint).PadLeft(rowGutterSize);
                sb.Append(rowGutter);
                sb.Append(' ');

                for (int col = 0; col < solution.ColumnCount; col++)
                {
                    var value = solution.Tiles[row, col];

                    sb.Append(" ");
                    sb.Append(value ? Filled : Empty);
                    sb.Append(" ");
                }
                sb.AppendLine();
            }

            return sb.ToString();
        }

        private static int MaxArrayLength(HintCollection hints) => hints.Count == 0 ? 0 : hints.Max(h => h.Length);
    }
}
