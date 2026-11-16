using System.Globalization;

namespace SqlQueryBuilder
{
    public interface IAliasable
    {
        string? Alias { get; set; }
    }

    public interface ISelectExpression
    {
        string ToSelectSql();
    }

    public static class AliasExtension
    {
        public static T As<T>(this T obj,  string? alias) where T : IAliasable
        {
            obj.Alias = alias;
            return obj;
        }
    }

    public abstract class Table : IAliasable
    {
        public abstract string TableName_ { get; }
        public string? Alias { get; set; }
        public string RefName => Alias ?? TableName_;

        public Column Column(string name) => new Column(this, name);

        public string ToSql() => Alias is null ? TableName_ : $"{TableName_} AS {Alias}";
    }

    public sealed class Column : IAliasable, ISelectExpression
    {
        public Table Owner { get;  }
        public string Name { get; }
        public string? Alias { get; set; }

        public Column ( Table owner, string name )
        {
            Owner = owner;
            Name = name;
        }
        public string ToSql() => $"{Owner.RefName}.{Name}";

        public string ToSelectSql() => Alias is null ? ToSql() : $"{ToSql()} AS {Alias}";

        public Condition Eq(object? other) => new(this, other, "=");
        public Condition NotEq(object? other) => new(this, other, "!=");
        public Condition Lt(object? other) => new(this, other, "<");
        public Condition Lte(object? other) => new(this, other, "<=");
        public Condition Gt(object? other) => new(this, other, ">");
        public Condition Gte(object? other) => new(this, other, ">=");

    }

    public sealed record Asterisk(Table? Owner = null) : ISelectExpression
    {
        public string ToSelectSql() => Owner is null ? "*" : $"{Owner.RefName}.*";
    }

    public sealed record Condition(object? Left, object? Right, string Operator)
    {
        public Condition And(Condition other) => new(this, other, "AND");
        public Condition Or(Condition other) => new(this, other, "OR");

        public string ToSql() => $"{Sql.ToSql(Left)} {Operator} {Sql.ToSql(Right)}";
    }

    public enum JoinType
    {
        Inner,
        Left,
        Right,
        Outer,
    }

    public sealed record Join(Table Table, Condition On, JoinType Type)
    {
        public string ToSql() => $"{Type.ToString().ToUpper()} JOIN {Table.ToSql()} ON {On.ToSql()}";
    }

    public enum SortType
    {
        ASC,
        DESC
    }
    public sealed record Sort(Column Column, SortType SortType)
    {
        public string ToSql() => $"{Column.ToSql()} {SortType}";
    }

    public static class Sql
    {
        public static Asterisk All { get; } = new();
        public static Asterisk AllFrom(Table table) => new(table);
        public static string ToSql(object? value) => value switch
        {
            null => "NULL",
            Column column => column.ToSql(),
            Condition condition => condition.ToSql(),
            string text => $"'{text.Replace("'", "''")}'",
            bool boolean => boolean ? "1" : "0",
            DateTime date => $"'{date.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture)}'",
            IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
            _ => value.ToString() ?? "NULL"
        };
    }
}