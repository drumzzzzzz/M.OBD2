
using System.Collections.Generic;
using Android.Bluetooth;

namespace M.OBD2
{
    public class BluetoothConnection : List<BluetoothCmd>
    {
        public string device_name { get; set; }
        public string device_address { get; set; }
        public BluetoothDevice oBthDevice { get; set; }
        public BluetoothSocket oBthSocket { get; set; }

        public BluetoothConnection(string device_name, string device_address)
        {
            this.device_name = device_name;
            this.device_address = device_address;
            oBthDevice = null;
            oBthSocket = null;
        }
    }
}
