using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;

namespace DemoCutterGUI
{

    // Object to deserialize to
    public class DemoJSONMeta
    {
        [JsonPropertyName("hl")]
        public int? highlightOffset { get; set; }
        [JsonPropertyName("me")]
        public string metaEventsString { get; set; }
        [JsonPropertyName("cso")]
        public Int64? cutStartOffset { get; set; }
        [JsonPropertyName("oco")]
        // original cut offset. aka absolute offset from start of the once ancestral original demo before any cutting was done. this value is read back by subsequent cutting into originalFileAbsoluteCutOffset and then added to the new offset.
        public Int64? originalCutOffset { get; set; }
        [JsonPropertyName("odm")]
        [JsonConverter(typeof(UnixEpochDateTimeOffsetConverter))]
        public DateTime? originalDateModified { get; set; }
        [JsonPropertyName("oip")]
        public string originalIP { get; set; }
        [JsonPropertyName("ost")]
        [JsonConverter(typeof(UnixEpochDateTimeOffsetConverter))]
        public DateTime? originalStartTime { get; set; }
        [JsonPropertyName("wr")]
        public string writer { get; set; }
        [JsonPropertyName("of")]
        public string originalFilename { get; set; }

    }



    public class AdditionalHighlights : FullyObservableCollection<AdditionalHighlight>
    {
        /*public AdditionalHighlights()
        {
            this.CollectionChanged += AdditionalHighlights_CollectionChanged;
        }

        private void AdditionalHighlights_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            e.NewItems
        }*/

        //[JsonIgnore]
        //private Demo _owner = null;
        public void Add(int time, AdditionalHighlight.Type type = AdditionalHighlight.Type.METAEVENT_NONE, bool bypassExistsCheck = false)
        {
            lock (this)
            {
                if (bypassExistsCheck || !alreadyExists(time,type) )
                {
                    this.Add(new AdditionalHighlight() { time = time, /*associatedDemo = _owner,*/ type = type });
                }
            }
        }

        private bool alreadyExists(int time, AdditionalHighlight.Type type)
        {
            foreach (AdditionalHighlight ah in this)
            {
                if (ah.type == type && ah.time == time)
                {
                    return true;
                }
            }
            return false;
        }
        
        public void AddFromMetaString(string metaString, Int64 cutStartOffset)
        {
            string[] metaEventElements = metaString.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            lock (this)
            {
                for(int i = 0; i < metaEventElements.Length; i++)
                {
                    AdditionalHighlight.Type type;
                    int timeOffset;
                    if(AdditionalHighlight.TryParseMetaStringElement(metaEventElements[i], out type, out timeOffset))
                    {
                        timeOffset -= (int)cutStartOffset;
                        if (!alreadyExists(timeOffset, type))
                        {
                            this.Add(new AdditionalHighlight() { time = timeOffset/*, associatedDemo = _owner*/, type = type });
                        }
                        
                    }
                }
            }
        }
        /*
        public void SetOwner(Demo owner)
        {
            lock (this)
            {
                _owner = owner;
                foreach (var item in this)
                {
                    item.associatedDemo = _owner;
                }
            }
        }*/
    }



    public class AdditionalHighlight : INotifyPropertyChanged
    {
        static Regex metaStringRegex = new Regex(@"([A-Za-z]+)?([\d-]+)", RegexOptions.Compiled|RegexOptions.IgnoreCase|RegexOptions.CultureInvariant);

        public enum Type {
            METAEVENT_NONE,
            METAEVENT_TEAMCAPTURE,
            METAEVENT_ENEMYTEAMCAPTURE,
            METAEVENT_CAPTURE,
            METAEVENT_RETURN,
            METAEVENT_KILL,
            METAEVENT_DEATH,
            METAEVENT_JUMP,
            METAEVENT_SABERHIT, // any kind of saber hit, regardless of who is hit or who is ttacking
            METAEVENT_SABERBLOCK, // any saber block, no matter by who or to who
            METAEVENT_EFFECT, // effect event of any sort
            METAEVENT_COUNT
        }
        private static readonly Dictionary<string, Type> shortCutToType = new Dictionary<string, Type>()
        {
            {"tc",Type.METAEVENT_TEAMCAPTURE },
            {"ec",Type.METAEVENT_ENEMYTEAMCAPTURE },
            {"c",Type.METAEVENT_CAPTURE },
            {"r",Type.METAEVENT_RETURN },
            {"k",Type.METAEVENT_KILL },
            {"d",Type.METAEVENT_DEATH },
            {"j",Type.METAEVENT_JUMP },
            {"sh",Type.METAEVENT_SABERHIT },
            {"sb",Type.METAEVENT_SABERBLOCK },
            {"ef",Type.METAEVENT_EFFECT },
        };

        public static bool TryParseMetaStringElement(string metaString, out Type type, out int timeOffset)
        {
            Match match = metaStringRegex.Match(metaString);
            if (match.Success)
            {
                string key = match.Groups[1].Success ? match.Groups[1].Value : "";
                string timeOffsetString = match.Groups[2].Value;
                type = shortCutToType.ContainsKey(key) ? shortCutToType[key] : Type.METAEVENT_NONE;
                if(int.TryParse(timeOffsetString, out timeOffset))
                {
                    return true;
                } 
            }
            type = Type.METAEVENT_NONE;
            timeOffset = 0;
            return false;
        }

        [JsonIgnore]
        public string typeShortcut { set
            {
                if (shortCutToType.ContainsKey(value))
                {
                    type = shortCutToType[value];
                }
            } }

        public int time { get; set; } = 0;
        
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public Type type { get; set; } = Type.METAEVENT_NONE;
        //[JsonIgnore]
        //public Demo associatedDemo { get; internal set; } = null;
        //public AdditionalHighlight(int timeA) { time = timeA; }
        //public static implicit operator AdditionalHighlight(int timeA) => new AdditionalHighlight(timeA);

        public event PropertyChangedEventHandler PropertyChanged;
    }
    public class Demo : INotifyPropertyChanged
    {
        public string name { get; set; } = "";
        public int highlightDemoTime { get; set; } = 0;
        public int highlightOffset { get; set; } = 10000;
        private AdditionalHighlights _additionalHighlights = null;
        public AdditionalHighlights additionalHighlights { 
            get { return _additionalHighlights; } 
            init { 
                if(_additionalHighlights != null && _additionalHighlights != value)
                {
                    _additionalHighlights.CollectionChanged -= AdditionalHighlights_CollectionChanged;
                    _additionalHighlights.ItemPropertyChanged -= AdditionalHighlights_ItemPropertyChanged;
                }
                _additionalHighlights = value;
                /*_additionalHighlights.SetOwner(this);*/
                additionalHighlights.CollectionChanged += AdditionalHighlights_CollectionChanged;
                additionalHighlights.ItemPropertyChanged += AdditionalHighlights_ItemPropertyChanged;
            } 
        }

        [JsonIgnore]
        internal Demo wishAfter = null;
        [JsonIgnore]
        internal Demo wishBefore = null;

        public event PropertyChangedEventHandler PropertyChanged;

        public Demo(AdditionalHighlights additionalHighlightsA = null)
        {
            additionalHighlights = additionalHighlightsA == null? new AdditionalHighlights() : additionalHighlightsA;
            //additionalHighlights.SetOwner(this);
        }

        private void AdditionalHighlights_ItemPropertyChanged(object sender, ItemPropertyChangedEventArgs e)
        {
            OnPropertyChanged(new PropertyChangedEventArgs("additionalHighlights"));
        }

        private void AdditionalHighlights_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(new PropertyChangedEventArgs("additionalHighlights"));
        }

        public Demo()
        {
            additionalHighlights = new AdditionalHighlights();
            //additionalHighlights.SetOwner(this);
        }

        ~Demo()
        {
            if(additionalHighlights != null){
                additionalHighlights.CollectionChanged -= AdditionalHighlights_CollectionChanged;
                additionalHighlights.ItemPropertyChanged -= AdditionalHighlights_ItemPropertyChanged;
            }
        }

        public void loadDataFromMeta(DemoJSONMeta meta)
        {
            Int64 cutStartOffsetMeta = meta.cutStartOffset != null ? meta.cutStartOffset.Value : 0;
            if(meta.highlightOffset != null)
            {
                highlightOffset = (int)(meta.highlightOffset.Value - cutStartOffsetMeta);
            }
            if(meta.metaEventsString != null)
            {
                additionalHighlights.AddFromMetaString(meta.metaEventsString,cutStartOffsetMeta);
            }

        }
        protected void OnPropertyChanged(PropertyChangedEventArgs eventArgs)
        {
            PropertyChanged?.Invoke(this, eventArgs);
        }

        [JsonIgnore]
        public int sortIndex { get; internal set; } = 0;
    }


    class Demos
    {

        public delegate void ForeachHandler(in Demo point); // Technically the in keyword doessn't prevent changes to the point, but just be respectful. It's a hint for you.	

        public event EventHandler Updated;

        List<Demo> demos = new List<Demo>();
        ObservableCollection<Demo> demosObservable = new ObservableCollection<Demo>();

        ListView boundView = null;
        ICollectionView cv = null;

        public void bindListView(ListView view)
        {
            lock (demos)
            {
                boundView = view;
                cv = CollectionViewSource.GetDefaultView(demosObservable);
                cv.SortDescriptions.Add(new SortDescription("sortIndex", ListSortDirection.Ascending));
                view.ItemsSource = cv;
            }
        }

        public void Add(Demo demo, Demo after= null, Demo before= null)
        {
            lock (demos)
            {
                demo.PropertyChanged += Demo_PropertyChanged;
                demo.wishAfter = after;
                demo.wishBefore = before;
                demos.Add(demo);
                demosObservable.Add(demo);
                callOnUpdate();
            }
        }

        private void Demo_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnUpdated();
        }

        private void OnUpdated()
        {
            Updated?.Invoke(this, new EventArgs());
        }

        public void callOnUpdate()
        {
            lock (demos)
            {
                // Correctly sort in any that has wishAfter or wishBefore

                bool nothingToResort = false;

                while (!nothingToResort) // Maybe could be done more efficiently idk. But this seems stable. Can improve later.
                {
                    // First find and remove one demo that has a resort requested.
                    Demo toResort = null;
                    int count = demos.Count;
                    for (int i = count - 1; i >= 0; i--)
                    {
                        Demo demo = demos[i];
                        if (demo.wishAfter != null || demo.wishBefore != null)
                        {
                            toResort = demo;
                            demos.Remove(demo);
                            break;
                        }
                    }

                    if(toResort == null)
                    {
                        nothingToResort = true;
                        //continue;
                    }

                    // Store all the other demos in other array and rewrite the list while taking the to-be-resorted demo's wish into account
                    Demo[] otherDemos = demos.ToArray();
                    demos.Clear();

                    bool resorted = false;
                    int sortIndex = 0;
                    foreach(Demo demo in otherDemos)
                    {
                        if(toResort != null && toResort.wishBefore == demo)
                        {
                            demos.Add(toResort);
                            toResort.wishBefore = null;
                            toResort.wishAfter = null;
                            toResort.sortIndex = sortIndex++;
                            demos.Add(demo);
                            demo.sortIndex = sortIndex++;
                            resorted = true;
                        } else if(toResort != null && toResort.wishAfter == demo)
                        {
                            demos.Add(demo);
                            demo.sortIndex = sortIndex++;
                            demos.Add(toResort);
                            toResort.wishBefore = null;
                            toResort.wishAfter = null;
                            toResort.sortIndex = sortIndex++;
                            resorted = true;
                        }
                        else
                        {
                            demos.Add(demo);
                            demo.sortIndex = sortIndex++;
                        }
                    }
                    if (!resorted && toResort != null)
                    {
                        throw new Exception("Demos::callOnUpdate: Wasn't able to sort in the demo. This indicates a programming/logic error somewhere.");
                    }
                }
                if(cv != null)
                {
                    cv.Refresh();
                }

                OnUpdated();
            }

        }

        public void Remove(Demo demo)
        {
            lock (demos)
            {
                demo.PropertyChanged -= Demo_PropertyChanged;
                demos.Remove(demo);
                demosObservable.Remove(demo);
                callOnUpdate();
            }
        }
        public void Clear()
        {
            lock (demos)
            {
                Demo[] currentDemos = demos.ToArray();
                foreach(Demo demo in currentDemos)
                {
                    this.Remove(demo);
                }
                currentDemos = null;
            }
        }


        public void Foreach(ForeachHandler handler)
        {
            lock (demos)
            {
                foreach (Demo demo in demos)
                {
                    handler(demo);
                }
            }
        }
    }
}
