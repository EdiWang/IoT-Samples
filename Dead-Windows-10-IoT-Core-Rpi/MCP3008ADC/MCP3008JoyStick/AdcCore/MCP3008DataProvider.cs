using System;
using System.Diagnostics;
using System.Threading;

namespace MCP3008JoyStick.AdcCore
{
    public delegate void DataReceivedEventHandler(object sender, SensorDataEventArgs e);

    public class MCP3008DataProvider
    {
        const byte SoilMoistureChannel = 0;
        const float ReferenceVoltage = 5.0F;
        public event DataReceivedEventHandler DataReceived;
        public MCP3008 mcp3008;

        private Timer _timer;

        public MCP3008DataProvider()
        {
            mcp3008 = new MCP3008(ReferenceVoltage);
        }

        public void StartTimer()
        {
            _timer = new Timer(TimerCallback, this, 0, 1000);
        }

        /**
         * This method records on a timer the data measured by the soil moisture sensor,
         * then organizes all of the information collected.  
         * */
        public void TimerCallback(object state)
        {
            //MCP3008 is an ADC and checks to see if this is initialized. 
            //the soil moisture sensor and the photocell are on different channels of the ADC
            if (mcp3008 == null)
            {
                Debug.WriteLine("mcp3008 is null");
            }
            else
            {
                float currentSoilMoisture = mcp3008.ReadADC(SoilMoistureChannel);
                Debug.WriteLine(currentSoilMoisture);
                var soilmoistureArgs = new SensorDataEventArgs()
                {
                    SensorName = "SoilMoisture",
                    SensorValue = currentSoilMoisture,
                    Timestamp = DateTime.Now
                };
                OnDataReceived(soilmoistureArgs);
            }
        }

        protected virtual void OnDataReceived(SensorDataEventArgs e)
        {
            DataReceived?.Invoke(this, e);
        }
    }

    public class SensorDataEventArgs : EventArgs
    {
        public string SensorName;
        public float SensorValue;
        public DateTime Timestamp;
    }
}
