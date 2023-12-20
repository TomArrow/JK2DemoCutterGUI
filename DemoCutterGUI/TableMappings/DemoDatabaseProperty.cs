using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using INTEGER = System.Int64;
using TEXT = System.String;
using NUM = System.Boolean;
using BOOLEAN = System.Boolean;
using REAL = System.Double;
using TIMESTAMP = System.Int64;

namespace DemoCutterGUI.TableMappings
{
    class DemoDatabaseProperty
    {
        public TEXT? propertyName { get; set; } = null;
        public TEXT? value { get; set; } = null;

    }
}
