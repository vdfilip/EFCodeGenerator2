using EFCodeGenerator.Logic;

namespace EFCodeGenerator.Model
{
    /// <summary>
    /// This class represents the metadata for a database column.
    /// </summary>
    public class Column
    {
        public string DataType { get; set; }
        public bool IsIdentity { get; set; }
        public bool IsRequired { get; set; }
        public int? MaximumLength { get; set; }
        public string Name { get; set; }
        public int OrdinalPosition { get; set; }
        public string Schema { get; set; }
        public string SchemaDescription { get; set; }
        public string Table { get; set; }

        /// <summary>
        /// Returns the field name for a value attribute in a POCO class. 
        /// EntityID is abbreviated ID, and ParentEntityID is abbreviated ParentID.
        /// </summary>
        public string ValueFieldName
        {
            get
            {
                if (Name == Table + "ID")
                    return "ID";

                if (Name == "Parent" + Table + "ID")
                    return "ParentID";

                return Name;
            }
        }

        /// <summary>
        /// Returns the name without the trailing "ID" when the name ends with "ID".
        /// </summary>
        public string NameWithoutID
        {
            get
            {
                if (!Name.EndsWith("ID") && !Name.EndsWith("id"))
                    //throw CustomException.Create("ID Not Found: {0}", Name);
                    return Name;

                return Name.Substring(0, Name.Length - 2);
            }
        }

        /// <summary>
        /// Returns the field name for a reference attribute in a POCO class. 
        /// ParentEntity is abbreviated Parent, and any trailing "ID" is removed.
        /// </summary>
        public string ReferenceFieldName
        {
            get { return NameWithoutID == "Parent" + Table ? "Parent" : NameWithoutID; }
        }

        /// <summary>
        /// Returns the .NET system type name that corresponds with the database type name.
        /// </summary>
        public string SystemTypeName
        {
            get
            {
                string name;

                switch (DataType)
                {
                    case "bit":
                        name = "bool";
                        break;

                    case "int":
                    case "smallint": //*
                    case "tinyint": //*
                        name = "int";
                        break;

                    case "bigint": //*
                        name = "long";
                        break;

                    case "nvarchar":
                    case "varchar":
                    case "char": //*
                    case "nchar": //*
                    case "ntext": //*
                    case "text": //*
                        name = "string";
                        break;

                    case "date":
                    case "datetime":
                    case "smalldatetime":
                        name = "DateTime";
                        break;

                    case "decimal":
                    case "money":
                    case "numeric": //*
                        name = "Decimal";
                        break;

                    case "float"://*
                        name = "float";
                        break;

                    case "uniqueidentifier"://*
                        name = "Guid";
                        break;

                    case "varbinary":
                    case "image":
                        name = "byte[]";
                        break;

                    default:
                        throw CustomException.Create("Unexpected Data Type: {0}", DataType);
                }

                return name;
            }
        }


        /// <summary>
        /// Returns true if the schema name, table name, and column name match.
        /// </summary>
        public bool Equals(Column x)
        {
            return Schema == x.Schema && Table == x.Table && Name == x.Name;
        }
    }
}