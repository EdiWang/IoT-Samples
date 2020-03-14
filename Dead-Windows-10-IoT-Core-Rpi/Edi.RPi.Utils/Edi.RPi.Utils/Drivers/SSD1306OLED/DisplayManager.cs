namespace Edi.RPi.Utils.Drivers.SSD1306OLED
{
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
}
