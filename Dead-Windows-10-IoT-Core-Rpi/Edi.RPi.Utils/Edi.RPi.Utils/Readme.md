#Edi.RPi.Utils

## Drivers
### SSD1306 OLED 128x64 Display (I2C)  

by Stefan Gordon

https://github.com/stefangordon/IoTCore-SSD1306-Driver

Usage:

<pre>
    static class DisplayManager
    {
        private static Display _display;

        public static void Init()
        {
            _display = new Display();
            _display.Init(true);
            Update();
        }

        static void Update()
        {
            _display.ClearDisplayBuf();
            DrawBody();
            _display.DisplayUpdate();
        }

        static void DrawBody()
        {
            // Row 0, and image
            _display.WriteImageDisplayBuf(DisplayImages.Connected, 0, 0);

            // Row 1 - 3
            _display.WriteLineDisplayBuf("A", 0, 1);
            _display.WriteLineDisplayBuf("B", 0, 2);
            _display.WriteLineDisplayBuf("C", 0, 3);
        }     
    }
</pre>

### 74HC595 4 Digit LED Tube

by Edi Wang

https://www.hackster.io/ediwang/74hc595-4-digits-led-tube-with-windows-10-c3e293

## Sensors

