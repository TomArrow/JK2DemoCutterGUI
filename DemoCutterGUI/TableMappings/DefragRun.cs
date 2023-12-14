using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using INTEGER = System.Int64;
using TEXT = System.String;
using BOOLEAN = System.Boolean;
using REAL = System.Double;
using TIMESTAMP = System.Int64;

namespace DemoCutterGUI.TableMappings
{
    class DefragRun : TableMapping
    {
        public bool IsLikelySameRun(DefragRun otherRun)
        {
            return this.serverName == otherRun.serverName
                && this.totalMilliseconds == otherRun.totalMilliseconds
                && this.playerName == otherRun.playerName
                && this.runnerClientNum == otherRun.runnerClientNum
                && this.style == otherRun.style
                && Math.Abs(this.serverTime.GetValueOrDefault(-99999) - otherRun.serverTime.GetValueOrDefault(-99999)) <= 1000L; // Defrag runs are usually registered via prints, which are reliable commands, so in theory this is uncertain since we can potentially get a print many seconds later due to extreme loss of packets. But it will simply have to do.
                
        }
        public TEXT? map { get; set; } = null;
        public TEXT? serverName { get; set; } = null;
        public TEXT? serverNameStripped { get; set; } = null;
        public TEXT? readableTime { get; set; } = null;
        public INTEGER? totalMilliseconds { get; set; } = null;
        public TEXT? style { get; set; } = null;
        public TEXT? playerName { get; set; } = null;
        public TEXT? playerNameStripped { get; set; } = null;
        public BOOLEAN? isTop10 { get; set; } = null;
        public BOOLEAN? isNumber1 { get; set; } = null;
        public BOOLEAN? isPersonalBest { get; set; } = null;
        public BOOLEAN? wasVisible { get; set; } = null;
        public BOOLEAN? wasFollowed { get; set; } = null;
        public BOOLEAN? wasFollowedOrVisible { get; set; } = null;
        public REAL? averageStrafeDeviation { get; set; } = null;
        public INTEGER? runnerClientNum { get; set; } = null;
        public INTEGER? resultingLaughs { get; set; } = null;
        public INTEGER? demoRecorderClientnum { get; set; } = null;
        public TEXT? demoName { get; set; } = null;
        public TEXT? demoPath { get; set; } = null;
        public INTEGER? demoTime { get; set; } = null;
        public INTEGER? lastGamestateDemoTime { get; set; } = null;
        public INTEGER? serverTime { get; set; } = null;
        public TIMESTAMP? demoDateTime { get; set; } = null;
    }
}
