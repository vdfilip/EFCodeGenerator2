using EFCodeGenerator.Model;
using EFCodeGenerator.Repo;
using System.IO;
using System.Linq;
using System.Text;

namespace EFCodeGenerator.Logic
{
    public class Generator
    {
        private readonly string _databaseName;
        private readonly string _path;
        private readonly MetaDataRepository _repository;
        private TableCollection _tables;

        public string EntityBaseClassName { get; set; }
        public string PkPropertyName { get; set; }

        public Generator(string databaseName, string outputPath, MetaDataRepository repository)
        {
            _databaseName = databaseName;
            _path = outputPath;
            _repository = repository;
        }

        public void Run()
        {
            _tables = _repository.LoadTableMetaData();


            WriteContextClass();
            WriteEntityAndMappingClasses();
            WriteSummaryText();
        }

        #region Code generation

        private string ModelBuilderConfiguration(string parameter, Indent indent)
        {
            var sb = new StringBuilder();

            foreach (var table in _tables)
            {
                if (parameter == null)
                    sb.AppendFormat("{0}builder.Configurations.Add(new {1}Map());\n", indent, table.Name);
                else
                    sb.AppendFormat(
                        "{0}builder.Configurations.Add(new {1}Map({2}));\n",
                        indent,
                        table.Name,
                        parameter);
            }

            return sb.ToString();
        }

        private void WriteContextClass()
        {
            var indent = new Indent();

            var sb = new StringBuilder();

            // Disable ReSharper inspections
            //sb.AppendLine("// ReSharper disable DoNotCallOverridableMethodsInConstructor");
            //sb.AppendLine("// ReSharper disable RedundantUsingDirective");
            sb.AppendLine();

            // Add using statements
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using System.Data.Entity;");
            sb.AppendLine("using System.Data.Entity.Infrastructure;");
            sb.AppendLine("using System.Data.Entity.ModelConfiguration;");
            sb.AppendLine("using DatabaseGeneratedOption = System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption;");
            sb.AppendLine();

            // Begin the namespace
            sb.AppendFormat("namespace {0}.Domain\n{{\n", _databaseName);

            indent.Increment();

            // Begin the entity class definition.
            sb.AppendFormat("{0}public class {1}DbContext : DbContext\n", indent, _databaseName);
            sb.AppendFormat("{0}{{\n", indent);

            indent.Increment();

            foreach (var table in _tables)
            {
                sb.AppendFormat(
                    "{0}public DbSet<{1}> {2} {{ get; set; }}\n",
                    indent,
                    table.Name,
                    Inflector.ToPlural(table.Name));
            }

            indent.Increment();

            sb.AppendFormat(@"
        static {0}DbContext()
        {{
            Database.SetInitializer<{0}DbContext>(null);
        }}

        public {0}DbContext()
            : base(""Name=ConnectionStrings.{0}"")
        {{
        }}

        public {0}DbContext(string connectionString) : base(connectionString)
        {{
        }}

        public {0}DbContext(string connectionString, DbCompiledModel model) : base(connectionString, model)
        {{
        }}

        protected override void OnModelCreating(DbModelBuilder builder)
        {{
            base.OnModelCreating(builder);

{1}
        }}

        public static DbModelBuilder CreateModel(DbModelBuilder builder, string schema)
        {{
{2}
            return builder;
        }}
", _databaseName, ModelBuilderConfiguration(null, indent), ModelBuilderConfiguration("schema", indent));

            indent.Decrement();
            indent.Decrement();

            sb.AppendFormat("{0}}}\n}}", indent);

            var file = Path.Combine(_path, _databaseName + "DbContext.cs");
            File.WriteAllText(file, sb.ToString());
        }

        private void WriteEntityAndMappingClasses()
        {
            foreach (var table in _tables)
            {
                var indent = new Indent();

                var sb = new StringBuilder();

                // Disable ReSharper inspections
                //sb.AppendLine("// ReSharper disable DoNotCallOverridableMethodsInConstructor");
                //sb.AppendLine("// ReSharper disable RedundantUsingDirective");
                //sb.AppendLine();

                // Add using statements
                sb.AppendLine("using System;");
                sb.AppendLine("using System.Collections.Generic;");
                sb.AppendLine("using System.Data.Entity.ModelConfiguration;");
                sb.AppendLine("using DatabaseGeneratedOption = System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption;");
                sb.AppendLine();

                // Begin the namespace
                sb.AppendFormat("namespace {0}.Domain\n{{\n", _databaseName);

                indent.Increment();

                // Begin the entity class definition.
                sb.AppendFormat("{0}public class {1} {2}\n", indent, table.Name, EntityBaseClassName == null ? string.Empty : (" : " + EntityBaseClassName));
                sb.AppendFormat("{0}{{\n", indent);

                indent.Increment();

                // Fields

                foreach (var column in table.Columns)
                {
                    var columnName = column.ValueFieldName;
                    if (PkPropertyName != null && column.IsIdentity && table.Columns.Count(x => x.IsIdentity) == 1)
                    {
                        columnName = PkPropertyName;
                    }
                    var isNullable = !column.IsRequired && column.SystemTypeName != "string";
                    sb.AppendFormat(
                        "{0}public {1}{2} {3} {{ get; set; }}\n",
                        indent,
                        column.SystemTypeName,
                        isNullable ? "?" : "",
                        columnName);
                }

                // Properties

                if (table.ForeignKeys.Count > 0)
                    sb.AppendLine();

                foreach (var fk in table.ForeignKeys)
                {
                    sb.AppendFormat(
                        "{0}public {1} {2} {{ get; set; }}\n",
                        indent,
                        fk.To.Table,
                        fk.From.ReferenceFieldName);
                }

                // Collections

                if (table.Dependencies.Count > 0)
                    sb.AppendLine();

                foreach (var dependency in table.Dependencies)
                {
                    if (dependency.Relationship == RelationshipType.OneToMany)
                        sb.AppendFormat(
                            "{0}public List<{1}> {2} {{ get; set; }}\n",
                            indent,
                            dependency.From.Table,
                            dependency.FromCollectionName);
                    else
                        sb.AppendFormat(
                            "{0}public {1} {2} {{ get; set; }}\n",
                            indent,
                            dependency.From.Table,
                            dependency.From.Table);
                }

                // Constructor

                if (table.RequiresConstructor)
                {
                    sb.AppendLine();

                    sb.AppendFormat("{0}public {1}()\n{0}{{\n", indent, table.Name);

                    indent.Increment();

                    //foreach (var column in table.Columns)
                    //{
                    //    if (!column.IsRequired && column.SystemTypeName == "DateTime")
                    //        sb.AppendFormat("{0}{1} = DateTime.Now;\n", indent, column.Name);
                    //}
                    //if (table.Dependencies.Count > 0) sb.AppendLine();

                    foreach (var dependency in table.Dependencies)
                    {
                        if (dependency.Relationship == RelationshipType.OneToMany)
                            sb.AppendFormat(
                                "{0}{1} = new List<{2}>();\n",
                                indent,
                                dependency.FromCollectionName,
                                dependency.From.Table);
                    }

                    indent.Decrement();

                    sb.AppendFormat("{0}}}\n", indent);
                }

                indent.Decrement();

                sb.AppendFormat("{0}}}\n\n", indent);

                // Begin the mapping class definition.

                sb.AppendFormat("{0}public class {1}Map : EntityTypeConfiguration<{1}>\n", indent, table.Name);
                sb.AppendFormat("{0}{{\n", indent);

                indent.Increment();

                sb.AppendFormat("{0}public {1}Map() : this(\"{2}\") {{ }}\n\n", indent, table.Name, table.Schema);

                sb.AppendFormat("{0}public {1}Map(string schema)\n{0}{{\n", indent, table.Name);

                indent.Increment();

                sb.AppendFormat("{0}ToTable(\"{1}\", schema);\n", indent, table.OriginalName);

                if (PkPropertyName != null)
                {
                    sb.AppendFormat("{0}HasKey(x => x.{1});\n\n", indent, PkPropertyName);
                }
                else
                {
                    sb.AppendFormat("{0}HasKey(x => new {{ {1} }} );\n\n", indent, table.GetPrimaryKeyFieldNames("x"));
                }


                foreach (var column in table.Columns)
                {
                    var columnName = column.ValueFieldName;
                    if (PkPropertyName != null && column.IsIdentity && table.Columns.Count(x => x.IsIdentity) == 1)
                    {
                        columnName = PkPropertyName;
                    }
                    sb.AppendFormat("{0}Property(x => x.{1})", indent, columnName);

                    indent.Increment();

                    sb.AppendFormat("\n{0}.HasColumnName(\"{1}\")", indent, column.Name);

                    sb.AppendFormat("\n{0}.Is{1}()", indent, column.IsRequired ? "Required" : "Optional");

                    if (column.DataType == "varchar") sb.AppendFormat("\n{0}.IsUnicode(false)", indent);

                    sb.AppendFormat("\n{0}.HasColumnType(\"{1}\")", indent, column.DataType);

                    if ((column.SystemTypeName == "string") && column.MaximumLength > 0)
                        sb.AppendFormat("\n{0}.HasMaxLength({1})", indent, column.MaximumLength);

                    if (column.IsIdentity)
                        sb.AppendFormat("\n{0}.HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)", indent);

                    sb.Append(";");
                    sb.AppendLine();
                    sb.AppendLine();

                    indent.Decrement();
                }

                if (table.ForeignKeys.Count == 0) sb.AppendLine();

                foreach (var fk in table.ForeignKeys)
                {
                    if (fk.Relationship == RelationshipType.OneToMany)
                        sb.AppendFormat(
                            "{0}Has{1}(a => a.{2}).WithMany(b => b.{3}).HasForeignKey(c => c.{4});\n",
                            indent,
                            fk.From.IsRequired ? "Required" : "Optional",
                            fk.From.ReferenceFieldName,
                            fk.FromCollectionName,
                            fk.From.ValueFieldName
                            );
                    else
                        sb.AppendFormat(
                            "{0}Has{1}(a => a.{2}).WithOptional(b => b.{3});\n",
                            indent,
                            fk.From.IsRequired ? "Required" : "Optional",
                            fk.From.ReferenceFieldName,
                            fk.From.Table
                            );
                }

                indent.Decrement();

                sb.AppendFormat("{0}}}\n", indent);

                indent.Decrement();

                sb.AppendFormat("{0}}}\n}}", indent);

                var file = Path.Combine(_path, table.Name + ".cs");
                File.WriteAllText(file, sb.ToString());
            }
        }

        #endregion

        #region Plain-text summarization

        private void WriteSummaryText()
        {
            var sb = new StringBuilder();

            sb.AppendLine("LEGEND:");
            sb.AppendLine(" + Required Field");
            sb.AppendLine(" - Optional Field");
            sb.AppendLine("fk Foreign Key");
            sb.AppendLine("pk Primary Key");
            sb.AppendLine(" i Identity Column");
            sb.AppendLine(new string(' ', 78));
            sb.AppendLine();
            sb.AppendLine();

            foreach (var table in _tables)
            {
                sb.AppendFormat("{0}.{1}", table.Schema, table.Name);
                sb.AppendLine();

                foreach (var column in table.Columns)
                {
                    var pk = string.Empty;
                    if (table.PrimaryKey.Columns.Contains(column))
                    {
                        var n = table.Dependencies.CountTo(column);
                        var label = string.Empty;
                        if (n > 0)
                            label = n == 1 ? " with 1 dependency" : $" with {n} dependencies";

                        pk = $" (pk{label})";
                    }

                    sb.AppendFormat(
                        "   {3} {4} {0} {1}{2}",
                        column.IsRequired ? "+" : "-",
                        column.Name,
                        pk,
                        table.ForeignKeys.CountFrom(column) > 0 ? "fk" : "  ",
                        column.IsIdentity ? "i" : " "
                        );

                    sb.AppendLine();
                }

                sb.AppendLine();
            }

            var file = Path.Combine(_path, "_tables.txt");
            File.WriteAllText(file, sb.ToString());
        }

        #endregion
    }
}