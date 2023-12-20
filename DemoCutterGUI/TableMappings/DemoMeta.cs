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
    class DemoMeta
    {
        public TEXT? demoName { get; set; } = null;
        public TEXT? demoPath { get; set; } = null;
        public INTEGER? fileSize { get; set; } = null;
        public INTEGER? demoTimeDuration { get; set; } = null;
        public TIMESTAMP? demoDateTime { get; set; } = null;
    }
}
