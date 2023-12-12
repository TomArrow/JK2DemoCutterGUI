using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using INTEGER = System.Int64;
using TEXT = System.String;
using REAL = System.Double;
using TIMESTAMP = System.Int64;

namespace DemoCutterGUI.TableMappings
{
    class KillSpree : TableMapping
    {
        public TEXT? hash { get; set; } = null;
        public TEXT? shorthash { get; set; } = null;
        public INTEGER? maxDelay { get; set; } = null;
        public INTEGER? maxDelayActual { get; set; } = null;
        public TEXT? map { get; set; } = null;
        public TEXT? killerName { get; set; } = null;
        public TEXT? killerNameStripped { get; set; } = null;
        public TEXT? victimNames { get; set; } = null;
        public TEXT? victimNamesStripped { get; set; } = null;
        public TEXT? killTypes { get; set; } = null;
        public INTEGER? killTypesCount { get; set; } = null;
        public TEXT? killHashes { get; set; } = null;
        public INTEGER? killerClientNum { get; set; } = null;
        public TEXT? victimClientNums { get; set; } = null;
        public INTEGER? countKills { get; set; } = null;
        public INTEGER? countRets { get; set; } = null;
        public INTEGER? countTeamKills { get; set; } = null;
        public INTEGER? countUniqueTargets { get; set; } = null;
        public INTEGER? countDooms { get; set; } = null;
        public INTEGER? countExplosions { get; set; } = null;
        public INTEGER? countThirdPersons { get; set; } = null;
        public TEXT? nearbyPlayers { get; set; } = null;
        public INTEGER? nearbyPlayerCount { get; set; } = null;
        public INTEGER? demoRecorderClientnum { get; set; } = null;
        public REAL? maxSpeedAttacker { get; set; } = null;
        public REAL? maxSpeedTargets { get; set; } = null;
        public INTEGER? resultingCaptures { get; set; } = null;
        public INTEGER? resultingSelfCaptures { get; set; } = null;
        public INTEGER? resultingCapturesAfter { get; set; } = null;
        public INTEGER? resultingSelfCapturesAfter { get; set; } = null;
        public INTEGER? resultingLaughs { get; set; } = null;
        public INTEGER? resultingLaughsAfter { get; set; } = null;
        public TEXT? metaEvents { get; set; } = null;
        public TEXT? demoName { get; set; } = null;
        public TEXT? demoPath { get; set; } = null;
        public INTEGER? demoTime { get; set; } = null;
        public INTEGER? lastGamestateDemoTime { get; set; } = null;
        public INTEGER? duration { get; set; } = null;
        public INTEGER? serverTime { get; set; } = null;
        public TIMESTAMP? demoDateTime { get; set; } = null;
    }
}
