using LocalDatabase;
using RajeshPanda.EF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestBulkInsertEF
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var db = new BulkInsertEFEntities())
            {
                Console.WriteLine("Starting test. Enter number of records you wish to test for :");
                var count = int.Parse(Console.ReadLine());

                var list = new List<TestTable>();

                for(int i = 0; i < count; i++)
                {
                    list.Add(new TestTable {
                        GuidId = Guid.NewGuid(),
                        VarcharType = "Rajesh Panda",
                        OnlyDate = DateTime.Now.Date,
                        OffsetTypeDate = new DateTimeOffset(DateTime.Now).UtcDateTime,
                        DateTimeType = DateTime.Now,
                        BooleanType = false
                    });
                }

                var bulkInsert = new BulkInsert(db);

                bulkInsert.BulkInsertCommand(list);

                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }
        }
    }
}
