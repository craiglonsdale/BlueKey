using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Content.PM;
using Android.Util;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BluetoothKeyboard
{
	public class MultiTouchActivity : Activity, View.IOnTouchListener
	{
		private View m_parentView;
		private readonly List<List<View>> m_recentTouchedViewsIndex = new List<List<View>> (){new List<View>(), new List<View>(), new List<View>(), new List<View>(), new List<View>()};
		private readonly List<List<View>> m_downTouchedViewsIndex = new List<List<View>> (){new List<View>(), new List<View>(), new List<View>(), new List<View>(), new List<View>()};
		private readonly List<View> m_moveOutsideEnabledViews = new List<View> ();
		private int m_touchSlop = 24;
		private VelocityTracker m_velocityTracker = null;

		public void AddMoveOutsideEnabledViews(View view)
		{
			m_moveOutsideEnabledViews.Add (view);
		}

		public Action OnFourFingerDrag;

		public MultiTouchActivity()
		{
		}
		protected override void OnCreate(Bundle instance) 
		{
			base.OnCreate(instance);

		//	RequestWindowFeature(WindowFeatures.NoTitle);
		//	Window.AddFlags(WindowManagerFlags.Fullscreen);
		//	Window.ClearFlags(WindowManagerFlags.ForceNotFullscreen);
			m_parentView = FindViewById (global::Android.Resource.Id.Content).RootView;
			m_parentView.SetOnTouchListener(this);
			m_touchSlop = ViewConfiguration.Get(ApplicationContext).ScaledTouchSlop;
		}

		public bool OnTouch(View view, MotionEvent motionEvent)
		{
			// index of the pointer which starts this Event
			int actionPointerIndex = motionEvent.ActionIndex;

			switch(motionEvent.ActionMasked)
			{
				case MotionEventActions.Down:
					if (m_velocityTracker == null) {
						m_velocityTracker = VelocityTracker.Obtain();
					} else {
						m_velocityTracker.Clear ();
					}

					m_velocityTracker.AddMovement(motionEvent);
					break;
	
				case MotionEventActions.Move:
					m_velocityTracker.AddMovement(motionEvent);
					m_velocityTracker.ComputeCurrentVelocity(1000);
					Log.Verbose("Velocity: ", m_velocityTracker.GetXVelocity(actionPointerIndex) + " " + m_velocityTracker.GetYVelocity(actionPointerIndex));
					break;
				case MotionEventActions.Up:
				case MotionEventActions.Cancel:
					m_velocityTracker.Recycle ();
					break;
			}


			// resolve the action as a basic type (up, down or move)
			int actionResolved = (int)motionEvent.ActionMasked;

 			if (actionResolved < 7 && actionResolved > 4) 
			{
				actionResolved = actionResolved - 5;
			}

			if (actionResolved == (int)MotionEventActions.Move && motionEvent.PointerCount == 4 && Math.Abs(m_velocityTracker.GetXVelocity(actionPointerIndex)) > 3000)
			{
				//for (int ptrIndex = 0; ptrIndex < motionEvent.PointerCount; ptrIndex++) 
				//{
					// only one event for all move events.
				HandleEvent(motionEvent.PointerCount, motionEvent, view, actionResolved);
				m_velocityTracker.Clear ();
				//}

			} 
			else 
			{
				HandleEvent(actionPointerIndex, motionEvent, view, actionResolved);
			}

			return true;
		}

		private List<View> GetTouchedViews(int x, int y) 
		{
			var touchedViews = new List<View>();
			var possibleViews = new View[50];
			var count = 0;

			if (m_parentView is ViewGroup) 
			{
				possibleViews[count] = m_parentView;

				foreach(var view in possibleViews)
				{
					if(view != null)
					{
						int[] location = new int[]{0,0};
						view.GetLocationOnScreen(location);

						if (((view.Height + location[1] >= y) & (view.Width + location[0] >= x) & (view.Left <= x) & (view.Top <= y))
						    || view is FrameLayout) 
						{
							touchedViews.Add(view);
							var childViews = GetChildViews (view);
							for(int i = 0; i < childViews.Count; ++i)
							{
								possibleViews[++count] = childViews[i];
							}
						}
					}
				}
			}

			return touchedViews;
		}

		private List<View> GetChildViews(View view) 
		{
			var views = new List<View>();

			if (view is ViewGroup) 
			{
				var v = (ViewGroup)view;

				if (v.ChildCount > 0) 
				{
					for (int i = 0; i < v.ChildCount; i++) 
					{
						views.Add(v.GetChildAt(i));
					}
				}
			}
			return views;
		}

		private void DumpEvent(MotionEvent motionEvent) 
		{
			var names = new String[]{ "DOWN", "UP", "MOVE", "CANCEL", "OUTSIDE", "POINTER_DOWN", "POINTER_UP", "7", "8", "9" };
			var sb = new StringBuilder();
			int action = (int)motionEvent.Action;
			int actionCode = (int)((MotionEventActions)action & MotionEventActions.Mask);

			sb.Append("event ACTION_").Append(names[actionCode]);
			if (actionCode == (int)MotionEventActions.PointerDown || actionCode == (int)MotionEventActions.PointerUp) 
			{
				sb.Append("(pid ").Append(action >> (int)MotionEventActions.PointerIdShift);
	          	sb.Append(")");
			}

			sb.Append("[");
            for (int i = 0; i < motionEvent.PointerCount; i++) 
			{
				sb.Append("#").Append(i);
				sb.Append("(pid ").Append(motionEvent.GetPointerId(i));
	            sb.Append(")=").Append((int) motionEvent.GetX(i));
				sb.Append(",").Append((int) motionEvent.GetY(i));
				if (i + 1 < motionEvent.PointerCount) 
				{
					sb.Append(";");
				}
             }
             sb.Append("]");
             Log.Debug("tag", sb.ToString());
        }

		private bool PointInView(float localX, float localY, float slop, float width, float height) 
		{
			return localX >= -slop && localY >= -slop && localX < ((width) + slop) && localY < ((height) + slop);
		}

		private void HandleEvent(int actionPointerIndex, MotionEvent motionEvent, View eventView, int actionResolved)
		{
			int rawX, rawY;
			int[] location = new int[]{0, 0};

			if (actionPointerIndex == 4) 
			{
				OnFourFingerDrag ();
				return;
			} 

			eventView.GetLocationOnScreen (location);
			rawX = (int)motionEvent.GetX (actionPointerIndex) + location[0];
			rawY = (int)motionEvent.GetY (actionPointerIndex) + location[1];

			int actionPointerID = motionEvent.GetPointerId (actionPointerIndex);
			var hoverViews = GetTouchedViews (rawX, rawY);

			if (actionResolved == (int)MotionEventActions.Down) 
			{
				m_downTouchedViewsIndex[actionPointerID] = (List<View>)hoverViews;
			}

			//Delete all view whichc were not clicked on ActionDown
			if(m_downTouchedViewsIndex[actionPointerID].Any())
			{
				var tempViews = hoverViews;
				tempViews = tempViews.Except(m_downTouchedViewsIndex[actionPointerID]).ToList ();
				hoverViews = hoverViews.Except(tempViews).ToList ();
			}                                                                                                                                                                                   

			if (m_recentTouchedViewsIndex[actionPointerID] != null && m_recentTouchedViewsIndex[actionPointerID].Any()) 
			{
				var recentTouchedViews = m_recentTouchedViewsIndex[actionPointerID];

				var shouldTouchViews = hoverViews;

				//When you take shouldTouchViews, take away recentTouchedViews, are there any elements left?
				if (shouldTouchViews.Except(recentTouchedViews).Any()) 
				{
					//Remove the ones that are there
					shouldTouchViews = shouldTouchViews.Except(recentTouchedViews).ToList();

					//And re-add so we know we have them all
					shouldTouchViews.AddRange(recentTouchedViews);

					var outsideTouchedViews = shouldTouchViews;

					outsideTouchedViews = outsideTouchedViews.Except(hoverViews).ToList();
				}

				m_recentTouchedViewsIndex[actionPointerID] = hoverViews;
				hoverViews = shouldTouchViews;
			} 
			else 
			{
				m_recentTouchedViewsIndex[actionPointerID] = hoverViews;
			}

			if(actionResolved == (int)MotionEventActions.Up)
			{
				m_recentTouchedViewsIndex [actionPointerID] = null;
				m_downTouchedViewsIndex [actionPointerID] = null;
			}

			DumpEvent (motionEvent);

			foreach (var hView in hoverViews) 
			{
				int x, y;
				hView.GetLocationOnScreen(location);
				x = rawX - location[0];
				y = rawY - location[1];

				// View does not recognize that the Pointer is
				// outside if the Pointer is not far away (&gt;mTouchSlop)
				if (m_recentTouchedViewsIndex[actionPointerID] != null && m_recentTouchedViewsIndex[actionPointerID].Any()) 
				{
					if (PointInView(x, y, m_touchSlop, hView.Width, hView.Height)) 
					{
						// Log.v(&quot;tag&quot;, &quot;added because &lt; mTouchSlop&quot;);

						if (!m_recentTouchedViewsIndex[actionPointerID].Contains(hView)) 
						{
							m_recentTouchedViewsIndex[actionPointerID].Add(hView);
						}

					} 
					else if (m_moveOutsideEnabledViews.Contains(hView)) 
					{
						Log.Verbose("tag", "outside but gets event");
						m_recentTouchedViewsIndex[actionPointerID].Add(hView);
					}
				}
				var me = MotionEvent.Obtain(motionEvent.DownTime,
		                                          motionEvent.EventTime, (MotionEventActions)actionResolved, x, y,
		                                          motionEvent.GetPressure(actionPointerIndex),
		                                          motionEvent.GetPressure(actionPointerIndex),
		                                          motionEvent.MetaState, motionEvent.XPrecision,
		                                          motionEvent.YPrecision, motionEvent.DeviceId,
		                                          motionEvent.EdgeFlags);
				me.SetLocation (x, y);

				if (!me.Equals(motionEvent)) 
				{
					// deals the Event
					hView.OnTouchEvent(me);
				}

				// debug
				if (actionResolved == (int)MotionEventActions.Move) 
				{
					Log.Verbose("tag",
					      "#" + actionPointerIndex + " Rawx:" + rawX + " rawy:"
					      + rawY + " x:" + x + " y:" + y + " "
					      + hView.ToString());
				}
			}
		}
	}
}


