using System.Diagnostics;

namespace Sorter
{
    /// <summary>
    /// Helper object for tracking an ISorter and the associated test
    /// configuration and results.
    /// </summary>
    internal class SorterInfo<T> where T : IComparable
    {
        /// <summary>
        /// the ISorter implementation to be tested
        /// </summary>
        internal ISorter<T> Sorter { get; }

        /// <summary>
        /// the number of elements that a list to be sorted may have
        /// </summary>
        internal int ListCount { get; }

        /// <summary>
        /// A list of timings that have been measured
        /// </summary>
        private readonly List<long> Timings = new();

        /// <summary>
        /// Creates a new SorterInfo.
        /// </summary>
        /// <param name="sorter">the ISorter implementation to be tested</param>
        /// <param name="listCount">the number of elements that a list to be
        /// sorted may have; specify lower numbers for less performant
        /// implementations</param>
        internal SorterInfo(ISorter<T> sorter, int listCount)
        {
            Sorter = sorter;
            ListCount = listCount;
        }

        /// <summary>
        /// Adds the result of one test run
        /// </summary>
        /// <param name="timing">the time needed in ms</param>
        internal void AddTiming(long timing)
        {
            Timings.Add(timing);
        }

        /// <summary>
        /// Returns the average of all timings measured so far
        /// </summary>
        internal long ComputeAverageTiming()
        {
            return (long)Timings.Average();
        }

        /// <summary>
        /// Clears out the test results.
        /// </summary>
        internal void ClearTimings()
        {
            Timings.Clear();
        }
    }

    /// <summary>
    /// The executable to be called in this solution.
    /// </summary>
    internal class Program
    {
        /// <summary>
        /// The source of randomness for all tests. 
        /// </summary>
        private static readonly Random Random = new();

        /// <summary>
        /// Creates a list of random int values. This method is meant to
        /// provide test data.
        /// </summary>
        /// <param name="length">the size of the list to be created</param>
        /// <returns>the list with the random values</returns>
        private static List<int> CreateListWithRandomInts(int length)
        {
            List<int> result = new(length);
            for (int i = 0; i < length; ++i)
            {
                int r = Random.Next();
                result.Add(r);
            }
            return result;
        }

        /// <summary>
        /// Creates a list of ordered int values. This method is meant to
        /// provide test data.
        /// </summary>
        /// <param name="length">the size of the list to be created</param>
        /// <returns>the list with the ordered values</returns>
        private static List<int> CreateListWithOrderedInts(int length)
        {
            List<int> result = new(length);
            for (int i = 0; i < length; ++i)
            {
                result.Add(i);
            }
            return result;
        }

        /// <summary>
        /// Randomly shuffle the ISorter implementations so that they don't
        /// always run in the same sequence.
        /// </summary>
        private static List<SorterInfo<int>> Shuffle(List<SorterInfo<int>> input)
        {
            List<SorterInfo<int>> copy = new(input);
            int n = copy.Count;
            while (n > 1)
            {
                n--;
                int k = Random.Next(n + 1);
                SorterInfo<int> value = copy[k];
                copy[k] = copy[n];
                copy[n] = value;
            }
            return copy;
        }

        /// <summary>
        /// Runs the sort test and adds the elapsed time.
        /// </summary>
        private static void TestAndAddTiming(SorterInfo<int> sorterInfo,
            List<int> data)
        {
            Stopwatch stopwatch = new();
            stopwatch.Start();
            sorterInfo.Sorter.Sort(data);
            sorterInfo.AddTiming(stopwatch.ElapsedMilliseconds);
        }

        /// <summary>
        /// Displays the results of the test run on the console.
        /// </summary>
        private static void WriteResults(List<SorterInfo<int>> sorterInfos,
            string type)
        {
            foreach (SorterInfo<int> sorterInfo in sorterInfos)
            {
                Console.WriteLine("Time to sort " + sorterInfo.ListCount
                    + " " + type + " ints with "
                    + sorterInfo.Sorter.GetType().Name + " : "
                    + sorterInfo.ComputeAverageTiming() + " ms");
            }
        }

        /// <summary>
        /// Entry point of the program.
        /// </summary>
        /// <param name="args">comman line arguments, not used</param>
        internal static void Main()
        {
            List<SorterInfo<int>> sorterInfos = new()
            {
                // slow
                new SorterInfo<int>(new ThreadQuickSorter<int>(), 1000),

                // medium
                new SorterInfo<int>(new BubbleSorter<int>(), 10000),
                new SorterInfo<int>(new RemovingInsertingQuickSorter<int>(), 10000),
                new SorterInfo<int>(new FirstIsPivotQuickSorter<int>(), 10000),

                // fast
                new SorterInfo<int>(new RandomPivotQuickSorter<int>(), 1000000),
                new SorterInfo<int>(new MedianPivotQuickSorter<int>(), 1000000),
                new SorterInfo<int>(new TaskQuickSorter<int>(), 1000000),
                new SorterInfo<int>(new StandardApiSorter<int>(), 1000000)
            };

            Console.WriteLine("Burning in the VM...");
            for (int i = 0; i < 3; ++i)
            {
                foreach (SorterInfo<int> sorterInfo in sorterInfos)
                {
                    List<int> data = CreateListWithRandomInts(sorterInfo.ListCount);
                    sorterInfo.Sorter.Sort(data);
                }
            }
            Console.WriteLine();

            Console.WriteLine("Running tests with random data...");
            for (int i = 0; i < 10; ++i)
            {
                List<SorterInfo<int>> copy = Shuffle(sorterInfos);
                foreach (SorterInfo<int> sorterInfo in copy)
                {
                    List<int> data = CreateListWithRandomInts(
                        sorterInfo.ListCount);
                    TestAndAddTiming(sorterInfo, data);
                }
            }
            WriteResults(sorterInfos, "random");
            Console.WriteLine();

            // clear out the results of the previous test
            foreach (SorterInfo<int> sorterInfo in sorterInfos)
            {
                sorterInfo.ClearTimings();
            }
            
            Console.WriteLine("Running tests with ordered data...");
            for (int i = 0; i < 10; ++i)
            {
                List<SorterInfo<int>> copy = Shuffle(sorterInfos);
                foreach (SorterInfo<int> sorterInfo in copy)
                {
                    List<int> data = CreateListWithOrderedInts(
                        sorterInfo.ListCount);
                    TestAndAddTiming(sorterInfo, data);
                }
            }
            WriteResults(sorterInfos, "ordered");
            Console.WriteLine();

            Console.WriteLine("Finished");
        }
    }
}
