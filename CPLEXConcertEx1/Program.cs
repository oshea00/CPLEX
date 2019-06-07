using System;
using System.Linq;
using ILOG.Concert;
using ILOG.CPLEX;
using static System.Math;

// CPLEX Concert API example.
// 
// The example is the naive portfolio optimization to find best
// allocation weights.
// (see CPLEX OPL example: portfolio.mod)
//
// This is the same model translated to C# using the Concert API
// for CPLEX to setup the variables (weigts), bounds, objective and
// constraints.
//
namespace CPLEXConcertEx1
{
    class Program
    {
        static void Main(string[] args)
        {
            // Portfolio asset daily price data:
            var assets = new[] { "A", "B", "C" };
            var prices = new double[][] {
                new double[] {.02,.03,.02,.05},
                new double[] {.01,.01,.05,.01},
                new double[] {.1,.05,.04,.02},
            };
            // Pre-process data
            var covmat = CovarianceMatrix(prices);
            DisplayCovariance(covmat);
            var expReturns = AssetReturns(prices);
            DisplayReturns(assets, expReturns);
            var rho = 0.05;

            // Create LP model

            try
            {
                Cplex cplex = new Cplex();

                // Weights bounded 0 <= weight <= 1.0
                var weights = cplex.NumVarArray(assets.Length, 0, 1.0);

                // x = weights.
                // Expected (mean) return of portfolio: ∑ᵢ x[i]*assetReturn[i]
                var expRet = cplex.ScalProd(expReturns, weights);

                // Portfolio variance : xᵀ∑x (matrix form of bi-linear: ∑ᵢ∑ⱼ x[i]*x[j]*cov(i,j)
                // where ∑ is the covariance matrix m*m of m assets.
                var pvar = cplex.QuadNumExpr();
                for (int i = 0; i < assets.Length; ++i)
                {
                    for (int j = 0; j < assets.Length; ++j)
                    {
                        pvar.AddTerm(covmat[i][j], weights[i], weights[j]);
                    }
                }

                // Objective (maximize): portfolio return  - (Rho/2) * portfolio variance.
                // Where 0 <= Rho <= 1 is the desired risk tolerance. 
                var obj = cplex.Diff(expRet, cplex.Prod(rho / 2, pvar));

                cplex.AddMaximize(obj);

                // Subject to:
                // Sum of weights = 1.0
                cplex.AddEq(cplex.Sum(weights), 1.0);

                // Exports model definition to readable LP format
                cplex.ExportModel(@"c:\users\mike\cplexfin.lp");

                // Solve and print results
                if (cplex.Solve())
                {
                    var w = new double[weights.Length];
                    Console.WriteLine("\nResulting Weights:");
                    for (int i = 0; i < weights.Length; i++)
                    {
                        Console.WriteLine($"{i + 1} : {cplex.GetValue(weights[i]) * 100:0.0}%");
                        w[i] = cplex.GetValue(weights[i]);
                    }
                    var totReturn = WeightedReturn(w, expReturns);
                    var totVariance = PortfolioVariance(w, covmat);
                    Console.WriteLine($"Total Return: {totReturn:0.00}, Total Variance: {totVariance:0.0000}");
                }

                cplex.End();
            }
            catch (ILOG.Concert.Exception e)
            {
                System.Console.WriteLine("Concert exception caught: " + e);
            }

            Console.WriteLine("Press any key to quit.");
            Console.ReadKey();
        }

        static double WeightedReturn(double[] weights, double[] returns)
        {
            return Enumerable.Zip(weights, returns, (wt, e) => wt * e).Sum();
        }

        static double PortfolioVariance(double[] weights, double[][] cov)
        {
            double variance =0 ;
            for (int i = 0; i < cov.Length; ++i)
            {
                for (int j = 0; j < cov.Length; ++j)
                {
                    variance += (cov[i][j] * weights[i] * weights[j]);
                }
            }
            return variance;
        }

        static double Covariance(double[] x, double[] y)
        {
            var xmean = x.Average();
            var ymean = y.Average();
            var n = x.Length;
            // cov = E[(X-meanX)(Y-meanY)]  *E is the "mean"
            return Enumerable.Zip(x, y, (vx, vy) => (vx - xmean) * (vy - ymean)).Sum() / (n - 1); // sample var
        }

        static double[][] CovarianceMatrix(double[][] prices)
        {
            // Each row in prices represents prices for an asset - m rows for m assets
            // Covariance will be m * m matrix
            var m = prices.Length;
            double[][] cov = new double[m][];
            for (int i = 0; i < m; i++)
            {
                cov[i] = new double[m];
                for (int j = 0; j < m; j++)
                {
                    cov[i][j] = Covariance(prices[i], prices[j]);
                }
            }
            return cov;
        }

        static double[] AssetReturns(double[][] prices)
        {
            var exp = new double[prices.Length];
            for (int i = 0; i < prices.Length; i++)
            {
                exp[i] = AnnualizedReturn(prices[i]);
            }
            return exp;
        }

        static double AnnualizedReturn(double[] dailyPrices)
        {
            var dailyReturn = new double[dailyPrices.Length - 1];
            for (int i = 0; i < dailyPrices.Length - 1; i++)
            {
                dailyReturn[i] = (dailyPrices[i + 1] - dailyPrices[i]) / dailyPrices[i];
            }
            // Assume 250 trading days per year - use average daily return
            var avgAnnual = Pow(Pow(1.0 + dailyReturn.Average(), 250.0), 1.0 / 250) - 1;

            return avgAnnual;
        }

        static void DisplayCovariance(double[][] cov)
        {
            var rows = cov.Length;
            if (rows == 0)
                return;
            Console.WriteLine("Covariance Matrix:");
            var cols = cov[0].Length;
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    Console.Write($"{cov[i][j]:0.0000}\t");
                }
                Console.WriteLine();
            }
            Console.WriteLine("".PadRight(6 * cols, '='));
        }

        static void DisplayReturns(string[] assets, double[] ret)
        {
            if (ret.Length == 0)
                return;
            Console.WriteLine("Annualized Returns");
            for (int i = 0; i < ret.Length; i++)
            {
                Console.WriteLine($"{assets[i]}\t{ret[i]:0.00}");
            }
            Console.WriteLine();

        }
    }
}
