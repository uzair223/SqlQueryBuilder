using System;
using System.Collections.Generic;
using System.Text;

namespace SqlQueryBuilder
{
    public sealed class QueryBuilder
    {
        private Table? table;
        private IReadOnlyList<ISelectExpression> selects = [];
        private List<Join> joins = [];
        private Condition? where;
        private List<Sort> sorts = [];

        public QueryBuilder Select(params ISelectExpression[] selects)
        {
            this.selects = selects;
            return this;
        }

        public QueryBuilder From(Table table)
        {
            this.table = table;
            return this;
        }
 
        public QueryBuilder Join(Table table, Condition on, JoinType type)
        {
            joins.Add(new Join(table, on, type));
            return this;
        }

        public QueryBuilder Where(Condition condition) {
            where = condition;
            return this;
        }

        public QueryBuilder Sort(Column column, SortType sortType)
        {
            sorts.Add(new Sort(column, sortType));
            return this;
        }
        public string ToSql()
        {
            if (table is null || selects.Count == 0)
            {
                throw new Exception("Table and columns must be specified");
            }
            var sql = $"SELECT {string.Join(", ", selects.Select(s => s.ToSelectSql()))}"
                + $"\nFROM {table.ToSql()}";

            foreach (var join in joins)
            {
                sql += "\n" + join.ToSql();
            }

            if (where is not null)
            {
                sql += $"\nWHERE { where.ToSql() }";
            }

            if(sorts.Count > 0)
            {
                sql += $"\nORDER BY {string.Join(", ", sorts.Select(s => s.ToSql()))}";
            }

            return sql + ";";
        }

    }
}
