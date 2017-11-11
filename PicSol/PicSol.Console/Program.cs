using PicSol.Tests;
using System;
using System.Linq;

namespace PicSol
{
    class Program
    {
        static void Main(string[] args)
        {
            var timeout = TimeSpan.FromSeconds(30);
            foreach (var nonogram in ExampleNonograms.AllNonograms)
            {
                Console.Clear();
                
                var solution = Solver.Solve(nonogram, timeout);
                Console.WriteLine(SolutionRenderer.RenderToString(solution));
                Console.WriteLine("Press ENTER for the next Nonogram.");
                Console.ReadLine();
            }
        }
    }
}
