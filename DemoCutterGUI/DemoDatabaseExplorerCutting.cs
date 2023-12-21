using BFF.DataVirtualizingCollection.DataVirtualizingCollection;
using DemoCutterGUI.DatabaseExplorerElements;
using DemoCutterGUI.TableMappings;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace DemoCutterGUI
{
    public class CuttingSettings {
        public int preBufferTime { get; set; } = 10000;
        public int postBufferTime { get; set; } = 10000;
        public bool reframe { get; set; } = true;
        public bool findOtherAngles { get; set; } = true;
        public bool merge { get; set; } = true;
        public bool interpolate { get; set; } = true;
        public bool discardProcessedDemos { get; set; } = true;
        public bool zipSecondaryDemos { get; set; } = false; // Not implemented

        public bool zipThirdPersons { get; set; } = true;
        public bool zipSimpleReframes { get; set; } = true;
        public bool zipFakeFindAngles { get; set; } = true;
        public bool zipKeepSimpleReframesIfNoMain { get; set; } = true;
    }


    public partial class DemoDatabaseExplorer
    {

        ObservableCollection<DemoCutGroup> demoCutsQueue = new ObservableCollection<DemoCutGroup>();
        const int LAUGHS_CUT_PRE_TIME = 10000;

        CuttingSettings CutSettings = new CuttingSettings();
        partial void Constructor2()
        {
            cuttingGroupBox.DataContext = CutSettings;
            cutQueueItemCountText.DataContext = demoCutsQueue;
        }

        private Dictionary<string, Tuple<Int64, int>> FindOtherDemosBasedOnKillTimes(string referenceDemo, Int64 demoTimeRangeStart, Int64 demoTimeRangeEnd, in HashSet<string> existingDemos)
        {
            HashSet<string> killHashes = new HashSet<string>();

            string cleanDemoPath = referenceDemo.Replace("'","''");
            List<Ret> resRef = dbConn.Query<Ret>($"SELECT ROWID,* FROM {categoryPanels[DatabaseFieldInfo.FieldCategory.Rets].tableName} WHERE demoTime>={demoTimeRangeStart} AND demoTime<={demoTimeRangeEnd} AND demoPath='{cleanDemoPath}'") as List<Ret>;
            if(resRef != null)
            {
                foreach(Ret ret in resRef)
                {
                    killHashes.Add(ret.hash);
                }
            }

            HashSet<string> newlyFoundDemoFiles = new HashSet<string>();
            Dictionary<string, Tuple<Int64, int>> newlyFoundDemoFileTimingsAndClientNums = new Dictionary<string, Tuple<Int64, int>>();
            foreach (string killHash in killHashes)
            {
                List<Ret> res = dbConn.Query<Ret>($"SELECT ROWID,* FROM {categoryPanels[DatabaseFieldInfo.FieldCategory.Rets].tableName} WHERE hash='{killHash}'") as List<Ret>;
                if (res != null)
                {
                    // First find the ret entry from the same demo as the killspree, for a reference time.
                    Int64 referenceDemoTime = 0;
                    bool referenceFileFound = false;
                    foreach (var result in res)
                    {
                        if (result.demoPath == referenceDemo)
                        {
                            referenceDemoTime = result.demoTime.Value;
                            referenceFileFound = true;
                            break;
                        }
                    }

                    if (referenceFileFound)
                    {
                        // Now find new files
                        foreach (var result in res)
                        {
                            if (!existingDemos.Contains(result.demoPath) && !newlyFoundDemoFiles.Contains(result.demoPath))
                            {
                                newlyFoundDemoFiles.Add(result.demoPath);
                                newlyFoundDemoFileTimingsAndClientNums[result.demoPath] = new Tuple<long, int>(result.demoTime.Value - referenceDemoTime, (int)result.demoRecorderClientnum.Value);
                            }
                        }
                    }
                }
            }
            return newlyFoundDemoFileTimingsAndClientNums;
        }


        private List<object> FindOtherAngles(object item,ref List<object> availableObjectPool)
        {
            List<object> otherItems = new List<object>();

            if (item is Ret)
            {
                Ret ret = item as Ret;
                if (ret == null) return otherItems;

                foreach (var otherItem in availableObjectPool)
                {
                    Ret otherRet = otherItem as Ret;
                    if (otherRet == ret) continue;
                    if(otherRet.hash == ret.hash)
                    {
                        otherItems.Add(otherItem);
                    }
                }
                foreach (var otherItem in otherItems)
                {
                    availableObjectPool.Remove(otherItem);
                }

                List<Ret> res = dbConn.Query<Ret>($"SELECT ROWID,* FROM {categoryPanels[DatabaseFieldInfo.FieldCategory.Rets].tableName} WHERE hash='{ret.hash}'") as List<Ret>;
                if(res != null)
                {
                    foreach (var result in res)
                    {
                        if (!otherItems.Contains(result) && !result.Equals(item))
                        {
                            otherItems.Add(result);
                        }
                    }
                }
            } else if (item is Capture)
            {
                Capture cap = item as Capture;
                if (cap == null) return otherItems;

                // Possible database methods of searching:
                // 1. Find kills near the cap. Find other demos that have these kills and build other demo list.
                // or 2. Find captures roughly around same serverTime with same capper name, same server, same team
                // 
                // 1 isn't bad but maybe overkill and 2 is kinda more elegant overall. But maybe I can just do both?

                HashSet<string> initialSourceDemoFiles = new HashSet<string>();
                initialSourceDemoFiles.Add(cap.demoPath);
                Capture longestCapAngle = cap;
                
                foreach(var otherItem in availableObjectPool)
                {
                    Capture otherCap = otherItem as Capture;
                    if (otherCap == cap) continue;
                    if (otherCap.IsLikelySameCapture(cap))
                    {
                        otherItems.Add(otherItem);
                        initialSourceDemoFiles.Add(otherCap.demoPath);
                        if (otherCap.flagHoldTime.Value > longestCapAngle.flagHoldTime.Value)
                        {
                            longestCapAngle = otherCap;
                        }
                    }
                }
                foreach (var otherItem in otherItems)
                {
                    availableObjectPool.Remove(otherItem);
                }

                string serverNameSearch = cap.serverName.Replace("'","''");
                string capperNameSearch = cap.capperName.Replace("'","''");
                List<Capture> res = dbConn.Query<Capture>($"SELECT id AS ROWID,* FROM {categoryPanels[DatabaseFieldInfo.FieldCategory.Captures].tableName} WHERE " +
                    $"serverName='{serverNameSearch}' AND " +
                    $"redScore={cap.redScore.Value} AND " +     // This whole block is just the SQL version of IsLikelySameCapture()
                    $"blueScore={cap.blueScore.Value} AND " +
                    $"redPlayerCount={cap.redPlayerCount.Value} AND " +
                    $"bluePlayerCount={cap.bluePlayerCount.Value} AND " +
                    $"capperName='{capperNameSearch}' AND " +
                    $"capperClientNum={cap.capperClientNum.Value} AND " +
                    $"flagTeam={cap.flagTeam.Value} AND " +
                    $"ABS(serverTime-{cap.serverTime})<={Constants.EVENT_VALID_MSEC}") as List<Capture>;
                if(res != null)
                {
                    foreach (var result in res)
                    {
                        if (!otherItems.Contains(result) && !result.Equals(item) && cap.IsLikelySameCapture(result))
                        {
                            otherItems.Add(result);
                            initialSourceDemoFiles.Add(result.demoPath);
                            if (result.flagHoldTime.Value > longestCapAngle.flagHoldTime.Value)
                            {
                                longestCapAngle = result;
                            }
                        }
                    }
                }

                // So, this all works decent enough now, but what if we have a demo that doesn't have the cap, but has some PART of the hold time in it?
                // Shouldn't we then maybe search for kills during the hold time and correlate them to other demos?
                // Or do we just let that be a TODO?
                // Nvm, here we go:
                Dictionary<string, Tuple<long, int>> newlyFoundDemoFileTimingsAndClientNums = FindOtherDemosBasedOnKillTimes(longestCapAngle.demoPath, longestCapAngle.demoTime.Value- longestCapAngle.flagHoldTime.Value, longestCapAngle.demoTime.Value, initialSourceDemoFiles);
                int index = -1;
                foreach (KeyValuePair<string, Tuple<long, int>> newDemoTiming in newlyFoundDemoFileTimingsAndClientNums)
                {
                    // Make a copy and change it up. We don't have a real entry since that killspree doesn't exist in that other demo file as a proper find from analyzer.
                    // Bit dirty but should do the trick.
                    Capture otherAngleCapture = longestCapAngle.Clone<Capture>();
                    otherAngleCapture.rowid = index--;
                    otherAngleCapture.demoTime = longestCapAngle.demoTime + newDemoTiming.Value.Item1;
                    otherAngleCapture.demoPath = newDemoTiming.Key;
                    otherAngleCapture.demoRecorderClientnum = newDemoTiming.Value.Item2;
                    otherItems.Add(otherAngleCapture);
                }
            } else if (item is Laughs)
            {
                Laughs laughs = item as Laughs;
                if (laughs == null) return otherItems;

                HashSet<string> initialSourceDemoFiles = new HashSet<string>();
                initialSourceDemoFiles.Add(laughs.demoPath);
                Laughs longestLaughAngle = laughs;
                
                foreach(var otherItem in availableObjectPool)
                {
                    Laughs otherLaugh = otherItem as Laughs;
                    if (otherLaugh == laughs) continue;
                    if (otherLaugh.IsLikelySameLaugh(laughs))
                    {
                        otherItems.Add(otherItem);
                        initialSourceDemoFiles.Add(otherLaugh.demoPath);
                        if (otherLaugh.duration.Value > longestLaughAngle.duration.Value)
                        {
                            longestLaughAngle = otherLaugh;
                        }
                    }
                }
                foreach (var otherItem in otherItems)
                {
                    availableObjectPool.Remove(otherItem);
                }

                /*
                 this.serverName == otherLaughs.serverName
                && this.laughs == otherLaughs.laughs
                && this.laughCount == otherLaughs.laughCount
                //&& this.chatlog == otherLaughs.chatlog
                && Math.Abs(this.serverTime.GetValueOrDefault(-99999) - otherLaughs.serverTime.GetValueOrDefault(-99999)) <= 1000L // Chats are reliable commands, so in theory this is uncertain since we can potentially get a print many seconds later due to extreme loss of packets. But it will simply have to do.
                && Math.Abs(this.duration.GetValueOrDefault(-99999) - otherLaughs.duration.GetValueOrDefault(-99999)) <= 1000L
                 */
                string serverNameSearch = laughs.serverName.Replace("'","''");
                string laughsSearch = laughs.laughs.Replace("'","''");
                List<Laughs> res = dbConn.Query<Laughs>($"SELECT id AS ROWID,* FROM {categoryPanels[DatabaseFieldInfo.FieldCategory.Laughs].tableName} WHERE " +
                    $"serverName='{serverNameSearch}' AND " +
                    $"laughs='{laughsSearch}' AND " +       // This whole block is just the SQL version of IsLikelySameLaugh()
                    $"ABS(serverTime-{laughs.serverTime})<=1000 AND " +
                    $"ABS(duration-{laughs.duration})<=1000") as List<Laughs>;
                if(res != null)
                {
                    foreach (var result in res)
                    {
                        if (!otherItems.Contains(result) && !result.Equals(item) && laughs.IsLikelySameLaugh(result))
                        {
                            otherItems.Add(result);
                            initialSourceDemoFiles.Add(result.demoPath);
                            if (result.duration.Value > longestLaughAngle.duration.Value)
                            {
                                longestLaughAngle = result;
                            }
                        }
                    }
                }

                // So, this all works decent enough now, but what if we have a demo that has laughs in spectator mode but we have another demo from the player's perspective that doesn't have the laughs?
                // So cross reference some kills over the duration of the laughs (and a bit before) and find other angles that way.
                Dictionary<string, Tuple<long, int>> newlyFoundDemoFileTimingsAndClientNums = FindOtherDemosBasedOnKillTimes(longestLaughAngle.demoPath, longestLaughAngle.demoTime.Value- longestLaughAngle.duration.Value- LAUGHS_CUT_PRE_TIME, longestLaughAngle.demoTime.Value, initialSourceDemoFiles);
                int index = -1;
                foreach (KeyValuePair<string, Tuple<long, int>> newDemoTiming in newlyFoundDemoFileTimingsAndClientNums)
                {
                    // Make a copy and change it up. We don't have a real entry since that killspree doesn't exist in that other demo file as a proper find from analyzer.
                    // Bit dirty but should do the trick.
                    Laughs otherAngleLaughs = longestLaughAngle.Clone<Laughs>();
                    otherAngleLaughs.rowid = index--;
                    otherAngleLaughs.demoTime = longestLaughAngle.demoTime + newDemoTiming.Value.Item1;
                    otherAngleLaughs.demoPath = newDemoTiming.Key;
                    otherAngleLaughs.demoRecorderClientnum = newDemoTiming.Value.Item2;
                    otherItems.Add(otherAngleLaughs);
                }
            } else if (item is DefragRun)
            {
                DefragRun run = item as DefragRun;
                if (run == null) return otherItems;

                // Possible database methods of searching:
                // 1. Find kills near the cap. Find other demos that have these kills and build other demo list.
                // or 2. Find captures roughly around same serverTime with same capper name, same server, same team
                // 
                // 1 isn't bad but maybe overkill and 2 is kinda more elegant overall. But maybe I can just do both?

                
                foreach(var otherItem in availableObjectPool)
                {
                    DefragRun otherRun = otherItem as DefragRun;
                    if (otherRun == run) continue;
                    if (otherRun.IsLikelySameRun(run))
                    {
                        otherItems.Add(otherItem);
                    }
                }
                foreach (var otherItem in otherItems)
                {
                    availableObjectPool.Remove(otherItem);
                }
                /*
                 this.serverName == otherRun.serverName
                && this.totalMilliseconds == otherRun.totalMilliseconds
                && this.playerName == otherRun.playerName
                && this.runnerClientNum == otherRun.runnerClientNum
                && this.style == otherRun.style
                && Math.Abs(this.serverTime.GetValueOrDefault(-99999) - otherRun.serverTime.GetValueOrDefault(-99999)) <= 1000L
                 */
                string serverNameSearch = run.serverName.Replace("'","''");
                string playerNameSearch = run.playerName.Replace("'","''");
                string styleSearch = run.style?.Replace("'","''");
                List<DefragRun> res = dbConn.Query<DefragRun>($"SELECT ROWID,* FROM {categoryPanels[DatabaseFieldInfo.FieldCategory.DefragRuns].tableName} WHERE " +
                    $"serverName='{serverNameSearch}' AND " +
                    $"totalMilliseconds={run.totalMilliseconds.Value} AND " +     // This whole block is just the SQL version of IsLikelySameRun()
                    $"playerName='{playerNameSearch}' AND " +
                    $"runnerClientNum={run.runnerClientNum.Value} AND " +
                    (styleSearch is null ? "" : $"style='{styleSearch}' AND ") +
                    $"ABS(serverTime-{run.serverTime})<=1000") as List<DefragRun>;
                if(res != null)
                {
                    foreach (var result in res)
                    {
                        if (!otherItems.Contains(result) && !result.Equals(item) && run.IsLikelySameRun(result))
                        {
                            otherItems.Add(result);
                        }
                    }
                }

            }
            else if (item is KillSpree)
            {
                KillSpree spree = item as KillSpree;
                if (spree == null) return otherItems;

                HashSet<string> initialSourceDemoFiles = new HashSet<string>();

                initialSourceDemoFiles.Add(spree.demoPath);

                foreach (var otherItem in availableObjectPool)
                {
                    KillSpree otherSpree = otherItem as KillSpree;
                    if (otherSpree == spree) continue;
                    if(otherSpree.hash == spree.hash)
                    {
                        otherItems.Add(otherItem);
                        initialSourceDemoFiles.Add(otherSpree.demoPath);
                    }
                }
                foreach (var otherItem in otherItems)
                {
                    availableObjectPool.Remove(otherItem);
                }

                // Find other demos with the killspree
                { 
                    List<KillSpree> res = dbConn.Query<KillSpree>($"SELECT ROWID,* FROM {categoryPanels[DatabaseFieldInfo.FieldCategory.KillSprees].tableName} WHERE hash='{spree.hash}'") as List<KillSpree>;
                    if (res != null)
                    {
                        foreach (var result in res)
                        {
                            if (!otherItems.Contains(result) && !result.Equals(item))
                            {
                                otherItems.Add(result);
                                initialSourceDemoFiles.Add(result.demoPath);
                            }
                        }
                    }
                }

                // For finding more other angles we gotta be a little bit more elaborate
                // because by default killsprees don't include invisible kills, so we gotta look for overlaps of individual kills
                // to get a fuller image
                string[] killHashes = spree.killHashes?.Split('\n',StringSplitOptions.RemoveEmptyEntries|StringSplitOptions.TrimEntries);

                HashSet<string> newlyFoundDemoFiles = new HashSet<string>();
                Dictionary<string, Tuple<Int64,int>> newlyFoundDemoFileTimingsAndClientNums = new Dictionary<string, Tuple<Int64, int>>();
                foreach(string killHash in killHashes)
                {
                    List<Ret> res = dbConn.Query<Ret>($"SELECT ROWID,* FROM {categoryPanels[DatabaseFieldInfo.FieldCategory.Rets].tableName} WHERE hash='{killHash}'") as List<Ret>;
                    if (res != null)
                    {
                        // First find the ret entry from the same demo as the killspree, for a reference time.
                        Int64 referenceDemoTime = 0;
                        bool referenceFileFound = false; 
                        foreach (var result in res)
                        {
                            if (result.demoPath == spree.demoPath)
                            {
                                referenceDemoTime = result.demoTime.Value;
                                referenceFileFound = true;
                                break;
                            }
                        }

                        if (referenceFileFound)
                        {
                            // Now find new files
                            foreach (var result in res)
                            {
                                if (!initialSourceDemoFiles.Contains(result.demoPath) && !newlyFoundDemoFiles.Contains(result.demoPath) )
                                {
                                    newlyFoundDemoFiles.Add(result.demoPath);
                                    newlyFoundDemoFileTimingsAndClientNums[result.demoPath] = new Tuple<long, int>( result.demoTime.Value - referenceDemoTime,(int)result.demoRecorderClientnum.Value);
                                }
                            }
                        }
                    }
                }

                int index = -1;
                foreach(KeyValuePair<string, Tuple<long, int>> newDemoTiming in newlyFoundDemoFileTimingsAndClientNums)
                {
                    // Make a copy and change it up. We don't have a real entry since that killspree doesn't exist in that other demo file as a proper find from analyzer.
                    // Bit dirty but should do the trick.
                    KillSpree otherAngleKillSpree = spree.Clone<KillSpree>();
                    otherAngleKillSpree.rowid = index--;
                    otherAngleKillSpree.demoTime = spree.demoTime + newDemoTiming.Value.Item1;
                    otherAngleKillSpree.demoPath = newDemoTiming.Key;
                    otherAngleKillSpree.demoRecorderClientnum = newDemoTiming.Value.Item2;
                    otherItems.Add(otherAngleKillSpree);
                }

                /*List<Ret> res = dbConn.Query<Ret>($"SELECT ROWID,* FROM {categoryPanels[DatabaseFieldInfo.FieldCategory.Rets].tableName} WHERE hash='{ret.hash}'") as List<Ret>;
                if(res != null)
                {
                    foreach (var result in res)
                    {
                        if (!otherItems.Contains(result) && !result.Equals(item))
                        {
                            otherItems.Add(result);
                        }
                    }
                }*/
            }

            return otherItems;
        }

        private void EnqueueCutEntries(List<object> items)
        {
            List<DemoCutGroup> cutData = new List<DemoCutGroup>();
            Dictionary<string,Tuple<DemoCut, DemoCutGroup>> originalDemoOutputPaths = new Dictionary<string, Tuple<DemoCut, DemoCutGroup>>();
            while (items.Count > 0)
            {
                DemoCutGroup newGroup = new DemoCutGroup();
                List<DemoCut> originalCuts = new List<DemoCut>();
                object mainItem = items[0];
                DemoCut mainCut = MakeDemoName(mainItem, CutSettings.preBufferTime, CutSettings.postBufferTime);
                originalCuts.Add(mainCut);
                newGroup.demoCuts.Add(mainCut);
                items.Remove(mainItem);

                if(CutSettings.discardProcessedDemos && mainCut.isPreProcessed)
                {
                    // This is a demo that was already reframed/merged before. We don't wanna use those as source usually.
                    continue;
                }

                if (originalDemoOutputPaths.ContainsKey(mainCut.GetFinalName(true)))
                {
                    // Avoid duplicates (can happen if a demo was analyzed multiple times by accident). Not the most elegant solution, maybe improve someday.
                    // Might break if we ever do custom file naming schemes
                    // Could break on stuff like multiple caps by the same person with the same time (unlikely but theoretically possible)

                    if (!CutSettings.findOtherAngles)
                    {
                        Tuple<DemoCut, DemoCutGroup> previousCut = originalDemoOutputPaths[mainCut.GetFinalName(true)];
                        bool removeOld = false;

                        // Compare the two.
                        if (demoMetaCache.Count > 0)
                        {
                            if (demoMetaCache.ContainsKey(mainCut.originalDemoPath) && demoMetaCache.ContainsKey(previousCut.Item1.originalDemoPath))
                            {
                                if(demoMetaCache[previousCut.Item1.originalDemoPath].fileSize >= demoMetaCache[mainCut.originalDemoPath].fileSize)
                                {
                                    // The current demo is probably a smaller cut (or just plain dupe) out of the other one before. Discard it.
                                    if (previousCut.Item1.demoCutTruncationOffset.HasValue && (!mainCut.demoCutTruncationOffset.HasValue || previousCut.Item1.demoCutTruncationOffset > mainCut.demoCutTruncationOffset))
                                    {
                                        MessageBox.Show($"{previousCut.Item1.originalDemoPath} is bigger or equal in size to {mainCut.originalDemoPath} but is MORE truncated, so using the latter. WEIRD!");
                                        removeOld = true;
                                    } else
                                    {
                                        continue;
                                    }
                                } else
                                {
                                    removeOld = true;
                                }
                            } else
                            {
                                MessageBox.Show($"{mainCut.originalDemoPath} or {previousCut.Item1.originalDemoPath} not found in demoMetaCache! WEIRD!");
                                continue;
                            }
                        } else
                        {
                            // We don't really know which is better necessarily, forget about it.
                            // Bad solution. We could check which of the demoPaths has more finds etc., but ultimately people should use a current version of 
                            // HighlightFinder anyway.
                            // We can do a quick and dirty check if one is more truncated...
                            if (previousCut.Item1.demoCutTruncationOffset.HasValue && (!mainCut.demoCutTruncationOffset.HasValue || previousCut.Item1.demoCutTruncationOffset > mainCut.demoCutTruncationOffset))
                            {
                                removeOld = true;
                            }
                            else
                            {
                                continue;
                            }
                        }

                        if (removeOld)
                        {
                            // Current demo is probably the main demo and the old one was a cut. Discard the old one.
                            cutData.Remove(previousCut.Item2);
                            originalDemoOutputPaths.Remove(previousCut.Item1.GetFinalName(true));
                        }

                    } else
                    {
                        MessageBox.Show($"originalDemoOutputPaths already contains {mainCut.GetFinalName(true)}, but finding other angles is active. Should have been found. WEIRD! Discarding new duplicate.");
                        continue;
                    }
                }
                originalDemoOutputPaths.Add(mainCut.GetFinalName(true), new Tuple<DemoCut, DemoCutGroup>( mainCut, newGroup));

                if (CutSettings.findOtherAngles)
                {
                    List<object> otherAngles = FindOtherAngles(mainItem, ref items);
                    foreach (var otherItem in otherAngles)
                    {
                        DemoCut otherAngleCut = MakeDemoName(otherItem, CutSettings.preBufferTime, CutSettings.postBufferTime);

                        if (CutSettings.discardProcessedDemos && otherAngleCut.isPreProcessed)
                        {
                            // This is a demo that was already reframed/merged before. We don't wanna use those as source usually.
                            continue;
                        }
                        if (originalDemoOutputPaths.ContainsKey(otherAngleCut.GetFinalName(true)))
                        {
                            // Avoid duplicates (can happen if a demo was analyzed multiple times by accident). Not the most elegant solution, maybe improve someday.
                            // Might break if we ever do custom file naming schemes
                            // Could break on stuff like multiple caps by the same person with the same time (unlikely but theoretically possible?)

                            Tuple<DemoCut, DemoCutGroup> previousCut = originalDemoOutputPaths[otherAngleCut.GetFinalName(true)];
                            bool removeOld = false;

                            // Compare the two.
                            if (demoMetaCache.Count > 0)
                            {
                                if (demoMetaCache.ContainsKey(otherAngleCut.originalDemoPath) && demoMetaCache.ContainsKey(previousCut.Item1.originalDemoPath))
                                {
                                    if (demoMetaCache[previousCut.Item1.originalDemoPath].fileSize >= demoMetaCache[otherAngleCut.originalDemoPath].fileSize)
                                    {
                                        // The current demo is probably a smaller cut (or just plain dupe) of the other one before. Discard it.
                                        if (previousCut.Item1.demoCutTruncationOffset.HasValue && (!otherAngleCut.demoCutTruncationOffset.HasValue || previousCut.Item1.demoCutTruncationOffset > otherAngleCut.demoCutTruncationOffset))
                                        {
                                            MessageBox.Show($"{previousCut.Item1.originalDemoPath} is bigger or equal in size to {otherAngleCut.originalDemoPath} but is MORE truncated, so using the latter. WEIRD!");
                                            removeOld = true;
                                        }
                                        else
                                        {
                                            continue;
                                        }
                                    }
                                    else
                                    {
                                        removeOld = true;
                                    }
                                }
                                else
                                {
                                    MessageBox.Show($"{otherAngleCut.originalDemoPath} or {previousCut.Item1.originalDemoPath} not found in demoMetaCache! WEIRD!");
                                    continue;
                                }
                            }
                            else
                            {
                                // We don't really know which is better, forget about it.
                                // Bad solution. We could check which of the demoPaths has more finds etc., but ultimately people should use a current version of 
                                // HighlightFinder anyway.
                                // We can do a quick and dirty check if one is more truncated...
                                if (previousCut.Item1.demoCutTruncationOffset.HasValue && (!otherAngleCut.demoCutTruncationOffset.HasValue || previousCut.Item1.demoCutTruncationOffset > otherAngleCut.demoCutTruncationOffset))
                                {
                                    removeOld = true;
                                }
                                else
                                {
                                    continue;
                                }
                            }

                            if (removeOld)
                            {
                                // Current demo is probably the main demo and the old one was a cut. Discard the old one.
                                if (newGroup != previousCut.Item2)
                                {
                                    MessageBox.Show($"{previousCut.Item1.GetFinalName(true)} is a duplicate but not part of the current group!! WEIRD!!");
                                    continue;
                                }
                                else
                                {
                                    newGroup.demoCuts.Remove(previousCut.Item1);
                                    originalDemoOutputPaths.Remove(previousCut.Item1.GetFinalName(true));
                                }
                            }

                        }
                        originalDemoOutputPaths.Add(otherAngleCut.GetFinalName(true), new Tuple<DemoCut, DemoCutGroup>(otherAngleCut, newGroup));
                        originalCuts.Add(otherAngleCut);
                        newGroup.demoCuts.Add(otherAngleCut);
                    }
                    
                }
                if (CutSettings.reframe)
                {
                    List<DemoCut> reframeDemoCuts = new List<DemoCut>();
                    foreach (DemoCut cut in newGroup.demoCuts)
                    {
                        if (cut.type == DemoCutType.CUT && cut.reframeClientNum != cut.demoRecorderClientNum) // Don't reframe if it was recorded by the relevant player himself. There is no way that there was any switching away from the angle then.
                        {
                            reframeDemoCuts.Add(new DemoCut()
                            {
                                originalDemoPath = $"{cut.GetFinalName()}{Path.GetExtension(cut.originalDemoPath)}",
                                demoName = $"{cut.GetFinalName()}_reframe{cut.reframeClientNum}",
                                type = DemoCutType.REFRAME,
                                reframeClientNum = cut.reframeClientNum
                            });
                        }
                    }
                    newGroup.demoCuts.AddRange(reframeDemoCuts);
                }
                if (CutSettings.merge && originalCuts.Count > 1)
                {
                    int? reframeClientNum = originalCuts[0].reframeClientNum;

                    // Find main reference name to use.
                    string bestBaseName = null;
                    VisibilityType bestVisType = VisibilityType.Invisible;
                    List<int> recorderClientNums = new List<int>();
                    List<string> sourceCutOutputNames = new List<string>();
                    foreach (DemoCut democut in originalCuts)
                    {
                        if (democut.visType >= bestVisType)
                        {
                            bestBaseName = democut.demoName;
                            bestVisType = democut.visType;
                        }
                        recorderClientNums.Add(democut.demoRecorderClientNum.Value);
                        sourceCutOutputNames.Add($"{democut.GetFinalName()}{Path.GetExtension(democut.originalDemoPath)}");
                        if (reframeClientNum != democut.reframeClientNum)
                        {
                            MessageBox.Show($"Wtf reframeclientnum different across group: {reframeClientNum} vs {democut.reframeClientNum}");
                        }
                    }
                    if (bestBaseName != null)
                    {
                        newGroup.demoCuts.Add(new DemoCut()
                        {
                            originalDemoPathsForMerge = sourceCutOutputNames.ToArray(),
                            demoRecorderClientNums = recorderClientNums.ToArray(),
                            demoName = $"{bestBaseName}_merge",
                            type = DemoCutType.MERGE,
                            reframeClientNum = reframeClientNum,
                            interpolate = CutSettings.interpolate
                        });
                    }
                }

                if (CutSettings.zipThirdPersons || CutSettings.zipSimpleReframes)
                {
                    bool mainAngleExists = false;
                    foreach (var cut in newGroup.demoCuts)
                    {
                        if(cut.visType == VisibilityType.Followed)
                        {
                            mainAngleExists = true;
                        }
                    }
                    foreach (var cut in newGroup.demoCuts)
                    {
                        if(cut.type == DemoCutType.CUT && CutSettings.zipThirdPersons && cut.visType < VisibilityType.Followed)
                        {
                            cut.zipAndDelete = true;
                        }
                        if(cut.type == DemoCutType.CUT && CutSettings.zipFakeFindAngles && cut.isFakeFind)
                        {
                            cut.zipAndDelete = true;
                        }
                        if(cut.type == DemoCutType.REFRAME && CutSettings.zipSimpleReframes && !(CutSettings.zipKeepSimpleReframesIfNoMain && !mainAngleExists))
                        {
                            cut.zipAndDelete = true;
                        }
                    }

                }

                cutData.Add(newGroup);
            }
            //foreach(var item in items)
            //{
            //    cutData.Add(MakeDemoName(item, CutSettings.preBufferTime, CutSettings.postBufferTime));
            //}
            //var demoName = MakeDemoName(midPanel.TheGrid.SelectedItem, CutSettings.preBufferTime, CutSettings.postBufferTime);
            //MessageBox.Show(demoName?.demoName);

            foreach(var item in cutData)
            {
                demoCutsQueue.Add(item);
            }
            //demoCutsQueue.AddRange(cutData);
            //MessageBox.Show(cutData.Count.ToString());
        }



        private Int64 queueItemCountCheckLimit = 1000;
        private void EnqueueCurrentViewEntriesBtn_Click(object sender, RoutedEventArgs e)
        {
            var category = GetActiveTabCategory();
            if (!category.HasValue) return;

            if (!sqlTableSyncDataFetchers.ContainsKey(category.Value) || !sqlTableItemsSources.ContainsKey(category.Value)) return;

            Int64 itemCount = sqlTableItemsSources[category.Value].Count;
            if (itemCount > queueItemCountCheckLimit)
            {
                if(MessageBox.Show($"Trying to queue {itemCount} items. Are you sure?",$"Over {queueItemCountCheckLimit} items?",MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                {
                    return;
                }
            }

            List<object> items = new List<object>();
            items.AddRange(sqlTableSyncDataFetchers[category.Value]());

            EnqueueCutEntries(items);
        }

        private void EnqueueSelectedEntriesBtn_Click(object sender, RoutedEventArgs e)
        {
            TabItem item = midSectionTabs.SelectedItem as TabItem;
            if (item == null) return;

            MidPanel midPanel = item.Content as MidPanel;
            if (midPanel == null)
            {
                midPanel = item.GetChildOfType<MidPanel>() as MidPanel; // future proofing a bit?
            }
            if (midPanel == null) return;

            List<object> items = new List<object>();
            var midPanelSelectedItems = midPanel.TheGrid.SelectedItems;
            if(midPanelSelectedItems != null)
            {
                foreach(var selectedItem in midPanelSelectedItems)
                {
                    items.Add(selectedItem);
                }
            }
            EnqueueCutEntries(items);
        }


        private void ShowEntryDemoNameBtn_Click(object sender, RoutedEventArgs e)
        {
            TabItem item = midSectionTabs.SelectedItem as TabItem;
            if (item == null) return;

            MidPanel midPanel = item.Content as MidPanel;
            if (midPanel == null)
            {
                midPanel = item.GetChildOfType<MidPanel>() as MidPanel; // future proofing a bit?
            }
            if (midPanel == null) return;

            var demoName = MakeDemoName(midPanel.TheGrid.SelectedItem, CutSettings.preBufferTime, CutSettings.postBufferTime);
            MessageBox.Show(demoName?.demoName);
        }


        private void clearCutQueueBtn_Click(object sender, RoutedEventArgs e)
        {
            demoCutsQueue.Clear();
        }

        private void generateCutScriptBtn_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Shell script (*.sh)|*.sh|Windows batch script (*.bat)|*.bat";
            if(sfd.ShowDialog() == true)
            {
                File.WriteAllText(sfd.FileName, GenerateCutScriptText(Path.GetExtension(sfd.FileName).Equals(".sh",StringComparison.InvariantCultureIgnoreCase)));
            }
        }

        private string GenerateCutScriptText(bool linuxShellScript)
        {
            StringBuilder sb = new StringBuilder();

            DemoCutGroup[] items = demoCutsQueue.ToArray();

            foreach(DemoCutGroup item in items)
            {
                List<string> filesToZipAndDelete = new List<string>();
                DemoCut[] cuts = item.demoCuts.ToArray();
                Array.Sort(cuts, (a,b)=> { return (int)a.type - (int)b.type; });
                DemoCutType lastCutType = DemoCutType.CUT;
                foreach(DemoCut cut in cuts)
                {
                    if (cut.type != lastCutType && linuxShellScript)
                    {
                        sb.Append("wait\n");
                    }
                    lastCutType = cut.type;
                    bool noMatch = false;
                    if (cut.type == DemoCutType.CUT)
                    {
                        sb.Append("DemoCutter ");
                        sb.Append($"\"{cut.originalDemoPath}\" ");
                        sb.Append($"\"{cut.GetFinalName()}\" ");
                        sb.Append($"{cut.demoTimeStart} ");
                        sb.Append($"{cut.demoTimeEnd}");
                        if (cut.zipAndDelete)
                        {
                            filesToZipAndDelete.Add($"{cut.GetFinalName()}{Path.GetExtension(cut.originalDemoPath)}");
                        }
                    } else if (cut.type == DemoCutType.REFRAME && cut.reframeClientNum.GetValueOrDefault(-1) >= 0 && cut.reframeClientNum.GetValueOrDefault(-1) < 64) // TODO dynamic for demo type?
                    {
                        sb.Append("DemoReframer ");
                        sb.Append($"\"{cut.originalDemoPath}\" ");
                        sb.Append($"\"{cut.GetFinalName()}{Path.GetExtension(cut.originalDemoPath)}\" ");
                        sb.Append($"{cut.reframeClientNum}");
                        if (cut.zipAndDelete)
                        {
                            filesToZipAndDelete.Add($"{cut.GetFinalName()}{Path.GetExtension(cut.originalDemoPath)}");
                        }
                    }
                    else if (cut.type == DemoCutType.MERGE && cut.originalDemoPathsForMerge.Length > 0) 
                    {
                        sb.Append("DemoMerger ");
                        sb.Append($"\"{cut.GetFinalName()}{Path.GetExtension(cut.originalDemoPathsForMerge[0])}\" ");
                        foreach (string originalDemo in cut.originalDemoPathsForMerge)
                        {
                            sb.Append($"\"{originalDemo}\" ");
                        }
                        if(cut.reframeClientNum.GetValueOrDefault(-1) >= 0 && cut.reframeClientNum.GetValueOrDefault(-1) < 64) // TODO dynamic for demo type?
                        {
                            sb.Append($"-r {cut.reframeClientNum}");
                            if (cut.interpolate)
                            {
                                sb.Append($" -i -I");
                            }
                        }
                        //sb.Append($"\n");
                        if (cut.zipAndDelete)
                        {
                            filesToZipAndDelete.Add($"{cut.GetFinalName()}{Path.GetExtension(cut.originalDemoPathsForMerge[0])}");
                        }
                    } else
                    {
                        noMatch = true;
                    }
                    if (!noMatch)
                    {
                        if (linuxShellScript)
                        {
                            sb.Append(" & \n");
                        }
                        else
                        {
                            sb.Append("\n");
                        }
                    }
                }
                if(filesToZipAndDelete.Count > 0)
                {
                    if (linuxShellScript)
                    {
                        sb.Append("wait\n");
                    }
                    sb.Append($"7za a lessImportantDemoAngles.7z");
                    foreach (string file in filesToZipAndDelete)
                    {
                        sb.Append($" \"{file}\"");
                    }
                    sb.Append($" -mx9 -myx9 -sdel");
                    if (linuxShellScript)
                    {
                        sb.Append(" & \n");
                    }
                    else
                    {
                        sb.Append("\n");
                    }
                }
                sb.Append($"\n\n");
            }

            return sb.ToString();
        }




        // TODO Add Laughs
        private string getResultingCapturesString(long? resultingSelfCaptures, long? resultingCaptures, long? resultingLaughs, long? resultingSelfCapturesAfter, long? resultingCapturesAfter, long? resultingLaughsAfter)
        {
            StringBuilder sb = new StringBuilder();
            if (resultingSelfCaptures >= 1)
            {
                long ledToCapturesAfter = resultingSelfCapturesAfter.HasValue ? resultingSelfCapturesAfter.Value: resultingSelfCaptures.Value;
                sb.Append("_LTSC");
                if (ledToCapturesAfter == resultingSelfCaptures)
                {
                    sb.Append("A");
                    if (resultingSelfCaptures > 1)
                    {
                        sb.Append(resultingSelfCaptures);
                    }
                }
                else
                {
                    if (resultingSelfCaptures > 1)
                    {
                        sb.Append(resultingSelfCaptures);
                    }
                    if (ledToCapturesAfter > 0)
                    {
                        sb.Append("_LTSCA");
                        if (ledToCapturesAfter > 1)
                        {
                            sb.Append(ledToCapturesAfter);
                        }
                    }
                }
            }
            if (resultingCaptures >= 1)
            {
                long ledToTeamCapturesAfter = resultingCapturesAfter.HasValue ? resultingCapturesAfter.Value : resultingCaptures.Value;
                sb.Append("_LTC");
                if (resultingCaptures == ledToTeamCapturesAfter)
                {
                    sb.Append("A");
                    if (resultingCaptures > 1)
                    {
                        sb.Append(resultingCaptures);
                    }
                }
                else
                {
                    if (resultingCaptures > 1)
                    {
                        sb.Append(resultingCaptures);
                    }
                    if (ledToTeamCapturesAfter > 0)
                    {
                        sb.Append("_LTCA");
                        if (ledToTeamCapturesAfter > 1)
                        {
                            sb.Append(ledToTeamCapturesAfter);
                        }
                    }
                }
            }
            if (resultingLaughs >= 1)
            {
                long ledToLaughsAfter = resultingLaughsAfter.HasValue ? resultingLaughsAfter.Value : resultingLaughs.Value;
                sb.Append("_LGH");
                if (resultingLaughs == ledToLaughsAfter)
                {
                    sb.Append("A");
                    if (resultingLaughs > 1)
                    {
                        sb.Append(resultingLaughs);
                    }
                }
                else
                {
                    if (resultingLaughs > 1)
                    {
                        sb.Append(resultingLaughs);
                    }
                    if (ledToLaughsAfter > 0)
                    {
                        sb.Append("_LGHA");
                        if (ledToLaughsAfter > 1)
                        {
                            sb.Append(ledToLaughsAfter);
                        }
                    }
                }
            }
            return sb.ToString();
        }

        enum DemoCutType
        {
            CUT,
            REFRAME,
            MERGE
        }
        enum VisibilityType
        {
            Invisible,
            Unknown,
            PartiallyInvisible,
            Thirdperson,
            Followed
        }

        class DemoCut {
            public const string recorderClientNumPlaceHolder = "#@#@#@#recorderClientNum#@#@#@#";
            public const string truncationPlaceHolder = "#@#@#@#demoCutTrim#@#@#@#";
            public DemoCutType type;
            public VisibilityType visType;
            public string[] originalDemoPathsForMerge = null;
            public string originalDemoPath;
            public string demoName;
            public int? reframeClientNum = null;
            public int? demoRecorderClientNum = null;
            public Int64? demoCutTruncationOffset = null;
            public int[] demoRecorderClientNums = null;
            public Int64 demoTimeStart;
            public Int64 demoTimeEnd;
            public bool zipAndDelete = false;
            public bool isFakeFind = false;
            public bool interpolate = false;
            public bool isPreProcessed = false;
            public string GetFinalName(bool genericTrim = false)
            {
                switch (type)
                {
                    default:
                        return "WEIRD DEMONAME WTF SPECIFY THE TAPE YOU ANIMAL";
                    case DemoCutType.CUT:
                        return Helpers.DemoCuttersanitizeFilename(demoName?.Replace(recorderClientNumPlaceHolder, demoRecorderClientNum.Value.ToString())?.Replace(truncationPlaceHolder, genericTrim ? "" : (demoCutTruncationOffset.HasValue? $"_tr{demoCutTruncationOffset.Value.ToString()}" : "")),false);
                    case DemoCutType.REFRAME:
                        return Helpers.DemoCuttersanitizeFilename(demoName,false);
                    case DemoCutType.MERGE:
                        return Helpers.DemoCuttersanitizeFilename(demoName?.Replace(recorderClientNumPlaceHolder, string.Join('_', demoRecorderClientNums))?.Replace(truncationPlaceHolder, genericTrim ? "" : (demoCutTruncationOffset.HasValue ? $"_tr{demoCutTruncationOffset.Value.ToString()}" : "")), false);
                }
            }
        }

        class DemoCutGroup
        {
            public List<DemoCut> demoCuts = new List<DemoCut>();

        }

        private bool IsDemoPreProcessedByKills(string demoPath)
        {
            string demoPathSearch = demoPath.Replace("'","''");
            //List<Ret> res = dbConn.Query<Ret>($"SELECT ROWID,* FROM {categoryPanels[DatabaseFieldInfo.FieldCategory.Rets].tableName} WHERE demoPath='{demoPathSearch}' AND serverName='^1^7^1FAKE ^4^7^4DEMO'") as List<Ret>;
            //return res.Count > 0;
            int res = dbConn.ExecuteScalar<int>($"SELECT COUNT(*) FROM {categoryPanels[DatabaseFieldInfo.FieldCategory.Rets].tableName} WHERE demoPath='{demoPathSearch}' AND serverName!='^1^7^1FAKE ^4^7^4DEMO'");
            return res == 0; // Sadly we have to do this reversed (make sure there is not a single kill WITHOUT fake demo), because serverName is not saved per kill angle but per kill so if the reframe is analyzed first, .... yikes. We need to change this up, but in the meantime this will have to do. We also should save serverName directly in the killspree.
        }

        private DemoCut MakeDemoName(object entry, int preBuffertime, int postBufferTime)
        {

            // BIG TODO: Do the meta events as well!

            DemoCut retVal = new DemoCut() { type = DemoCutType.CUT };

            if(!(entry is null))
            {
                retVal.isFakeFind = (entry as TableMapping).IsCopiedEntry;
            }

            if(entry is Ret)
            {
                Ret ret = entry as Ret;
                if (ret == null) return null;
                string boostString = ((ret.boostCountAttacker + ret.boostCountVictim) > 0 ? ( "_BST" +  (ret.boostCountAttacker > 0 ? $"{ret.boostCountAttacker}A" : "") +( ret.boostCountVictim > 0 ? $"{ret.boostCountVictim}V" : "")) : "");
                StringBuilder sb = new StringBuilder();
                retVal.originalDemoPath = ret.demoPath;
                retVal.reframeClientNum = (int?)ret.killerClientNum;
                sb.Append(ret.map);
                sb.Append("___");
                sb.Append(ret.meansOfDeathString);
                sb.Append(boostString);
                sb.Append(getResultingCapturesString(ret.resultingSelfCaptures, ret.resultingCaptures, ret.resultingLaughs, null, null, null));
                sb.Append("___");
                sb.Append(ret.killerName);
                sb.Append("___");
                sb.Append(ret.victimName);
                sb.Append("___");
                sb.Append((int)ret.maxSpeedAttacker);
                sb.Append("_");
                sb.Append((int)ret.maxSpeedTarget);
                sb.Append("ups");
                sb.Append(ret.attackerIsFollowed == true ? "" : "___thirdperson");
                sb.Append(ret.attackerIsFollowedOrVisible == true ? "" : "_attackerInvis");
                sb.Append(ret.targetIsFollowedOrVisible == true ? "" : "_targetInvis");
                sb.Append("_");
                sb.Append(ret.killerClientNum);
                sb.Append("_");
                sb.Append(DemoCut.recorderClientNumPlaceHolder);
                retVal.demoRecorderClientNum = (int?)ret.demoRecorderClientnum;

                retVal.visType = ret.attackerIsFollowed == true ? VisibilityType.Followed : (ret.attackerIsFollowedOrVisible == true ? VisibilityType.Thirdperson : (ret.targetIsFollowedOrVisible == true ? VisibilityType.PartiallyInvisible : VisibilityType.Invisible));

                long demoTime = ret.demoTime.Value; // Just assume that demoTime exists. Otherwise there's nothing we can do anyway.
                Int64 startTime = demoTime - preBuffertime;
                Int64 endTime = demoTime + postBufferTime;
                Int64 earliestPossibleStart = ret.lastGamestateDemoTime.GetValueOrDefault(0) + 1;
                bool isTruncated = false;
                Int64 truncationOffset = 0;
                if (earliestPossibleStart > startTime)
                {
                    truncationOffset = earliestPossibleStart - startTime;
                    startTime = earliestPossibleStart;
                    isTruncated = true;
                    retVal.demoCutTruncationOffset = truncationOffset;
                }

                sb.Append(DemoCut.truncationPlaceHolder);
                //sb.Append(isTruncated ? $"_tr{truncationOffset}" : "");
                sb.Append("_");
                sb.Append(ret.shorthash);

                sb.Append((entry as TableMapping).IsCopiedEntry ? "_fakeFindOtherAngle" : "");

                if (dbProperties.serverNameInKillAngles)
                {
                    // The way it's meant to be.
                    retVal.isPreProcessed = ret.serverName == "^1^7^1FAKE ^4^7^4DEMO";
                }
                else
                {
                    // Older DemoHighlightFidner versions save it into the kills table, aka unique for each kill, but not specific to each kill angle.
                    // So if a pre-processed demo is analyzed first, it just assumes that any kills in there are just generally pre-processed.
                    // This is a shitty workaround to test the entire demo (because usually processed ones are from smaller cuts), but it's not really 100% reliable, and slow.
                    retVal.isPreProcessed = IsDemoPreProcessedByKills(ret.demoPath);
                }
                retVal.demoName = sb.ToString();
                retVal.demoTimeStart = startTime;
                retVal.demoTimeEnd = endTime;
                return retVal;
            } else if(entry is Capture)
            {
                Capture cap = entry as Capture;
                if (cap == null) return null;
                
                StringBuilder sb = new StringBuilder();
                retVal.originalDemoPath = cap.demoPath;
                retVal.reframeClientNum = (int?)cap.capperClientNum;
                sb.Append(cap.map);
                sb.Append("___CAPTURE");
                sb.Append(cap.capperKills > 0 ? $"{cap.capperKills}K" : "");
                sb.Append(cap.capperRets > 0 ? $"{cap.capperRets}R": "");

                sb.Append(getResultingCapturesString(null, null, cap.resultingLaughs, null, null, cap.resultingLaughsAfter));

                sb.Append("___");
                int milliSeconds = (int)cap.flagHoldTime.Value;
                int pureMilliseconds = milliSeconds % 1000;
                int seconds = milliSeconds / 1000;
                int pureSeconds = seconds % 60;
                int minutes = seconds / 60;

                sb.Append(minutes.ToString("000"));
                sb.Append("-");
                sb.Append(pureSeconds.ToString("00"));
                sb.Append("-");
                sb.Append(pureMilliseconds.ToString("000"));

                sb.Append($"___{cap.capperName}");
                sb.Append($"___P{cap.flagPickupSource}T{cap.flagTeam}");
                sb.Append($"___P{(int)cap.moreThanOneVeryCloseEnemyTimePercent}");
                sb.Append($"DANGER{(int)(cap.averageVeryCloseEnemyCount*100)}");
                sb.Append($"___{(int)cap.maxSpeedCapper}_{(int)cap.averageSpeedCapper}ups");
                sb.Append(cap.capperWasFollowed == true ? "" : (cap.capperWasFollowedOrVisible == true ? "___thirdperson" : "___NOTvisible"));
                sb.Append("_");
                sb.Append(cap.capperClientNum);
                sb.Append("_");
                sb.Append(DemoCut.recorderClientNumPlaceHolder);
                retVal.demoRecorderClientNum = (int?)cap.demoRecorderClientnum;

                retVal.visType = cap.capperWasFollowed == true ? VisibilityType.Followed : (cap.capperWasFollowedOrVisible == true ? VisibilityType.Thirdperson : (cap.capperIsFollowedOrVisible == true ? VisibilityType.PartiallyInvisible : VisibilityType.Invisible));

                long demoTime = cap.demoTime.Value; // Just assume that demoTime exists. Otherwise there's nothing we can do anyway.

                Int64 capStart = demoTime - cap.flagHoldTime.Value;
                Int64 startTime = capStart - preBuffertime;
                Int64 endTime = demoTime + postBufferTime;
                Int64 earliestPossibleStart = cap.lastGamestateDemoTime.GetValueOrDefault(0) + 1;
                bool isTruncated = false;
                Int64 truncationOffset = 0;
                if (earliestPossibleStart > startTime)
                {
                    truncationOffset = earliestPossibleStart - startTime;
                    startTime = earliestPossibleStart;
                    isTruncated = true;
                    retVal.demoCutTruncationOffset = truncationOffset;
                }

                sb.Append(DemoCut.truncationPlaceHolder);
                //sb.Append(isTruncated ? $"_tr{truncationOffset}" : "");

                sb.Append((entry as TableMapping).IsCopiedEntry ? "_fakeFindOtherAngle" : "");

                retVal.isPreProcessed = cap.serverName == "^1^7^1FAKE ^4^7^4DEMO";
                retVal.demoName = sb.ToString();
                retVal.demoTimeStart = startTime;
                retVal.demoTimeEnd = endTime;
                return retVal;
            } else if(entry is KillSpree)
            {
                KillSpree spree = entry as KillSpree;
                if (spree == null) return null;
                
                StringBuilder sb = new StringBuilder();

                retVal.originalDemoPath = spree.demoPath;
                retVal.reframeClientNum = (int?)spree.killerClientNum;
                sb.Append(spree.map);
                sb.Append("___KILLSPREE");
                sb.Append(spree.maxDelay);
                sb.Append("_");
                sb.Append(spree.countKills);
                sb.Append(spree.countRets > 0 ? $"R{spree.countRets}" : "");
                sb.Append(spree.countDooms > 0 ? $"D{spree.countDooms}" : "");
                sb.Append(spree.countExplosions > 0 ? $"E{spree.countExplosions}" : "");
                sb.Append(spree.countTeamKills > 0 ? $"T{spree.countTeamKills}" : "");
                sb.Append("_U");
                sb.Append(spree.countUniqueTargets);

                sb.Append(getResultingCapturesString(spree.resultingSelfCaptures, spree.resultingCaptures, spree.resultingLaughs, spree.resultingSelfCapturesAfter, spree.resultingCapturesAfter, spree.resultingLaughsAfter));

                sb.Append("___");
                sb.Append(spree.killerName);
                sb.Append("__");
                string[] victims = spree.victimClientNums.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                for (int i = 0; i < victims.Length; i++)
                {
                    sb.Append($"_{victims[i]}");
                }
                sb.Append("___");
                sb.Append((int)spree.maxSpeedAttacker);
                sb.Append("_");
                sb.Append((int)spree.maxSpeedTargets);
                sb.Append("ups");
                sb.Append(spree.countThirdPersons > 0 ? $"___thirdperson{spree.countThirdPersons}" : "");
                sb.Append(spree.countInvisibles > 0 ? $"___invisibles{spree.countInvisibles}" : "");
                sb.Append("___");
                sb.Append(spree.killerClientNum);
                sb.Append("_");
                sb.Append(DemoCut.recorderClientNumPlaceHolder);
                retVal.demoRecorderClientNum = (int?)spree.demoRecorderClientnum;

                retVal.visType = spree.countInvisibles > 0 ? (spree.countInvisibles >= spree.countKills ? VisibilityType.Invisible : VisibilityType.PartiallyInvisible) : ( spree.countThirdPersons > 0 ? VisibilityType.Thirdperson : VisibilityType.Followed);

                long demoTime = spree.demoTime.Value; // Just assume that demoTime exists. Otherwise there's nothing we can do anyway.
                Int64 spreeStart = demoTime - spree.duration.Value;
                Int64 startTime = spreeStart - preBuffertime;
                Int64 endTime = demoTime + postBufferTime;
                Int64 earliestPossibleStart = spree.lastGamestateDemoTime.GetValueOrDefault(0) + 1;
                bool isTruncated = false;
                Int64 truncationOffset = 0;
                if (earliestPossibleStart > startTime)
                {
                    truncationOffset = earliestPossibleStart - startTime;
                    startTime = earliestPossibleStart;
                    isTruncated = true;
                    retVal.demoCutTruncationOffset = truncationOffset;
                }

                sb.Append(DemoCut.truncationPlaceHolder);
                //sb.Append(isTruncated ? $"_tr{truncationOffset}": "");
                sb.Append("_");
                sb.Append(spree.shorthash);

                sb.Append((entry as TableMapping).IsCopiedEntry ? "_fakeFindOtherAngle" : "");

                if (dbProperties.serverNameInKillSpree)
                {
                    retVal.isPreProcessed = spree.serverName == "^1^7^1FAKE ^4^7^4DEMO";
                } else
                {
                    retVal.isPreProcessed = IsDemoPreProcessedByKills(spree.demoPath);
                }
                retVal.demoName = sb.ToString();
                retVal.demoTimeStart = startTime;
                retVal.demoTimeEnd = endTime;
                return retVal;
            } else if(entry is DefragRun)
            {
                DefragRun run = entry as DefragRun;
                if (run == null) return null;
                
                StringBuilder sb = new StringBuilder();

                retVal.originalDemoPath = run.demoPath;
                retVal.reframeClientNum = (int?)run.runnerClientNum;
                sb.Append(run.map);
                sb.Append(!string.IsNullOrWhiteSpace(run.style) ? $"___%{run.style}" : "");

                int totalMilliSeconds = (int)run.totalMilliseconds.Value;
                int pureMilliseconds = totalMilliSeconds % 1000;
                int tmpSeconds = totalMilliSeconds / 1000;
                int pureSeconds = tmpSeconds % 60;
                int minutes = tmpSeconds / 60;

                sb.Append("___");
                sb.Append(minutes.ToString("000"));
                sb.Append("-");
                sb.Append(pureSeconds.ToString("00"));
                sb.Append("-");
                sb.Append(pureMilliseconds.ToString("000"));
                sb.Append("___");
                sb.Append(run.playerName);
                //sb.Append(run.isNumber1 == true ? "" : "___top10");
                sb.Append(run.isNumber1 == false && run.isTop10 == true ?  "___top10" : ""); // run.isTop10 actually just means isLogged
                sb.Append(run.isTop10 == true ? "" : (run.isNumber1 == true ? "___unloggedWR" : "___unlogged"));
                sb.Append(run.wasFollowed== true ? "" : (run.wasFollowedOrVisible == true ? "___thirdperson" : "___NOTvisible"));
                sb.Append("_");
                sb.Append(run.runnerClientNum);
                sb.Append("_");
                sb.Append(DemoCut.recorderClientNumPlaceHolder);
                retVal.demoRecorderClientNum = (int?)run.demoRecorderClientnum;

                retVal.visType = run.wasFollowed == true ? VisibilityType.Followed : (run.wasFollowedOrVisible == true ? VisibilityType.Thirdperson : VisibilityType.Invisible);

                long demoTime = run.demoTime.Value; // Just assume that demoTime exists. Otherwise there's nothing we can do anyway.
                Int64 spreeStart = demoTime - run.totalMilliseconds.Value;
                Int64 startTime = spreeStart - preBuffertime;
                Int64 endTime = demoTime + postBufferTime;
                Int64 earliestPossibleStart = run.lastGamestateDemoTime.GetValueOrDefault(0) + 1;
                bool isTruncated = false;
                Int64 truncationOffset = 0;
                if (earliestPossibleStart > startTime)
                {
                    truncationOffset = earliestPossibleStart - startTime;
                    startTime = earliestPossibleStart;
                    isTruncated = true;
                    retVal.demoCutTruncationOffset = truncationOffset;
                }

                sb.Append(DemoCut.truncationPlaceHolder);
                //sb.Append(isTruncated ? $"_tr{truncationOffset}" : "");

                sb.Append((entry as TableMapping).IsCopiedEntry ? "_fakeFindOtherAngle" : "");


                retVal.isPreProcessed = run.serverName == "^1^7^1FAKE ^4^7^4DEMO";
                retVal.demoName = sb.ToString();
                retVal.demoTimeStart = startTime;
                retVal.demoTimeEnd = endTime;
                return retVal;
            } else if(entry is Laughs)
            {
                Laughs laughs = entry as Laughs;
                if (laughs == null) return null;
                
                StringBuilder sb = new StringBuilder();

                retVal.originalDemoPath = laughs.demoPath;
                retVal.reframeClientNum = null;

                sb.Append(laughs.map);
                sb.Append("___LAUGHS");
                sb.Append(laughs.laughCount);
                sb.Append("_");
                sb.Append(laughs.duration);
                sb.Append(laughs.laughs.Length > 70 ? laughs.laughs.Substring(0,70) : laughs.laughs);
                sb.Append(laughs.laughs.Length > 70 ? "--" : "");
                sb.Append("_");
                sb.Append(DemoCut.recorderClientNumPlaceHolder);
                retVal.demoRecorderClientNum = (int?)laughs.demoRecorderClientnum;
                retVal.visType = VisibilityType.Unknown;

                long demoTime = laughs.demoTime.Value; // Just assume that demoTime exists. Otherwise there's nothing we can do anyway.
                Int64 laughsStart = demoTime - laughs.duration.GetValueOrDefault(0) - LAUGHS_CUT_PRE_TIME;
                Int64 startTime = laughsStart - preBuffertime;
                Int64 endTime = demoTime + postBufferTime;
                Int64 earliestPossibleStart = laughs.lastGamestateDemoTime.GetValueOrDefault(0) + 1;
                bool isTruncated = false;
                Int64 truncationOffset = 0;
                if (earliestPossibleStart > startTime)
                {
                    truncationOffset = earliestPossibleStart - startTime;
                    startTime = earliestPossibleStart;
                    isTruncated = true;
                    retVal.demoCutTruncationOffset = truncationOffset;
                }

                sb.Append(DemoCut.truncationPlaceHolder);
                //sb.Append(isTruncated ? $"_tr{truncationOffset}" : "");

                sb.Append((entry as TableMapping).IsCopiedEntry ? "_fakeFindOtherAngle" : "");

                retVal.isPreProcessed = laughs.serverName == "^1^7^1FAKE ^4^7^4DEMO";
                retVal.demoName = sb.ToString();
                retVal.demoTimeStart = startTime;
                retVal.demoTimeEnd = endTime;
                return retVal;
            }

            return null;
        }
    }
}
