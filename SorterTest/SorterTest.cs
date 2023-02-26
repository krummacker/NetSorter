using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sorter;

namespace SorterTest
{
    /// <summary>
    /// Helper class for formatting the contents of a list to a comma-separated
    /// string.
    /// </summary>
    /// <typeparam name="T">the type of elements in the list</typeparam>
    internal static class ListToStringFormatter<T> where T : IConvertible
    {
        internal static string Format(List<T> list)
        {
            string result = "";
            for (int j = 0; j < list.Count - 1; ++j)
            {
                result += "'" + list[j] + "'";
                result += ", ";
            }
            result += "'" + list[^1] + "'";
            return result;
        }
    }

    /// <summary>
    /// Central place where to define which different sort algorithms we
    /// support.
    /// </summary>
    /// <typeparam name="T">the type that shall be sorted</typeparam>
    internal static class SorterCreator<T> where T : IComparable
    {
        internal static List<ISorter<T>> CreateAll()
        {
            return new()
            {
                new StandardApiSorter<T>(),
                new BubbleSorter<T>(),
                new FirstIsPivotQuickSorter<T>(),
                new RandomPivotQuickSorter<T>(),
                new MedianPivotQuickSorter<T>(),
                new ThreadQuickSorter<T>(),
                new TaskQuickSorter<T>(),
                new RemovingInsertingQuickSorter<T>()
            };
        }
    }

    [TestClass]
    public class SorterTest
    {
        [TestMethod]
        public void Sorters_With5Strings_SortCorrectly()
        {
            // Arrange
            List<string> input = new() { "bb", "cd", "aa", "cc", "ab" };
            List<string> expected = new() { "aa", "ab", "bb", "cc", "cd" };
            List<ISorter<string>> sorters = SorterCreator<string>.CreateAll();

            foreach (ISorter<string> sorter in sorters)
            {
                // Act
                List<string> actual = sorter.Sort(input);

                // Assert
                bool equal = expected.SequenceEqual(actual);
                Assert.IsTrue(equal, sorter.GetType().Name
                    + "did not sort correctly: "
                    + ListToStringFormatter<string>.Format(actual));
            }
        }

        [TestMethod]
        public void Sorters_With5Longs_SortCorrectly()
        {
            // Arrange
            List<long> input = new() { 3, 4, 2, 1, 5 };
            List<long> expected = new() { 1, 2, 3, 4, 5 };
            List<ISorter<long>> sorters = SorterCreator<long>.CreateAll();

            foreach (ISorter<long> sorter in sorters)
            {
                // Act
                List<long> actual = sorter.Sort(input);

                // Assert
                bool equal = expected.SequenceEqual(actual);
                Assert.IsTrue(equal, sorter.GetType().Name
                    + "did not sort correctly: "
                    + ListToStringFormatter<long>.Format(actual));
            }
        }

        [TestMethod]
        public void Sorters_With5Chars_SortCorrectly()
        {
            // Arrange
            List<char> input = new() { 'g', 'i', 'c', 'e', 'a' };
            List<char> expected = new() { 'a', 'c', 'e', 'g', 'i' };
            List<ISorter<char>> sorters = SorterCreator<char>.CreateAll();

            foreach (ISorter<char> sorter in sorters)
            {
                // Act
                List<char> actual = sorter.Sort(input);

                // Assert
                bool equal = expected.SequenceEqual(actual);
                Assert.IsTrue(equal, sorter.GetType().Name
                    + "did not sort correctly: "
                    + ListToStringFormatter<char>.Format(actual));
            }
        }

        [TestMethod]
        public void Sorters_WithEmptyList_ReturnEmptyList()
        {
            // Arrange
            List<string> input = new();
            List<ISorter<string>> sorters = SorterCreator<string>.CreateAll();

            foreach (ISorter<string> sorter in sorters)
            {
                // Act
                List<string> actual = sorter.Sort(input);

                // Assert
                Assert.AreEqual(actual.Count, 0, sorter.GetType().Name
                    + "did not return empty list");
            }
        }

        [TestMethod]
        public void Sorters_With1Element_Return1Element()
        {
            // Arrange
            List<int> input = new() { 42 };
            List<int> expected = new() { 42 };
            List<ISorter<int>> sorters = SorterCreator<int>.CreateAll();

            foreach (ISorter<int> sorter in sorters)
            {
                // Act
                List<int> actual = sorter.Sort(input);

                // Assert
                bool equal = expected.SequenceEqual(actual);
                Assert.IsTrue(equal, sorter.GetType().Name
                    + "did not sort correctly: "
                    + ListToStringFormatter<int>.Format(actual));
            }
        }

        [TestMethod]
        public void Sorters_WithMultipleEqualElements_SortCorrectly()
        {
            // Arrange
            List<int> input = new() { 42, 1927345, 42, 42, 1927345 };
            List<int> expected = new() { 42, 42, 42, 1927345, 1927345 };
            List<ISorter<int>> sorters = SorterCreator<int>.CreateAll();

            foreach (ISorter<int> sorter in sorters)
            {
                // Act
                List<int> actual = sorter.Sort(input);

                // Assert
                bool equal = expected.SequenceEqual(actual);
                Assert.IsTrue(equal, sorter.GetType().Name
                    + "did not sort correctly: "
                    + ListToStringFormatter<int>.Format(actual));
            }
        }
    }
}
