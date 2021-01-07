using EFCodeGenerator.Model;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace EFCodeGenerator.Repo
{
    public class MetaDataRepository
    {
        private readonly string _connectionString;

        public MetaDataRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public TableCollection LoadTableMetaData()
        {
            var tables = CreateTables(Select(Queries.SelectTables));
            var columns = CreateColumns(Select(Queries.SelectColumns));
            var primaryKeys = CreatePrimaryKeys(Select(Queries.SelectPrimaryKeys), columns);
            var foreignKeys = CreateForeignKeys(Select(Queries.SelectForeignKeys), columns);

            LoadColumns(tables, columns);
            LoadKeys(tables, primaryKeys);
            LoadKeys(tables, foreignKeys);

            return tables;
        }


        private static ColumnCollection CreateColumns(DataTable data)
        {
            var columns = new ColumnCollection();

            foreach (DataRow row in data.Rows)
            {
                var column = new Column
                {
                    Schema = (string)row["TABLE_SCHEMA"],
                    //SchemaDescription = (string)row["SchemaDescription"],
                    Table = GetCleanTableName((string)row["TABLE_NAME"]),
                    Name = (string)row["COLUMN_NAME"],
                    DataType = (string)row["DATA_TYPE"],
                    IsIdentity = (int)row["IS_IDENTITY"] == 1,
                    IsRequired = (string)row["IS_NULLABLE"] == "NO",
                    OrdinalPosition = (int)row["ORDINAL_POSITION"]
                };

                if (!row.IsNull("CHARACTER_MAXIMUM_LENGTH"))
                {
                    var max = (int)row["CHARACTER_MAXIMUM_LENGTH"];
                    if (max > 0)
                        column.MaximumLength = max;
                }

                columns.Add(column);
            }

            return columns;
        }

        private static ForeignKeyCollection CreateForeignKeys(DataTable data, ColumnCollection columns)
        {
            var keys = new ForeignKeyCollection();

            for (var i = 0; i < data.Rows.Count; i++)
            {
                var row = data.Rows[i];

                var schema = (string)row["schema_name"];
                var table = GetCleanTableName((string)row["referenced_table"]);
                var column = (string)row["referenced_column"];
                var to = columns.FindByName(schema, table, column);

                schema = (string)row["schema_name"];
                table = GetCleanTableName((string)row["table"]);
                column = (string)row["column"];
                var from = columns.FindByName(schema, table, column);

                var key = new ForeignKey(from, to) { Name = (string)row["FK_NAME"] };
                keys.Add(key);
            }

            return keys;
        }

        private static string GetCleanTableName(string name)
        {
            var cleanName = name;
            if (name.ToLower().StartsWith("tsys"))
                cleanName = name.Substring(4);
            if (name.ToLower().StartsWith("tbl"))
                cleanName = name.Substring(3);
            if (name.ToLower().StartsWith("htbl"))
                cleanName = name.Substring(4);

            return cleanName.First().ToString().ToUpper() + cleanName.Substring(1);
        }

        private static PrimaryKeyCollection CreatePrimaryKeys(DataTable data, ColumnCollection columns)
        {
            var keys = new PrimaryKeyCollection();

            for (var i = 0; i < data.Rows.Count; i++)
            {
                var row = data.Rows[i];

                var key = new PrimaryKey
                {
                    Name = (string)row["CONSTRAINT_NAME"]
                };

                var schema = (string)row["TABLE_SCHEMA"];
                var table = GetCleanTableName((string)row["TABLE_NAME"]);
                var column = (string)row["COLUMN_NAME"];
                key.Columns.Add(columns.FindByName(schema, table, column));

                var next = i + 1;
                while (next < data.Rows.Count && key.Name == (string)data.Rows[next]["CONSTRAINT_NAME"])
                {
                    row = data.Rows[next];

                    schema = (string)row["TABLE_SCHEMA"];
                    table = GetCleanTableName((string)row["TABLE_NAME"]);
                    column = (string)row["COLUMN_NAME"];
                    key.Columns.Add(columns.FindByName(schema, table, column));

                    next++;
                    i++;
                }

                keys.Add(key);
            }

            return keys;
        }

        private static TableCollection CreateTables(DataTable data)
        {
            var tables = new TableCollection();

            foreach (DataRow row in data.Rows)
            {
                var table = new Table
                {
                    Schema = (string)row["TABLE_SCHEMA"],
                    //SchemaDescription = (string)row["SchemaDescription"],
                    OriginalName = (string)row["TABLE_NAME"],
                    Name = GetCleanTableName((string)row["TABLE_NAME"])
                };

                tables.Add(table);
            }

            return tables;
        }


        private static void LoadColumns(TableCollection tables, ColumnCollection columns)
        {
            foreach (var table in tables)
            {
                foreach (var column in columns)
                {
                    if (column.Schema == table.Schema && column.Table == table.Name)
                        table.Columns.Add(column);
                }
            }
        }

        private static void LoadKeys(TableCollection tables, PrimaryKeyCollection keys)
        {
            foreach (var table in tables)
            {
                foreach (var key in keys)
                {
                    foreach (var column in key.Columns)
                    {
                        if (column.Schema == table.Schema && column.Table == table.Name)
                            table.PrimaryKey.Columns.Add(column);
                    }
                }
            }
        }

        private static void LoadKeys(TableCollection tables, ForeignKeyCollection keys)
        {
            foreach (var table in tables)
            {
                foreach (var key in keys)
                {
                    foreach (var column in table.Columns)
                    {
                        if (key.From.Equals(column))
                        {
                            // If FK = PK then assume a one-to-one relationship.
                            if (table.PrimaryKey.Columns.Count == 1 && table.PrimaryKey.Columns[0] == column)
                                key.Relationship = RelationshipType.OneToOne;
                        }

                        if (key.From.Equals(column))
                            table.ForeignKeys.Add(key);

                        if (key.To.Equals(column))
                            table.Dependencies.Add(key);
                    }
                }
            }
        }

        private DataTable Select(string query)
        {
            var table = new DataTable();


            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var adapter = new SqlDataAdapter(query, connection);
                adapter.Fill(table);
                connection.Close();
            }

            return table;
        }
    }
}
