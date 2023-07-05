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
    public class Ret
    {
        // KillAngle
        public TEXT? hash { get; set; } = null;
        public TEXT? shorthash { get; set; } = null;
        public TEXT? map { get; set; } = null;
        public BOOLEAN? killerIsFlagCarrier { get; set; } = null;
        public BOOLEAN? isReturn { get; set; } = null;
        public INTEGER? victimCapperKills { get; set; } = null;
        public INTEGER? victimCapperRets { get; set; } = null;
        public BOOLEAN? victimCapperWasFollowedOrVisible { get; set; } = null;
        public REAL? victimCapperMaxNearbyEnemyCount { get; set; } = null;
        public REAL? victimCapperMoreThanOneNearbyEnemyTimePercent { get; set; } = null;
        public REAL? victimCapperAverageNearbyEnemyCount { get; set; } = null;
        public REAL? victimCapperMaxVeryCloseEnemyCount { get; set; } = null;
        public REAL? victimCapperAnyVeryCloseEnemyTimePercent { get; set; } = null;
        public REAL? victimCapperMoreThanOneVeryCloseEnemyTimePercent { get; set; } = null;
        public REAL? victimCapperAverageVeryCloseEnemyCount { get; set; } = null;
        public INTEGER? victimFlagPickupSource { get; set; } = null;
        public INTEGER? victimFlagHoldTime { get; set; } = null;
        public BOOLEAN? targetIsVisible { get; set; } = null;
        public BOOLEAN? targetIsFollowed { get; set; } = null;
        public BOOLEAN? targetIsFollowedOrVisible { get; set; } = null;
        public BOOLEAN? isSuicide { get; set; } = null;
        public BOOLEAN? isModSuicide { get; set; } = null;
        public BOOLEAN? attackerIsVisible { get; set; } = null;
        public BOOLEAN? attackerIsFollowed { get; set; } = null;
        public BOOLEAN? attackerIsFollowedOrVisible { get; set; } = null;
        public INTEGER? demoRecorderClientnum { get; set; } = null;
        public TEXT? boosts { get; set; } = null;
        public INTEGER? boostCountTotal { get; set; } = null;
        public INTEGER? boostCountAttacker { get; set; } = null;
        public INTEGER? boostCountVictim { get; set; } = null;
        public BOOLEAN? projectileWasAirborne { get; set; } = null;
        public REAL? baseFlagDistance { get; set; } = null;
        public INTEGER? headJumps { get; set; } = null;
        public INTEGER? specialJumps { get; set; } = null;
        public INTEGER? timeSinceLastSelfSentryJump { get; set; } = null;
        public INTEGER? resultingCaptures { get; set; } = null;
        public INTEGER? resultingSelfCaptures { get; set; } = null;
        public TEXT? metaEvents { get; set; } = null;
        public REAL? maxAngularSpeedAttacker { get; set; } = null;
        public REAL? maxAngularAccelerationAttacker { get; set; } = null;
        public REAL? maxAngularJerkAttacker { get; set; } = null;
        public REAL? maxAngularSnapAttacker { get; set; } = null;
        public REAL? maxSpeedAttacker { get; set; } = null;
        public REAL? maxSpeedTarget { get; set; } = null;
        public REAL? currentSpeedAttacker { get; set; } = null;
        public REAL? currentSpeedTarget { get; set; } = null;
        public REAL? lastSaberMoveChangeSpeed { get; set; } = null;
        public INTEGER? timeSinceLastSaberMoveChange { get; set; } = null;
        public INTEGER? timeSinceLastBackflip { get; set; } = null;
        public TEXT? meansOfDeathString { get; set; } = null;
        public TEXT? nearbyPlayers { get; set; } = null;
        public INTEGER? nearbyPlayerCount { get; set; } = null;
        public INTEGER? probableKillingWeapon { get; set; } = null;
        public REAL? attackerJumpHeight { get; set; } = null;
        public REAL? victimJumpHeight { get; set; } = null;
        public REAL? directionX { get; set; } = null;
        public REAL? directionY { get; set; } = null;
        public REAL? directionZ { get; set; } = null;
        public TEXT? demoName { get; set; } = null;
        public TEXT? demoPath { get; set; } = null;
        public INTEGER? demoTime { get; set; } = null;
        public INTEGER? serverTime { get; set; } = null;
        public TIMESTAMP? demoDateTime { get; set; } = null;

        // Kill

        //public TEXT? hash { get; set; } = null;
        //public TEXT? shorthash { get; set; } = null;
        //public TEXT? map { get; set; } = null;
        public TEXT? serverName { get; set; } = null;
        public TEXT? serverNameStripped { get; set; } = null;
        public TEXT? killerName { get; set; } = null;
        public TEXT? killerNameStripped { get; set; } = null;
        public TEXT? victimName { get; set; } = null;
        public TEXT? victimNameStripped { get; set; } = null;
        public INTEGER? killerTeam { get; set; } = null;
        public INTEGER? victimTeam { get; set; } = null;
        public INTEGER? redScore { get; set; } = null;
        public INTEGER? blueScore { get; set; } = null;
        public INTEGER? otherFlagStatus { get; set; } = null;
        public INTEGER? redPlayerCount { get; set; } = null;
        public INTEGER? bluePlayerCount { get; set; } = null;
        public INTEGER? sumPlayerCount { get; set; } = null;
        public INTEGER? killerClientNum { get; set; } = null;
        public INTEGER? victimClientNum { get; set; } = null;
        public BOOLEAN? isDoomKill { get; set; } = null;
        public BOOLEAN? isExplosion { get; set; } = null;
        //public BOOLEAN? isSuicide { get; set; } = null;
        //public BOOLEAN? isModSuicide { get; set; } = null;
        public INTEGER? meansOfDeath { get; set; } = null;
        public REAL? positionX { get; set; } = null;
        public REAL? positionY { get; set; } = null;
        public REAL? positionZ { get; set; } = null;
    }
}
