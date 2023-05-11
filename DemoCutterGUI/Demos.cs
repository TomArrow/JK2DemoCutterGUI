﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;

namespace DemoCutterGUI
{
    public class Demo : INotifyPropertyChanged
    {
        public string name { get; set; } = "";
        public int order { get; set; } = 0;
        public int highlightDemoTime { get; set; } = 0;
        public int highlightOffset { get; set; } = 10000;
        public List<int> additionalHighlights { get; init; } = new List<int>();

        internal Demo wishAfter = null;
        internal Demo wishBefore = null;

        public event PropertyChangedEventHandler PropertyChanged;

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
                        continue;
                    }

                    // Store all the other demos in other array and rewrite the list while taking the to-be-resorted demo's wish into account
                    Demo[] otherDemos = demos.ToArray();
                    demos.Clear();

                    bool resorted = false;
                    int sortIndex = 0;
                    foreach(Demo demo in otherDemos)
                    {
                        if(toResort.wishBefore == demo)
                        {
                            demos.Add(toResort);
                            toResort.wishBefore = null;
                            toResort.wishAfter = null;
                            toResort.sortIndex = sortIndex++;
                            demos.Add(demo);
                            demo.sortIndex = sortIndex++;
                            resorted = true;
                        } else if(toResort.wishAfter == demo)
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
                    if (!resorted)
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