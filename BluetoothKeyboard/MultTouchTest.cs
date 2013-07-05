using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Android.App;
using Android.Bluetooth;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Content.PM;
using Android.Util;

namespace BluetoothKeyboard
{


	[Activity (Label = "BLuetooth Keyboard Prototype", MainLauncher = true, ScreenOrientation=ScreenOrientation.Landscape)]
	public class BluetoothKeyboardActivity : MultiTouchActivity 
	{
		// Debugging
		private const string TAG = "BluetoothKeyboard";
		private const bool Debug = true;

		// Key names received from the BluetoothChatService Handler
		public const string DEVICE_NAME = "device_name";
		public const string TOAST = "toast";
		// Layout Views
		protected TextView title;

		private BluetoothAdapter m_bluetoothAdaptor = null;
		private BluetoothChatService m_bluetoothService = null;

		// Name of the connected device
		public string m_connectedDeviceName = null;

		// Array adapter for the conversation thread
		protected ArrayAdapter<string> m_conversationArrayAdapter;

		private Button btn1;
		private Button btn2;
		private Button btn3;
		private Button btn4;
		private Button btn5;
		private Button btn6;
		private Button btn7;
		private Button btn8;
		private Button btn9;
		private Button btn10;
		
		private Button btn11;
		private Button btn12;
		private Button btn13;
		private Button btn14;
		private Button btn15;
		private Button btn16;
		private Button btn17;
		private Button btn18;
		private Button btn19;

		private Button btn20;
		private Button btn21;
		private Button btn22;
		private Button btn23;
		private Button btn24;
		private Button btn25;
		private Button btn26;

		private Button spaceBar;

		public BluetoothKeyboardActivity() : base()
		{
		}

		public void ButtonCallback(object sender, EventArgs e)
		{
			if(m_bluetoothService == null)
			{
				((EditText)FindViewById(Resource.Id.editText1)).Text += ((Button)sender).Text;
			}
			else
			{
				var message = new Java.Lang.String (((Button)sender).Text);
				SendMessage (message);
			}
		}

		protected override void OnCreate(Bundle savedInstanceState) 
		{
			RequestWindowFeature (WindowFeatures.CustomTitle);
			SetContentView (Resource.Layout.Main);
			Window.SetFeatureInt (WindowFeatures.CustomTitle, Resource.Layout.custom_title);

			// Set up the custom title
			title = FindViewById<TextView> (Resource.Id.title_left_text);
			title.SetText (Resource.String.app_name);
			title = FindViewById<TextView> (Resource.Id.title_right_text);

			Action fourFinger = delegate(){((EditText)FindViewById(Resource.Id.editText1)).Text += "FOUR FINGER DRAG";};

			m_bluetoothAdaptor = BluetoothAdapter.DefaultAdapter;

			if (m_bluetoothAdaptor == null) {
				Toast.MakeText (this, "Bluetooth is not available", ToastLength.Long).Show ();
				Finish ();
				return;
			}

			base.OnCreate (savedInstanceState);
			this.SetContentView (Resource.Layout.Main);

			FindViewById(Resource.Id.button1).SetOnTouchListener(this);
			FindViewById(Resource.Id.button2).SetOnTouchListener(this);
			FindViewById(Resource.Id.button3).SetOnTouchListener(this);
			FindViewById(Resource.Id.button4).SetOnTouchListener(this);
			FindViewById(Resource.Id.button5).SetOnTouchListener(this);
			FindViewById(Resource.Id.button6).SetOnTouchListener(this);
			FindViewById(Resource.Id.button7).SetOnTouchListener(this);
			FindViewById(Resource.Id.button8).SetOnTouchListener(this);
			FindViewById(Resource.Id.button9).SetOnTouchListener(this);
			FindViewById(Resource.Id.button10).SetOnTouchListener(this);
			FindViewById(Resource.Id.button11).SetOnTouchListener(this);
			FindViewById(Resource.Id.button12).SetOnTouchListener(this);
			FindViewById(Resource.Id.button13).SetOnTouchListener(this);
			FindViewById(Resource.Id.button14).SetOnTouchListener(this);
			FindViewById(Resource.Id.button15).SetOnTouchListener(this);
			FindViewById(Resource.Id.button16).SetOnTouchListener(this);
			FindViewById(Resource.Id.button17).SetOnTouchListener(this);
			FindViewById(Resource.Id.button18).SetOnTouchListener(this);
			FindViewById(Resource.Id.button19).SetOnTouchListener(this);
			FindViewById(Resource.Id.button20).SetOnTouchListener(this);
			FindViewById(Resource.Id.button21).SetOnTouchListener(this);
			FindViewById(Resource.Id.button22).SetOnTouchListener(this);
			FindViewById(Resource.Id.button23).SetOnTouchListener(this);
			FindViewById(Resource.Id.button24).SetOnTouchListener(this);
			FindViewById(Resource.Id.button25).SetOnTouchListener(this);
			FindViewById(Resource.Id.button26).SetOnTouchListener(this);
			FindViewById(Resource.Id.spacebar).SetOnTouchListener(this);

			this.OnFourFingerDrag += fourFinger;

		}

		protected override void OnStart()
		{
			base.OnStart ();

			//If BT now on, request enabling
			if (!m_bluetoothAdaptor.IsEnabled) {
				var enableIntent = new Intent (BluetoothAdapter.ActionRequestEnable);
				StartActivityForResult (enableIntent, (int)IntentRequestCodes.REQUEST_ENABLE_BT);
			}
			else{
				m_bluetoothService = new BluetoothChatService (this, new MyHandler (this));
			}

		}

		protected override void OnResume()
		{
			base.OnResume ();

			//Oerform this check in onResume() cover the case in which BT was not enabled during OnStart(), so we
			//were paused to enable it. OnResume() will be called when ACTION_REQUESR_ENABLE activitity returns
			if (m_bluetoothService != null) {
				//Only if the state is STATE_NOW
				if (m_bluetoothService.GetState () == (int)ConnectionState.STATE_NONE) {
					m_bluetoothService.Start ();
				}
			}
		}

		public override bool OnCreateOptionsMenu(IMenu item)
		{
			var inflater = MenuInflater;
			inflater.Inflate(Resource.Menu.option_menu, item);
			return true;
		}

		public override bool OnOptionsItemSelected(IMenuItem item)
		{
			switch (item.ItemId) {
				case Resource.Id.scan:
					var serverIntent = new Intent(this, typeof(DeviceListActivity));
					StartActivityForResult(serverIntent, (int)IntentRequestCodes.REQUEST_CONNECT_DEVICE);
					return true;
				case Resource.Id.discoverable:
					// Ensure this device is discoverable by others
					EnsureDiscoverable();
					return true;
			}

			return false;
		}
	
		private void EnsureDiscoverable ()
		{
			Log.Debug (TAG, "ensure discoverable");

			if (m_bluetoothAdaptor.ScanMode != ScanMode.ConnectableDiscoverable) 
			{
				Intent discoverableIntent = new Intent (BluetoothAdapter.ActionRequestDiscoverable);
				discoverableIntent.PutExtra (BluetoothAdapter.ExtraDiscoverableDuration, 300);
				StartActivity (discoverableIntent);
			}
		}

		public void AddDebugToEditBox(String text)
		{
			((EditText)FindViewById (Resource.Id.editText1)).Text += text;
		}

		/// <summary>
		/// Sends a message.
		/// </summary>
		/// <param name='message'>
		/// A string of text to send.
		/// </param>
		private void SendMessage (Java.Lang.String message)
		{
			// Check that we're actually connected before trying anything
			if (m_bluetoothService.GetState () != ConnectionState.STATE_CONNECTED) 
			{
				Toast.MakeText (this, Resource.String.not_connected, ToastLength.Short).Show ();
				return;
			}

			// Check that there's actually something to send
			if (message.Length () > 0) {
				// Get the message bytes and tell the BluetoothChatService to write
				byte[] send = message.GetBytes ();
			    m_bluetoothService.Write (send);

				// Reset out string buffer to zero and clear the edit text field
			}
		}

		protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
		{
			Log.Debug (TAG, "onActivityResult " + resultCode);

			switch(requestCode)
			{
				case (int)IntentRequestCodes.REQUEST_CONNECT_DEVICE:
				// When DeviceListActivity returns with a device to connect
				if( resultCode == Result.Ok)
				{
					// Get the device MAC address
					var address = data.Extras.GetString(DeviceListActivity.EXTRA_DEVICE_ADDRESS);
					// Get the BLuetoothDevice object
					BluetoothDevice device = m_bluetoothAdaptor.GetRemoteDevice (address);
					// Attempt to connect to the device
					m_bluetoothService.Connect(device);
				}
				break;
				case (int)IntentRequestCodes.REQUEST_ENABLE_BT:
				// When the request to enable Bluetooth returns
				if(resultCode == Result.Ok)
				{
					// Bluetooth is now enabled, so set up a chat session
					m_bluetoothService = new BluetoothChatService (this, new MyHandler (this));
				}
				else
				{
					// User did not enable Bluetooth or an error occured
					Log.Debug(TAG, "BT not enabled");
					Toast.MakeText(this, Resource.String.bt_not_enabled_leaving, ToastLength.Short).Show();
					Finish();
				}
				break;
			}
		}

		// The Handler that gets information back from the BluetoothChatService
		private class MyHandler : Handler
		{
			BluetoothKeyboardActivity m_bluetoothKeyboard;

			public MyHandler (BluetoothKeyboardActivity keyboard)
			{
				m_bluetoothKeyboard = keyboard;	
			}

			public override void HandleMessage (Message msg)
			{
				switch (msg.What) 
				{
					case (int)MessageType.MESSAGE_STATE_CHANGE:
						if (Debug)
							Log.Info (TAG, "MESSAGE_STATE_CHANGE: " + msg.Arg1);

						switch (msg.Arg1) 
						{
							case (int)ConnectionState.STATE_CONNECTED:
								m_bluetoothKeyboard.AddDebugToEditBox("Bluetooth Connected\n");
								//m_bluetoothKeyboard.title.SetText (Resource.String.title_connected_to);
								//m_bluetoothKeyboard.title.Append (bluetoothChat.connectedDeviceName);
								break;
							case (int)ConnectionState.STATE_CONNECTING:
								//m_bluetoothKeyboard.title.SetText (Resource.String.title_connecting);
								break;
							case (int)ConnectionState.STATE_LISTEN:
							case (int)ConnectionState.STATE_NONE:
								//m_bluetoothKeyboard.title.SetText (Resource.String.title_not_connected);
								break;
						}
						break;
					case (int)MessageType.MESSAGE_WRITE:
						byte[] writeBuf = (byte[])msg.Obj;
						// construct a string from the buffer
						var writeMessage = new Java.Lang.String (writeBuf);
						break;
					case (int)MessageType.MESSAGE_READ:
						byte[] readBuf = (byte[])msg.Obj;
						// construct a string from the valid bytes in the buffer
						var readMessage = new Java.Lang.String (readBuf, 0, msg.Arg1);
						m_bluetoothKeyboard.AddDebugToEditBox (readMessage.ToString());
						break;
					case (int)MessageType.MESSAGE_DEVICE_NAME:
						// save the connected device's name
						m_bluetoothKeyboard.m_connectedDeviceName = msg.Data.GetString (DEVICE_NAME);
						Toast.MakeText (m_bluetoothKeyboard.BaseContext, "Connected to " + m_bluetoothKeyboard.m_connectedDeviceName, ToastLength.Short).Show ();
						break;
					case (int)MessageType.MESSAGE_TOAST:
						Toast.MakeText (m_bluetoothKeyboard.BaseContext, msg.Data.GetString (TOAST), ToastLength.Short).Show ();
						break;
				}
			}
		}
	}
}

