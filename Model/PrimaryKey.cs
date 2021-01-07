namespace EFCodeGenerator.Model
{
    /// <summary>
    /// This class represents the metadata for a primary key.
    /// </summary>
    public class PrimaryKey
    {
        public string Name { get; set; }
        public ColumnCollection Columns { get; set; }

        public PrimaryKey()
        {
            Columns = new ColumnCollection();
        }
    }
}
