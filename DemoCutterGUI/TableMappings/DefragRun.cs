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
    class DefragRun
    {
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
