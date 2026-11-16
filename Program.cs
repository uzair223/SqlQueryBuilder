using SqlQueryBuilder;

// Examples

var c = new CustomersTable().As("c");
var o = new OrdersTable().As("o");
var p = new PaymentsTable().As("p");
var s = new ShipmentsTable().As("s");

Console.WriteLine("=== Example 1 - Query with wildcard ===\n");
var query = new QueryBuilder()
    .Select(Sql.All)
    .From(new CustomersTable());
Console.WriteLine(query.ToSql());

Console.WriteLine("\n\n=== Example 2 - Aliases, WHERE, ORDER BY ===\n");
query = new QueryBuilder()
    .Select(c.Name.As("CustomerName"))
    .From(c)
    .Where(c.Id.Lt(100))
    .Sort(c.Name, SortType.ASC);
Console.WriteLine(query.ToSql());

Console.WriteLine("\n\n=== Example 3 - Single join, multi-field sort ===\n");
query = new QueryBuilder()
    .Select(c.Id.As("CustId"), o.Id.As("OrderId"), c.Name, o.Date)
    .From(c)
    .Join(o, c.Id.Eq(o.CustomerId), JoinType.Inner)
    .Where(c.Name.Eq("John Doe").And(o.Date.Lt(new DateTime(2026, 11, 16))))
    .Sort(o.Date, SortType.DESC)
    .Sort(c.Name, SortType.ASC);
Console.WriteLine(query.ToSql());

Console.WriteLine("\n\n=== Example 4 - Multiple joins ===\n");
query = new QueryBuilder()
    .Select(
        c.Id.As("CustId"),
        c.Name,
        o.Id.As("OrderId"),
        o.Date,
        p.Amount.As("PaymentAmount"),
        s.TrackingNumber
    )
    .From(c)
    .Join(
        o,
        c.Id.Eq(o.CustomerId),
        JoinType.Inner
    )
    .Join(
        p,
        o.Id.Eq(p.OrderId),
        JoinType.Left
    )
    .Join(
        s,
        o.Id.Eq(s.OrderId),
        JoinType.Right
    )
    .Where(
        c.Name.Eq("John Doe")
        .And(o.Date.Lt(new DateTime(2026, 11, 16)))
    );
Console.WriteLine(query.ToSql());


Console.WriteLine("\n\n=== FUTURE DEVELOPMENT ===\n" +
    "Implement query validation to detect errors like duplicate aliases, invalid joins, etc.\n" +
    "I would also add support for subqueries and extend functionality with GROUP BY, WITH, HAVING etc.");



// We can define SQL Tables by implementing the schema as classes inheriting the generic Table class
public sealed class CustomersTable : Table
{
    public override string TableName_ => "Customers";
    public Column Id => Column("Id");
    public Column Name => Column("Name");
    public Column Email => Column("Email");
}

public sealed class OrdersTable : Table
{
    public override string TableName_ => "Orders";
    public Column Id => Column("Id");
    public Column CustomerId => Column("CustomerId");
    public Column Date => Column("Date");
}

public sealed class PaymentsTable : Table
{
    public override string TableName_ => "Payments";
    public Column Id => Column("Id");
    public Column OrderId => Column("OrderId");
    public Column Amount => Column("Amount");
    public Column PaymentDate => Column("PaymentDate");
}

public sealed class ShipmentsTable : Table
{
    public override string TableName_ => "Shipments";
    public Column Id => Column("Id");
    public Column OrderId => Column("OrderId");
    public Column TrackingNumber => Column("TrackingNumber");
    public Column ShippedDate => Column("ShippedDate");
}