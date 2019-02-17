using System;
using System.Collections.Generic;
using Newtonsoft.Json;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable MemberHidesStaticFromOuterClass

namespace FaunaDB.Driver
{
    public class Expr { }
    public static class QueryModel
    {
        public static Expr Obj(object value)
        {
            return new ObjO(value);
        }

        public static Expr Obj(string name1, object value1)
        {
            return new ObjO(new Dictionary<string, object> {{name1, value1}});
        }

        public static Expr Obj(string name1, object value1, string name2, object value2)
        {
            return new ObjO(new Dictionary<string, object>
            {
                {name1, value1},
                {name2, value2}
            });
        }
        
        public static Expr Obj(string name1, object value1, string name2, object value2, string name3, object value3)
        {
            return new ObjO(new Dictionary<string, object>
            {
                {name1, value1},
                {name2, value2},
                {name3, value3}
            });
        }

        [JsonConverter(typeof(ObjectSerializer))]
        public class ObjO : Expr
        {
            [JsonProperty("object")]
            public object O { get; set; }

            public ObjO(object @object)
            {
                O = @object;
            }
        }

        public static Expr Ref(object @ref)
        {
            return new RefO(@ref);
        }

        public class RefO : Expr
        {
            [JsonProperty("@ref")]
            public object Ref { get; set; }

            public RefO(object @ref)
            {
                Ref = @ref;
            }
        }

        public static Expr Let(object bindings, object @in)
        {
            return new LetO(bindings, @in);
        }

        public class LetO : Expr
        {
            public object Let { get; set; }
            public object In { get; set; }

            public LetO(object let, object @in)
            {
                Let = @let;
                In = @in;
            }
        }

        public static Expr Var(object varName)
        {
            return new VarO(varName);
        }

        public class VarO : Expr
        {
            public object Var { get; set; }

            public VarO(object var)
            {
                Var = var;
            }
        }

        public static Expr If(object condition, object trueExpr, object falseExpr)
        {
            return new IfO(condition, trueExpr, falseExpr);
        }

        public class IfO : Expr
        {
            public object If { get; set; }
            public object Then { get; set; }
            public object Else { get; set; }

            public IfO(object @if, object then, object @else)
            {
                If = @if;
                Then = then;
                Else = @else;
            }
        }

        public static Expr Do(params Expr[] expressions)
        {
            return new DoO(expressions);
        }

        public class DoO : Expr
        {
            public Expr[] Do { get; set; }

            public DoO(Expr[] @do)
            {
                Do = @do;
            }
        }

        public static Expr Lambda(object param, object body)
        {
            return new LambdaO(param, body);
        }

        public class LambdaO : Expr
        {
            public object Lambda { get; set; }
            public object Expr { get; set; }

            public LambdaO(object lambda, object expr)
            {
                Lambda = lambda;
                Expr = expr;
            }
        }

        public static Expr Lambda(object[] @params, object body)
        {
            return new LambdaO2(@params, body);
        }

        public class LambdaO2 : Expr
        {
            public object[] Lambda { get; set; }
            public object Expr { get; set; }

            public LambdaO2(object[] lambda, object expr)
            {
                Lambda = lambda;
                Expr = expr;
            }
        }

        public static Expr Call(object function, params object[] arguments)
        {
            return new CallO(function, arguments);
        }

        public class CallO : Expr
        {
            public object Call { get; set; }
            public object[] Arguments { get; set; }

            public CallO(object call, object[] arguments)
            {
                Call = call;
                Arguments = arguments;
            }
        }

        public static Expr At(object timeStamp, object expr)
        {
            return new AtO(timeStamp, expr);
        }

        public class AtO : Expr
        {
            public object At { get; set; }
            public object Expr { get; set; }

            public AtO(object at, object expr)
            {
                At = at;
                Expr = expr;
            }
        }

        public static Expr Map(object from, object lambda)
        {
            return new MapO(lambda, from);
        }

        public class MapO : Expr
        {
            public object Map { get; set; }
            public object Collection { get; set; }

            public MapO(object map, object collection)
            {
                Map = map;
                Collection = collection;
            }
        }

        public static Expr Foreach(object lambda, object collection)
        {
            return new ForeachO(lambda, collection);
        }

        public class ForeachO : Expr
        {
            public object Foreach { get; set; }
            public object Collection { get; set; }

            public ForeachO(object @foreach, object collection)
            {
                Foreach = @foreach;
                Collection = collection;
            }
        }

        public static Expr Filter(object collection, object lambda)
        {
            return new FilterO(lambda, collection);
        }

        public class FilterO : Expr
        {
            public object Filter { get; set; }
            public object Collection { get; set; }

            public FilterO(object filter, object collection)
            {
                Filter = filter;
                Collection = collection;
            }
        }

        public static Expr Take(object num, object collection)
        {
            return new TakeO(num, collection);
        }

        public class TakeO : Expr
        {
            public object Take { get; set; }
            public object Collection { get; set; }

            public TakeO(object take, object collection)
            {
                Take = take;
                Collection = collection;
            }
        }

        public static Expr Drop(object num, object collection)
        {
            return new DropO(num, collection);
        }

        public class DropO : Expr
        {
            public object Drop { get; set; }
            public object Collection { get; set; }

            public DropO(object drop, object collection)
            {
                Drop = drop;
                Collection = collection;
            }
        }

        public static Expr Prepend(object elems, object array)
        {
            return new PrependO(elems, array);
        }

        public class PrependO : Expr
        {
            public object Prepend { get; set; }
            public object Array { get; set; }

            public PrependO(object prepend, object array)
            {
                Prepend = prepend;
                Array = array;
            }
        }

        public static Expr Append(object elems, object array)
        {
            return new AppendO(elems, array);
        }

        public class AppendO : Expr
        {
            public object Append { get; set; }
            public object Array { get; set; }

            public AppendO(object append, object array)
            {
                Append = append;
                Array = array;
            }
        }

        public static Expr Get(object @ref, object ts = null)
        {
            return new GetO(@ref, ts);
        }

        public class GetO : Expr
        {
            public object Get { get; set; }
            public object Ts { get; set; }

            public GetO(object get, object ts)
            {
                Get = get;
                Ts = ts;
            }
        }

        public static Expr Paginate(object set, object ts = null, object after = null, object before = null, object size = null, object events = null, object sources = null)
        {
            return new PaginateO(set, ts, after, before, size, events, sources);
        }

        public class PaginateO : Expr
        {
            public object Paginate { get; set; }
            public object Ts { get; set; }
            public object After { get; set; }
            public object Before { get; set; }
            public object Size { get; set; }
            public object Events { get; set; }
            public object Sources { get; set; }


            public PaginateO(object paginate, object ts, object after, object before, object size, object events, object sources)
            {
                Paginate = paginate;
                Ts = ts;
                After = after;
                Before = before;
                Size = size;
                Events = events;
                Sources = sources;
            }
        }

        public static Expr Exists(object @ref, object ts = null)
        {
            return new ExistO(@ref, ts);
        }

        public class ExistO : Expr
        {
            public object Exists { get; set; }
            public object Ts { get; set; }

            public ExistO(object exists, object ts)
            {
                Exists = exists;
                Ts = ts;
            }
        }

        public static Expr KeyFromSecret(object secret)
        {
            return new KeyFromSecretO(secret);
        }

        public class KeyFromSecretO : Expr
        {
            [JsonProperty("key_from_secret")]
            public object Secret { get; set; }

            public KeyFromSecretO(object secret)
            {
                Secret = secret;
            }
        }

        public static Expr Create(object classRef, object @params)
        {
            return new CreateO(classRef, @params);
        }

        public class CreateO : Expr
        {
            public object Create { get; set; }
            public object Params { get; set; }

            public CreateO(object create, object @params)
            {
                Create = create;
                Params = @params;
            }
        }

        public static Expr Update(object @ref, object @params)
        {
            return new UpdateO(@ref, @params);
        }

        public class UpdateO : Expr
        {
            public object Update { get; set; }
            public object Params { get; set; }

            public UpdateO(object update, object @params)
            {
                Update = update;
                Params = @params;
            }
        }

        public static Expr Replace(object @ref, object @params)
        {
            return new ReplaceO(@ref, @params);
        }

        public class ReplaceO : Expr
        {
            public object Replace { get; set; }
            public object Params { get; set; }

            public ReplaceO(object replace, object @params)
            {
                Replace = replace;
                Params = @params;
            }
        }

        public static Expr Delete(object @ref)
        {
            return new DeleteO(@ref);
        }

        public class DeleteO : Expr
        {
            public object Delete { get; set; }

            public DeleteO(object delete)
            {
                Delete = delete;
            }
        }

        public static Expr Insert(object @ref, object ts, object action, object @params)
        {
            return new InsertO(@ref, ts, action, @params);
        }

        public class InsertO : Expr
        {
            public object Insert { get; set; }
            public object Ts { get; set; }
            public object Action { get; set; }
            public object Params { get; set; }

            public InsertO(object insert, object ts, object action, object @params)
            {
                Insert = insert;
                Ts = ts;
                Action = action;
                Params = @params;
            }
        }

        public static Expr Remove(object @ref, object ts, object action)
        {
            return new RemoveO(@ref, ts, action);
        }

        public class RemoveO : Expr
        {
            public object Remove { get; set; }
            public object Ts { get; set; }
            public object Action { get; set; }

            public RemoveO(object remove, object ts, object action)
            {
                Remove = remove;
                Ts = ts;
                Action = action;
            }
        }

        public static Expr CreateKey(object @params)
        {
            return new CreateKeyO(@params);
        }

        public class CreateKeyO : Expr
        {
            [JsonProperty("create_key")]
            public object CreateKey { get; set; }

            public CreateKeyO(object createKey)
            {
                CreateKey = createKey;
            }
        }

        public static Expr CreateDatabase(object @params)
        {
            return new CreateDatabaseO(@params);
        }

        public class CreateDatabaseO : Expr
        {
            [JsonProperty("create_database")]
            public object CreateDatabase { get; set; }

            public CreateDatabaseO(object createDatabase)
            {
                CreateDatabase = createDatabase;
            }
        }

        public static Expr CreateClass(object @params)
        {
            return new CreateClassO(@params);
        }

        public class CreateClassO : Expr
        {
            [JsonProperty("create_class")]
            public object CreateClass { get; set; }

            public CreateClassO(object createClass)
            {
                CreateClass = createClass;
            }
        }

        public static Expr CreateIndex(object @params)
        {
            return new CreateIndexO(@params);
        }

        public class CreateIndexO : Expr
        {
            [JsonProperty("create_index")]
            public object CreateIndex { get; set; }

            public CreateIndexO(object createIndex)
            {
                CreateIndex = createIndex;
            }
        }

        public static Expr CreateFunction(object @params)
        {
            return new CreateFunctionO(@params);
        }

        public class CreateFunctionO : Expr
        {
            [JsonProperty("create_function")]
            public object CreateFunction { get; set; }

            public CreateFunctionO(object createFunction)
            {
                CreateFunction = createFunction;
            }
        }

        public static Expr Match(object indexRef, object term)
        {
            return new MatchO2(indexRef, term);
        }

        public class MatchO2 : Expr
        {
            public object Match { get; }
            public object Term { get; }

            public MatchO2(object match, object term)
            {
                Match = match;
                Term = term;
            }
        }

        public static Expr Match(object indexRef, params object[] terms)
        {
            return new MatchO(indexRef, terms);
        }

        public class MatchO : Expr
        {
            public object Match { get; set; }
            public object[] Terms { get; set; }

            public MatchO(object match, object[] terms)
            {
                Match = match;
                Terms = terms;
            }
        }

        public static Expr Union(params object[] sets)
        {
            return new UnionO(sets);
        }

        public class UnionO : Expr
        {
            public object[] Union { get; set; }

            public UnionO(object[] union)
            {
                Union = union;
            }
        }

        public static Expr Intersection(params object[] sets)
        {
            return new IntersectionO(sets);
        }

        public class IntersectionO : Expr
        {
            public object[] Intersection { get; set; }

            public IntersectionO(object[] intersection)
            {
                Intersection = intersection;
            }
        }

        public static Expr Difference(params object[] sets)
        {
            return new DifferenceO(sets);
        }

        public class DifferenceO : Expr
        {
            public object[] Difference { get; set; }

            public DifferenceO(object[] difference)
            {
                Difference = difference;
            }
        }

        public static Expr Distinct(object set)
        {
            return new DistictO(set);
        }

        public class DistictO : Expr
        {
            public object Distinct { get; set; }

            public DistictO(object distinct)
            {
                Distinct = distinct;
            }
        }

        public static Expr Join(object source, object with)
        {
            return new JoinO(source, with);
        }

        public class JoinO : Expr
        {
            public object Join { get; set; }
            public object With { get; set; }

            public JoinO(object join, object with)
            {
                Join = @join;
                With = with;
            }
        }

        public static Expr Login(object @ref, object @params)
        {
            return new LoginO(@ref, @params);
        }

        public class LoginO : Expr
        {
            public object Login { get; set; }
            public object Params { get; set; }

            public LoginO(object login, object @params)
            {
                Login = login;
                Params = @params;
            }
        }

        public static Expr Logout(params object[] tokens)
        {
            return new LogoutO(tokens);
        }

        public class LogoutO : Expr
        {
            public object[] Logout { get; set; }

            public LogoutO(object[] logout)
            {
                Logout = logout;
            }
        }

        public static Expr Identify(object @ref, object password)
        {
            return new IdentifyO(@ref, password);
        }

        public class IdentifyO : Expr
        {
            public object Identify { get; set; }
            public object Password { get; set; }

            public IdentifyO(object identify, object password)
            {
                Identify = identify;
                Password = password;
            }
        }

        public static Expr Concat(params object[] strings)
        {
            return new ConcatO(strings);
        }

        public class ConcatO : Expr
        {
            public object[] Concat { get; set; }

            public ConcatO(object[] concat)
            {
                Concat = concat;
            }
        }

        public static Expr Casefold(object str, object normalizer = null)
        {
            return new CasefoldO(str, normalizer);
        }

        public class CasefoldO : Expr
        {
            public object Casefold { get; set; }
            public object Normalizer { get; set; }

            public CasefoldO(object casefold, object normalizer)
            {
                Casefold = casefold;
                Normalizer = normalizer;
            }
        }

        public static Expr Time(object str)
        {
            return new TimeO(str);
        }

        public class TimeO : Expr
        {
            public object Time { get; set; }

            public TimeO(object time)
            {
                Time = time;
            }
        }

        public static Expr Epoch(object num, object unit)
        {
            return new EpochO(num, unit);
        }

        public class EpochO : Expr
        {
            public object Epoch { get; set; }
            public object Unit { get; set; }

            public EpochO(object epoch, object unit)
            {
                Epoch = epoch;
                Unit = unit;
            }
        }

        public static Expr Date(object str)
        {
            return new DateO(str);
        }

        public class DateO : Expr
        {
            public object Date { get; set; }

            public DateO(object date)
            {
                Date = date;
            }
        }

        public static Expr NextId()
        {
            return new NextIdO();
        }

        public class NextIdO : Expr
        {
            [JsonProperty("next_id")]
            public object NextId { get; set; }
        }

        public static Expr Database(object name)
        {
            return new DatabaseO(name);
        }

        public class DatabaseO : Expr
        {
            public object Database { get; set; }

            public DatabaseO(object database)
            {
                Database = database;
            }
        }

        public static Expr Class(object name)
        {
            return new ClassO(name);
        }

        public class ClassO : Expr
        {
            public object Class { get; set; }

            public ClassO(object @class)
            {
                Class = @class;
            }
        }

        public static Expr Index(object name)
        {
            return new IndexO(name);
        }

        public class IndexO : Expr
        {
            public object Index { get; set; }

            public IndexO(object index)
            {
                Index = index;
            }
        }

        public static Expr Function(object name)
        {
            return new FunctionO(name);
        }

        public class FunctionO : Expr
        {
            public object Function { get; set; }

            public FunctionO(object function)
            {
                Function = function;
            }
        }

        public static Expr EqualsFn(params object[] values)
        {
            return new EqualsO(values);
        }

        public class EqualsO : Expr
        {
            public object[] Equals { get; set; }

            public EqualsO(object[] equals)
            {
                Equals = @equals;
            }
        }

        public static Expr Contains(object[] path, object @in)
        {
            return new ContainsO(path, @in);
        }

        public class ContainsO : Expr
        {
            public object[] Contains { get; set; }
            public object In { get; set; }

            public ContainsO(object[] contains, object @in)
            {
                Contains = contains;
                In = @in;
            }
        }

        public static Expr Select(object[] path, object from, object @default = null, object all = null)
        {
            return new SelectO(path, from, @default, all);
        }

        public static Expr Select(object path, object from, object @default = null, object all = null) => Select(new[] {path}, from, @default, all);

        public class SelectO : Expr
        {
            public object[] Select { get; set; }
            public object From { get; set; }
            public object Default { get; set; }
            public object All { get; set; }

            public SelectO(object[] select, object @from, object @default, object all)
            {
                Select = @select;
                From = @from;
                Default = @default;
                All = all;
            }
        }

        public static Expr Add(params object[] values)
        {
            return new AddO(values);
        }

        public class AddO : Expr
        {
            public object[] Add { get; set; }

            public AddO(object[] add)
            {
                Add = add;
            }
        }

        public static Expr Multiply(params object[] values)
        {
            return new MultiplyO(values);
        }

        public class MultiplyO : Expr
        {
            public object[] Multiply { get; set; }

            public MultiplyO(object[] multiply)
            {
                Multiply = multiply;
            }
        }

        public static Expr Subtract(params object[] values)
        {
            return new SubtractO(values);
        }

        public class SubtractO : Expr
        {
            public object[] Subtract { get; set; }

            public SubtractO(object[] subtract)
            {
                Subtract = subtract;
            }
        }

        public static Expr Divide(params object[] values)
        {
            return new DivideO(values);
        }

        public class DivideO : Expr
        {
            public object[] Divide { get; set; }

            public DivideO(object[] divide)
            {
                Divide = divide;
            }
        }

        public static Expr Modulo(params object[] values)
        {
            return new ModuloO(values);
        }

        public class ModuloO : Expr
        {
            public object[] Modulo { get; set; }

            public ModuloO(object[] modulo)
            {
                Modulo = modulo;
            }
        }

        public static Expr LT(params object[] values)
        {
            return new LTO(values);
        }

        public class LTO : Expr
        {
            public object[] Lt { get; set; }

            public LTO(object[] lt)
            {
                Lt = lt;
            }
        }

        public static Expr LTE(params object[] values)
        {
            return new LTEO(values);
        }

        public class LTEO : Expr
        {
            public object[] Lte { get; set; }

            public LTEO(object[] lte)
            {
                Lte = lte;
            }
        }

        public static Expr GT(params object[] values)
        {
            return new GTO(values);
        }

        public class GTO : Expr
        {
            public object[] Gt { get; set; }

            public GTO(object[] gt)
            {
                Gt = gt;
            }
        }

        public static Expr GTE(params object[] values)
        {
            return new GTEO(values);
        }

        public class GTEO : Expr
        {
            public object[] Gte { get; set; }

            public GTEO(object[] gte)
            {
                Gte = gte;
            }
        }

        public static Expr And(params object[] values)
        {
            return new AndO(values);
        }

        public class AndO : Expr
        {
            public object[] And { get; set; }

            public AndO(object[] and)
            {
                And = and;
            }
        }

        public static Expr Or(params object[] values)
        {
            return new OrO(values);
        }

        public class OrO : Expr
        {
            public object[] Or { get; set; }

            public OrO(object[] or)
            {
                Or = or;
            }
        }

        public static Expr Not(object value)
        {
            return new NotO(value);
        }

        public class NotO : Expr
        {
            public object Not { get; set; }

            public NotO(object not)
            {
                Not = not;
            }
        }

        public class TimeStampV : Expr
        {
            [JsonProperty("@ts")]
            public object Ts { get; set; }

            public TimeStampV(string ts)
            {
                Ts = ts;
            }

            [JsonConstructor]
            public TimeStampV(DateTime ts)
            {
                Ts = ts.ToUniversalTime().ToString("O");
            }
        }

        public class DateV : Expr
        {
            [JsonProperty("@date")]
            public string Date { get; set; }

            public DateV(string date)
            {
                Date = date;
            }
        }

        public class BytesV : Expr
        {
            [JsonProperty("@bytes")]
            public string Bytes { get; set; }

            public BytesV(string base64)
            {
                Bytes = base64;
            }
        }
    }
}