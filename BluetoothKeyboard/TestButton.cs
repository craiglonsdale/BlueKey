using System;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Widget;
using Android.Util;
using Android.App;
using Android.Views;
using Android.Content.PM;

namespace BluetoothKeyboard
{
	public class TestButton : Button 
	{
		public TestButton(Context context, IAttributeSet attrs) : base(context, attrs)
		{
			// TODO Auto-generated constructor stub
		}
		
		public bool onTouchEvent(MotionEvent motionEvent) 
		{
			Log.Verbose("tag", "I get touched");
			Text = "I recive a MotionEvent";
			if (motionEvent.Action == MotionEventActions.Up) 
			{
				Text = "I can recive Move events outside of my View";
			}
			return base.OnTouchEvent(motionEvent);
		}
	}
}

