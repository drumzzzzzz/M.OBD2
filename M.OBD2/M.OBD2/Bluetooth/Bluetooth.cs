#region Using Statements
using Android.Bluetooth;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#endregion

namespace M.OBD2
{
    public class Bluetooth : List<BluetoothConnection>
    {
        #region Declarations

        private static string status_message;
        private const char END_CHAR = '>';
        private const string LINE_BREAK = " \r";
        private static readonly string[] REPLACE_VALUES = { "SEARCHING", "\\s", "\r", ">" };
        private const string CONNECTION_VERIFY = "ELM327";
        private const int MAX_LENGTH = 1000;
        private static bool isDebug;
        private BluetoothConnection oBluetoothConnection;

        #endregion

        #region Main Control

        public Bluetooth(bool _isDebug)
        {
            status_message = string.Empty;
            isDebug = _isDebug;
        }

        public async Task RunProcesses(BlueToothCmds oBlueToothCmds)
        {
            DateTime dtCurrent = DateTime.UtcNow;

            foreach (BluetoothCmd bcmd in oBlueToothCmds)
            {
                bcmd.dtNext = dtCurrent.AddMilliseconds(bcmd.Rate);
            }

            await Task.Run(() =>
            {
                while (true)
                {
                    foreach (BluetoothCmd bcmd in oBlueToothCmds)
                    {
                        dtCurrent = DateTime.UtcNow;

                        if (dtCurrent >= bcmd.dtNext)
                        {
                            // ToDo: main processing routines!

                            bcmd.dtNext = dtCurrent.AddMilliseconds(bcmd.Rate);
                            Debug.WriteLine("Process:" + bcmd.Name);
                        }
                    }
                }
            }).ConfigureAwait(false);
        }

        #endregion

        #region Connection Initialization Routines

        public bool LoadPairedDevices()
        {
            try
            {
                if (!BluetoothAdapter.DefaultAdapter.IsEnabled)
                    throw new Exception("Bluetooth is not enabled");

                AddRange(BluetoothAdapter.DefaultAdapter.BondedDevices.Select(device => new BluetoothConnection(device.Name, device.Address)).ToList());

                SetStatusMessage(string.Format("{0} paired devices found", Count));

                return true;
            }
            catch (Exception e)
            {
                SetErrorMessage("Error Getting Paired Devices", e);
                return false;
            }
        }

        public bool CheckPairedDevices()
        {
            return Count > 0;
        }

        public List<BluetoothConnection> GetPairedDevices()
        {
            return this;
        }

        public async Task<bool> OpenPairedDevice(string name, string address)
        {
            oBluetoothConnection = await GetPairedDevice(name, address);

            return oBluetoothConnection != null;
        }

        private async Task<BluetoothConnection> GetPairedDevice(string name, string address)
        {
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(address))
            {
                SetStatusMessage("No device parameters");
                return null;
            }

            foreach (BluetoothConnection bc in this) // Iterate paired devices
            {
                if (bc.device_name.Equals(name, StringComparison.OrdinalIgnoreCase)) // ToDo: address dynamically following an OS pair operation - remove?
                    //&& bc.device_address.Equals(address, StringComparison.OrdinalIgnoreCase)) 
                {
                    if (await OpenDevice(bc) && await SendCommand(bc, "AT Z"))
                    {
                        SetStatusMessage("Device Connected!");
                        return bc;
                    }

                    SetStatusMessage("Failed to open device");
                    return null;
                }
            }

            SetStatusMessage("Could not find device");
            return null;
        }

        private static async Task<bool>  OpenDevice(BluetoothConnection bc)
        {
            try
            {
                if (isDebug)
                    Debug.WriteLine("Opening Device Name: {0} Address: {1}", bc.device_name, bc.device_address);

                BluetoothDevice oBthDevice = BluetoothAdapter.DefaultAdapter.GetRemoteDevice(bc.device_address);
        
                if (oBthDevice == null)
                    throw new Exception("Unable to connect to device");

                Android.OS.ParcelUuid[] puids = oBthDevice.GetUuids();

                if (puids == null || puids.Length == 0)
                    throw new Exception("Invalid device UUID's");

                BluetoothSocket oBthSocket = oBthDevice.CreateRfcommSocketToServiceRecord(puids[0].Uuid);

                if (oBthSocket == null)
                    throw new Exception("Unable to create connection socket");

                await oBthSocket.ConnectAsync();

                if(!oBthSocket.IsConnected)
                    throw new Exception("Unable to connect to socket");

                bc.oBthDevice = oBthDevice;
                bc.oBthSocket = oBthSocket;

                //status_message = string.Format("Device {0} Connected!", bc.device_name);

                return true;
            }
            catch (Exception e)
            {
                SetErrorMessage("Error Opening Device " + bc.device_name, e);
                return false;
            }
        }

        public static bool CheckAdapterPresent()
        {
            if (BluetoothAdapter.DefaultAdapter == null)
            {
                SetStatusMessage("Bluetooth is not available on this device");
                return false;
            }
            return true;
        }

        public static bool CheckAdapterEnabled()
        {
            if (!BluetoothAdapter.DefaultAdapter.IsEnabled)
            {
                SetStatusMessage("Bluetooth is not enabled");
                return false;
            }

            return true;
        }

        #endregion

        #region Command Sending and Receiving

        public static async Task<bool> SendCommand(BluetoothConnection bc, string command)
        {
            if (bc == null || !bc.oBthSocket.IsConnected)
                return false;

            if (isDebug)
                Debug.WriteLine("Tx: " + command);

            byte[] cmd = Encoding.ASCII.GetBytes(command + LINE_BREAK);

            try
            {
                await bc.oBthSocket.OutputStream.WriteAsync(cmd, 0, cmd.Length);
                await bc.oBthSocket.OutputStream.FlushAsync();

                string response = ReadData(bc.oBthSocket);

                if (!string.IsNullOrEmpty(response)) // ToDo: process values!
                {
                    if (isDebug)
                        Debug.WriteLine("Rx: " + response);
                }

                return true;
            }
            catch (Exception e)
            {
                status_message = string.Format("{0}: {1}", "Read Error", e.Message);

                if (isDebug)
                    Debug.WriteLine(status_message);
                return false;
            }
        }

        private static string ReadData(BluetoothSocket bs)
        {
            StringBuilder sb = new StringBuilder();
            char c;

            do
            {
                c = (char)bs.InputStream.ReadByte();
                sb.Append(c);
            }
            while (c != END_CHAR);

            if (sb.Length == 0 || sb.Length > MAX_LENGTH)
                return string.Empty;

            foreach (string str in REPLACE_VALUES)
            {
                sb.Replace(str, string.Empty);
            }

            return sb.ToString();
        }

        #endregion

        #region Status Messages

        public static string GetStatusMessage()
        {
            return status_message;
        }

        private static void SetStatusMessage(string message)
        {
            status_message = message;
        }

        private static void SetErrorMessage(string msg, Exception e)
        {
            status_message = string.Format("{0}: {1}", msg, e.Message);

            if (isDebug)
                Debug.WriteLine(status_message);
        }

        #endregion
    }
}
