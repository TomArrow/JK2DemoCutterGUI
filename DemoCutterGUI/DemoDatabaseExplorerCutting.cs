using BFF.DataVirtualizingCollection.DataVirtualizingCollection;
using DemoCutterGUI.DatabaseExplorerElements;
using DemoCutterGUI.TableMappings;
using System;
using System.Collections.Generic;
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

    }


    public partial class DemoDatabaseExplorer
    {

        CuttingSettings CutSettings = new CuttingSettings();
        partial void Constructor2()
        {
            cuttingGroupBox.DataContext = CutSettings;
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

        private List<object> FindOtherAngles(object item,ref List<object> availableObjectPool)
        {
            List<object> otherItems = new List<object>();

            if (item is Ret)
            {
                Ret ret = item as Ret;
                if (ret == null) return otherItems;

                foreach(var otherItem in availableObjectPool)
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
            }

            return otherItems;
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

            List<DemoCutGroup> cutData = new List<DemoCutGroup>();
            while (items.Count > 0)
            {
                DemoCutGroup newGroup = new DemoCutGroup();
                object mainItem = items[0];
                DemoCutName mainCut = MakeDemoName(mainItem, CutSettings.preBufferTime, CutSettings.postBufferTime);
                newGroup.demoCuts.Add(mainCut);
                items.Remove(mainItem);

                if (CutSettings.reframe)
                {
                    newGroup.demoCuts.Add(new DemoCutName()
                    {
                        originalDemoPath = $"{mainCut.demoName}{Path.GetExtension(mainCut.originalDemoPath)}",
                        demoName = $"{mainCut.demoName}_reframe{mainCut.reframeClientNum}",
                        type = DemoCutType.REFRAME,
                        reframeClientNum = mainCut.reframeClientNum
                    });
                }
                if (CutSettings.findOtherAngles)
                {
                    List<object> otherAngles = FindOtherAngles(mainItem, ref items);
                    foreach(var otherItem in otherAngles)
                    {
                        DemoCutName otherAngleCut = MakeDemoName(otherItem, CutSettings.preBufferTime, CutSettings.postBufferTime);
                        newGroup.demoCuts.Add(otherAngleCut);
                        if (CutSettings.reframe)
                        {
                            newGroup.demoCuts.Add(new DemoCutName()
                            {
                                originalDemoPath = $"{otherAngleCut.demoName}{Path.GetExtension(otherAngleCut.originalDemoPath)}",
                                demoName = $"{otherAngleCut.demoName}_reframe{otherAngleCut.reframeClientNum}",
                                type = DemoCutType.REFRAME,
                                reframeClientNum = otherAngleCut.reframeClientNum
                            });
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



            MessageBox.Show(cutData.Count.ToString());
        }

        private void EnqueueSelectedEntriesBtn_Click(object sender, RoutedEventArgs e)
        {

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

        class DemoCutName {
            public DemoCutType type;
            public string originalDemoPath;
            public string demoName;
            public int? reframeClientNum = null;
            public Int64 demoTimeStart;
            public Int64 demoTimeEnd;
        }

        class DemoCutGroup
        {
            public List<DemoCutName> demoCuts = new List<DemoCutName>();

        }

        private DemoCutName MakeDemoName(object entry, int preBuffertime, int postBufferTime)
        {

            // BIG TODO: Do the meta events as well!

            DemoCutName retVal = new DemoCutName() { type = DemoCutType.CUT };

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
                sb.Append((ret.attackerIsFollowed == true ? "" : "___thirdperson"));
                sb.Append((ret.attackerIsFollowedOrVisible == true ? "" : "_attackerInvis"));
                sb.Append((ret.targetIsFollowedOrVisible == true ? "" : "_targetInvis"));
                sb.Append("_");
                sb.Append(ret.killerClientNum);
                sb.Append("_");
                sb.Append(ret.demoRecorderClientnum);

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
                }

                sb.Append(isTruncated ? $"_tr{truncationOffset}" : "");
                sb.Append("_");
                sb.Append(ret.shorthash);


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
                sb.Append(cap.demoRecorderClientnum);

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
                }

                sb.Append(isTruncated ? $"_tr{truncationOffset}" : "");


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
                sb.Append("___");
                sb.Append(spree.killerClientNum);
                sb.Append("_");
                sb.Append(spree.demoRecorderClientnum);

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
                }

                sb.Append(isTruncated ? $"_tr{truncationOffset}": "");
                sb.Append("_");
                sb.Append(spree.shorthash);


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
                sb.Append(run.demoRecorderClientnum);

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
                }

                sb.Append(isTruncated ? $"_tr{truncationOffset}" : "");


                retVal.demoName = sb.ToString();
                retVal.demoTimeStart = startTime;
                retVal.demoTimeEnd = endTime;
                return retVal;
            } else if(entry is Laughs)
            {
                const int LAUGHS_CUT_PRE_TIME = 10000;
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
                sb.Append(laughs.laughs.Substring(0,70));
                sb.Append(laughs.laughs.Length > 70 ? "--" : "");
                sb.Append("_");
                sb.Append(laughs.demoRecorderClientnum);

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
                }

                sb.Append(isTruncated ? $"_tr{truncationOffset}" : "");
                /*
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
                sb.Append(run.demoRecorderClientnum);

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
                }

                sb.Append(isTruncated ? $"_tr{truncationOffset}" : "");

                */
                retVal.demoName = sb.ToString();
                retVal.demoTimeStart = startTime;
                retVal.demoTimeEnd = endTime;
                return retVal;
            }

            return null;
        }
    }
}
