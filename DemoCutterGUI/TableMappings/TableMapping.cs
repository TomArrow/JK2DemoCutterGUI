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
            return this.GetType() == obj.GetType() && (obj as TableMapping)?.rowid == this.rowid;
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(this.GetType().GetHashCode(), rowid.GetHashCode());
        }
        public T Clone<T>() where T : TableMapping
        {
            T retVal = (T)((T)this).MemberwiseClone();
            retVal.IsCopiedEntry = true;
            return retVal;
        }
    }
}
