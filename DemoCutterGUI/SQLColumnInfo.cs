using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoCutterGUI
{
    // This is all a bit copypasted from the SQLite library, I basically just add some new columns for more info.

    public static class SQLiteExtensions
    {
        public static List<SQLColumnInfo> GetTableInfoMore(this SQLiteConnection conn,string tableName)
        {
            var query = "pragma table_info(\"" + tableName + "\")";
            return conn.Query<SQLColumnInfo>(query);
        }
    }

    // Cuz normal SQLite library doesn't really give enough tbh
    public class SQLColumnInfo
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string dflt_value { get; set; }
        public int pk { get; set; }
        public int cid { get; set; }
        public bool notnull { get; set; }
        public override string ToString()
        {
            return $"name:{Name},type:{Type},dflt_value:{dflt_value},cid:{cid},notnull:{notnull},pk:{pk}";
        }

    }
}
