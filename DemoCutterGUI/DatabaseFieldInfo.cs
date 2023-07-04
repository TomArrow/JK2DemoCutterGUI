using PropertyChanged;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoCutterGUI
{
    public class DatabaseFieldInfo : INotifyPropertyChanged // Used for search
    {

        public bool Nullable { get; set; } = false;
        public bool IsNull { get; set; } = false;
        public bool Bool { get; set; } = false;
        [DependsOn("Bool")]
        public bool NotBool { 
            get {
                return !Bool;
            }
        }
        public bool BoolContent { get; set; } = false;
        public bool Active { get; set; } = false;
        public string FieldName { get; set; } = "";
        public string Content { get; set; } = "";
        public string Category { get; set; } = "";
        public string SubCategory { get; set; } = "";

        public event PropertyChangedEventHandler PropertyChanged;


        public static DatabaseFieldInfo[] GetDatabaseFieldInfos()
        {
            return new DatabaseFieldInfo[] {

                // Kills
                new DatabaseFieldInfo(){ FieldName="hash", Nullable = true, Bool = false, Category="Kills",SubCategory="Kill"},
                new DatabaseFieldInfo(){ FieldName="shorthash", Nullable = true, Bool = false, Category="Kills",SubCategory="Kill"},
                new DatabaseFieldInfo(){ FieldName="map", Nullable = false, Bool = false, Category="Kills",SubCategory="Names"},
                new DatabaseFieldInfo(){ FieldName="serverName", Nullable = false, Bool = false, Category="Kills",SubCategory="Names"},
                new DatabaseFieldInfo(){ FieldName="serverNameStripped", Nullable = false, Bool = false, Category="Kills",SubCategory="Names"},
                new DatabaseFieldInfo(){ FieldName="killerName", Nullable = false, Bool = false, Category="Kills",SubCategory="Names"},
                new DatabaseFieldInfo(){ FieldName="killerNameStripped", Nullable = false, Bool = false, Category="Kills",SubCategory="Names"},
                new DatabaseFieldInfo(){ FieldName="victimName", Nullable = false, Bool = false, Category="Kills",SubCategory="Names"},
                new DatabaseFieldInfo(){ FieldName="victimNameStripped", Nullable = false, Bool = false, Category="Kills",SubCategory="Names"},
                new DatabaseFieldInfo(){ FieldName="killerTeam", Nullable = true, Bool = false, Category="Kills",SubCategory="Names"},
                new DatabaseFieldInfo(){ FieldName="victimTeam", Nullable = false, Bool = false, Category="Kills",SubCategory="Names"},
                new DatabaseFieldInfo(){ FieldName="redScore", Nullable = true, Bool = false, Category="Kills",SubCategory="Names"},
                new DatabaseFieldInfo(){ FieldName="blueScore", Nullable = true, Bool = false, Category="Kills",SubCategory="Names"},
                new DatabaseFieldInfo(){ FieldName="otherFlagStatus", Nullable = true, Bool = false, Category="Kills",SubCategory="Names"},
                new DatabaseFieldInfo(){ FieldName="redPlayerCount", Nullable = true, Bool = false, Category="Kills",SubCategory="Names"},
                new DatabaseFieldInfo(){ FieldName="bluePlayerCount", Nullable = true, Bool = false, Category="Kills",SubCategory="Names"},
                new DatabaseFieldInfo(){ FieldName="sumPlayerCount", Nullable = true, Bool = false, Category="Kills",SubCategory="Names"},
                new DatabaseFieldInfo(){ FieldName="killerClientNum", Nullable = false, Bool = false, Category="Kills",SubCategory="Names"},
                new DatabaseFieldInfo(){ FieldName="victimClientNum", Nullable = false, Bool = false, Category="Kills",SubCategory="Names"},
                new DatabaseFieldInfo(){ FieldName="isDoomKill", Nullable = false, Bool = true, Category="Kills",SubCategory="Kill"},
                new DatabaseFieldInfo(){ FieldName="isExplosion", Nullable = false, Bool = true, Category="Kills",SubCategory="Kill"},
                new DatabaseFieldInfo(){ FieldName="isSuicide", Nullable = false, Bool = true, Category="Kills",SubCategory="Kill"},
                new DatabaseFieldInfo(){ FieldName="isModSuicide", Nullable = false, Bool = true, Category="Kills",SubCategory="Kill"},
                new DatabaseFieldInfo(){ FieldName="meansOfDeath", Nullable = false, Bool = false, Category="Kills",SubCategory="Kill"},
                new DatabaseFieldInfo(){ FieldName="positionX", Nullable = true, Bool = false, Category="Kills",SubCategory="Position"},
                new DatabaseFieldInfo(){ FieldName="positionY", Nullable = true, Bool = false, Category="Kills",SubCategory="Position"},
                new DatabaseFieldInfo(){ FieldName="positionZ", Nullable = true, Bool = false, Category="Kills",SubCategory="Position"},

                // KillAngles
                //new DatabaseFieldInfo(){ FieldName="hash", Nullable = true, Bool = false, Category="KillAngles",SubCategory=""},
                //new DatabaseFieldInfo(){ FieldName="shorthash", Nullable = true, Bool = false, Category="KillAngles",SubCategory=""},
                //new DatabaseFieldInfo(){ FieldName="map", Nullable = false, Bool = false, Category="KillAngles",SubCategory=""},
                new DatabaseFieldInfo(){ FieldName="killerIsFlagCarrier", Nullable = false, Bool = true, Category="KillAngles",SubCategory="Kill"},
                new DatabaseFieldInfo(){ FieldName="isReturn", Nullable = false, Bool = true, Category="KillAngles",SubCategory="Kill"},
                new DatabaseFieldInfo(){ FieldName="victimCapperKills", Nullable = true, Bool = false, Category="KillAngles",SubCategory="Kill"},
                new DatabaseFieldInfo(){ FieldName="victimCapperRets", Nullable = true, Bool = false, Category="KillAngles",SubCategory="Kill"},
                new DatabaseFieldInfo(){ FieldName="victimCapperWasFollowedOrVisible", Nullable = true, Bool = true, Category="KillAngles",SubCategory=""},
                new DatabaseFieldInfo(){ FieldName="victimCapperMaxNearbyEnemyCount", Nullable = true, Bool = false, Category="KillAngles",SubCategory=""},
                new DatabaseFieldInfo(){ FieldName="victimCapperMoreThanOneNearbyEnemyTimePercent", Nullable = true, Bool = false, Category="KillAngles",SubCategory=""},
                new DatabaseFieldInfo(){ FieldName="victimCapperAverageNearbyEnemyCount", Nullable = true, Bool = false, Category="KillAngles",SubCategory=""},
                new DatabaseFieldInfo(){ FieldName="victimCapperMaxVeryCloseEnemyCount", Nullable = true, Bool = false, Category="KillAngles",SubCategory=""},
                new DatabaseFieldInfo(){ FieldName="victimCapperAnyVeryCloseEnemyTimePercent", Nullable = true, Bool = false, Category="KillAngles",SubCategory=""},
                new DatabaseFieldInfo(){ FieldName="victimCapperMoreThanOneVeryCloseEnemyTimePercent", Nullable = true, Bool = false, Category="KillAngles",SubCategory=""},
                new DatabaseFieldInfo(){ FieldName="victimCapperAverageVeryCloseEnemyCount", Nullable = true, Bool = false, Category="KillAngles",SubCategory=""},
                new DatabaseFieldInfo(){ FieldName="victimFlagPickupSource", Nullable = true, Bool = false, Category="KillAngles",SubCategory="Names"},
                new DatabaseFieldInfo(){ FieldName="victimFlagHoldTime", Nullable = true, Bool = false, Category="KillAngles",SubCategory="Names"},
                new DatabaseFieldInfo(){ FieldName="targetIsVisible", Nullable = false, Bool = true, Category="KillAngles",SubCategory="Names"},
                new DatabaseFieldInfo(){ FieldName="targetIsFollowed", Nullable = false, Bool = true, Category="KillAngles",SubCategory="Names"},
                new DatabaseFieldInfo(){ FieldName="targetIsFollowedOrVisible", Nullable = false, Bool = true, Category="KillAngles",SubCategory="Kill"},
                //new DatabaseFieldInfo(){ FieldName="isSuicide", Nullable = false, Bool = true, Category="KillAngles",SubCategory=""},
                //new DatabaseFieldInfo(){ FieldName="isModSuicide", Nullable = false, Bool = true, Category="KillAngles",SubCategory=""},
                new DatabaseFieldInfo(){ FieldName="attackerIsVisible", Nullable = false, Bool = true, Category="KillAngles",SubCategory="Names"},
                new DatabaseFieldInfo(){ FieldName="attackerIsFollowed", Nullable = false, Bool = true, Category="KillAngles",SubCategory="Names"},
                new DatabaseFieldInfo(){ FieldName="attackerIsFollowedOrVisible", Nullable = false, Bool = true, Category="KillAngles",SubCategory="Kill"},
                new DatabaseFieldInfo(){ FieldName="demoRecorderClientnum", Nullable = false, Bool = false, Category="KillAngles",SubCategory="Names"},
                new DatabaseFieldInfo(){ FieldName="boosts", Nullable = true, Bool = false, Category="KillAngles",SubCategory="Position"},
                new DatabaseFieldInfo(){ FieldName="boostCountTotal", Nullable = false, Bool = false, Category="KillAngles",SubCategory="Position"},
                new DatabaseFieldInfo(){ FieldName="boostCountAttacker", Nullable = false, Bool = false, Category="KillAngles",SubCategory="Position"},
                new DatabaseFieldInfo(){ FieldName="boostCountVictim", Nullable = false, Bool = false, Category="KillAngles",SubCategory="Position"},
                new DatabaseFieldInfo(){ FieldName="projectileWasAirborne", Nullable = true, Bool = true, Category="KillAngles",SubCategory="Kill"},
                new DatabaseFieldInfo(){ FieldName="baseFlagDistance", Nullable = true, Bool = false, Category="KillAngles",SubCategory="Position"},
                new DatabaseFieldInfo(){ FieldName="headJumps", Nullable = true, Bool = false, Category="KillAngles",SubCategory="Position"},
                new DatabaseFieldInfo(){ FieldName="specialJumps", Nullable = true, Bool = false, Category="KillAngles",SubCategory="Position"},
                new DatabaseFieldInfo(){ FieldName="timeSinceLastSelfSentryJump", Nullable = true, Bool = false, Category="KillAngles",SubCategory="Position"},
                new DatabaseFieldInfo(){ FieldName="resultingCaptures", Nullable = true, Bool = false, Category="KillAngles",SubCategory="Kill"},
                new DatabaseFieldInfo(){ FieldName="resultingSelfCaptures", Nullable = true, Bool = false, Category="KillAngles",SubCategory="Kill"},
                new DatabaseFieldInfo(){ FieldName="metaEvents", Nullable = true, Bool = false, Category="KillAngles",SubCategory="Names"},
                new DatabaseFieldInfo(){ FieldName="maxAngularSpeedAttacker", Nullable = true, Bool = false, Category="KillAngles",SubCategory="Position"},
                new DatabaseFieldInfo(){ FieldName="maxAngularAccelerationAttacker", Nullable = true, Bool = false, Category="KillAngles",SubCategory="Position"},
                new DatabaseFieldInfo(){ FieldName="maxAngularJerkAttacker", Nullable = true, Bool = false, Category="KillAngles",SubCategory="Position"},
                new DatabaseFieldInfo(){ FieldName="maxAngularSnapAttacker", Nullable = true, Bool = false, Category="KillAngles",SubCategory="Position"},
                new DatabaseFieldInfo(){ FieldName="maxSpeedAttacker", Nullable = true, Bool = false, Category="KillAngles",SubCategory="Position"},
                new DatabaseFieldInfo(){ FieldName="maxSpeedTarget", Nullable = true, Bool = false, Category="KillAngles",SubCategory="Position"},
                new DatabaseFieldInfo(){ FieldName="currentSpeedAttacker", Nullable = true, Bool = false, Category="KillAngles",SubCategory="Position"},
                new DatabaseFieldInfo(){ FieldName="currentSpeedTarget", Nullable = true, Bool = false, Category="KillAngles",SubCategory="Position"},
                new DatabaseFieldInfo(){ FieldName="lastSaberMoveChangeSpeed", Nullable = true, Bool = false, Category="KillAngles",SubCategory="Position"},
                new DatabaseFieldInfo(){ FieldName="timeSinceLastSaberMoveChange", Nullable = true, Bool = false, Category="KillAngles",SubCategory="Position"},
                new DatabaseFieldInfo(){ FieldName="timeSinceLastBackflip", Nullable = true, Bool = false, Category="KillAngles",SubCategory="Position"},
                new DatabaseFieldInfo(){ FieldName="meansOfDeathString", Nullable = false, Bool = false, Category="KillAngles",SubCategory="Kill"},
                new DatabaseFieldInfo(){ FieldName="nearbyPlayers", Nullable = true, Bool = false, Category="KillAngles",SubCategory="Names"},
                new DatabaseFieldInfo(){ FieldName="nearbyPlayerCount", Nullable = false, Bool = false, Category="KillAngles",SubCategory="Names"},
                new DatabaseFieldInfo(){ FieldName="probableKillingWeapon", Nullable = false, Bool = false, Category="KillAngles",SubCategory="Kill"},
                new DatabaseFieldInfo(){ FieldName="attackerJumpHeight", Nullable = true, Bool = false, Category="KillAngles",SubCategory="Position"},
                new DatabaseFieldInfo(){ FieldName="victimJumpHeight", Nullable = true, Bool = false, Category="KillAngles",SubCategory="Position"},
                new DatabaseFieldInfo(){ FieldName="directionX", Nullable = true, Bool = false, Category="KillAngles",SubCategory="Position"},
                new DatabaseFieldInfo(){ FieldName="directionY", Nullable = true, Bool = false, Category="KillAngles",SubCategory="Position"},
                new DatabaseFieldInfo(){ FieldName="directionZ", Nullable = true, Bool = false, Category="KillAngles",SubCategory="Position"},
                new DatabaseFieldInfo(){ FieldName="demoName", Nullable = false, Bool = false, Category="KillAngles",SubCategory="Names"},
                new DatabaseFieldInfo(){ FieldName="demoPath", Nullable = false, Bool = false, Category="KillAngles",SubCategory="Names"},
                new DatabaseFieldInfo(){ FieldName="demoTime", Nullable = false, Bool = false, Category="KillAngles",SubCategory="Kill"},
                new DatabaseFieldInfo(){ FieldName="serverTime", Nullable = false, Bool = false, Category="KillAngles",SubCategory="Kill"},
                new DatabaseFieldInfo(){ FieldName="demoDateTime", Nullable = false, Bool =false, Category="KillAngles",SubCategory="Kill"},

                // Captures
                new DatabaseFieldInfo(){ FieldName="id", Nullable = true, Bool = false, Category="Captures", SubCategory=""},
                new DatabaseFieldInfo(){ FieldName="map", Nullable = false, Bool = false, Category="Captures", SubCategory=""},
                new DatabaseFieldInfo(){ FieldName="serverName", Nullable = false, Bool = false, Category="Captures", SubCategory=""},
                new DatabaseFieldInfo(){ FieldName="serverNameStripped", Nullable = false, Bool = false, Category="Captures", SubCategory=""},
                new DatabaseFieldInfo(){ FieldName="flagHoldTime", Nullable = false, Bool = false, Category="Captures", SubCategory=""},
                new DatabaseFieldInfo(){ FieldName="flagPickupSource", Nullable = false, Bool = false, Category="Captures", SubCategory=""},
                new DatabaseFieldInfo(){ FieldName="capperName", Nullable = false, Bool = false, Category="Captures", SubCategory=""},
                new DatabaseFieldInfo(){ FieldName="capperNameStripped", Nullable = false, Bool = false, Category="Captures", SubCategory=""},
                new DatabaseFieldInfo(){ FieldName="capperClientNum", Nullable = false, Bool = false, Category="Captures", SubCategory=""},
                new DatabaseFieldInfo(){ FieldName="capperIsVisible", Nullable = false, Bool = true, Category="Captures", SubCategory=""},
                new DatabaseFieldInfo(){ FieldName="capperIsFollowed", Nullable = false, Bool = true, Category="Captures", SubCategory=""},
                new DatabaseFieldInfo(){ FieldName="capperIsFollowedOrVisible", Nullable = false, Bool = true, Category="Captures", SubCategory=""},
                new DatabaseFieldInfo(){ FieldName="capperWasVisible", Nullable = false, Bool = true, Category="Captures", SubCategory=""},
                new DatabaseFieldInfo(){ FieldName="capperWasFollowed", Nullable = false, Bool = true, Category="Captures", SubCategory=""},
                new DatabaseFieldInfo(){ FieldName="capperWasFollowedOrVisible", Nullable = false, Bool = true, Category="Captures", SubCategory=""},
                new DatabaseFieldInfo(){ FieldName="demoRecorderClientnum", Nullable = false, Bool = false, Category="Captures", SubCategory=""},
                new DatabaseFieldInfo(){ FieldName="flagTeam", Nullable = false, Bool = false, Category="Captures", SubCategory=""},
                new DatabaseFieldInfo(){ FieldName="capperKills", Nullable = false, Bool = false, Category="Captures", SubCategory=""},
                new DatabaseFieldInfo(){ FieldName="capperRets", Nullable = false, Bool = false, Category="Captures", SubCategory=""},
                new DatabaseFieldInfo(){ FieldName="redScore", Nullable = true, Bool = false, Category="Captures", SubCategory=""},
                new DatabaseFieldInfo(){ FieldName="blueScore", Nullable = true, Bool = false, Category="Captures", SubCategory=""},
                new DatabaseFieldInfo(){ FieldName="redPlayerCount", Nullable = true, Bool = false, Category="Captures", SubCategory=""},
                new DatabaseFieldInfo(){ FieldName="bluePlayerCount", Nullable = true, Bool = false, Category="Captures", SubCategory=""},
                new DatabaseFieldInfo(){ FieldName="sumPlayerCount", Nullable = true, Bool = false, Category="Captures", SubCategory=""},
                new DatabaseFieldInfo(){ FieldName="maxSpeedCapperLastSecond", Nullable = true, Bool = false, Category="Captures", SubCategory=""},
                new DatabaseFieldInfo(){ FieldName="maxSpeedCapper", Nullable = true, Bool = false, Category="Captures", SubCategory=""},
                new DatabaseFieldInfo(){ FieldName="averageSpeedCapper", Nullable = true, Bool = false, Category="Captures", SubCategory=""},
                new DatabaseFieldInfo(){ FieldName="metaEvents", Nullable = true, Bool = false, Category="Captures", SubCategory=""},
                new DatabaseFieldInfo(){ FieldName="nearbyPlayers", Nullable = true, Bool = false, Category="Captures", SubCategory=""},
                new DatabaseFieldInfo(){ FieldName="nearbyPlayerCount", Nullable = false, Bool = false, Category="Captures", SubCategory=""},
                new DatabaseFieldInfo(){ FieldName="nearbyEnemies", Nullable = true, Bool = false, Category="Captures", SubCategory=""},
                new DatabaseFieldInfo(){ FieldName="nearbyEnemyCount", Nullable = false, Bool = false, Category="Captures", SubCategory=""},
                new DatabaseFieldInfo(){ FieldName="maxNearbyEnemyCount", Nullable = false, Bool = false, Category="Captures", SubCategory=""},
                new DatabaseFieldInfo(){ FieldName="moreThanOneNearbyEnemyTimePercent", Nullable = false, Bool = false, Category="Captures", SubCategory=""},
                new DatabaseFieldInfo(){ FieldName="averageNearbyEnemyCount", Nullable = false, Bool = false, Category="Captures", SubCategory=""},
                new DatabaseFieldInfo(){ FieldName="maxVeryCloseEnemyCount", Nullable = false, Bool = false, Category="Captures", SubCategory=""},
                new DatabaseFieldInfo(){ FieldName="anyVeryCloseEnemyTimePercent", Nullable = false, Bool = false, Category="Captures", SubCategory=""},
                new DatabaseFieldInfo(){ FieldName="moreThanOneVeryCloseEnemyTimePercent", Nullable = false, Bool = false, Category="Captures", SubCategory=""},
                new DatabaseFieldInfo(){ FieldName="averageVeryCloseEnemyCount", Nullable = false, Bool = false, Category="Captures", SubCategory=""},
                new DatabaseFieldInfo(){ FieldName="directionX", Nullable = true, Bool = false, Category="Captures", SubCategory=""},
                new DatabaseFieldInfo(){ FieldName="directionY", Nullable = true, Bool = false, Category="Captures", SubCategory=""},
                new DatabaseFieldInfo(){ FieldName="directionZ", Nullable = true, Bool = false, Category="Captures", SubCategory=""},
                new DatabaseFieldInfo(){ FieldName="positionX", Nullable = true, Bool = false, Category="Captures", SubCategory=""},
                new DatabaseFieldInfo(){ FieldName="positionY", Nullable = true, Bool = false, Category="Captures", SubCategory=""},
                new DatabaseFieldInfo(){ FieldName="positionZ", Nullable = true, Bool = false, Category="Captures", SubCategory=""},
                new DatabaseFieldInfo(){ FieldName="demoName", Nullable = false, Bool = false, Category="Captures", SubCategory=""},
                new DatabaseFieldInfo(){ FieldName="demoPath", Nullable = false, Bool = false, Category="Captures", SubCategory=""},
                new DatabaseFieldInfo(){ FieldName="demoTime", Nullable = false, Bool = false, Category="Captures", SubCategory=""},
                new DatabaseFieldInfo(){ FieldName="serverTime", Nullable = false, Bool = false, Category="Captures", SubCategory=""},
                new DatabaseFieldInfo(){ FieldName="demoDateTime", Nullable = false, Bool =false, Category="Captures", SubCategory=""},

                // Killsprees
                new DatabaseFieldInfo(){ FieldName="hash", Nullable = true, Bool = false, Category="KillSprees", SubCategory=""},
                new DatabaseFieldInfo(){ FieldName="shorthash", Nullable = true, Bool = false, Category="KillSprees", SubCategory=""},
                new DatabaseFieldInfo(){ FieldName="maxDelay", Nullable = false, Bool = false, Category="KillSprees", SubCategory=""},
                new DatabaseFieldInfo(){ FieldName="maxDelayActual", Nullable = false, Bool = false, Category="KillSprees", SubCategory=""},
                new DatabaseFieldInfo(){ FieldName="map", Nullable = false, Bool = false, Category="KillSprees", SubCategory=""},
                new DatabaseFieldInfo(){ FieldName="killerName", Nullable = false, Bool = false, Category="KillSprees", SubCategory=""},
                new DatabaseFieldInfo(){ FieldName="killerNameStripped", Nullable = false, Bool = false, Category="KillSprees", SubCategory=""},
                new DatabaseFieldInfo(){ FieldName="victimNames", Nullable = false, Bool = false, Category="KillSprees", SubCategory=""},
                new DatabaseFieldInfo(){ FieldName="victimNamesStripped", Nullable = false, Bool = false, Category="KillSprees", SubCategory=""},
                new DatabaseFieldInfo(){ FieldName="killTypes", Nullable = false, Bool = false, Category="KillSprees", SubCategory=""},
                new DatabaseFieldInfo(){ FieldName="killTypesCount", Nullable = false, Bool = false, Category="KillSprees", SubCategory=""},
                new DatabaseFieldInfo(){ FieldName="killHashes", Nullable = false, Bool = false, Category="KillSprees", SubCategory=""},
                new DatabaseFieldInfo(){ FieldName="killerClientNum", Nullable = false, Bool = false, Category="KillSprees", SubCategory=""},
                new DatabaseFieldInfo(){ FieldName="victimClientNums", Nullable = false, Bool = false, Category="KillSprees", SubCategory=""},
                new DatabaseFieldInfo(){ FieldName="countKills", Nullable = false, Bool = false, Category="KillSprees", SubCategory=""},
                new DatabaseFieldInfo(){ FieldName="countRets", Nullable = false, Bool = false, Category="KillSprees", SubCategory=""},
                new DatabaseFieldInfo(){ FieldName="countDooms", Nullable = false, Bool = false, Category="KillSprees", SubCategory=""},
                new DatabaseFieldInfo(){ FieldName="countExplosions", Nullable = false, Bool = false, Category="KillSprees", SubCategory=""},
                new DatabaseFieldInfo(){ FieldName="countThirdPersons", Nullable = false, Bool = false, Category="KillSprees", SubCategory=""},
                new DatabaseFieldInfo(){ FieldName="nearbyPlayers", Nullable = true, Bool = false, Category="KillSprees", SubCategory=""},
                new DatabaseFieldInfo(){ FieldName="nearbyPlayerCount", Nullable = false, Bool = false, Category="KillSprees", SubCategory=""},
                new DatabaseFieldInfo(){ FieldName="demoRecorderClientnum", Nullable = false, Bool = false, Category="KillSprees", SubCategory=""},
                new DatabaseFieldInfo(){ FieldName="maxSpeedAttacker", Nullable = true, Bool = false, Category="KillSprees", SubCategory=""},
                new DatabaseFieldInfo(){ FieldName="maxSpeedTargets", Nullable = true, Bool = false, Category="KillSprees", SubCategory=""},
                new DatabaseFieldInfo(){ FieldName="resultingCaptures", Nullable = true, Bool = false, Category="KillSprees", SubCategory=""},
                new DatabaseFieldInfo(){ FieldName="resultingSelfCaptures", Nullable = true, Bool = false, Category="KillSprees", SubCategory=""},
                new DatabaseFieldInfo(){ FieldName="resultingCapturesAfter", Nullable = true, Bool = false, Category="KillSprees", SubCategory=""},
                new DatabaseFieldInfo(){ FieldName="resultingSelfCapturesAfter", Nullable = true, Bool = false, Category="KillSprees", SubCategory=""},
                new DatabaseFieldInfo(){ FieldName="metaEvents", Nullable = true, Bool = false, Category="KillSprees", SubCategory=""},
                new DatabaseFieldInfo(){ FieldName="demoName", Nullable = false, Bool = false, Category="KillSprees", SubCategory=""},
                new DatabaseFieldInfo(){ FieldName="demoPath", Nullable = false, Bool = false, Category="KillSprees", SubCategory=""},
                new DatabaseFieldInfo(){ FieldName="demoTime", Nullable = false, Bool = false, Category="KillSprees", SubCategory=""},
                new DatabaseFieldInfo(){ FieldName="duration", Nullable = false, Bool = false, Category="KillSprees", SubCategory=""},
                new DatabaseFieldInfo(){ FieldName="serverTime", Nullable = false, Bool = false, Category="KillSprees", SubCategory=""},
                new DatabaseFieldInfo(){ FieldName="demoDateTime", Nullable = false, Bool =false, Category="KillSprees", SubCategory=""},

                // DefragRuns
                new DatabaseFieldInfo(){ FieldName="map", Nullable = false, Bool = false, Category="DefragRuns", SubCategory=""},
                new DatabaseFieldInfo(){ FieldName="serverName", Nullable = false, Bool = false, Category="DefragRuns", SubCategory=""},
                new DatabaseFieldInfo(){ FieldName="serverNameStripped", Nullable = false, Bool = false, Category="DefragRuns", SubCategory=""},
                new DatabaseFieldInfo(){ FieldName="readableTime", Nullable = false, Bool = false, Category="DefragRuns", SubCategory=""},
                new DatabaseFieldInfo(){ FieldName="totalMilliseconds", Nullable = true, Bool = false, Category="DefragRuns", SubCategory=""},
                new DatabaseFieldInfo(){ FieldName="playerName", Nullable = false, Bool = false, Category="DefragRuns", SubCategory=""},
                new DatabaseFieldInfo(){ FieldName="playerNameStripped", Nullable = false, Bool = false, Category="DefragRuns", SubCategory=""},
                new DatabaseFieldInfo(){ FieldName="isTop10", Nullable = false, Bool = true, Category="DefragRuns", SubCategory=""},
                new DatabaseFieldInfo(){ FieldName="isNumber1", Nullable = false, Bool = true, Category="DefragRuns", SubCategory=""},
                new DatabaseFieldInfo(){ FieldName="isPersonalBest", Nullable = false, Bool = true, Category="DefragRuns", SubCategory=""},
                new DatabaseFieldInfo(){ FieldName="wasVisible", Nullable = false, Bool = true, Category="DefragRuns", SubCategory=""},
                new DatabaseFieldInfo(){ FieldName="wasFollowed", Nullable = false, Bool = true, Category="DefragRuns", SubCategory=""},
                new DatabaseFieldInfo(){ FieldName="wasFollowedOrVisible", Nullable = false, Bool = true, Category="DefragRuns", SubCategory=""},
                new DatabaseFieldInfo(){ FieldName="averageStrafeDeviation", Nullable = true, Bool = false, Category="DefragRuns", SubCategory=""},
                new DatabaseFieldInfo(){ FieldName="runnerClientNum", Nullable = false, Bool = false, Category="DefragRuns", SubCategory=""},
                new DatabaseFieldInfo(){ FieldName="demoRecorderClientnum", Nullable = false, Bool = false, Category="DefragRuns", SubCategory=""},
                new DatabaseFieldInfo(){ FieldName="demoName", Nullable = false, Bool = false, Category="DefragRuns", SubCategory=""},
                new DatabaseFieldInfo(){ FieldName="demoPath", Nullable = false, Bool = false, Category="DefragRuns", SubCategory=""},
                new DatabaseFieldInfo(){ FieldName="demoTime", Nullable = false, Bool = false, Category="DefragRuns", SubCategory=""},
                new DatabaseFieldInfo(){ FieldName="serverTime", Nullable = false, Bool = false, Category="DefragRuns", SubCategory=""},
                new DatabaseFieldInfo(){ FieldName="demoDateTime", Nullable = false, Bool =false, Category="DefragRuns", SubCategory=""},

                // Laughs
                new DatabaseFieldInfo(){ FieldName="id", Nullable = true, Bool = false, Category="Laughs", SubCategory=""},
                new DatabaseFieldInfo(){ FieldName="map", Nullable = false, Bool = false, Category="Laughs", SubCategory=""},
                new DatabaseFieldInfo(){ FieldName="serverName", Nullable = false, Bool = false, Category="Laughs", SubCategory=""},
                new DatabaseFieldInfo(){ FieldName="serverNameStripped", Nullable = false, Bool = false, Category="Laughs", SubCategory=""},
                new DatabaseFieldInfo(){ FieldName="laughs", Nullable = false, Bool = false, Category="Laughs", SubCategory=""},
                new DatabaseFieldInfo(){ FieldName="chatlog", Nullable = false, Bool = false, Category="Laughs", SubCategory=""},
                new DatabaseFieldInfo(){ FieldName="chatlogStripped", Nullable = false, Bool = false, Category="Laughs", SubCategory=""},
                new DatabaseFieldInfo(){ FieldName="laughCount", Nullable = false, Bool = false, Category="Laughs", SubCategory=""},
                new DatabaseFieldInfo(){ FieldName="demoRecorderClientnum", Nullable = false, Bool = false, Category="Laughs", SubCategory=""},
                new DatabaseFieldInfo(){ FieldName="demoName", Nullable = false, Bool = false, Category="Laughs", SubCategory=""},
                new DatabaseFieldInfo(){ FieldName="demoPath", Nullable = false, Bool = false, Category="Laughs", SubCategory=""},
                new DatabaseFieldInfo(){ FieldName="duration", Nullable = false, Bool = false, Category="Laughs", SubCategory=""},
                new DatabaseFieldInfo(){ FieldName="demoTime", Nullable = false, Bool = false, Category="Laughs", SubCategory=""},
                new DatabaseFieldInfo(){ FieldName="serverTime", Nullable = false, Bool = false, Category="Laughs", SubCategory=""},
                new DatabaseFieldInfo(){ FieldName="demoDateTime", Nullable = false, Bool =false, Category="Laughs", SubCategory=""},
            };
        }
    }

}
