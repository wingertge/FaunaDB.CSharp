using System;

namespace FaunaDB.LINQ.Types
{
    /// <summary>
    /// Attribute marking a property as indexed by a database index
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class IndexedAttribute : Attribute
    {
        public Type TargetType { get; }
        public string Name { get; }

        public IndexedAttribute() { }

        public IndexedAttribute(string name)
        {
            Name = name;
        }

        public IndexedAttribute(string name, Type targetType)
        {
            Name = name;
            TargetType = targetType;
        }
    }

#pragma warning disable 660,661
    /// <summary>
    /// Stub used in query building
    /// </summary>
    /// <typeparam name="T1">The type of the first composite parameter.</typeparam>
    /// <typeparam name="T2">The type of the second composite parameter.</typeparam>
    public class CompositeIndex<T1, T2> : Tuple<T1, T2>, IEquatable<ValueTuple<T1, T2>>
    {
        public CompositeIndex(T1 item1, T2 item2) : base(item1, item2) {}

        public bool Equals(ValueTuple<T1, T2> other)
        {
            var (item1, item2) = other;
            return (Item1?.Equals(item1) ?? item1 == null) && (Item2?.Equals(item2) ?? item2 == null);
        }

        public static bool operator ==(CompositeIndex<T1, T2> index, ValueTuple<T1, T2> other)
        {
            return index?.Equals(other) ?? false;
        }

        public static bool operator !=(CompositeIndex<T1, T2> index, (T1, T2) other)
        {
            return !(index == other);
        }
    }

    /// <summary>
    /// Stub used in query building
    /// </summary>
    /// <typeparam name="T1">The type of the first composite parameter.</typeparam>
    /// <typeparam name="T2">The type of the second composite parameter.</typeparam>
    /// <typeparam name="T3">The type of the third composite parameter.</typeparam>
    public class CompositeIndex<T1, T2, T3> : Tuple<T1, T2, T3>, IEquatable<ValueTuple<T1, T2, T3>>
    {
        public CompositeIndex(T1 item1, T2 item2, T3 item3) : base(item1, item2, item3) { }

        public bool Equals(ValueTuple<T1, T2, T3> other)
        {
            var (item1, item2, item3) = other;
            return (Item1?.Equals(item1) ?? item1 == null) && (Item2?.Equals(item2) ?? item2 == null) &&
                   (Item3?.Equals(item3) ?? item3 == null);
        }

        public static bool operator ==(CompositeIndex<T1, T2, T3> index, ValueTuple<T1, T2, T3> other)
        {
            return index?.Equals(other) ?? false;
        }

        public static bool operator !=(CompositeIndex<T1, T2, T3> index, ValueTuple<T1, T2, T3> other)
        {
            return !(index == other);
        }
    }

    /// <summary>
    /// Stub used in query building
    /// </summary>
    /// <typeparam name="T1">The type of the first composite parameter.</typeparam>
    /// <typeparam name="T2">The type of the second composite parameter.</typeparam>
    /// <typeparam name="T3">The type of the third composite parameter.</typeparam>
    /// <typeparam name="T4">The type of the fourth composite parameter.</typeparam>
    public class CompositeIndex<T1, T2, T3, T4> : Tuple<T1, T2, T3, T4>, IEquatable<ValueTuple<T1, T2, T3, T4>>
    {
        public CompositeIndex(T1 item1, T2 item2, T3 item3, T4 item4) : base(item1, item2, item3, item4) { }

        public bool Equals(ValueTuple<T1, T2, T3, T4> other)
        {
            var (item1, item2, item3, item4) = other;
            return (Item1?.Equals(item1) ?? item1 == null) && (Item2?.Equals(item2) ?? item2 == null) &&
                   (Item3?.Equals(item3) ?? item3 == null) && (Item4?.Equals(item4) ?? item4 == null);
        }

        public static bool operator ==(CompositeIndex<T1, T2, T3, T4> index, ValueTuple<T1, T2, T3, T4> other)
        {
            return index?.Equals(other) ?? false;
        }

        public static bool operator !=(CompositeIndex<T1, T2, T3, T4> index, ValueTuple<T1, T2, T3, T4> other)
        {
            return !(index == other);
        }
    }

    /// <summary>
    /// Stub used in query building
    /// </summary>
    /// <typeparam name="T1">The type of the first composite parameter.</typeparam>
    /// <typeparam name="T2">The type of the second composite parameter.</typeparam>
    /// <typeparam name="T3">The type of the third composite parameter.</typeparam>
    /// <typeparam name="T4">The type of the fourth composite parameter.</typeparam>
    /// <typeparam name="T5">The type of the fifth composite parameter.</typeparam>
    public class CompositeIndex<T1, T2, T3, T4, T5> : Tuple<T1, T2, T3, T4, T5>, IEquatable<ValueTuple<T1, T2, T3, T4, T5>>
    {
        public CompositeIndex(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5) : base(item1, item2, item3, item4, item5) { }

        public bool Equals(ValueTuple<T1, T2, T3, T4, T5> other)
        {
            var (item1, item2, item3, item4, item5) = other;
            return (Item1?.Equals(item1) ?? item1 == null) && (Item2?.Equals(item2) ?? item2 == null) &&
                   (Item3?.Equals(item3) ?? item3 == null) && (Item4?.Equals(item4) ?? item4 == null) &&
                   (Item5?.Equals(item5) ?? item5 == null);
        }

        public static bool operator ==(CompositeIndex<T1, T2, T3, T4, T5> index, ValueTuple<T1, T2, T3, T4, T5> other)
        {
            return index?.Equals(other) ?? false;
        }

        public static bool operator !=(CompositeIndex<T1, T2, T3, T4, T5> index, ValueTuple<T1, T2, T3, T4, T5> other)
        {
            return !(index == other);
        }
    }

    /// <summary>
    /// Stub used in query building
    /// </summary>
    /// <typeparam name="T1">The type of the first composite parameter.</typeparam>
    /// <typeparam name="T2">The type of the second composite parameter.</typeparam>
    /// <typeparam name="T3">The type of the third composite parameter.</typeparam>
    /// <typeparam name="T4">The type of the fourth composite parameter.</typeparam>
    /// <typeparam name="T5">The type of the fifth composite parameter.</typeparam>
    /// <typeparam name="T6">The type of the sixth composite parameter.</typeparam>
    public class CompositeIndex<T1, T2, T3, T4, T5, T6> : Tuple<T1, T2, T3, T4, T5, T6>, IEquatable<ValueTuple<T1, T2, T3, T4, T5, T6>>
    {
        public CompositeIndex(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6) : base(item1, item2, item3, item4, item5, item6) { }

        public bool Equals(ValueTuple<T1, T2, T3, T4, T5, T6> other)
        {
            var (item1, item2, item3, item4, item5, item6) = other;
            return (Item1?.Equals(item1) ?? item1 == null) && (Item2?.Equals(item2) ?? item2 == null) &&
                   (Item3?.Equals(item3) ?? item3 == null) && (Item4?.Equals(item4) ?? item4 == null) &&
                   (Item5?.Equals(item5) ?? item5 == null) && (Item6?.Equals(item6) ?? item6 == null);
        }

        public static bool operator ==(CompositeIndex<T1, T2, T3, T4, T5, T6> index, ValueTuple<T1, T2, T3, T4, T5, T6> other)
        {
            return index?.Equals(other) ?? false;
        }

        public static bool operator !=(CompositeIndex<T1, T2, T3, T4, T5, T6> index, ValueTuple<T1, T2, T3, T4, T5, T6> other)
        {
            return !(index == other);
        }
    }
#pragma warning restore 660, 661
}