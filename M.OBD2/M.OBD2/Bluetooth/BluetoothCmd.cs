using System;
using System.Collections.Generic;

namespace M.OBD2
{
    public class BluetoothCmd
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Units { get; set; }
        public bool isImperial { get; set; }
        public string Cmd { get; set; }
        public int Rate { get; set; }
        public int Decimals { get; set; }
        public string Expression { get; set; }
        public BlueToothCmds.COMMAND_TYPE[] Command_Types { get; set; }
        public DateTime dtNext { get; set; }

        public BluetoothCmd()
        {
        }
    }

    public class BlueToothCmds : List<BluetoothCmd>
    {
        public enum COMMAND_TYPE
        {
            DEFAULT,
            AFR,
            VSS,
            MAF,
            MPG
        }

        public BlueToothCmds() // ToDo: load commands from Db
        {
        }

        public void CreateTestCommands()
        {
            Add(new BluetoothCmd()
            {
                Id = 0,
                Name = "RPM",
                Units = "",
                isImperial = true,
                Cmd = "",
                Rate =  200,
                Decimals = 0,
                Expression = "a*1",
                Command_Types = new[] { COMMAND_TYPE.DEFAULT }
            });

            Add(new BluetoothCmd()
            {
                Id = 1,
                Name = "VOLTS",
                Units = "VDC",
                isImperial = true,
                Cmd = "",
                Rate = 200,
                Decimals = 0,
                Expression = "a*1",
                Command_Types = new[] { COMMAND_TYPE.DEFAULT }
            });

            Add(new BluetoothCmd()
                {
                    Id = 2,
                    Name = "AFR",
                    Units = "",
                    isImperial = true,
                    Cmd = "",
                    Rate = 1000,
                    Decimals = 2,
                    Expression = "a*1",
                    Command_Types = new[] { COMMAND_TYPE.AFR }
            });

            Add(new BluetoothCmd()
            {
                Id = 3,
                Name = "VSS",
                Units = "Mph",
                isImperial = true,
                Cmd = "",
                Rate = 1000,
                Decimals = 2,
                Expression = "a*1",
                Command_Types = new[] { COMMAND_TYPE.VSS }
            });

            Add(new BluetoothCmd()
            {
                Id = 4,
                Name = "MAF",
                Units = "g/s",
                isImperial = true,
                Cmd = "",
                Rate = 500,
                Decimals = 2,
                Expression = "a*1",
                Command_Types = new[] { COMMAND_TYPE.MAF }
            });

            Add(new BluetoothCmd()
            {
                Id = 5,
                Name = "MPG",
                Units = "",
                isImperial = true,
                Cmd = null,
                Rate = 5000,
                Decimals = 1,
                Expression = "(a*b*1740.572)/(3600*c/100",
                Command_Types = new[] { COMMAND_TYPE.MPG, COMMAND_TYPE.AFR, COMMAND_TYPE.VSS, COMMAND_TYPE.MAF }
            });
        }
    }
}
