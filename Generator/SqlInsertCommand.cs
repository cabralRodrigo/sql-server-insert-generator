using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace CabralRodrigo.Util.SqlServerInsertGenerator.Generator
{
    internal partial class SqlInsertCommandTemplate
    {
        /// <summary>
        /// Initializes a new instance of <seealso cref="SqlInsertCommandTemplate"/>.
        /// </summary>
        /// <param name="metadata">The metadata used to transform the template into the insert command.</param>
        /// <param name="fillData">The data used to fill the insert command.</param>
        public SqlInsertCommandTemplate(SqlInsertCommandMetadata metadata, List<Dictionary<string, string>> fillData)
        {
            this.Metadata = metadata;
            this.FillData = fillData;

            if (this.FillData == null)
                this.FillData = new List<Dictionary<string, string>>();

            if (this.FillData.Count < 1)
                this.FillData.Add(null);
        }

        protected SqlInsertCommandTemplate() { }

        /// <summary>
        /// The data used to fill the insert command.
        /// </summary>
        public List<Dictionary<string, string>> FillData { get; private set; }

        /// <summary>
        /// The metadata used to transform the template into the insert command.
        /// </summary>
        public SqlInsertCommandMetadata Metadata { get; set; }

        /// <summary>
        /// Determines when the <seealso cref="SqlDbType"/> has to be quoted in a sql query.
        /// </summary>
        /// <param name="dbType">The <seealso cref="SqlDbType"/> to be checked.</param>
        /// <param name="objectValue">The value of the column.</param>
        /// <returns>True if the <seealso cref="SqlDbType"/> has to be quoted in a sql query, False if not.</returns>
        private bool DbTypeValueHasToBeQuoted(SqlDbType dbType, string objectValue)
        {
            return !new SqlDbType[]
            {
                SqlDbType.BigInt,
                SqlDbType.Bit,
                SqlDbType.Decimal,
                SqlDbType.Float,
                SqlDbType.Int,
                SqlDbType.Money,
                SqlDbType.SmallInt,
                SqlDbType.SmallMoney,
                SqlDbType.TinyInt
            }.Contains(dbType) && objectValue != "null";
        }

        /// <summary>
        /// Gets the value for the column for using on the insert command.
        /// </summary>
        /// <param name="column">The column to get the value.</param>
        /// <param name="data">The data used to fill the insert command.</param>
        /// <returns>The value to be used on the insert command.</returns>
        private string GetColumnValue(SqlInsertCommandColumnMetadata column, Dictionary<string, string> data)
        {
            if (data == null)
                return column.Value;
            else {
                if (this.DbTypeValueHasToBeQuoted(column.DbType, data[column.RawName]))
                    return $"'{data[column.RawName]}'";
                else
                    return data[column.RawName];
            }
        }
    }
}
