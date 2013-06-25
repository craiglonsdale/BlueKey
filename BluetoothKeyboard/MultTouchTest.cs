using System;
using System.Collections.Generic;
using System.Linq;
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
	[Activity (Label = "crapcrapcrap!!!", MainLauncher = true, ScreenOrientation=ScreenOrientation.Landscape)]
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
		// String buffer for outgoing messages
		private StringBuilder m_outStringBuffer;

		// Message types sent from the BluetoothChatService Handler
		// TODO: Make into Enums
		public const int MESSAGE_STATE_CHANGE = 1;
		public const int MESSAGE_READ = 2;
		public const int MESSAGE_WRITE = 3;
		public const int MESSAGE_DEVICE_NAME = 4;
		public const int MESSAGE_TOAST = 5;

		// Intent request codes
		// TODO: Make into Enums
		private const int REQUEST_CONNECT_DEVICE = 1;
		private const int REQUEST_ENABLE_BT = 2;

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

		protected override void OnCreate(Bundle savedInstanceState) 
		{
			RequestWindowFeature (WindowFeatures.CustomTitle);
			SetContentView (Resource.Layout.Main);
			Window.SetFeatureInt (WindowFeatures.CustomTitle, Resource.Layout.custom_title);

			// Set up the custom title
			title = FindViewById<TextView> (Resource.Id.title_left_text);
			title.SetText (Resource.String.app_name);
			title = FindViewById<TextView> (Resource.Id.title_right_text);

			EventHandler letterAction = delegate(object sender, EventArgs e)
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
			};

			Action fourFinger = delegate(){((EditText)FindViewById(Resource.Id.editText1)).Text += "FOUR FINGER DRAG";};

			m_bluetoothAdaptor = BluetoothAdapter.DefaultAdapter;

			if (m_bluetoothAdaptor == null) {
				Toast.MakeText (this, "Bluetooth is not available", ToastLength.Long).Show ();
				Finish ();
				return;
			}

			base.OnCreate (savedInstanceState);
			this.SetContentView (Resource.Layout.Main);

			btn1 = (Button) FindViewById(Resource.Id.button1);
			btn1.Text = "Q";
			btn1.SetOnTouchListener (this);
			btn1.Click += letterAction;
			btn2 = (Button) FindViewById(Resource.Id.button2);
			btn2.SetOnTouchListener (this);
			btn2.Text = "W";
			btn2.Click += letterAction;
			btn3 = (Button) FindViewById(Resource.Id.button3);
			btn3.SetOnTouchListener(this);
			btn3.Text = "E";
			btn3.Click += letterAction;
			btn4 = (Button) FindViewById(Resource.Id.button4);
			btn4.SetOnTouchListener(this);
			btn4.Text = "R";
			btn4.Click += letterAction;
			btn5 = (Button) FindViewById(Resource.Id.button5);
			btn5.SetOnTouchListener(this);
			btn5.Text = "T";
			btn5.Click += letterAction;
			btn6 = (Button) FindViewById(Resource.Id.button6);
			btn6.SetOnTouchListener(this);
			btn6.Text = "Y";
			btn6.Click += letterAction;
			btn7 = (Button) FindViewById(Resource.Id.button7);
			btn7.SetOnTouchListener(this);
			btn7.Text = "U";
			btn7.Click += letterAction;
			btn8 = (Button) FindViewById(Resource.Id.button8);
			btn8.SetOnTouchListener(this);
			btn8.Text = "I";
			btn8.Click += letterAction;
			btn9 = (Button) FindViewById(Resource.Id.button9);
			btn9.SetOnTouchListener(this);
			btn9.Text = "O";
			btn9.Click += letterAction;
			btn10 = (Button) FindViewById(Resource.Id.button10);
			btn10.SetOnTouchListener(this);
			btn10.Text = "P";
			btn10.Click += letterAction;

			btn11 = (Button) FindViewById(Resource.Id.button11);
			btn11.SetOnTouchListener(this);
			btn11.Text = "A";
			btn11.Click += letterAction;
			btn12 = (Button) FindViewById(Resource.Id.button12);
			btn12.SetOnTouchListener(this);
			btn12.Text = "S";
			btn12.Click += letterAction;
			btn13 = (Button) FindViewById(Resource.Id.button13);
			btn13.SetOnTouchListener(this);
			btn13.Text = "D";
			btn13.Click += letterAction;
			btn14 = (Button) FindViewById(Resource.Id.button14);
			btn14.SetOnTouchListener(this);
			btn14.Text = "F";
			btn14.Click += letterAction;
			btn15 = (Button) FindViewById(Resource.Id.button15);
			btn15.SetOnTouchListener(this);
			btn15.Text = "G";
			btn15.Click += letterAction;
			btn16 = (Button) FindViewById(Resource.Id.button16);
			btn16.SetOnTouchListener(this);
			btn16.Text = "H";
			btn16.Click += letterAction;
			btn17 = (Button) FindViewById(Resource.Id.button17);
			btn17.SetOnTouchListener(this);
			btn17.Text = "J";
			btn17.Click += letterAction;
			btn18 = (Button) FindViewById(Resource.Id.button18);
			btn18.SetOnTouchListener(this);
			btn18.Text = "K";
			btn18.Click += letterAction;
			btn19 = (Button) FindViewById(Resource.Id.button19);
			btn19.SetOnTouchListener(this);
			btn19.Text = "L";
			btn19.Click += letterAction;

			btn20 = (Button) FindViewById(Resource.Id.button20);
			btn20.SetOnTouchListener(this);
			btn20.Text = "Z";
			btn20.Click += letterAction;
			btn21 = (Button) FindViewById(Resource.Id.button21);
			btn21.SetOnTouchListener(this);
			btn21.Text = "X";
			btn21.Click += letterAction;
			btn22 = (Button) FindViewById(Resource.Id.button22);
			btn22.SetOnTouchListener(this);
			btn22.Text = "C";
			btn22.Click += letterAction;
			btn23 = (Button) FindViewById(Resource.Id.button23);
			btn23.SetOnTouchListener(this);
			btn23.Text = "V";
			btn23.Click += letterAction;
			btn24 = (Button) FindViewById(Resource.Id.button24);
			btn24.SetOnTouchListener(this);
			btn24.Text = "B";
			btn24.Click += letterAction;
			btn25 = (Button) FindViewById(Resource.Id.button25);
			btn25.SetOnTouchListener(this);
			btn25.Text = "N";
			btn25.Click += letterAction;
			btn26 = (Button) FindViewById(Resource.Id.button26);
			btn26.SetOnTouchListener(this);
			btn26.Text = "M";
			btn26.Click += letterAction;

			spaceBar = (Button) FindViewById(Resource.Id.spacebar);
			spaceBar.SetOnTouchListener(this);
			spaceBar.Text = " ";
			spaceBar.Click += letterAction;

			this.OnFourFingerDrag += fourFinger;

		}

		protected override void OnStart()
		{
			base.OnStart ();

			//If BT now on, request enabling
			if (!m_bluetoothAdaptor.IsEnabled) {
				var enableIntent = new Intent (BluetoothAdapter.ActionRequestEnable);
				StartActivityForResult (enableIntent, REQUEST_ENABLE_BT);
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
				if (m_bluetoothService.GetState () == BluetoothChatService.STATE_NONE) {
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
					StartActivityForResult(serverIntent, REQUEST_CONNECT_DEVICE);
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
			if (m_bluetoothService.GetState () != BluetoothChatService.STATE_CONNECTED) 
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
				case REQUEST_CONNECT_DEVICE:
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
				case REQUEST_ENABLE_BT:
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
					case MESSAGE_STATE_CHANGE:
						if (Debug)
							Log.Info (TAG, "MESSAGE_STATE_CHANGE: " + msg.Arg1);

						switch (msg.Arg1) 
						{
							case (int)BluetoothChatService.STATE_CONNECTED:
								m_bluetoothKeyboard.AddDebugToEditBox("Bluetooth Connected\n");
								//m_bluetoothKeyboard.title.SetText (Resource.String.title_connected_to);
								//m_bluetoothKeyboard.title.Append (bluetoothChat.connectedDeviceName);
								break;
								case (int)BluetoothChatService.STATE_CONNECTING:
								//m_bluetoothKeyboard.title.SetText (Resource.String.title_connecting);
								break;
								case (int)BluetoothChatService.STATE_LISTEN:
								case (int)BluetoothChatService.STATE_NONE:
								//m_bluetoothKeyboard.title.SetText (Resource.String.title_not_connected);
								break;
						}
						break;
					case MESSAGE_WRITE:
						byte[] writeBuf = (byte[])msg.Obj;
						// construct a string from the buffer
						var writeMessage = new Java.Lang.String (writeBuf);
						//bluetoothChat.conversationArrayAdapter.Add ("Me: " + writeMessage);
						break;
				case MESSAGE_READ:
					byte[] readBuf = (byte[])msg.Obj;
							// construct a string from the valid bytes in the buffer
					var readMessage = new Java.Lang.String (readBuf, 0, msg.Arg1);
						m_bluetoothKeyboard.AddDebugToEditBox (readMessage.ToString());
						//bluetoothChat.conversationArrayAdapter.Add (bluetoothChat.connectedDeviceName + ":  " + readMessage);
					break;
					case MESSAGE_DEVICE_NAME:
						// save the connected device's name
						m_bluetoothKeyboard.m_connectedDeviceName = msg.Data.GetString (DEVICE_NAME);
						Toast.MakeText (m_bluetoothKeyboard.BaseContext, "Connected to " + m_bluetoothKeyboard.m_connectedDeviceName, ToastLength.Short).Show ();
					break;
					case MESSAGE_TOAST:
						Toast.MakeText (m_bluetoothKeyboard.BaseContext, msg.Data.GetString (TOAST), ToastLength.Short).Show ();
						break;
				}
			}
		}
	}
}

