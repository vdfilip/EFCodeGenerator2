using EFCodeGenerator.Logic;
using System.Collections.Generic;

namespace EFCodeGenerator.Model
{
    public class ColumnCollection : List<Column>
    {
        public Column FindByName(string schema, string table, string column)
        {
            foreach (var item in this)
                if (item.Schema == schema && item.Table == table && item.Name == column)
                    return item;

            var error = string.Format("Column Not Found: {0}.{1}.{2}", schema, table, column);
            throw CustomException.Create(error);
        }
    }
}
