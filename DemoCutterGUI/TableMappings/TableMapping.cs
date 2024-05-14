using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
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


        static JsonSerializerOptions opts = new JsonSerializerOptions()
        { NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowNamedFloatingPointLiterals | System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString };
        public static string reformatMetaEvents(string metaEventsField, Int64 bufferTimeReal)
        {

            StringBuilder sb = new StringBuilder();
            sb.Append("{\"hl\":");
            sb.Append(bufferTimeReal);
            sb.Append(",\"me\":");
            if (string.IsNullOrWhiteSpace(metaEventsField))
            {
                sb.Append("null");
            } else
            {
                try
                {
                    MetaEventsField metaEvents = JsonSerializer.Deserialize<MetaEventsField>(metaEventsField, opts);
                    string[] events = metaEvents.me.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    if(events.Length > 0)
                    {
                        sb.Append("\"");
                        int index = 0;
                        foreach (string ev in events)
                        {
                            if (string.IsNullOrWhiteSpace(ev)) continue;
                            int letters = 0;
                            for(int i = 0; i < ev.Length; i++)
                            {
                                if(ev[i] >= 'a' && ev[i] <= 'z' || ev[i] >= 'A' && ev[i] <= 'Z')
                                {
                                    letters++;
                                }
                                else
                                {
                                    break;
                                }
                            }
                            if (letters == ev.Length) continue;
                            string eventType = ev.Substring(0, letters);
                            string number = ev.Substring(letters);
                            //string[] parts = ev.Split('-', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                            //if (parts.Length != 2) continue;

                            Int64 num;
                            if(Int64.TryParse(number, out num))
                            {
                                if (index != 0)
                                {
                                    sb.Append(",");
                                }
                                sb.Append(eventType); // meta events type
                                //sb.Append("-");

                                sb.Append(bufferTimeReal + num);
                                index++;
                            }

                        }
                        sb.Append("\"");
                    } else
                    {
                        sb.Append("null");
                    }

                }
                catch (Exception ex)
                {
                    sb.Append("null");
                }
            }
            sb.Append("}");
            return sb.ToString();
        }
    }


    public class MetaEventsField
    {
        public string me { get; set; }
    }

}
