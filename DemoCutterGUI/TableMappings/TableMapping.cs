using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoCutterGUI.TableMappings
{
    abstract public class TableMapping
    {
        public Int64? rowid { get; set; } = -1;
        public bool IsCopiedEntry { get; private set; } = false;

        // Hmm this doesn't do the trick... ends up comparing -1 to -1...
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            return this.GetType() == obj.GetType() && (obj as TableMapping)?.rowid == this.rowid;
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(this.GetType().GetHashCode(), rowid.GetHashCode());
        }
        public static bool operator ==(TableMapping item1, TableMapping item2)
        {
            return item1 is null && item2 is null || !(item1 is null) && !(item2 is null) && item1.Equals(item2);
        }
        public static bool operator !=(TableMapping item1, TableMapping item2)
        {
            return !(item1 == item2);
        }
        public T Clone<T>() where T : TableMapping
        {
            T retVal = (T)((T)this).MemberwiseClone();
            retVal.IsCopiedEntry = true;
            return retVal;
        }
    }
}
