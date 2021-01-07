using System.Collections.Generic;

namespace EFCodeGenerator.Model
{
    public class ForeignKeyCollection : List<ForeignKey>
    {
        /// <summary>
        /// Returns the number of keys with a matching From column.
        /// </summary>
        public int CountFrom(Column column)
        {
            var count = 0;
            foreach (var item in this)
            {
                if (item.From.Equals(column))
                    count++;
            }
            return count;
        }

        /// <summary>
        /// Returns the number of keys with a matching To column.
        /// </summary>
        public int CountTo(Column column)
        {
            var count = 0;
            foreach (var item in this)
            {
                if (item.To.Equals(column))
                    count++;
            }
            return count;
        }
    }
}
