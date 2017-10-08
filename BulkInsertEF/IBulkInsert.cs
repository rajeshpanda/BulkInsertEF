using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RajeshPanda.EF
{
    public interface IBulkInsert
    {
        /// <summary>
        /// To bulk insert records.
        /// </summary>
        /// <param name="values">IEnumerable(object) of the type of records</param>
        void BulkInsertCommand(IEnumerable<object> values);

        /// <summary>
        /// To bulk insert records in case of TPT inheritance
        /// </summary>
        /// <param name="values">IEnumerable(object) of the type of records</param>
        /// <param name="parentName">Parent class name</param>
        /// <param name="foreignKeys">comma separated foreign keys if any</param>
        void BulkInsertCommandForTPT(IEnumerable<object> values, string parentName, string foreignKeys);
    }
}
