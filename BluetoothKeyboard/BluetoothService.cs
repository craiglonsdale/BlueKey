using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Java.Util;
using Android.Bluetooth;
using System.Threading;
using Android.Util;
using System.IO;
using System.Runtime.CompilerServices;
using System.Reflection;


namespace BluetoothKeyboard
{
	public enum CONNECTION_STATE
	{
		// Constants that indicate the current connection state
		STATE_NONE = 0,       // we're doing nothing
		STATE_LISTEN = 1,     // now listening for incoming connections
		STATE_CONNECTING = 2, // now initiating an outgoing connection
		STATE_CONNECTED = 3  // now connected to a remote device
	}

	public class BluetoothService
	{
		//Debuggin stuff
		private const String TAG = "BlueToothService";
		private const bool DEBUG = true;

		//Nameo fhr the SDP record when creating a server socket
		private const String NAME = "BluetoothTestApp";

		//UUID for this application
		private static UUID MY_UUID = UUID.FromString ("fa87c0d0-afac-11de-8a39-0800200c9a66");

		protected static BluetoothAdapter m_adapter;
		protected static Handler m_handler;
		protected static CONNECTION_STATE m_state;
		private static AcceptThread m_acceptThread;
		protected ConnectThread m_connectThread;
		private ConnectedThread m_connectedThread;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="context"></param>
		/// <param name="handler"></param>
		public BluetoothService(Context context, Handler handler)
		{
			m_adapter = BluetoothAdapter.DefaultAdapter;
			m_state = CONNECTION_STATE.STATE_NONE;
			m_handler = handler;
		}

		/// <summary>
		/// Set the current state of the chat connection.
		/// </summary>
		/// <param name="state"A connection state></param>
		[MethodImpl(MethodImplOptions.Synchronized)]
		private void SetState(CONNECTION_STATE state)
		{
			if (DEBUG)
			{
				Log.Debug(TAG, "setState() " + m_state + " -> " + state);
			}

			m_state = state;

			//Give the state to the Handler so the UI activity can update
			m_handler.ObtainMessage(BluetoothKeyboardActivity.MESSAGE_STATE_CHANGE, (int)state, -1).SendToTarget();
		}

		/// <summary>
		/// Current connection state
		/// </summary>
		/// <returns>Returns the current connection state</returns>
		[MethodImpl(MethodImplOptions.Synchronized)]
		public CONNECTION_STATE GetState()
		{
			return m_state;
		}

		/// <summary>
		/// Starts the chat service
		/// </summary>
		[MethodImpl(MethodImplOptions.Synchronized)]
		public void Start()
		{
			if (DEBUG)
			{
				Log.Debug(TAG, "Start");
			}

			//Cancel any thread making a connection
			if (m_connectThread != null)
			{
				m_connectThread.Cancel();
				m_connectThread = null;
			}

			//Cancel anu current connection
			if (m_connectedThread != null)
			{
				m_connectedThread.Cancel();
				m_connectedThread = null;
			}

			//Start a thread to listen for connections
			if (m_acceptThread == null)
			{
				m_acceptThread = new AcceptThread(this);
				m_acceptThread.Start();
			}
		}

		/// <summary>
		/// Start the ConnectThread to initiate a connection to a remote device.
		/// </summary>
		/// <param name='device'>
		/// The BluetoothDevice to connect.
		/// </param>
		[MethodImpl(MethodImplOptions.Synchronized)]
		public void Connect(BluetoothDevice device)
		{
			if (DEBUG)
				Log.Debug(TAG, "connect to: " + device);

			// Cancel any thread attempting to make a connection
			if (m_state == CONNECTION_STATE.STATE_CONNECTING)
			{
				if (m_connectThread != null)
				{
					m_connectThread.Cancel();
					m_connectThread = null;
				}
			}

			// Cancel any thread currently running a connection
			if (m_connectedThread != null)
			{
				m_connectedThread.Cancel();
				m_connectedThread = null;
			}

			// Start the thread to connect with the given device
			m_connectThread = new ConnectThread(device, this);
			m_connectThread.Start();

			SetState(CONNECTION_STATE.STATE_CONNECTING);
		}

		/// <summary>
		/// Start the ConnectedThread to begin managing a Bluetooth connection
		/// </summary>
		/// <param name='socket'>
		/// The BluetoothSocket on which the connection was made.
		/// </param>
		/// <param name='device'>
		/// The BluetoothDevice that has been connected.
		/// </param>
		[MethodImpl(MethodImplOptions.Synchronized)]
		public void Connected(BluetoothSocket socket, BluetoothDevice device)
		{
			if (DEBUG)
				Log.Debug(TAG, "connected");

			// Cancel the thread that completed the connection
			if (m_connectThread != null)
			{
				m_connectThread.Cancel();
				m_connectThread = null;
			}

			// Cancel any thread currently running a connection
			if (m_connectedThread != null)
			{
				m_connectedThread.Cancel();
				m_connectedThread = null;
			}

			// Cancel the accept thread because we only want to connect to one device
			if (m_acceptThread != null)
			{
				m_acceptThread.Cancel();
				m_acceptThread = null;
			}

			// Start the thread to manage the connection and perform transmissions
			m_connectedThread = new ConnectedThread(socket, this);
			m_connectedThread.Start();

			// Send the name of the connected device back to the UI Activity
			var msg = m_handler.ObtainMessage(BluetoothKeyboardActivity.MESSAGE_DEVICE_NAME);
			Bundle bundle = new Bundle();
			bundle.PutString(BluetoothKeyboardActivity.DEVICE_NAME, device.Name);
			msg.Data = bundle;
			m_handler.SendMessage(msg);

			SetState(CONNECTION_STATE.STATE_CONNECTED);
		}

		/// <summary>
		/// Stop all threads.
		/// </summary>
		[MethodImpl(MethodImplOptions.Synchronized)]
		public void Stop()
		{
			if (DEBUG)
				Log.Debug(TAG, "stop");

			if (m_connectThread != null)
			{
				m_connectThread.Cancel();
				m_connectThread = null;
			}

			if (m_connectedThread != null)
			{
				m_connectedThread.Cancel();
				m_connectedThread = null;
			}

			if (m_acceptThread != null)
			{
				m_acceptThread.Cancel();
				m_acceptThread = null;
			}

			SetState(CONNECTION_STATE.STATE_NONE);
		}

		/// <summary>
		/// Write to the ConnectedThread in an unsynchronized manner
		/// </summary>
		/// <param name='out'>
		/// The bytes to write.
		/// </param>
		public void Write(byte[] @out)
		{
			// Create temporary object
			ConnectedThread r;
			// Synchronize a copy of the ConnectedThread
			lock (this)
			{
				if (m_state != CONNECTION_STATE.STATE_CONNECTED)
					return;
				r = m_connectedThread;
			}
			// Perform the write unsynchronized
			r.Write(@out);
		}

		/// <summary>
		/// Indicate that the connection attempt failed and notify the UI Activity.
		/// </summary>
		private void ConnectionFailed()
		{
			SetState(CONNECTION_STATE.STATE_LISTEN);

			// Send a failure message back to the Activity
			var msg = m_handler.ObtainMessage(BluetoothKeyboardActivity.MESSAGE_TOAST);
			Bundle bundle = new Bundle();
			bundle.PutString(BluetoothKeyboardActivity.TOAST, "Unable to connect device");
			msg.Data = bundle;
			m_handler.SendMessage(msg);
		}

		/// <summary>
		/// Indicate that the connection was lost and notify the UI Activity.
		/// </summary>
		public void ConnectionLost()
		{
			SetState(CONNECTION_STATE.STATE_LISTEN);

			// Send a failure message back to the Activity
			var msg = m_handler.ObtainMessage(BluetoothKeyboardActivity.MESSAGE_TOAST);
			Bundle bundle = new Bundle();
			bundle.PutString(BluetoothKeyboardActivity.TOAST, "Device connection was lost");
			msg.Data = bundle;
			m_handler.SendMessage(msg);
		}

		/// <summary>
		/// Thread that runs and listens for incoming connection requests.
		/// Behaves like a server-side client.
		/// Runs until connection is accepted or cancelled.
		/// </summary>
		private class AcceptThread
		{
			private BluetoothServerSocket m_serverSocket;
			private BluetoothService m_service;
			public Thread m_internalThread;

			public AcceptThread(BluetoothService bluetoothService)
			{
				m_internalThread = null;
				m_service = bluetoothService;
				BluetoothServerSocket tmp = null;

				//Create a listening server socket
				try
				{
					tmp = m_adapter.ListenUsingRfcommWithServiceRecord(NAME, MY_UUID);//ListenUsingInsecureRfcommWithServiceRecord(NAME, MY_UUID);
				}
				catch (Java.IO.IOException ex)
				{
					Log.Error(TAG, "listen() failed", ex.Message);
				}

				m_serverSocket = tmp;
			}

			public void Start()
			{
				m_internalThread = new Thread (() =>
				                              {
					if (DEBUG)
					{
						Log.Debug(TAG, "BEGIN AcceptThread " + this.ToString());
					}

					BluetoothSocket socket = null;

					//Listen to the server socket if we are not connected
					while (m_state != CONNECTION_STATE.STATE_CONNECTED)
					{
						try
						{
							//THis is a blocking call and will only return on a successful connection or an exception
							socket = m_serverSocket.Accept();
						}
						catch (Java.IO.IOException ex)
						{
							Log.Error(TAG, "accept() failed", ex.Message);
						}

						//If a connection was accepted
						if (socket != null)
						{
							lock (this)
							{
								switch (m_state)
								{
									case CONNECTION_STATE.STATE_LISTEN:
									case CONNECTION_STATE.STATE_CONNECTING:
									//Situation A-Ok. Start connection thread.
									m_service.Connected(socket, socket.RemoteDevice);
									break;
									case CONNECTION_STATE.STATE_NONE:
									case CONNECTION_STATE.STATE_CONNECTED:
									//Not ready or already connected. Terminate new socket
									try
									{
										socket.Close();
									}
									catch (Java.IO.IOException ex)
									{
										Log.Error(TAG, "Could not close unwanted socket", ex.Message);
									}
									break;
								}
							}
						}
					}

					if (DEBUG)
					{
						Log.Info(TAG, "END AcceptThread");
					}
				});

				m_internalThread.Start ();
			}

			public void Cancel()
			{
				if (DEBUG)
				{
					Log.Debug(TAG, "Cancel" + this.ToString());
				}

				try
				{
					m_serverSocket.Close();
				}
				catch (Java.IO.IOException ex)
				{
					Log.Error(TAG, "close() of server failed", ex.Message);
				}
			}
		}

		/// <summary>
		/// This thread runs while attempting to make an outgoing connection.
		/// It runs straight through; the connection either succeeds or fails.
		/// </summary>
		protected class ConnectThread
		{
			private BluetoothSocket m_socket;
			private BluetoothDevice m_device;
			private BluetoothService m_service;
			private Thread m_internalThread;

			public ConnectThread(BluetoothDevice device, BluetoothService service)
			{
				m_device = device;
				m_service = service;
				m_internalThread = null;
				BluetoothSocket tmp = null;

				// Get a BluetoothSocket for a connection with the
				// given BluetoothDevice
				try
				{
					var methodInfo = device.GetType().GetMethod("CreateRfcommSocketToServiceRecord");//, new Class[] {int.class});
					tmp = (BluetoothSocket) methodInfo.Invoke(device, new object[]{MY_UUID});
					//tmp = device.CreateRfcommSocketToServiceRecord(m_device);
				}
				catch (Java.IO.IOException ex)
				{
					Log.Error(TAG, "create() failed", ex.Message);
				}

				m_socket = tmp;
			}

			public void Start()
			{
				m_internalThread = new Thread (() =>
				                              {
					Log.Info(TAG, "BEGIN mConnectThread");
					//Name = "ConnectThread";

					// Always cancel discovery because it will slow down a connection
					m_adapter.CancelDiscovery();

					// Make a connection to the BluetoothSocket
					try
					{
						// This is a blocking call and will only return on a
						// successful connection or an exception
						m_socket.Connect();
					}
					catch (Java.IO.IOException ex)
					{
						m_service.ConnectionFailed();
						// Close the socket
						try
						{
							m_socket.Close();
						}
						catch (Java.IO.IOException ex2)
						{
							Log.Error(TAG, "unable to close() socket during connection failure", ex2.Message);
						}

						// Start the service over to restart listening mode
						m_service.Start();
						return;
					}

					// Reset the ConnectThread because we're done
					lock (this)
					{
						m_service.m_connectThread = null;
					}

					// Start the connected thread
					m_service.Connected(m_socket, m_device);
				});

				m_internalThread.Start();
			}

			public void Cancel()
			{
				try
				{
					m_socket.Close();
				}
				catch (Java.IO.IOException ex)
				{
					Log.Error(TAG, "close() of connect socket failed", ex.Message);
				}
			}
		}

		/// <summary>
		/// This thread runs during a connection with a remote device.
		/// It handles all incoming and outgoing transmissions.
		/// </summary>
		private class ConnectedThread
		{
			private Thread m_internalThread;
			private BluetoothSocket m_socket;
			private Stream m_inStream;
			private Stream m_outStream;
			private BluetoothService m_service;

			public ConnectedThread(BluetoothSocket socket, BluetoothService service)
			{
				Log.Debug(TAG, "create ConnectedThread: ");
				m_socket = socket;
				m_service = service;
				Stream tmpIn = null;
				Stream tmpOut = null;

				// Get the BluetoothSocket input and output streams
				try
				{
					tmpIn = socket.InputStream;
					tmpOut = socket.OutputStream;
				}
				catch (Java.IO.IOException ex)
				{
					Log.Error(TAG, "temp sockets not created", ex.Message);
				}

				m_inStream = tmpIn;
				m_outStream = tmpOut;
			}

			public void Start()
			{
				m_internalThread = new Thread (() =>
				                              {
					Log.Info(TAG, "BEGIN mConnectedThread");
					byte[] buffer = new byte[1024];
					int bytes;

					// Keep listening to the InputStream while connected
					while (true)
					{
						try
						{
							// Read from the InputStream
							bytes = m_inStream.Read(buffer, 0, buffer.Length);

							// Send the obtained bytes to the UI Activity
							m_handler.ObtainMessage(BluetoothKeyboardActivity.MESSAGE_READ, bytes, -1, buffer)
								.SendToTarget();
						}
						catch (Java.IO.IOException ex)
						{
							Log.Error(TAG, "disconnected", ex.Message);
							m_service.ConnectionLost();
							break;
						}
					}
				});

				m_internalThread.Start();
			}

			/// <summary>
			/// Write to the connected OutStream.
			/// </summary>
			/// <param name='buffer'>
			/// The bytes to write
			/// </param>
			public void Write(byte[] buffer)
			{
				try
				{
					m_outStream.Write(buffer, 0, buffer.Length);

					// Share the sent message back to the UI Activity
					m_handler.ObtainMessage(BluetoothKeyboardActivity.MESSAGE_WRITE, -1, -1, buffer)
						.SendToTarget();
				}
				catch (Java.IO.IOException ex)
				{
					Log.Error(TAG, "Exception during write", ex.Message);
				}
			}

			public void Cancel()
			{
				try
				{
					m_socket.Close();
				}
				catch (Java.IO.IOException ex)
				{
					Log.Error(TAG, "close() of connect socket failed", ex.Message);
				}
			}
		}
	}


}