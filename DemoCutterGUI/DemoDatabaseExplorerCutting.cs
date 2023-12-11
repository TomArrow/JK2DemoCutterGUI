using DemoCutterGUI.TableMappings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoCutterGUI
{
    public partial class DemoDatabaseExplorer
    {


        private string getResultingCapturesString(long? resultingSelfCaptures, long? resultingCaptures, long? resultingSelfCapturesAfter, long? resultingCapturesAfter)
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
            return sb.ToString();
        }

        class DemoCutName {
            public string demoName;
            public Int64 demoTimeStart;
            public Int64 demoTimeEnd;
        }

        private DemoCutName MakeDemoName(object entry, int preBuffertime, int postBufferTime)
        {

            // BIG TODO: Do the meta events as well!

            DemoCutName retVal = new DemoCutName();

            if(entry is Ret)
            {
                Ret ret = entry as Ret;
                if (ret == null) return null;
                string boostString = ((ret.boostCountAttacker + ret.boostCountVictim) > 0 ? ( "_BST" +  (ret.boostCountAttacker > 0 ? $"{ret.boostCountAttacker}A" : "") +( ret.boostCountVictim > 0 ? $"{ret.boostCountVictim}V" : "")) : "");
                StringBuilder sb = new StringBuilder();
                sb.Append(ret.map);
                sb.Append("___");
                sb.Append(ret.meansOfDeathString);
                sb.Append(boostString);
                sb.Append(getResultingCapturesString(ret.resultingSelfCaptures, ret.resultingCaptures, null, null));
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
                sb.Append(cap.map);
                sb.Append("___CAPTURE");
                sb.Append(cap.capperKills > 0 ? $"{cap.capperKills}K" : "");
                sb.Append(cap.capperRets > 0 ? $"{cap.capperRets}R": "");

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
            }

            return null;
        }
    }
}
