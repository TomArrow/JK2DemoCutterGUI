using PropertyChanged;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;

namespace DemoCutterGUI
{


	public class DemoLinePoint : INotifyPropertyChanged
	{
		public DemoLinePoint next { get; set; }
		public DemoLinePoint prev { get; set; }
		public JommeTimePoints pointsCollection = null;
		public int time { get; set; }
		public int demoTime {get;set;}

		[DependsOn("time", "demoTime", "next","prev")]
		public double effectiveSpeed
        {
			get
            {
				if(prev == null)
                {
					return (double)demoTime/(double)time ;
                } else
                {
					return (double)(demoTime - prev.demoTime) / (double)Math.Max(1,time-prev.time); // Avoid division by zero
				}
            }
            set
            {
				double desiredSpeed = value;
				int prevTime = prev == null ? 0 : prev.time;
				int prevDemoTime = prev == null ? 0 : prev.demoTime;
				int timeDelta = time - prevTime;
				int demoTimeDelta = demoTime - prevDemoTime;

				int newTime = time;
				int newDemoTime = demoTime;
				bool demoTimeMode = pointsCollection != null && pointsCollection.cutterWindow != null && pointsCollection.cutterWindow.speedChangeDemoTimeMode;
				if (demoTimeMode)
                {
					newDemoTime = (int)((double)prevDemoTime + (double)timeDelta * desiredSpeed);
                } else
                {
					newTime = (int)((double)prevTime + (double)demoTimeDelta / desiredSpeed);
				}

				// Not yet implemented
				if (pointsCollection != null && pointsCollection.cutterWindow != null && pointsCollection.cutterWindow.speedPreservationMode)
                {
					pointsCollection.updatingOnPropertyChange = false;
					// Propagate change to later points to sorta preserve their speed
					// Otherwise changing our stuff here would also change the effective speed of later points.
					if (demoTimeMode)
                    {
						int demoTimeChange = newDemoTime - demoTime;
						// Just propagate this to all following points too, that should do the trick.
						DemoLinePoint point = this;
						while(point != null)
                        {
							point.demoTime += demoTimeChange;
							point = point.next;
                        }
					} else
                    {
						// K this will be a bit more complex.
						// But not too much more because all following ones will be applied in demotime mode because
						// we don't want their positions to shift around. 
						DemoLinePoint nextPoint = this.next;
						if (nextPoint != null)
                        {
							// We need to do a proper adjustment for the next point.
							double nextCurrentSpeed = (double)(nextPoint.demoTime - demoTime) / (double)Math.Max(1, nextPoint.time - time);
							time = newTime;
							int nextPointNewDemoTime = (int)((double)demoTime + (double)(nextPoint.time-time) * nextCurrentSpeed); // Preserve the speed it had before
							int nextPointDemoTimeChange = nextPointNewDemoTime - nextPoint.demoTime;
							while(nextPoint != null)
                            {
								nextPoint.demoTime += nextPointDemoTimeChange;
								nextPoint = nextPoint.next;
                            }
						} else
                        { // Doesn't matter then, nothing coming after this.
							time = newTime;
						}
                    }
					pointsCollection.updatingOnPropertyChange = true;
					pointsCollection.callThisOnChange();
				} else
                {
					// Simple mode. Just change time of this.
					time = newTime;
					demoTime = newDemoTime;
				}
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;



    }

	public class JommeTimePoints
    {

		public event EventHandler Updated;

		const int SPEED_SHIFT = 14;

		public bool updatingOnPropertyChange = true;


		public int lowestTime { get; private set; }
		public int highestTime { get; private set; }

		private List<DemoLinePoint> linePoints = new List<DemoLinePoint>();

		ListView boundView = null;
		public CombineCutter cutterWindow { get; private set; }

		public void addPoint(DemoLinePoint newPoint)
        {
            newPoint.PropertyChanged += NewPoint_PropertyChanged;
			newPoint.pointsCollection = this;
			linePoints.Add(newPoint);
			callThisOnChange();
        }

        private void NewPoint_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (updatingOnPropertyChange)
            {
				callThisOnChange();
			}
		}

		private void OnUpdated()
        {
			Updated?.Invoke(this, new EventArgs());
		}

        public void removePoint(DemoLinePoint pointToRemove)
        {
            if (linePoints.Contains(pointToRemove))
            {
				pointToRemove.PropertyChanged += NewPoint_PropertyChanged;
				linePoints.Remove(pointToRemove);
				callThisOnChange();
			} else
            {
				throw new Exception("Trying to remove point that's not in list.");
            }
        }

		public void bindListView(ListView view)
        {
			boundView = view;
			view.ItemsSource = linePoints;
        }
		public void bindCutterWindow(CombineCutter cutterWindowA)
        {
			cutterWindow = cutterWindowA;
        }

		// Manages order and references to next and previous etc.
		// Not efficient or elegant but this isn't the game, it's just a silly little GUI, we can handle a bit of inefficiency
		// and it makes it easier to work with this
		public void callThisOnChange()
        {
			linePoints.Sort((a,b)=> a.time - b.time);
			lowestTime = 0;
			highestTime = 0;
			if (linePoints.Count > 1)
            {
				for(int i = 0; i < linePoints.Count; i++)
				{
					if (i == 0)
					{
						linePoints[i].prev = null;
						linePoints[i].next = linePoints[i + 1];
						lowestTime = linePoints[i].time;
					}
					else if (i == linePoints.Count - 1)
					{

						linePoints[i].next = null;
						linePoints[i].prev = linePoints[i - 1];
						highestTime = linePoints[i].time;
					}
					else
					{

						linePoints[i].next = linePoints[i + 1];
						linePoints[i].prev = linePoints[i - 1];
					}
				}
			} else if (linePoints.Count == 1)
            {
				lowestTime = highestTime = linePoints[0].time;
				linePoints[0].next = null;
				linePoints[0].prev = null;
            }
            else
			if(boundView != null)
            {
				ICollectionView view = CollectionViewSource.GetDefaultView(boundView.ItemsSource);
				view.Refresh();
            }
			OnUpdated();
		}


		private float currentSpeed = 1.0f;
		public void setSpeed(float speed)
        {
			currentSpeed = speed;
        }






		DemoLinePoint linePointSynch(int playTime)
		{
			if (linePoints.Count == 0)
            {
				return null;
            }
            else
            {
				DemoLinePoint point = linePoints[0];
				for (; point.next != null && point.next.time <= playTime; point = point.next) ;
				return point;
			}
		}
		public void lineInterpolate(int playTime, float playTimeFraction, ref int demoTime, ref float demoTimeFraction, ref float demoSpeed)
		{
			Vector3 dx, dy;
			DemoLinePoint point = linePointSynch(playTime);
			if (point == null || point.next == null || point.time > playTime)
			{
				Int64 calcTimeLow, calcTimeHigh;
				Int64 speed = (Int64)((float)(1 << SPEED_SHIFT) * currentSpeed);// demo.line.speed;
				if (point != null)
					playTime -= point.time;
				calcTimeHigh = (playTime >> 16) * speed;
				calcTimeLow = (playTime & 0xffff) * speed;
				calcTimeLow += (Int64)(playTimeFraction * speed);
				/***/demoTime = (int)((calcTimeHigh << (16 - SPEED_SHIFT)) + (calcTimeLow >> SPEED_SHIFT));
				/***/demoTimeFraction = (float)(calcTimeLow & ((1 << SPEED_SHIFT) - 1)) / (1 << SPEED_SHIFT);
				if (point != null)
					/***/demoTime += point.demoTime;
				/***/demoTime += 0;//demo.line.offset; // Forgot what this even does, shrug
				/***/demoSpeed = currentSpeed;//demo.line.speed;
				return;
			}
			dx.Y = point.next.time - point.time;
			dy.Y = point.next.demoTime - point.demoTime;
			if (point.prev != null)
			{
				dx.X = point.time - point.prev.time;
				dy.X = point.demoTime - point.prev.demoTime;
			}
			else
			{
				dx.X = dx.Y;
				dy.X = dy.Y;
			}
			if (point.next.next != null)
			{
				dx.Z = point.next.next.time - point.next.time; ;
				dy.Z = point.next.next.demoTime - point.next.demoTime; ;
			}
			else
			{
				dx.Z = dx.Y;
				dy.Z = dy.Y;
			}
			/***/demoTimeFraction = JommeTimeCalculation.dsplineCalc((playTime - point.time) + playTimeFraction, dx, dy, ref demoSpeed);
			/***/demoTime = (int)/***/demoTimeFraction;
			/***/demoTimeFraction -= /***/demoTime;
			/***/demoTime += point.demoTime;
		}
		public void lineAt(int playTime, float playTimeFraction, ref int demoTime, ref float demoTimeFraction, ref float demoSpeed)
		{
			//if (!demo.line.locked)
			if (false)
			{
				Int64 calcTimeLow, calcTimeHigh;
				Int64 speed =  (Int64)((float)(1 << SPEED_SHIFT) * currentSpeed);//demo.line.speed;

				calcTimeHigh = (playTime >> 16) * speed;
				calcTimeLow = (playTime & 0xffff) * speed;
				calcTimeLow += (Int64)(playTimeFraction * speed);
				/***/demoTime = (int)((calcTimeHigh << (16 - SPEED_SHIFT)) + (calcTimeLow >> SPEED_SHIFT));
				/***/demoTimeFraction = (float)(calcTimeLow & ((1 << SPEED_SHIFT) - 1)) / (1 << SPEED_SHIFT);
				/***/demoTime += 0;// demo.line.offset;
				/***/demoSpeed = currentSpeed;//demo.line.speed;
			}
			else
			{
				lineInterpolate(playTime, playTimeFraction, ref demoTime,ref  demoTimeFraction, ref demoSpeed);
			}
			if (/***/demoTime < 0 || /***/demoTimeFraction < 0)
			{
				/***/demoTimeFraction = 0;
				/***/demoTime = 0;
			}
		}
	}

    class JommeTimeCalculation
    {


		static float dsMax(float x, float y)
		{
			if (y > x)
				return y;
			else
				return x;
		}
		static float dsMin(float x, float y)
		{
			if (y < x)
				return y;
			else
				return x;
		}
		static float dsplineTangent(float h1, float h2, float d1, float d2)
		{
			float hsum = h1 + h2;
			float del1 = d1 / h1;
			float del2 = d2 / h2;

			if (del1 * del2 == 0)
			{
				return 0;
			}
			else
			{
				float hsumt3 = 3 * hsum;
				float w1 = (hsum + h1) / hsumt3;
				float w2 = (hsum + h2) / hsumt3;
				float dmax = dsMax(Math.Abs(del1), Math.Abs(del2));
				float dmin = dsMin(Math.Abs(del1), Math.Abs(del2));
				float drat1 = del1 / dmax;
				float drat2 = del2 / dmax;
				return dmin / (w1 * drat1 + w2 * drat2);
			}
		}

		static public float dsplineCalc(float x, Vector3 dx, Vector3 dy, ref float deriv)
		{
			float tan1, tan2;
			float c2, c3;
			float delta, del1, del2;

			//tan1 = dsplineTangent(dx[0], dx[1], dy[0], dy[1]);
			tan1 = dsplineTangent(dx.X, dx.Y, dy.X, dy.Y);
			//tan2 = dsplineTangent(dx[1], dx[2], dy[1], dy[2]);
			tan2 = dsplineTangent(dx.Y, dx.Z, dy.Y, dy.Z);

			//delta = dy[1] / dx[1];
			delta = dy.Y / dx.Y;
            //del1 = (tan1 - delta) / dx[1];
            del1 = (tan1 - delta) / dx.Y;
            //del2 = (tan2 - delta) / dx[1];
            del2 = (tan2 - delta) / dx.Y;
			c2 = -(del1 + del1 + del2);
			//c3 = (del1 + del2) / dx[1];
			c3 = (del1 + del2) / dx.Y;
			//if (deriv != null)
			//{
				//*deriv = tan1 + 2 * x * c2 + 3 * x * x * c3;
				deriv = tan1 + 2 * x * c2 + 3 * x * x * c3;
			//}
			return x * (tan1 + x * (c2 + x * c3));
		}

	}
}
