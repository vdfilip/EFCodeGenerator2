namespace EFCodeGenerator.Model
{
    /// <summary>
    /// This class represents the metadata for a database table.
    /// </summary>
    public class Table
    {
        public string Name { get; set; }
        public string OriginalName { get; set; }
        public string Schema { get; set; }
        public string SchemaDescription { get; set; }

        public ColumnCollection Columns { get; set; }
        public PrimaryKey PrimaryKey { get; set; }
        public ForeignKeyCollection ForeignKeys { get; set; }
        public ForeignKeyCollection Dependencies { get; set; }

        public Table()
        {
            Columns = new ColumnCollection();
            PrimaryKey = new PrimaryKey();
            ForeignKeys = new ForeignKeyCollection();
            Dependencies = new ForeignKeyCollection();
        }


        /// <summary>
        /// Returns a comma-separated list of primary key field names, where each field name is
        /// assigned a prefix.
        /// </summary>
        public string GetPrimaryKeyFieldNames(string prefix)
        {
            var csv = "";
            foreach (var column in PrimaryKey.Columns)
            {
                if (csv.Length > 0)
                    csv += ", ";

                csv += prefix + "." + column.ValueFieldName;
            }
            return csv;
        }

        /// <summary>
        /// Returns true if the table has any dependencies or nullable DateTime columns.
        /// </summary>
        public bool RequiresConstructor
        {
            get
            {
                foreach (var column in Columns)
                    if (!column.IsRequired && column.SystemTypeName == "DateTime")
                        return true;

                return Dependencies.Count > 0;
            }
        }
    }
}
