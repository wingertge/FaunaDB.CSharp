namespace FaunaDB.LINQ.Modeling
{
    public class DbPropertyInfo
    {
        public string Name { get; set; }
        public DbPropertyType Type { get; set; }
    }

    public class IndexPropertyInfo : DbPropertyInfo
    {
        public string IndexName { get; set; }
    }

    public enum DbPropertyType
    {
        Primitive,
        ValueType,
        Reference,
        PrimitiveIndex,
        CompositeIndex,
        Key,
        Timestamp
    }
}