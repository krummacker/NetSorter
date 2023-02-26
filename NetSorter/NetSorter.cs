namespace NetSorter
{
    /// <summary>
    /// A Sorter can sort Lists with comparable objects. Classes that implement
    /// this interface usually use different sort algorithms such as bubble
    /// sort, quick sort, merge sort and others.
    /// </summary>
    public interface ISorter<T> where T : IComparable
    {
        /// <summary>
        /// Sorts the specified list and returns the result. Does not change
        /// the parameter.
        /// </summary>
        /// <param name="input">the list to be sorted</param>
        /// <returns>a sorted list of all input elements</returns>
        List<T> Sort(List<T> input);
    }

    /// <summary>
    /// Sorts Lists using the standard mechanism of the .NET API (IntroSort).
    /// </summary>
    public class StandardApiSorter<T> : ISorter<T> where T : IComparable
    {
        public List<T> Sort(List<T> input)
        {
            List<T> result = new(input);
            result.Sort();
            return result;
        }
    }

    /// <summary>
    /// Sorts Lists using the bubble sort algorithm.
    /// </summary>
    public class BubbleSorter<T> : ISorter<T> where T : IComparable
    {
        public List<T> Sort(List<T> input)
        {
            List<T> result = new(input);
            for (int i = 0; i < result.Count - 1; ++i)
            {
                for (int j = 0; j < result.Count - 1 - i; ++j)
                {
                    if (result[j].CompareTo(result[j + 1]) > 0)
                    {
                        T temp = result[j];
                        result[j] = result[j + 1];
                        result[j + 1] = temp;
                    }
                }
            }
            return result;
        }
    }

    /// <summary>
    /// Abstract base class for sorters that use the quicksort algorithm. This
    /// implementation creates new list objects in each sorting step instead of
    /// removing/inserting in place.
    /// </summary>
    public abstract class AbstractCreatingQuickSorter<T> : ISorter<T> where T : IComparable
    {
        public List<T> Sort(List<T> input)
        {
            if (input.Count < 2)
            {
                // already sorted
                return input;
            }

            // Let subclasses compute the pivot index.
            List<T> copy = new(input);
            int pivotIndex = ComputePivotIndex(copy);
            T pivot = copy[pivotIndex];
            copy.RemoveAt(pivotIndex);

            // This implementation creates new lists in every step but this
            // has turned out to be quicker than using the Insert() and
            // RemoveAt() methods in List.
            List<T> smaller = new();
            List<T> bigger = new();

            List<T>.Enumerator enumerator = copy.GetEnumerator();
            while (enumerator.MoveNext())
            {
                T c = enumerator.Current;
                if (c.CompareTo(pivot) < 0)
                {
                    smaller.Add(c);
                }
                else
                {
                    bigger.Add(c);
                }
            }

            return SortSublistsAndConcatenate(smaller, pivot, bigger);
        }

        /// <summary>
        /// Can be overridden in subclasses to facilitate parallel processing.
        /// </summary>
        /// <param name="smaller">items that are smaller than the pivot</param>
        /// <param name="pivot">the pivor element</param>
        /// <param name="bigger">items that are larger than the pivot</param>
        /// <returns>the fully sorted list</returns>
        internal virtual List<T> SortSublistsAndConcatenate(List<T> smaller,
            T pivot, List<T> bigger)
        {
            List<T> first = Sort(smaller);
            List<T> last = Sort(bigger);

            List<T> result = new();
            result.AddRange(first);
            result.Add(pivot);
            result.AddRange(last);
            return result;
        }

        /// <summary>
        /// Returns the index within the specified list from which the pivot
        /// element shall be taken.
        /// </summary>
        /// <param name="input">the list to be sorted</param>
        /// <returns>the pivot element</returns>
        internal abstract int ComputePivotIndex(List<T> input);
    }

    /// <summary>
    /// Sorts Lists using the quicksort algorithm. The pivot element is the
    /// first element of the sublist to be sorted.
    /// </summary>
    public class FirstIsPivotQuickSorter<T> : AbstractCreatingQuickSorter<T> where T : IComparable
    {
        internal override int ComputePivotIndex(List<T> input)
        {
            return 0;
        }
    }

    /// <summary>
    /// Sorts Lists using the quicksort algorithm. It picks a random pivot and
    /// thus should perform better on already sorted lists.
    /// </summary>
    public class RandomPivotQuickSorter<T> : AbstractCreatingQuickSorter<T> where T : IComparable
    {
        private static readonly Random Random = new();

        internal override int ComputePivotIndex(List<T> input)
        {
            return Random.Next(input.Count);
        }
    }

    /// <summary>
    /// Sorts Lists using the quicksort algorithm. The pivot is the median of
    /// the first, the middle and the last element of the sublist to be sorted.
    /// </summary>
    public class MedianPivotQuickSorter<T> : AbstractCreatingQuickSorter<T> where T : IComparable
    {
        internal override int ComputePivotIndex(List<T> input)
        {
            int startIndex = 0;
            int middleIndex = input.Count / 2;
            int endIndex = input.Count - 1;

            T a = input[startIndex];
            T b = input[middleIndex];
            T c = input[endIndex];

            if (a.CompareTo(b) > 0)
            {
                if (b.CompareTo(c) > 0)
                {
                    return middleIndex;
                }
                else
                {
                    return a.CompareTo(c) > 0 ? endIndex : startIndex;
                }
            }
            else
            {
                if (b.CompareTo(c) < 0)
                {
                    return middleIndex;
                }
                else
                {
                    return a.CompareTo(c) < 0 ? endIndex : startIndex;
                }
            }
        }
    }

    /// <summary>
    /// A method object for encapsulating a call to an ISorter's Sort method.
    /// </summary>
    internal class SortExecutor<T> where T : IComparable
    {
        private ISorter<T> Sorter { get; }
        private List<T> Input { get; }
        internal List<T> Output { get; set; }

        internal SortExecutor(ISorter<T> sorter, List<T> input)
        {
            Sorter = sorter;
            Input = input;
        }

        internal void Execute()
        {
            Output = Sorter.Sort(Input);
        }
    }

    /// <summary>
    /// Sorts lists using quicksort with a pivot element determined as a median
    /// of three elements. Uses very simple threading to facilitate parallel
    /// processing.
    ///
    /// Works only on lists up to 5,000 elements on a M1 MacBook Pro and is
    /// always slower than other ISorter implementations.
    /// </summary>
    public class ThreadQuickSorter<T> : MedianPivotQuickSorter<T> where T : IComparable
    {
        /// <summary>
        /// Overrides the virtual base class implementation by sorting the
        /// smaller and the bigger list each one in their own thread.
        /// </summary>
        internal override List<T> SortSublistsAndConcatenate(List<T> smaller,
            T pivot, List<T> bigger)
        {
            SortExecutor<T> runner1 = new(this, smaller);
            SortExecutor<T> runner2 = new(this, bigger);

            Thread thread1 = new(runner1.Execute);
            Thread thread2 = new(runner2.Execute);

            thread1.Start();
            thread2.Start();

            thread1.Join();
            thread2.Join();

            List<T> first = runner1.Output;
            List<T> last = runner2.Output;

            List<T> result = new();
            result.AddRange(first);
            result.Add(pivot);
            result.AddRange(last);
            return result;
        }
    }

    /// <summary>
    /// Sorts lists using quicksort with a pivot element determined as a median
    /// of three elements. Uses tasks to facilitate parallel processing.
    ///
    /// This algorithm is always slower than a single-threaded median-based
    /// quicksorter, presumably because of the overhead related to task
    /// creation.
    /// </summary>
    public class TaskQuickSorter<T> : MedianPivotQuickSorter<T> where T : IComparable
    {
        internal override List<T> SortSublistsAndConcatenate(List<T> smaller,
            T pivot, List<T> bigger)
        {
            SortExecutor<T> runner1 = new(this, smaller);
            SortExecutor<T> runner2 = new(this, bigger);

            Task task1 = Task.Run(runner1.Execute);
            Task task2 = Task.Run(runner2.Execute);

            task1.Wait();
            task2.Wait();

            List<T> first = runner1.Output;
            List<T> last = runner2.Output;

            List<T> result = new();
            result.AddRange(first);
            result.Add(pivot);
            result.AddRange(last);
            return result;
        }
    }

    /// <summary>
    /// Sorts Lists using the quick sort algorithm. This implementation does
    /// not create any new objects but removes/inserts in place. The pivot
    /// element is the first element of the sublist to be sorted.
    /// </summary>
    public class RemovingInsertingQuickSorter<T> : ISorter<T> where T : IComparable
    {
        public List<T> Sort(List<T> input)
        {
            List<T> copy = new(input);
            Sort(copy, 0, input.Count - 1);
            return copy;
        }

        private void Sort(List<T> list, int start, int end)
        {
            if (start >= end)
            {
                // list is empty or has only 1 element = already sorted
                return;
            }

            int pivotIndex = start;
            T pivot = list[pivotIndex];
            for (int i = start + 1; i <= end; ++i)
            {
                T current = list[i];
                if (current.CompareTo(pivot) < 0)
                {
                    // these are slow
                    list.RemoveAt(i);
                    list.Insert(start, current);

                    ++pivotIndex;
                }
            }

            Sort(list, start, pivotIndex - 1);
            Sort(list, pivotIndex + 1, end);
        }
    }
}
