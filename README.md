#### Bulk insert for a single class and TPT type inheritance using entity framework. 
No limitation on the number of records that can be passed. 

New changes - 
1. Now supporting string, number, guids, date, datetime, datetimeoffset and boolean types. 
2. Instead of declaring the dbcontext every time, just set it once while declaring the BulkInsert object.

#### Just pass the *DbContext* as parameter to the constructor and use the following methods as per your needs.

#### Following methods have been exposed out of this library -

#### To bulk insert records.

*void BulkInsertCommand(IEnumerable<object> values);*

#### To bulk insert records in case of TPT inheritance

*void BulkInsertCommandForTPT(IEnumerable<object> values, string parentName, string foreignKeys);*
