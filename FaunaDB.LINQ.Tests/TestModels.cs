using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FaunaDB.Driver.Errors;
using FaunaDB.LINQ.Modeling;
using FaunaDB.LINQ.Types;
using Newtonsoft.Json;

namespace FaunaDB.LINQ.Tests
{
    public class ReferenceModel
    {
        [Key]
        public string Id { get; set; }
        [Indexed("index_1")]
        public string Indexed1 { get; set; }
        [Indexed("index_2")]
        public string Indexed2 { get; set; }
        [Timestamp]
        public DateTime TimeStamp { get; set; }

        [Indexed("composite_index")]
        public CompositeIndex<string, string> CompositeIndex { get; set; }

        protected bool Equals(ReferenceModel other)
        {
            return string.Equals(Indexed1, other.Indexed1) && string.Equals(Indexed2, other.Indexed2);
        }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((ReferenceModel) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Indexed1 != null ? Indexed1.GetHashCode() : 0) * 397) ^ (Indexed2 != null ? Indexed2.GetHashCode() : 0);
            }
        }
    }

    public class ReferenceModelMapping : FluentTypeConfiguration<ReferenceModel>
    {
        public ReferenceModelMapping()
        {
            this.HasKey(a => a.Id)
                .HasIndex(a => a.Indexed1, "index_1")
                .HasIndex(a => a.Indexed2, "index_2")
                .HasTimestamp(a => a.TimeStamp)
                .HasCompositeIndex(a => a.CompositeIndex, "composite_index");
        }
    }

    public class PrimitivesReferenceModel
    {
        [Key]
        public string Id { get; set; }
        public string StringVal { get; set; }
        public int IntVal { get; set; }
        public bool BooleanVal { get; set; }
        public byte ByteVal { get; set; }
        public long LongVal { get; set; }
        public float FloatVal { get; set; }
        public double DoubleVal { get; set; }
        public DateTime DateTimeVal { get; set; }
        public short ShortVal { get; set; }
        public ushort UShortVal { get; set; }
        public uint UIntVal { get; set; }
        public ulong ULongVal { get; set; }
        public sbyte SByteVal { get; set; }
        public char CharVal { get; set; }

        protected bool Equals(PrimitivesReferenceModel other)
        {
            return string.Equals(StringVal, other.StringVal) && IntVal == other.IntVal &&
                   BooleanVal == other.BooleanVal && ByteVal == other.ByteVal && LongVal == other.LongVal &&
                   FloatVal.Equals(other.FloatVal) && DoubleVal.Equals(other.DoubleVal) &&
                   DateTimeVal.Equals(other.DateTimeVal) && ShortVal == other.ShortVal &&
                   UShortVal == other.UShortVal && UIntVal == other.UIntVal && ULongVal == other.ULongVal &&
                   SByteVal == other.SByteVal && CharVal == other.CharVal;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((PrimitivesReferenceModel) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (StringVal != null ? StringVal.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ IntVal;
                hashCode = (hashCode * 397) ^ BooleanVal.GetHashCode();
                hashCode = (hashCode * 397) ^ ByteVal.GetHashCode();
                hashCode = (hashCode * 397) ^ LongVal.GetHashCode();
                hashCode = (hashCode * 397) ^ FloatVal.GetHashCode();
                hashCode = (hashCode * 397) ^ DoubleVal.GetHashCode();
                hashCode = (hashCode * 397) ^ DateTimeVal.GetHashCode();
                hashCode = (hashCode * 397) ^ ShortVal.GetHashCode();
                hashCode = (hashCode * 397) ^ UShortVal.GetHashCode();
                hashCode = (hashCode * 397) ^ (int) UIntVal;
                hashCode = (hashCode * 397) ^ ULongVal.GetHashCode();
                hashCode = (hashCode * 397) ^ SByteVal.GetHashCode();
                hashCode = (hashCode * 397) ^ CharVal.GetHashCode();
                return hashCode;
            }
        }
    }

    public class PrimitivesReferenceModelMapping : FluentTypeConfiguration<PrimitivesReferenceModel>
    {
        public PrimitivesReferenceModelMapping()
        {
            HasKey(a => a.Id);
        }
    }

    public class ValueTypesReferenceModel
    {
        [Key]
        public string Id { get; set; }

        public ValueModel ValueModel { get; set; }
        public List<ValueModel> ValueModels1 { get; set; }
        public ValueModel[] ValueModels2 { get; set; }
    }

    public class ValueTypesReferenceModelMapping : FluentTypeConfiguration<ValueTypesReferenceModel>
    {
        public ValueTypesReferenceModelMapping()
        {
            HasKey(a => a.Id);
        }
    }

    public class ReferenceTypesReferenceModel
    {
        [Key]
        public string Id { get; set; }

        [Reference]
        public ReferenceModel ReferenceModel { get; set; }
        [Reference]
        public List<ReferenceModel> ReferenceModels1 { get; set; }
        [Reference]
        public ReferenceModel[] ReferenceModels2 { get; set; }

        protected bool Equals(ReferenceTypesReferenceModel other)
        {
            return string.Equals(Id, other.Id) && Equals(ReferenceModel, other.ReferenceModel) &&
                   (ReferenceModels1?.SequenceEqual(other.ReferenceModels1) ?? other.ReferenceModels1 == null) && 
                   (ReferenceModels2?.SequenceEqual(other.ReferenceModels2) ?? other.ReferenceModels2 == null);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ReferenceTypesReferenceModel) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Id != null ? Id.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (ReferenceModel != null ? ReferenceModel.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (ReferenceModels1 != null ? ReferenceModels1.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (ReferenceModels2 != null ? ReferenceModels2.GetHashCode() : 0);
                return hashCode;
            }
        }
    }

    public class ReferenceTypesReferenceModelMapping : FluentTypeConfiguration<ReferenceTypesReferenceModel>
    {
        public ReferenceTypesReferenceModelMapping()
        {
            this.HasKey(a => a.Id)
                .HasReference(a => a.ReferenceModel)
                .HasReference(a => a.ReferenceModels1)
                .HasReference(a => a.ReferenceModels2);
        }
    }

    public class ValueModel
    {
        public string Value1 { get; set; }
        public string Value2 { get; set; }
    }

    public class NamedPropertyModel
    {
        [Key]
        public string Id { get; set; }
        
        [JsonProperty("not_a_test_string")]
        public string TestString { get; set; }

        protected bool Equals(NamedPropertyModel other)
        {
            return string.Equals(Id, other.Id) && string.Equals(TestString, other.TestString);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((NamedPropertyModel) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Id != null ? Id.GetHashCode() : 0) * 397) ^ (TestString != null ? TestString.GetHashCode() : 0);
            }
        }
    }

    public class NamedPropertyModelMapping : FluentTypeConfiguration<NamedPropertyModel>
    {
        public NamedPropertyModelMapping()
        {
            this.HasKey(a => a.Id)
                .HasName(a => a.TestString, "not_a_test_string");
        }
    }

    public class UnmappedModel
    {
        [Key]
        public string Id { get; set; }
        
        [JsonProperty("not_a_test_string")]
        public string TestString { get; set; }
    }

    public class ModelWithUnmappedReference
    {
        [Key]
        public string Id { get; set; }
        
        [Reference]
        public UnmappedModel Model { get; set; }
    }

    public class ModelWithUnmappedReferenceMapping : FluentTypeConfiguration<ModelWithUnmappedReference>
    {
        public ModelWithUnmappedReferenceMapping()
        {
            this.HasKey(a => a.Id)
                .HasReference(a => a.Model);
        }
    }

    public class SupportedCustomCollectionModel
    {
        [Key]
        public string Id { get; set; }
        public SupportedCustomCollection<string> CustomCollection { get; set; }

        protected bool Equals(SupportedCustomCollectionModel other)
        {
            return string.Equals(Id, other.Id) && (CustomCollection?.SequenceEqual(other.CustomCollection) ?? other.CustomCollection == null);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((SupportedCustomCollectionModel) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Id != null ? Id.GetHashCode() : 0) * 397) ^ (CustomCollection != null ? CustomCollection.GetHashCode() : 0);
            }
        }
    }

    public class SupportedCustomCollectionModelMapping : FluentTypeConfiguration<SupportedCustomCollectionModel>
    {
        public SupportedCustomCollectionModelMapping()
        {
            this.HasKey(a => a.Id);
        }
    }
    
    public class SupportedCustomCollectionModel2
    {
        [Key]
        public string Id { get; set; }
        public SupportedCustomCollection<DateTime> CustomCollection { get; set; }
        public DateTime[] DatesArray { get; set; }
        
        protected bool Equals(SupportedCustomCollectionModel2 other)
        {
            return string.Equals(Id, other.Id) && (CustomCollection?.SequenceEqual(other.CustomCollection) ?? other.CustomCollection == null);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((SupportedCustomCollectionModel2) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Id != null ? Id.GetHashCode() : 0) * 397) ^ (CustomCollection != null ? CustomCollection.GetHashCode() : 0);
            }
        }
    }

    public class SupportedCustomCollectionModelMapping2 : FluentTypeConfiguration<SupportedCustomCollectionModel2>
    {
        public SupportedCustomCollectionModelMapping2()
        {
            this.HasKey(a => a.Id);
        }
    }

    public class UnsupportedCustomCollectionModel
    {
        [Key]
        public string Id { get; set; }
        public UnsupportedCustomCollection<string> CustomCollection { get; set; }
    }

    public class UnsupportedCustomCollectionModelMapping : FluentTypeConfiguration<UnsupportedCustomCollectionModel>
    {
        public UnsupportedCustomCollectionModelMapping()
        {
            this.HasKey(a => a.Id);
        }
    }
    
    public class UnsupportedCustomCollectionModel2
    {
        [Key]
        public string Id { get; set; }
        public UnsupportedCustomCollection<ReferenceModel> CustomCollection { get; set; }
    }

    public class UnsupportedCustomCollectionModelMapping2 : FluentTypeConfiguration<UnsupportedCustomCollectionModel2>
    {
        public UnsupportedCustomCollectionModelMapping2()
        {
            this.HasKey(a => a.Id);
        }
    }
    
    public class UnsupportedCustomCollectionModel3
    {
        [Key]
        public string Id { get; set; }
        public UnsupportedCustomCollection<DateTime> CustomCollection { get; set; }
    }

    public class UnsupportedCustomCollectionModelMapping3 : FluentTypeConfiguration<UnsupportedCustomCollectionModel3>
    {
        public UnsupportedCustomCollectionModelMapping3()
        {
            this.HasKey(a => a.Id);
        }
    }

    public class SupportedCustomCollection<T> : IEnumerable<T>
    {
        internal List<T> WrappedData { get; set; }
        
        public SupportedCustomCollection(IEnumerable<T> collection)
        {
            WrappedData = new List<T>(collection);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return WrappedData.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class UnsupportedCustomCollection<T> : IEnumerable<T>
    {
        public IEnumerator<T> GetEnumerator()
        {
            throw new UnsupportedMethodException("GetEnumerator");
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class IncludeModel
    {
        [Key]
        public string Id { get; set; }
        
        [Indexed("index_1")]
        public string Indexed1 { get; set; }
        
        [Reference]
        public IncludeModel Reference { get; set; }
        
        [Reference]
        public List<IncludeModel> References { get; set; }
    }

    public class IncludeModelMapping : FluentTypeConfiguration<IncludeModel>
    {
        public IncludeModelMapping()
        {
            this.HasKey(a => a.Id)
                .HasIndex(a => a.Indexed1, "index_1")
                .HasReference(a => a.Reference)
                .HasReference(a => a.References);
        }
    }
}