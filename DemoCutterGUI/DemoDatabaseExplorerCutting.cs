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

            DemoCutName retVal = new DemoCutName();

            if(entry is Ret)
            {
                Ret ret = entry as Ret;
                if (entry == null) return null;
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
                //(isTruncated ? va("_tr%d", truncationOffset) : "")
                sb.Append("_");
                sb.Append(ret.shorthash);


                retVal.demoName = sb.ToString();
                long? demoTime = ret.demoTime;
                retVal.demoTimeStart = !demoTime.HasValue ? -1: demoTime.Value - preBuffertime;
                retVal.demoTimeEnd = !demoTime.HasValue ? -1: demoTime.Value + postBufferTime;
                return retVal;
            }

            return null;
        }
    }
}
