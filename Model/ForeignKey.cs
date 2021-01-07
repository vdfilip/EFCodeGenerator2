using EFCodeGenerator.Logic;

namespace EFCodeGenerator.Model
{
    public enum RelationshipType { OneToMany, OneToOne }

    /// <summary>
    /// This class represents the metadata for a foreign key reference.
    /// </summary>
    public class ForeignKey
    {
        public string Name { get; set; }
        public Column From { get; set; }
        public Column To { get; set; }
        public RelationshipType Relationship { get; set; }

        /// <summary>
        /// Returns the name of the collection on the From side of the relationship. This is the
        /// collection of foreign-key entities referencing a primary-key entity. For example, in a
        /// one-to-many relationship [manager -> employee], were employee references manager 
        /// through a FK ManagerID, the "From" collection name is "Employees".
        /// </summary>
        //public string FromCollectionName
        //{
        //    get
        //    {
        //        var name = From.NameWithoutID;

        //        if (From.NameWithoutID != To.Table && From.NameWithoutID.EndsWith(To.Table))
        //            name = From.NameWithoutID.Replace(To.Table, "");

        //        if (name == To.Table)
        //            return Inflector.ToPlural(From.Table);

        //        if (name == "Parent")
        //            return "Children";

        //        return name + Inflector.ToPlural(From.Table);
        //    }
        //}

        public string FromCollectionName
        {
            get
            {
                return Inflector.ToPlural(From.Table);
            }
        }

        public ForeignKey(Column from, Column to)
        {
            From = from;
            To = to;
        }
    }
}
