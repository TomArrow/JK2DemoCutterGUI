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
    class Capture : TableMapping
    {
        public INTEGER? id { get; set; } = null;
        public TEXT? map { get; set; } = null;
        public TEXT? serverName { get; set; } = null;
        public TEXT? serverNameStripped { get; set; } = null;
        public INTEGER? flagHoldTime { get; set; } = null;
        public INTEGER? flagPickupSource { get; set; } = null;
        public TEXT? capperName { get; set; } = null;
        public TEXT? capperNameStripped { get; set; } = null;
        public INTEGER? capperClientNum { get; set; } = null;
        public BOOLEAN? capperIsVisible { get; set; } = null;
        public BOOLEAN? capperIsFollowed { get; set; } = null;
        public BOOLEAN? capperIsFollowedOrVisible { get; set; } = null;
        public BOOLEAN? capperWasVisible { get; set; } = null;
        public BOOLEAN? capperWasFollowed { get; set; } = null;
        public BOOLEAN? capperWasFollowedOrVisible { get; set; } = null;
        public INTEGER? demoRecorderClientnum { get; set; } = null;
        public INTEGER? flagTeam { get; set; } = null;
        public INTEGER? capperKills { get; set; } = null;
        public INTEGER? capperRets { get; set; } = null;
        public INTEGER? redScore { get; set; } = null;
        public INTEGER? blueScore { get; set; } = null;
        public INTEGER? redPlayerCount { get; set; } = null;
        public INTEGER? bluePlayerCount { get; set; } = null;
        public INTEGER? sumPlayerCount { get; set; } = null;
        public REAL? maxSpeedCapperLastSecond { get; set; } = null;
        public REAL? maxSpeedCapper { get; set; } = null;
        public REAL? averageSpeedCapper { get; set; } = null;
        public TEXT? metaEvents { get; set; } = null;
        public TEXT? nearbyPlayers { get; set; } = null;
        public INTEGER? nearbyPlayerCount { get; set; } = null;
        public TEXT? nearbyEnemies { get; set; } = null;
        public INTEGER? nearbyEnemyCount { get; set; } = null;
        public REAL? maxNearbyEnemyCount { get; set; } = null;
        public REAL? moreThanOneNearbyEnemyTimePercent { get; set; } = null;
        public REAL? averageNearbyEnemyCount { get; set; } = null;
        public REAL? maxVeryCloseEnemyCount { get; set; } = null;
        public REAL? anyVeryCloseEnemyTimePercent { get; set; } = null;
        public REAL? moreThanOneVeryCloseEnemyTimePercent { get; set; } = null;
        public REAL? averageVeryCloseEnemyCount { get; set; } = null;
        public REAL? directionX { get; set; } = null;
        public REAL? directionY { get; set; } = null;
        public REAL? directionZ { get; set; } = null;
        public REAL? positionX { get; set; } = null;
        public REAL? positionY { get; set; } = null;
        public REAL? positionZ { get; set; } = null;
        public INTEGER? resultingLaughs { get; set; } = null;
        public INTEGER? resultingLaughsAfter { get; set; } = null;
        public TEXT? demoName { get; set; } = null;
        public TEXT? demoPath { get; set; } = null;
        public INTEGER? demoTime { get; set; } = null;
        public INTEGER? lastGamestateDemoTime { get; set; } = null;
        public INTEGER? serverTime { get; set; } = null;
        public TIMESTAMP? demoDateTime { get; set; } = null;
    }
}
