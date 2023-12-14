using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using INTEGER = System.Int64;
using TEXT = System.String;
using TIMESTAMP = System.Int64;
namespace DemoCutterGUI.TableMappings
{
    class Laughs : TableMapping
    {
        public bool IsLikelySameLaugh(Laughs otherLaughs)
        {
            return this.serverName == otherLaughs.serverName
                && this.laughs == otherLaughs.laughs
                && this.laughCount == otherLaughs.laughCount
                //&& this.chatlog == otherLaughs.chatlog
                && Math.Abs(this.serverTime.GetValueOrDefault(-99999) - otherLaughs.serverTime.GetValueOrDefault(-99999)) <= 1000L // Chats are reliable commands, so in theory this is uncertain since we can potentially get a print many seconds later due to extreme loss of packets. But it will simply have to do.
                && Math.Abs(this.duration.GetValueOrDefault(-99999) - otherLaughs.duration.GetValueOrDefault(-99999)) <= 1000L;

        }
        public INTEGER? id { get; set; } = null;
        public TEXT? map { get; set; } = null;
        public TEXT? serverName { get; set; } = null;
        public TEXT? serverNameStripped { get; set; } = null;
        public TEXT? laughs { get; set; } = null;
        public TEXT? chatlog { get; set; } = null;
        public TEXT? chatlogStripped { get; set; } = null;
        public INTEGER? laughCount { get; set; } = null;
        public INTEGER? demoRecorderClientnum { get; set; } = null;
        public TEXT? demoName { get; set; } = null;
        public TEXT? demoPath { get; set; } = null;
        public INTEGER? duration { get; set; } = null;
        public INTEGER? demoTime { get; set; } = null;
        public INTEGER? lastGamestateDemoTime { get; set; } = null;
        public INTEGER? serverTime { get; set; } = null;
        public TIMESTAMP? demoDateTime { get; set; } = null;
    }
}
