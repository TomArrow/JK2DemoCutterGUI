using PropertyChanged;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoCutterGUI
{
    public class DataBaseFieldInfoManager
    {
        static HashSet<DatabaseFieldInfo> fieldInfos = new HashSet<DatabaseFieldInfo>();
        static HashSet<DatabaseFieldInfo> activeFields = new HashSet<DatabaseFieldInfo>();

        public event EventHandler<DatabaseFieldInfo> fieldInfoChanged;

        private void OnFieldInfoChanged(DatabaseFieldInfo fi)
        {
            fieldInfoChanged?.Invoke(this,fi);
        }

        public void RegisterFieldInfo(DatabaseFieldInfo fi)
        {
            lock (fieldInfos)
            {
                fieldInfos.Add(fi);
                fi.PropertyChanged += Fi_PropertyChanged;
                if (fi.Active)
                {
                    activeFields.Add(fi);
                }
                else
                {
                    activeFields.Remove(fi);
                }
            }
        }

        public bool? IsFieldActive(DatabaseFieldInfo fi)
        {
            lock (fieldInfos) {
                if (fieldInfos.Contains(fi))
                {
                    return activeFields.Contains(fi);
                } else
                {
                    return null;
                }
            }
        }

        public DatabaseFieldInfo[] getActiveFields()
        {
            lock (fieldInfos)
            {
                return activeFields.ToArray();
            }
        }

        private void Fi_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            DatabaseFieldInfo fi = sender as DatabaseFieldInfo;
            lock (fieldInfos) { 

                if (fi.Active)
                {
                    activeFields.Add(fi);
                } else
                {
                    activeFields.Remove(fi);
                }
            }
            OnFieldInfoChanged(fi);
        }

        ~DataBaseFieldInfoManager()
        {
            Clear();
        }

        public void Clear()
        {
            lock (fieldInfos)
            {
                foreach (DatabaseFieldInfo fi in fieldInfos)
                {
                    fi.PropertyChanged += Fi_PropertyChanged;
                }
                fieldInfos.Clear();
                activeFields.Clear();
            }
        }


    }

    public class DatabaseFieldInfo : INotifyPropertyChanged // Used for search
    {

        public enum FieldCategory
        {
            None,
            Rets,
            Captures,
            DefragRuns,
            KillSprees,
            Laughs,
        }

        public enum FieldSubCategory
        {
            None,
            All, // Don't use this, it's just a helper to make stuff more convenient elsewhere
            Column1,
            Column2,
            Column3,
            Rets_Meta = Column1,
            Rets_Kill = Column2,
            Rets_Movement = Column3,
            Captures_Meta = Column1,
            Captures_Capture = Column2,
            Captures_Movement = Column3,
            KillSprees_Meta = Column1,
            KillSprees_Kills = Column2,
            KillSprees_Movement = Column3,
            Defrag_Meta = Column1,
            Defrag_Run = Column2,
            Defrag_Movement = Column3,
            Laughs_Meta = Column1,
            Laughs_Laughs = Column2,
            Laughs_Whatever = Column3,

        }

        public bool Numeric { get; init; } = false;
        public bool Nullable { get; init; } = false;
        public bool IsNull { get; set; } = false;
        public bool Bool { get; init; } = false;
        [DependsOn("Bool")]
        public bool NotBool { 
            get {
                return !Bool;
            }
        }
        public bool BoolContent { get; set; } = false;
        public bool Active { get; set; } = false;
        public string FieldName { get; init; } = "";
        public string Content { get; set; } = "";
        public FieldCategory Category { get; init; } = FieldCategory.None;
        public FieldSubCategory SubCategory { get; init; } = FieldSubCategory.None;

        public event PropertyChangedEventHandler PropertyChanged;


        public static DatabaseFieldInfo[] GetDatabaseFieldInfos()
        {
            return new DatabaseFieldInfo[] {

                // Kills
                new DatabaseFieldInfo(){ FieldName="hash", Nullable = true, Bool = false, Category=FieldCategory.Rets,SubCategory=FieldSubCategory.Rets_Kill},
                new DatabaseFieldInfo(){ FieldName="shorthash", Nullable = true, Bool = false, Category=FieldCategory.Rets,SubCategory=FieldSubCategory.Rets_Kill},
                new DatabaseFieldInfo(){ FieldName="map", Nullable = false, Bool = false, Category=FieldCategory.Rets,SubCategory=FieldSubCategory.Rets_Meta},
                new DatabaseFieldInfo(){ FieldName="serverName", Nullable = false, Bool = false, Category=FieldCategory.Rets,SubCategory=FieldSubCategory.Rets_Meta},
                new DatabaseFieldInfo(){ FieldName="serverNameStripped", Nullable = false, Bool = false, Category=FieldCategory.Rets,SubCategory=FieldSubCategory.Rets_Meta},
                new DatabaseFieldInfo(){ FieldName="killerName", Nullable = false, Bool = false, Category=FieldCategory.Rets,SubCategory=FieldSubCategory.Rets_Meta},
                new DatabaseFieldInfo(){ FieldName="killerNameStripped", Nullable = false, Bool = false, Category=FieldCategory.Rets,SubCategory=FieldSubCategory.Rets_Meta},
                new DatabaseFieldInfo(){ FieldName="victimName", Nullable = false, Bool = false, Category=FieldCategory.Rets,SubCategory=FieldSubCategory.Rets_Meta},
                new DatabaseFieldInfo(){ FieldName="victimNameStripped", Nullable = false, Bool = false, Category=FieldCategory.Rets,SubCategory=FieldSubCategory.Rets_Meta},
                new DatabaseFieldInfo(){ FieldName="killerTeam", Nullable = true, Bool = false, Category=FieldCategory.Rets,SubCategory=FieldSubCategory.Rets_Meta, Numeric=true},
                new DatabaseFieldInfo(){ FieldName="victimTeam", Nullable = false, Bool = false, Category=FieldCategory.Rets,SubCategory=FieldSubCategory.Rets_Meta, Numeric=true},
                new DatabaseFieldInfo(){ FieldName="redScore", Nullable = true, Bool = false, Category=FieldCategory.Rets,SubCategory=FieldSubCategory.Rets_Meta, Numeric=true},
                new DatabaseFieldInfo(){ FieldName="blueScore", Nullable = true, Bool = false, Category=FieldCategory.Rets,SubCategory=FieldSubCategory.Rets_Meta, Numeric=true},
                new DatabaseFieldInfo(){ FieldName="otherFlagStatus", Nullable = true, Bool = false, Category=FieldCategory.Rets,SubCategory=FieldSubCategory.Rets_Meta, Numeric=true},
                new DatabaseFieldInfo(){ FieldName="redPlayerCount", Nullable = true, Bool = false, Category=FieldCategory.Rets,SubCategory=FieldSubCategory.Rets_Meta, Numeric=true},
                new DatabaseFieldInfo(){ FieldName="bluePlayerCount", Nullable = true, Bool = false, Category=FieldCategory.Rets,SubCategory=FieldSubCategory.Rets_Meta, Numeric=true},
                new DatabaseFieldInfo(){ FieldName="sumPlayerCount", Nullable = true, Bool = false, Category=FieldCategory.Rets,SubCategory=FieldSubCategory.Rets_Meta, Numeric=true},
                new DatabaseFieldInfo(){ FieldName="killerClientNum", Nullable = false, Bool = false, Category=FieldCategory.Rets,SubCategory=FieldSubCategory.Rets_Meta, Numeric=true},
                new DatabaseFieldInfo(){ FieldName="victimClientNum", Nullable = false, Bool = false, Category=FieldCategory.Rets,SubCategory=FieldSubCategory.Rets_Meta, Numeric=true},
                new DatabaseFieldInfo(){ FieldName="isDoomKill", Nullable = false, Bool = true, Category=FieldCategory.Rets,SubCategory=FieldSubCategory.Rets_Kill},
                new DatabaseFieldInfo(){ FieldName="isExplosion", Nullable = false, Bool = true, Category=FieldCategory.Rets,SubCategory=FieldSubCategory.Rets_Kill},
                new DatabaseFieldInfo(){ FieldName="isSuicide", Nullable = false, Bool = true, Category=FieldCategory.Rets,SubCategory=FieldSubCategory.Rets_Kill},
                new DatabaseFieldInfo(){ FieldName="isModSuicide", Nullable = false, Bool = true, Category=FieldCategory.Rets,SubCategory=FieldSubCategory.Rets_Kill},
                new DatabaseFieldInfo(){ FieldName="meansOfDeath", Nullable = false, Bool = false, Category=FieldCategory.Rets,SubCategory=FieldSubCategory.Rets_Kill},
                new DatabaseFieldInfo(){ FieldName="positionX", Nullable = true, Bool = false, Category=FieldCategory.Rets,SubCategory=FieldSubCategory.Rets_Movement, Numeric=true},
                new DatabaseFieldInfo(){ FieldName="positionY", Nullable = true, Bool = false, Category=FieldCategory.Rets,SubCategory=FieldSubCategory.Rets_Movement, Numeric=true},
                new DatabaseFieldInfo(){ FieldName="positionZ", Nullable = true, Bool = false, Category=FieldCategory.Rets,SubCategory=FieldSubCategory.Rets_Movement, Numeric=true},

                // KillAngles
                //new DatabaseFieldInfo(){ FieldName="hash", Nullable = true, Bool = false, Category=FieldCategory.Rets,SubCategory=FieldSubCategory.None},
                //new DatabaseFieldInfo(){ FieldName="shorthash", Nullable = true, Bool = false, Category=FieldCategory.Rets,SubCategory=FieldSubCategory.None},
                //new DatabaseFieldInfo(){ FieldName="map", Nullable = false, Bool = false, Category=FieldCategory.Rets,SubCategory=FieldSubCategory.None},
                new DatabaseFieldInfo(){ FieldName="killerIsFlagCarrier", Nullable = false, Bool = true, Category=FieldCategory.Rets,SubCategory=FieldSubCategory.Rets_Kill},
                new DatabaseFieldInfo(){ FieldName="isReturn", Nullable = false, Bool = true, Category=FieldCategory.Rets,SubCategory=FieldSubCategory.Rets_Kill},
                new DatabaseFieldInfo(){ FieldName="isTeamKill", Nullable = false, Bool = true, Category=FieldCategory.Rets,SubCategory=FieldSubCategory.Rets_Kill},
                new DatabaseFieldInfo(){ FieldName="victimCapperKills", Nullable = true, Bool = false, Category=FieldCategory.Rets,SubCategory=FieldSubCategory.Rets_Kill, Numeric=true},
                new DatabaseFieldInfo(){ FieldName="victimCapperRets", Nullable = true, Bool = false, Category=FieldCategory.Rets,SubCategory=FieldSubCategory.Rets_Kill, Numeric=true},
                new DatabaseFieldInfo(){ FieldName="victimCapperWasFollowedOrVisible", Nullable = true, Bool = true, Category=FieldCategory.Rets,SubCategory=FieldSubCategory.Rets_Kill},
                new DatabaseFieldInfo(){ FieldName="victimCapperMaxNearbyEnemyCount", Nullable = true, Bool = false, Category=FieldCategory.Rets,SubCategory=FieldSubCategory.Rets_Kill, Numeric=true},
                new DatabaseFieldInfo(){ FieldName="victimCapperMoreThanOneNearbyEnemyTimePercent", Nullable = true, Bool = false, Category=FieldCategory.Rets,SubCategory=FieldSubCategory.Rets_Kill, Numeric=true},
                new DatabaseFieldInfo(){ FieldName="victimCapperAverageNearbyEnemyCount", Nullable = true, Bool = false, Category=FieldCategory.Rets,SubCategory=FieldSubCategory.Rets_Kill, Numeric=true},
                new DatabaseFieldInfo(){ FieldName="victimCapperMaxVeryCloseEnemyCount", Nullable = true, Bool = false, Category=FieldCategory.Rets,SubCategory=FieldSubCategory.Rets_Kill, Numeric=true},
                new DatabaseFieldInfo(){ FieldName="victimCapperAnyVeryCloseEnemyTimePercent", Nullable = true, Bool = false, Category=FieldCategory.Rets,SubCategory=FieldSubCategory.Rets_Kill, Numeric=true},
                new DatabaseFieldInfo(){ FieldName="victimCapperMoreThanOneVeryCloseEnemyTimePercent", Nullable = true, Bool = false, Category=FieldCategory.Rets,SubCategory=FieldSubCategory.Rets_Kill, Numeric=true},
                new DatabaseFieldInfo(){ FieldName="victimCapperAverageVeryCloseEnemyCount", Nullable = true, Bool = false, Category=FieldCategory.Rets,SubCategory=FieldSubCategory.Rets_Kill, Numeric=true},
                new DatabaseFieldInfo(){ FieldName="victimFlagPickupSource", Nullable = true, Bool = false, Category=FieldCategory.Rets,SubCategory=FieldSubCategory.Rets_Kill, Numeric=true},
                new DatabaseFieldInfo(){ FieldName="victimFlagHoldTime", Nullable = true, Bool = false, Category=FieldCategory.Rets,SubCategory=FieldSubCategory.Rets_Kill, Numeric=true},
                new DatabaseFieldInfo(){ FieldName="targetIsVisible", Nullable = false, Bool = true, Category=FieldCategory.Rets,SubCategory=FieldSubCategory.Rets_Meta},
                new DatabaseFieldInfo(){ FieldName="targetIsFollowed", Nullable = false, Bool = true, Category=FieldCategory.Rets,SubCategory=FieldSubCategory.Rets_Meta},
                new DatabaseFieldInfo(){ FieldName="targetIsFollowedOrVisible", Nullable = false, Bool = true, Category=FieldCategory.Rets,SubCategory=FieldSubCategory.Rets_Kill},
                //new DatabaseFieldInfo(){ FieldName="isSuicide", Nullable = false, Bool = true, Category=FieldCategory.Rets,SubCategory=FieldSubCategory.None},
                //new DatabaseFieldInfo(){ FieldName="isModSuicide", Nullable = false, Bool = true, Category=FieldCategory.Rets,SubCategory=FieldSubCategory.None},
                new DatabaseFieldInfo(){ FieldName="attackerIsVisible", Nullable = false, Bool = true, Category=FieldCategory.Rets,SubCategory=FieldSubCategory.Rets_Meta},
                new DatabaseFieldInfo(){ FieldName="attackerIsFollowed", Nullable = false, Bool = true, Category=FieldCategory.Rets,SubCategory=FieldSubCategory.Rets_Meta},
                new DatabaseFieldInfo(){ FieldName="attackerIsFollowedOrVisible", Nullable = false, Bool = true, Category=FieldCategory.Rets,SubCategory=FieldSubCategory.Rets_Kill},
                new DatabaseFieldInfo(){ FieldName="demoRecorderClientnum", Nullable = false, Bool = false, Category=FieldCategory.Rets,SubCategory=FieldSubCategory.Rets_Meta, Numeric=true},
                new DatabaseFieldInfo(){ FieldName="boosts", Nullable = true, Bool = false, Category=FieldCategory.Rets,SubCategory=FieldSubCategory.Rets_Movement},
                new DatabaseFieldInfo(){ FieldName="boostCountTotal", Nullable = false, Bool = false, Category=FieldCategory.Rets,SubCategory=FieldSubCategory.Rets_Movement, Numeric=true},
                new DatabaseFieldInfo(){ FieldName="boostCountAttacker", Nullable = false, Bool = false, Category=FieldCategory.Rets,SubCategory=FieldSubCategory.Rets_Movement, Numeric=true},
                new DatabaseFieldInfo(){ FieldName="boostCountVictim", Nullable = false, Bool = false, Category=FieldCategory.Rets,SubCategory=FieldSubCategory.Rets_Movement, Numeric=true},
                new DatabaseFieldInfo(){ FieldName="projectileWasAirborne", Nullable = true, Bool = true, Category=FieldCategory.Rets,SubCategory=FieldSubCategory.Rets_Kill},
                new DatabaseFieldInfo(){ FieldName="sameFrameRet", Nullable = true, Bool = true, Category=FieldCategory.Rets,SubCategory=FieldSubCategory.Rets_Kill},
                new DatabaseFieldInfo(){ FieldName="baseFlagDistance", Nullable = true, Bool = false, Category=FieldCategory.Rets,SubCategory=FieldSubCategory.Rets_Movement, Numeric=true},
                new DatabaseFieldInfo(){ FieldName="headJumps", Nullable = true, Bool = false, Category=FieldCategory.Rets,SubCategory=FieldSubCategory.Rets_Movement, Numeric=true},
                new DatabaseFieldInfo(){ FieldName="specialJumps", Nullable = true, Bool = false, Category=FieldCategory.Rets,SubCategory=FieldSubCategory.Rets_Movement, Numeric=true},
                new DatabaseFieldInfo(){ FieldName="timeSinceLastSelfSentryJump", Nullable = true, Bool = false, Category=FieldCategory.Rets,SubCategory=FieldSubCategory.Rets_Movement, Numeric=true},
                new DatabaseFieldInfo(){ FieldName="lastSneak", Nullable = true, Bool = false, Category=FieldCategory.Rets,SubCategory=FieldSubCategory.Rets_Movement, Numeric=true},
                new DatabaseFieldInfo(){ FieldName="lastSneakDuration", Nullable = true, Bool = false, Category=FieldCategory.Rets,SubCategory=FieldSubCategory.Rets_Movement, Numeric=true},
                new DatabaseFieldInfo(){ FieldName="resultingCaptures", Nullable = true, Bool = false, Category=FieldCategory.Rets,SubCategory=FieldSubCategory.Rets_Kill, Numeric=true},
                new DatabaseFieldInfo(){ FieldName="resultingSelfCaptures", Nullable = true, Bool = false, Category=FieldCategory.Rets,SubCategory=FieldSubCategory.Rets_Kill, Numeric=true},
                new DatabaseFieldInfo(){ FieldName="resultingLaughs", Nullable = true, Bool = false, Category=FieldCategory.Rets,SubCategory=FieldSubCategory.Rets_Meta, Numeric=true},
                new DatabaseFieldInfo(){ FieldName="metaEvents", Nullable = true, Bool = false, Category=FieldCategory.Rets,SubCategory=FieldSubCategory.Rets_Kill},
                new DatabaseFieldInfo(){ FieldName="maxAngularSpeedAttacker", Nullable = true, Bool = false, Category=FieldCategory.Rets,SubCategory=FieldSubCategory.Rets_Movement, Numeric=true},
                new DatabaseFieldInfo(){ FieldName="maxAngularAccelerationAttacker", Nullable = true, Bool = false, Category=FieldCategory.Rets,SubCategory=FieldSubCategory.Rets_Movement, Numeric=true},
                new DatabaseFieldInfo(){ FieldName="maxAngularJerkAttacker", Nullable = true, Bool = false, Category=FieldCategory.Rets,SubCategory=FieldSubCategory.Rets_Movement, Numeric=true},
                new DatabaseFieldInfo(){ FieldName="maxAngularSnapAttacker", Nullable = true, Bool = false, Category=FieldCategory.Rets,SubCategory=FieldSubCategory.Rets_Movement, Numeric=true},
                new DatabaseFieldInfo(){ FieldName="maxSpeedAttacker", Nullable = true, Bool = false, Category=FieldCategory.Rets,SubCategory=FieldSubCategory.Rets_Movement, Numeric=true},
                new DatabaseFieldInfo(){ FieldName="maxSpeedTarget", Nullable = true, Bool = false, Category=FieldCategory.Rets,SubCategory=FieldSubCategory.Rets_Movement, Numeric=true},
                new DatabaseFieldInfo(){ FieldName="currentSpeedAttacker", Nullable = true, Bool = false, Category=FieldCategory.Rets,SubCategory=FieldSubCategory.Rets_Movement, Numeric=true},
                new DatabaseFieldInfo(){ FieldName="currentSpeedTarget", Nullable = true, Bool = false, Category=FieldCategory.Rets,SubCategory=FieldSubCategory.Rets_Movement, Numeric=true},
                new DatabaseFieldInfo(){ FieldName="lastSaberMoveChangeSpeed", Nullable = true, Bool = false, Category=FieldCategory.Rets,SubCategory=FieldSubCategory.Rets_Movement, Numeric=true},
                new DatabaseFieldInfo(){ FieldName="timeSinceLastSaberMoveChange", Nullable = true, Bool = false, Category=FieldCategory.Rets,SubCategory=FieldSubCategory.Rets_Movement, Numeric=true},
                new DatabaseFieldInfo(){ FieldName="timeSinceLastBackflip", Nullable = true, Bool = false, Category=FieldCategory.Rets,SubCategory=FieldSubCategory.Rets_Movement, Numeric=true},
                new DatabaseFieldInfo(){ FieldName="meansOfDeathString", Nullable = false, Bool = false, Category=FieldCategory.Rets,SubCategory=FieldSubCategory.Rets_Kill},
                new DatabaseFieldInfo(){ FieldName="nearbyPlayers", Nullable = true, Bool = false, Category=FieldCategory.Rets,SubCategory=FieldSubCategory.Rets_Meta},
                new DatabaseFieldInfo(){ FieldName="nearbyPlayerCount", Nullable = false, Bool = false, Category=FieldCategory.Rets,SubCategory=FieldSubCategory.Rets_Meta, Numeric=true},
                new DatabaseFieldInfo(){ FieldName="probableKillingWeapon", Nullable = false, Bool = false, Category=FieldCategory.Rets,SubCategory=FieldSubCategory.Rets_Kill, Numeric=true},
                new DatabaseFieldInfo(){ FieldName="attackerJumpHeight", Nullable = true, Bool = false, Category=FieldCategory.Rets,SubCategory=FieldSubCategory.Rets_Movement, Numeric=true},
                new DatabaseFieldInfo(){ FieldName="victimJumpHeight", Nullable = true, Bool = false, Category=FieldCategory.Rets,SubCategory=FieldSubCategory.Rets_Movement, Numeric=true},
                new DatabaseFieldInfo(){ FieldName="directionX", Nullable = true, Bool = false, Category=FieldCategory.Rets,SubCategory=FieldSubCategory.Rets_Movement, Numeric=true},
                new DatabaseFieldInfo(){ FieldName="directionY", Nullable = true, Bool = false, Category=FieldCategory.Rets,SubCategory=FieldSubCategory.Rets_Movement, Numeric=true},
                new DatabaseFieldInfo(){ FieldName="directionZ", Nullable = true, Bool = false, Category=FieldCategory.Rets,SubCategory=FieldSubCategory.Rets_Movement, Numeric=true},
                new DatabaseFieldInfo(){ FieldName="demoName", Nullable = false, Bool = false, Category=FieldCategory.Rets,SubCategory=FieldSubCategory.Rets_Meta},
                new DatabaseFieldInfo(){ FieldName="demoPath", Nullable = false, Bool = false, Category=FieldCategory.Rets,SubCategory=FieldSubCategory.Rets_Meta},
                new DatabaseFieldInfo(){ FieldName="demoTime", Nullable = false, Bool = false, Category=FieldCategory.Rets,SubCategory=FieldSubCategory.Rets_Meta, Numeric=true},
                new DatabaseFieldInfo(){ FieldName="lastGamestateDemoTime", Nullable = false, Bool = false, Category=FieldCategory.Rets,SubCategory=FieldSubCategory.Rets_Meta, Numeric=true},
                new DatabaseFieldInfo(){ FieldName="serverTime", Nullable = false, Bool = false, Category=FieldCategory.Rets,SubCategory=FieldSubCategory.Rets_Meta, Numeric=true},
                new DatabaseFieldInfo(){ FieldName="demoDateTime", Nullable = false, Bool =false, Category=FieldCategory.Rets,SubCategory=FieldSubCategory.Rets_Meta, Numeric=true},

                // Captures
                new DatabaseFieldInfo(){ FieldName="id", Nullable = true, Bool = false, Category=FieldCategory.Captures, SubCategory=FieldSubCategory.Captures_Capture, Numeric=true},
                new DatabaseFieldInfo(){ FieldName="map", Nullable = false, Bool = false, Category=FieldCategory.Captures, SubCategory=FieldSubCategory.Captures_Meta},
                new DatabaseFieldInfo(){ FieldName="serverName", Nullable = false, Bool = false, Category=FieldCategory.Captures, SubCategory=FieldSubCategory.Captures_Meta},
                new DatabaseFieldInfo(){ FieldName="serverNameStripped", Nullable = false, Bool = false, Category=FieldCategory.Captures, SubCategory=FieldSubCategory.Captures_Meta},
                new DatabaseFieldInfo(){ FieldName="flagHoldTime", Nullable = false, Bool = false, Category=FieldCategory.Captures, SubCategory=FieldSubCategory.Captures_Capture, Numeric=true},
                new DatabaseFieldInfo(){ FieldName="flagPickupSource", Nullable = false, Bool = false, Category=FieldCategory.Captures, SubCategory=FieldSubCategory.Captures_Capture, Numeric=true},
                new DatabaseFieldInfo(){ FieldName="capperName", Nullable = false, Bool = false, Category=FieldCategory.Captures, SubCategory=FieldSubCategory.Captures_Meta},
                new DatabaseFieldInfo(){ FieldName="capperNameStripped", Nullable = false, Bool = false, Category=FieldCategory.Captures, SubCategory=FieldSubCategory.Captures_Meta},
                new DatabaseFieldInfo(){ FieldName="capperClientNum", Nullable = false, Bool = false, Category=FieldCategory.Captures, SubCategory=FieldSubCategory.Captures_Meta, Numeric=true},
                new DatabaseFieldInfo(){ FieldName="capperIsVisible", Nullable = false, Bool = true, Category=FieldCategory.Captures, SubCategory=FieldSubCategory.Captures_Meta},
                new DatabaseFieldInfo(){ FieldName="capperIsFollowed", Nullable = false, Bool = true, Category=FieldCategory.Captures, SubCategory=FieldSubCategory.Captures_Meta},
                new DatabaseFieldInfo(){ FieldName="capperIsFollowedOrVisible", Nullable = false, Bool = true, Category=FieldCategory.Captures, SubCategory=FieldSubCategory.Captures_Meta},
                new DatabaseFieldInfo(){ FieldName="capperWasVisible", Nullable = false, Bool = true, Category=FieldCategory.Captures, SubCategory=FieldSubCategory.Captures_Meta},
                new DatabaseFieldInfo(){ FieldName="capperWasFollowed", Nullable = false, Bool = true, Category=FieldCategory.Captures, SubCategory=FieldSubCategory.Captures_Meta},
                new DatabaseFieldInfo(){ FieldName="capperWasFollowedOrVisible", Nullable = false, Bool = true, Category=FieldCategory.Captures, SubCategory=FieldSubCategory.Captures_Meta},
                new DatabaseFieldInfo(){ FieldName="demoRecorderClientnum", Nullable = false, Bool = false, Category=FieldCategory.Captures, SubCategory=FieldSubCategory.Captures_Meta, Numeric=true},
                new DatabaseFieldInfo(){ FieldName="flagTeam", Nullable = false, Bool = false, Category=FieldCategory.Captures, SubCategory=FieldSubCategory.Captures_Capture, Numeric=true},
                new DatabaseFieldInfo(){ FieldName="capperKills", Nullable = false, Bool = false, Category=FieldCategory.Captures, SubCategory=FieldSubCategory.Captures_Capture, Numeric=true},
                new DatabaseFieldInfo(){ FieldName="capperRets", Nullable = false, Bool = false, Category=FieldCategory.Captures, SubCategory=FieldSubCategory.Captures_Capture, Numeric=true},
                new DatabaseFieldInfo(){ FieldName="redScore", Nullable = true, Bool = false, Category=FieldCategory.Captures, SubCategory=FieldSubCategory.Captures_Capture, Numeric=true},
                new DatabaseFieldInfo(){ FieldName="blueScore", Nullable = true, Bool = false, Category=FieldCategory.Captures, SubCategory=FieldSubCategory.Captures_Capture, Numeric=true},
                new DatabaseFieldInfo(){ FieldName="redPlayerCount", Nullable = true, Bool = false, Category=FieldCategory.Captures, SubCategory=FieldSubCategory.Captures_Meta, Numeric=true},
                new DatabaseFieldInfo(){ FieldName="bluePlayerCount", Nullable = true, Bool = false, Category=FieldCategory.Captures, SubCategory=FieldSubCategory.Captures_Meta, Numeric=true},
                new DatabaseFieldInfo(){ FieldName="sumPlayerCount", Nullable = true, Bool = false, Category=FieldCategory.Captures, SubCategory=FieldSubCategory.Captures_Meta, Numeric=true},
                new DatabaseFieldInfo(){ FieldName="maxSpeedCapperLastSecond", Nullable = true, Bool = false, Category=FieldCategory.Captures, SubCategory=FieldSubCategory.Captures_Movement, Numeric=true},
                new DatabaseFieldInfo(){ FieldName="maxSpeedCapper", Nullable = true, Bool = false, Category=FieldCategory.Captures, SubCategory=FieldSubCategory.Captures_Movement, Numeric=true},
                new DatabaseFieldInfo(){ FieldName="averageSpeedCapper", Nullable = true, Bool = false, Category=FieldCategory.Captures, SubCategory=FieldSubCategory.Captures_Movement, Numeric=true},
                new DatabaseFieldInfo(){ FieldName="metaEvents", Nullable = true, Bool = false, Category=FieldCategory.Captures, SubCategory=FieldSubCategory.Captures_Meta},
                new DatabaseFieldInfo(){ FieldName="nearbyPlayers", Nullable = true, Bool = false, Category=FieldCategory.Captures, SubCategory=FieldSubCategory.Captures_Meta},
                new DatabaseFieldInfo(){ FieldName="nearbyPlayerCount", Nullable = false, Bool = false, Category=FieldCategory.Captures, SubCategory=FieldSubCategory.Captures_Meta, Numeric=true},
                new DatabaseFieldInfo(){ FieldName="nearbyEnemies", Nullable = true, Bool = false, Category=FieldCategory.Captures, SubCategory=FieldSubCategory.Captures_Capture},
                new DatabaseFieldInfo(){ FieldName="nearbyEnemyCount", Nullable = false, Bool = false, Category=FieldCategory.Captures, SubCategory=FieldSubCategory.Captures_Capture, Numeric=true},
                new DatabaseFieldInfo(){ FieldName="maxNearbyEnemyCount", Nullable = false, Bool = false, Category=FieldCategory.Captures, SubCategory=FieldSubCategory.Captures_Capture, Numeric=true},
                new DatabaseFieldInfo(){ FieldName="moreThanOneNearbyEnemyTimePercent", Nullable = false, Bool = false, Category=FieldCategory.Captures, SubCategory=FieldSubCategory.Captures_Capture, Numeric=true},
                new DatabaseFieldInfo(){ FieldName="averageNearbyEnemyCount", Nullable = false, Bool = false, Category=FieldCategory.Captures, SubCategory=FieldSubCategory.Captures_Capture, Numeric=true},
                new DatabaseFieldInfo(){ FieldName="maxVeryCloseEnemyCount", Nullable = false, Bool = false, Category=FieldCategory.Captures, SubCategory=FieldSubCategory.Captures_Capture, Numeric=true},
                new DatabaseFieldInfo(){ FieldName="anyVeryCloseEnemyTimePercent", Nullable = false, Bool = false, Category=FieldCategory.Captures, SubCategory=FieldSubCategory.Captures_Capture, Numeric=true},
                new DatabaseFieldInfo(){ FieldName="moreThanOneVeryCloseEnemyTimePercent", Nullable = false, Bool = false, Category=FieldCategory.Captures, SubCategory=FieldSubCategory.Captures_Capture, Numeric=true},
                new DatabaseFieldInfo(){ FieldName="averageVeryCloseEnemyCount", Nullable = false, Bool = false, Category=FieldCategory.Captures, SubCategory=FieldSubCategory.Captures_Capture, Numeric=true},
                new DatabaseFieldInfo(){ FieldName="directionX", Nullable = true, Bool = false, Category=FieldCategory.Captures, SubCategory=FieldSubCategory.Captures_Movement, Numeric=true},
                new DatabaseFieldInfo(){ FieldName="directionY", Nullable = true, Bool = false, Category=FieldCategory.Captures, SubCategory=FieldSubCategory.Captures_Movement, Numeric=true},
                new DatabaseFieldInfo(){ FieldName="directionZ", Nullable = true, Bool = false, Category=FieldCategory.Captures, SubCategory=FieldSubCategory.Captures_Movement, Numeric=true},
                new DatabaseFieldInfo(){ FieldName="positionX", Nullable = true, Bool = false, Category=FieldCategory.Captures, SubCategory=FieldSubCategory.Captures_Movement, Numeric=true},
                new DatabaseFieldInfo(){ FieldName="positionY", Nullable = true, Bool = false, Category=FieldCategory.Captures, SubCategory=FieldSubCategory.Captures_Movement, Numeric=true},
                new DatabaseFieldInfo(){ FieldName="positionZ", Nullable = true, Bool = false, Category=FieldCategory.Captures, SubCategory=FieldSubCategory.Captures_Movement, Numeric=true},
                new DatabaseFieldInfo(){ FieldName="resultingLaughs", Nullable = true, Bool = false, Category=FieldCategory.Captures, SubCategory=FieldSubCategory.Captures_Meta, Numeric=true},
                new DatabaseFieldInfo(){ FieldName="resultingLaughsAfter", Nullable = true, Bool = false, Category=FieldCategory.Captures, SubCategory=FieldSubCategory.Captures_Meta, Numeric=true},
                new DatabaseFieldInfo(){ FieldName="demoName", Nullable = false, Bool = false, Category=FieldCategory.Captures, SubCategory=FieldSubCategory.Captures_Meta},
                new DatabaseFieldInfo(){ FieldName="demoPath", Nullable = false, Bool = false, Category=FieldCategory.Captures, SubCategory=FieldSubCategory.Captures_Meta},
                new DatabaseFieldInfo(){ FieldName="demoTime", Nullable = false, Bool = false, Category=FieldCategory.Captures, SubCategory=FieldSubCategory.Captures_Meta, Numeric=true},
                new DatabaseFieldInfo(){ FieldName="lastGamestateDemoTime", Nullable = false, Bool = false, Category=FieldCategory.Captures, SubCategory=FieldSubCategory.Captures_Meta, Numeric=true},
                new DatabaseFieldInfo(){ FieldName="serverTime", Nullable = false, Bool = false, Category=FieldCategory.Captures, SubCategory=FieldSubCategory.Captures_Meta, Numeric=true},
                new DatabaseFieldInfo(){ FieldName="demoDateTime", Nullable = false, Bool =false, Category=FieldCategory.Captures, SubCategory=FieldSubCategory.Captures_Meta, Numeric=true},

                // Killsprees
                new DatabaseFieldInfo(){ FieldName="hash", Nullable = true, Bool = false, Category=FieldCategory.KillSprees, SubCategory=FieldSubCategory.KillSprees_Kills},
                new DatabaseFieldInfo(){ FieldName="shorthash", Nullable = true, Bool = false, Category=FieldCategory.KillSprees, SubCategory=FieldSubCategory.KillSprees_Kills},
                new DatabaseFieldInfo(){ FieldName="maxDelay", Nullable = false, Bool = false, Category=FieldCategory.KillSprees, SubCategory=FieldSubCategory.KillSprees_Kills, Numeric=true},
                new DatabaseFieldInfo(){ FieldName="maxDelayActual", Nullable = false, Bool = false, Category=FieldCategory.KillSprees, SubCategory=FieldSubCategory.KillSprees_Kills, Numeric=true},
                new DatabaseFieldInfo(){ FieldName="map", Nullable = false, Bool = false, Category=FieldCategory.KillSprees, SubCategory=FieldSubCategory.KillSprees_Meta},
                new DatabaseFieldInfo(){ FieldName="killerName", Nullable = false, Bool = false, Category=FieldCategory.KillSprees, SubCategory=FieldSubCategory.KillSprees_Meta},
                new DatabaseFieldInfo(){ FieldName="killerNameStripped", Nullable = false, Bool = false, Category=FieldCategory.KillSprees, SubCategory=FieldSubCategory.KillSprees_Meta},
                new DatabaseFieldInfo(){ FieldName="victimNames", Nullable = false, Bool = false, Category=FieldCategory.KillSprees, SubCategory=FieldSubCategory.KillSprees_Meta},
                new DatabaseFieldInfo(){ FieldName="victimNamesStripped", Nullable = false, Bool = false, Category=FieldCategory.KillSprees, SubCategory=FieldSubCategory.KillSprees_Meta},
                new DatabaseFieldInfo(){ FieldName="killTypes", Nullable = false, Bool = false, Category=FieldCategory.KillSprees, SubCategory=FieldSubCategory.KillSprees_Kills},
                new DatabaseFieldInfo(){ FieldName="killTypesCount", Nullable = false, Bool = false, Category=FieldCategory.KillSprees, SubCategory=FieldSubCategory.KillSprees_Kills, Numeric=true},
                new DatabaseFieldInfo(){ FieldName="killHashes", Nullable = false, Bool = false, Category=FieldCategory.KillSprees, SubCategory=FieldSubCategory.KillSprees_Kills},
                new DatabaseFieldInfo(){ FieldName="killerClientNum", Nullable = false, Bool = false, Category=FieldCategory.KillSprees, SubCategory=FieldSubCategory.KillSprees_Meta, Numeric=true},
                new DatabaseFieldInfo(){ FieldName="victimClientNums", Nullable = false, Bool = false, Category=FieldCategory.KillSprees, SubCategory=FieldSubCategory.KillSprees_Meta},
                new DatabaseFieldInfo(){ FieldName="countKills", Nullable = false, Bool = false, Category=FieldCategory.KillSprees, SubCategory=FieldSubCategory.KillSprees_Kills, Numeric=true},
                new DatabaseFieldInfo(){ FieldName="countRets", Nullable = false, Bool = false, Category=FieldCategory.KillSprees, SubCategory=FieldSubCategory.KillSprees_Kills, Numeric=true},
                new DatabaseFieldInfo(){ FieldName="countTeamKills", Nullable = false, Bool = false, Category=FieldCategory.KillSprees, SubCategory=FieldSubCategory.KillSprees_Kills, Numeric=true},
                new DatabaseFieldInfo(){ FieldName="countUniqueTargets", Nullable = false, Bool = false, Category=FieldCategory.KillSprees, SubCategory=FieldSubCategory.KillSprees_Kills, Numeric=true},
                new DatabaseFieldInfo(){ FieldName="countDooms", Nullable = false, Bool = false, Category=FieldCategory.KillSprees, SubCategory=FieldSubCategory.KillSprees_Kills, Numeric=true},
                new DatabaseFieldInfo(){ FieldName="countExplosions", Nullable = false, Bool = false, Category=FieldCategory.KillSprees, SubCategory=FieldSubCategory.KillSprees_Kills, Numeric=true},
                new DatabaseFieldInfo(){ FieldName="countThirdPersons", Nullable = false, Bool = false, Category=FieldCategory.KillSprees, SubCategory=FieldSubCategory.KillSprees_Meta, Numeric=true},
                new DatabaseFieldInfo(){ FieldName="countInvisibles", Nullable = false, Bool = false, Category=FieldCategory.KillSprees, SubCategory=FieldSubCategory.KillSprees_Meta, Numeric=true},
                new DatabaseFieldInfo(){ FieldName="nearbyPlayers", Nullable = true, Bool = false, Category=FieldCategory.KillSprees, SubCategory=FieldSubCategory.KillSprees_Meta},
                new DatabaseFieldInfo(){ FieldName="nearbyPlayerCount", Nullable = false, Bool = false, Category=FieldCategory.KillSprees, SubCategory=FieldSubCategory.KillSprees_Meta, Numeric=true},
                new DatabaseFieldInfo(){ FieldName="demoRecorderClientnum", Nullable = false, Bool = false, Category=FieldCategory.KillSprees, SubCategory=FieldSubCategory.KillSprees_Meta, Numeric=true},
                new DatabaseFieldInfo(){ FieldName="maxSpeedAttacker", Nullable = true, Bool = false, Category=FieldCategory.KillSprees, SubCategory=FieldSubCategory.KillSprees_Movement, Numeric=true},
                new DatabaseFieldInfo(){ FieldName="maxSpeedTargets", Nullable = true, Bool = false, Category=FieldCategory.KillSprees, SubCategory=FieldSubCategory.KillSprees_Movement, Numeric=true},
                new DatabaseFieldInfo(){ FieldName="resultingCaptures", Nullable = true, Bool = false, Category=FieldCategory.KillSprees, SubCategory=FieldSubCategory.KillSprees_Kills, Numeric=true},
                new DatabaseFieldInfo(){ FieldName="resultingSelfCaptures", Nullable = true, Bool = false, Category=FieldCategory.KillSprees, SubCategory=FieldSubCategory.KillSprees_Kills, Numeric=true},
                new DatabaseFieldInfo(){ FieldName="resultingCapturesAfter", Nullable = true, Bool = false, Category=FieldCategory.KillSprees, SubCategory=FieldSubCategory.KillSprees_Kills, Numeric=true},
                new DatabaseFieldInfo(){ FieldName="resultingSelfCapturesAfter", Nullable = true, Bool = false, Category=FieldCategory.KillSprees, SubCategory=FieldSubCategory.KillSprees_Kills, Numeric=true},
                new DatabaseFieldInfo(){ FieldName="resultingLaughs", Nullable = true, Bool = false, Category=FieldCategory.KillSprees, SubCategory=FieldSubCategory.KillSprees_Meta, Numeric=true},
                new DatabaseFieldInfo(){ FieldName="resultingLaughsAfter", Nullable = true, Bool = false, Category=FieldCategory.KillSprees, SubCategory=FieldSubCategory.KillSprees_Meta, Numeric=true},
                new DatabaseFieldInfo(){ FieldName="metaEvents", Nullable = true, Bool = false, Category=FieldCategory.KillSprees, SubCategory=FieldSubCategory.KillSprees_Meta},
                new DatabaseFieldInfo(){ FieldName="demoName", Nullable = false, Bool = false, Category=FieldCategory.KillSprees, SubCategory=FieldSubCategory.KillSprees_Meta},
                new DatabaseFieldInfo(){ FieldName="demoPath", Nullable = false, Bool = false, Category=FieldCategory.KillSprees, SubCategory=FieldSubCategory.KillSprees_Meta},
                new DatabaseFieldInfo(){ FieldName="demoTime", Nullable = false, Bool = false, Category=FieldCategory.KillSprees, SubCategory=FieldSubCategory.KillSprees_Meta, Numeric=true},
                new DatabaseFieldInfo(){ FieldName="lastGamestateDemoTime", Nullable = false, Bool = false, Category=FieldCategory.KillSprees, SubCategory=FieldSubCategory.KillSprees_Meta, Numeric=true},
                new DatabaseFieldInfo(){ FieldName="duration", Nullable = false, Bool = false, Category=FieldCategory.KillSprees, SubCategory=FieldSubCategory.KillSprees_Meta, Numeric=true},
                new DatabaseFieldInfo(){ FieldName="serverTime", Nullable = false, Bool = false, Category=FieldCategory.KillSprees, SubCategory=FieldSubCategory.KillSprees_Meta, Numeric=true},
                new DatabaseFieldInfo(){ FieldName="demoDateTime", Nullable = false, Bool =false, Category=FieldCategory.KillSprees, SubCategory=FieldSubCategory.KillSprees_Meta, Numeric=true},

                // DefragRuns
                new DatabaseFieldInfo(){ FieldName="map", Nullable = false, Bool = false, Category=FieldCategory.DefragRuns, SubCategory=FieldSubCategory.Defrag_Meta},
                new DatabaseFieldInfo(){ FieldName="serverName", Nullable = false, Bool = false, Category=FieldCategory.DefragRuns, SubCategory=FieldSubCategory.Defrag_Meta},
                new DatabaseFieldInfo(){ FieldName="serverNameStripped", Nullable = false, Bool = false, Category=FieldCategory.DefragRuns, SubCategory=FieldSubCategory.Defrag_Meta},
                new DatabaseFieldInfo(){ FieldName="readableTime", Nullable = false, Bool = false, Category=FieldCategory.DefragRuns, SubCategory=FieldSubCategory.Defrag_Run},
                new DatabaseFieldInfo(){ FieldName="totalMilliseconds", Nullable = true, Bool = false, Category=FieldCategory.DefragRuns, SubCategory=FieldSubCategory.Defrag_Run, Numeric=true},
                new DatabaseFieldInfo(){ FieldName="style", Nullable = false, Bool = false, Category=FieldCategory.DefragRuns, SubCategory=FieldSubCategory.Defrag_Run},
                new DatabaseFieldInfo(){ FieldName="playerName", Nullable = false, Bool = false, Category=FieldCategory.DefragRuns, SubCategory=FieldSubCategory.Defrag_Meta},
                new DatabaseFieldInfo(){ FieldName="playerNameStripped", Nullable = false, Bool = false, Category=FieldCategory.DefragRuns, SubCategory=FieldSubCategory.Defrag_Meta},
                new DatabaseFieldInfo(){ FieldName="isTop10", Nullable = false, Bool = true, Category=FieldCategory.DefragRuns, SubCategory=FieldSubCategory.Defrag_Run},
                new DatabaseFieldInfo(){ FieldName="isNumber1", Nullable = false, Bool = true, Category=FieldCategory.DefragRuns, SubCategory=FieldSubCategory.Defrag_Run},
                new DatabaseFieldInfo(){ FieldName="isPersonalBest", Nullable = false, Bool = true, Category=FieldCategory.DefragRuns, SubCategory=FieldSubCategory.Defrag_Run},
                new DatabaseFieldInfo(){ FieldName="wasVisible", Nullable = false, Bool = true, Category=FieldCategory.DefragRuns, SubCategory=FieldSubCategory.Defrag_Meta},
                new DatabaseFieldInfo(){ FieldName="wasFollowed", Nullable = false, Bool = true, Category=FieldCategory.DefragRuns, SubCategory=FieldSubCategory.Defrag_Meta},
                new DatabaseFieldInfo(){ FieldName="wasFollowedOrVisible", Nullable = false, Bool = true, Category=FieldCategory.DefragRuns, SubCategory=FieldSubCategory.Defrag_Meta},
                new DatabaseFieldInfo(){ FieldName="averageStrafeDeviation", Nullable = true, Bool = false, Category=FieldCategory.DefragRuns, SubCategory=FieldSubCategory.Defrag_Movement, Numeric=true},
                new DatabaseFieldInfo(){ FieldName="runnerClientNum", Nullable = false, Bool = false, Category=FieldCategory.DefragRuns, SubCategory=FieldSubCategory.Defrag_Meta, Numeric=true},
                new DatabaseFieldInfo(){ FieldName="resultingLaughs", Nullable = false, Bool = false, Category=FieldCategory.DefragRuns, SubCategory=FieldSubCategory.Defrag_Meta, Numeric=true},
                new DatabaseFieldInfo(){ FieldName="demoRecorderClientnum", Nullable = false, Bool = false, Category=FieldCategory.DefragRuns, SubCategory=FieldSubCategory.Defrag_Meta, Numeric=true},
                new DatabaseFieldInfo(){ FieldName="demoName", Nullable = false, Bool = false, Category=FieldCategory.DefragRuns, SubCategory=FieldSubCategory.Defrag_Meta},
                new DatabaseFieldInfo(){ FieldName="demoPath", Nullable = false, Bool = false, Category=FieldCategory.DefragRuns, SubCategory=FieldSubCategory.Defrag_Meta},
                new DatabaseFieldInfo(){ FieldName="demoTime", Nullable = false, Bool = false, Category=FieldCategory.DefragRuns, SubCategory=FieldSubCategory.Defrag_Meta, Numeric=true},
                new DatabaseFieldInfo(){ FieldName="lastGamestateDemoTime", Nullable = false, Bool = false, Category=FieldCategory.DefragRuns, SubCategory=FieldSubCategory.Defrag_Meta, Numeric=true},
                new DatabaseFieldInfo(){ FieldName="serverTime", Nullable = false, Bool = false, Category=FieldCategory.DefragRuns, SubCategory=FieldSubCategory.Defrag_Meta, Numeric=true},
                new DatabaseFieldInfo(){ FieldName="demoDateTime", Nullable = false, Bool =false, Category=FieldCategory.DefragRuns, SubCategory=FieldSubCategory.Defrag_Meta, Numeric=true},

                // Laughs
                new DatabaseFieldInfo(){ FieldName="id", Nullable = true, Bool = false, Category=FieldCategory.Laughs, SubCategory=FieldSubCategory.Laughs_Meta, Numeric=true},
                new DatabaseFieldInfo(){ FieldName="map", Nullable = false, Bool = false, Category=FieldCategory.Laughs, SubCategory=FieldSubCategory.Laughs_Meta},
                new DatabaseFieldInfo(){ FieldName="serverName", Nullable = false, Bool = false, Category=FieldCategory.Laughs, SubCategory=FieldSubCategory.Laughs_Meta},
                new DatabaseFieldInfo(){ FieldName="serverNameStripped", Nullable = false, Bool = false, Category=FieldCategory.Laughs, SubCategory=FieldSubCategory.Laughs_Meta},
                new DatabaseFieldInfo(){ FieldName="laughs", Nullable = false, Bool = false, Category=FieldCategory.Laughs, SubCategory=FieldSubCategory.Laughs_Laughs},
                new DatabaseFieldInfo(){ FieldName="chatlog", Nullable = false, Bool = false, Category=FieldCategory.Laughs, SubCategory=FieldSubCategory.Laughs_Laughs},
                new DatabaseFieldInfo(){ FieldName="chatlogStripped", Nullable = false, Bool = false, Category=FieldCategory.Laughs, SubCategory=FieldSubCategory.Laughs_Laughs},
                new DatabaseFieldInfo(){ FieldName="laughCount", Nullable = false, Bool = false, Category=FieldCategory.Laughs, SubCategory=FieldSubCategory.Laughs_Laughs, Numeric=true},
                new DatabaseFieldInfo(){ FieldName="demoRecorderClientnum", Nullable = false, Bool = false, Category=FieldCategory.Laughs, SubCategory=FieldSubCategory.Laughs_Meta, Numeric=true},
                new DatabaseFieldInfo(){ FieldName="demoName", Nullable = false, Bool = false, Category=FieldCategory.Laughs, SubCategory=FieldSubCategory.Laughs_Meta},
                new DatabaseFieldInfo(){ FieldName="demoPath", Nullable = false, Bool = false, Category=FieldCategory.Laughs, SubCategory=FieldSubCategory.Laughs_Meta},
                new DatabaseFieldInfo(){ FieldName="duration", Nullable = false, Bool = false, Category=FieldCategory.Laughs, SubCategory=FieldSubCategory.Laughs_Meta, Numeric=true},
                new DatabaseFieldInfo(){ FieldName="demoTime", Nullable = false, Bool = false, Category=FieldCategory.Laughs, SubCategory=FieldSubCategory.Laughs_Meta, Numeric=true},
                new DatabaseFieldInfo(){ FieldName="lastGamestateDemoTime", Nullable = false, Bool = false, Category=FieldCategory.Laughs, SubCategory=FieldSubCategory.Laughs_Meta, Numeric=true},
                new DatabaseFieldInfo(){ FieldName="serverTime", Nullable = false, Bool = false, Category=FieldCategory.Laughs, SubCategory=FieldSubCategory.Laughs_Meta, Numeric=true},
                new DatabaseFieldInfo(){ FieldName="demoDateTime", Nullable = false, Bool =false, Category=FieldCategory.Laughs, SubCategory=FieldSubCategory.Laughs_Meta, Numeric=true},
            };
        }
    }

}
