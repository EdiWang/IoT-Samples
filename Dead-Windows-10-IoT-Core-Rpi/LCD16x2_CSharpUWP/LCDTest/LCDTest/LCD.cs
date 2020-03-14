using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Gpio;

namespace LCDTest
{
    internal class Lcd
    {
        // commands
        private const int LcdCleardisplay = 0x01;
        private const int LcdReturnhome = 0x02;
        private const int LcdEntrymodeset = 0x04;
        private const int LcdDisplayControl = 0x08;
        private const int LcdCursorshift = 0x10;
        private const int LcdFunctionset = 0x20;
        private const int LcdSetcgramaddr = 0x40;
        private const int LcdSetddramaddr = 0x80;

        // flags for display entry mode
        private const int LcdEntryright = 0x00;
        private const int LcdEntryleft = 0x02;
        private const int LcdEntryshiftincrement = 0x01;
        private const int LcdEntryshiftdecrement = 0x00;

        // flags for display on/off control
        private const int LcdDisplayon = 0x04;
        private const int LcdDisplayoff = 0x00;
        private const int LcdCursoron = 0x02;
        private const int LcdCursoroff = 0x00;
        private const int LcdBlinkon = 0x01;
        private const int LcdBlinkoff = 0x00;

        // flags for display/cursor shift
        private const int LcdDisplaymove = 0x08;
        private const int LcdCursormove = 0x00;
        private const int LcdMoveright = 0x04;
        private const int LcdMoveleft = 0x00;

        // flags for function set
        private const int Lcd_8Bitmode = 0x10;
        private const int Lcd_4Bitmode = 0x00;
        private const int Lcd_2Line = 0x08;
        private const int Lcd_1Line = 0x00;
        public const int Lcd_5X10Dots = 0x04;
        public const int Lcd_5X8Dots = 0x00;

        private readonly int _cols;
        private int _currentrow;
        private readonly int _rows;

        public bool AutoScroll = false;

        private readonly string[] _buffer;
        private readonly string _cleanline = "";

        public GpioController Controller { get; set; }
        private int _displayControl;

        private int _displayFunction;
        private int _displayMode;
        private readonly GpioPin[] _dPin = new GpioPin[8];
        private GpioPin _enPin;
        private GpioPin _rsPin;

        public Lcd(int cols, int rows, int charsize = Lcd_5X8Dots)
        {
            _cols = cols;
            _rows = rows;

            _buffer = new string[rows];

            for (var i = 0; i < cols; i++)
                _cleanline = _cleanline + " ";

            _displayFunction = charsize;
            if (_rows > 1)
                _displayFunction = _displayFunction | Lcd_2Line;
            else
                _displayFunction = _displayFunction | Lcd_1Line;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DelayMicroseconds(int uS)
        {
            if (uS > 2000)
                throw new Exception("Invalid param, use Task.Delay for 2ms and more");

            if (uS < 100) //call takes more time than 100uS 
                return;

            var tickToReach = DateTime.UtcNow.Ticks + uS*1000; //1GHz Raspi2 Clock
            while (DateTime.UtcNow.Ticks < tickToReach)
            {
            }
        }

        private async Task Begin()
        {
            await Task.Delay(50);
            // Now we pull both RS and R/W low to begin commands
            _rsPin.Write(GpioPinValue.Low);
            _enPin.Write(GpioPinValue.Low);

            //put the LCD into 4 bit or 8 bit mode
            if ((_displayFunction & Lcd_8Bitmode) != Lcd_8Bitmode)
            {
                // we start in 8bit mode, try to set 4 bit mode
                Write4Bits(0x03);
                await Task.Delay(5); // wait min 4.1ms

                // second try
                Write4Bits(0x03);
                await Task.Delay(5); // wait min 4.1ms

                // third go!
                Write4Bits(0x03);
                DelayMicroseconds(150);

                // finally, set to 4-bit interface
                Write4Bits(0x02);
            }
            else
            {
                // Send function set command sequence
                Command((byte) (LcdFunctionset | _displayFunction));
                await Task.Delay(5); // wait min 4.1ms

                // second try
                Command((byte) (LcdFunctionset | _displayFunction));
                DelayMicroseconds(150);

                // third go
                Command((byte) (LcdFunctionset | _displayFunction));
            }

            Command((byte) (LcdFunctionset | _displayFunction));
            _displayControl = LcdDisplayon | LcdCursoroff | LcdBlinkoff;
            DisplayOn();

            await ClearAsync();

            _displayMode = LcdEntryleft | LcdEntryshiftdecrement;
            Command((byte) (LcdEntrymodeset | _displayMode));
        }

        public async Task<bool> InitAsync(GpioController ctl, int rs, int enable, int d4, int d5, int d6, int d7)
        {
            try
            {
                Controller = ctl;

                _displayFunction = _displayFunction | Lcd_4Bitmode;

                _rsPin = Controller.OpenPin(rs);
                _rsPin.SetDriveMode(GpioPinDriveMode.Output);

                _enPin = Controller.OpenPin(enable);
                _enPin.SetDriveMode(GpioPinDriveMode.Output);

                _dPin[0] = Controller.OpenPin(d4);
                _dPin[0].SetDriveMode(GpioPinDriveMode.Output);

                _dPin[1] = Controller.OpenPin(d5);
                _dPin[1].SetDriveMode(GpioPinDriveMode.Output);

                _dPin[2] = Controller.OpenPin(d6);
                _dPin[2].SetDriveMode(GpioPinDriveMode.Output);

                _dPin[3] = Controller.OpenPin(d7);
                _dPin[3].SetDriveMode(GpioPinDriveMode.Output);

                await Begin();

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return false;
            }
        }

        private void PulseEnable()
        {
            _enPin.Write(GpioPinValue.Low);
            // delayMicroseconds(1);
            _enPin.Write(GpioPinValue.High);
            //delayMicroseconds(1);
            _enPin.Write(GpioPinValue.Low);
            //delayMicroseconds(50);
        }

        private void Write4Bits(byte value)
        {
            //String s = "Value :"+value.ToString();
            for (var i = 0; i < 4; i++)
            {
                var val = (GpioPinValue) ((value >> i) & 0x01);
                _dPin[i].Write(val);
                //  s = DPin[i].PinNumber.ToString()+" /"+ i.ToString() + "=" + val.ToString() + "  " + s;
            }
            //Debug.WriteLine(s);

            PulseEnable();
        }


        private void Write8Bits(byte value)
        {
            for (var i = 0; i < 8; i++)
            {
                var val = (GpioPinValue) ((value >> 1) & 0x01);
                _dPin[i].Write(val);
            }

            PulseEnable();
        }

        private void Send(byte value, GpioPinValue bit8Mode)
        {
            //Debug.WriteLine("send :"+value.ToString());

            _rsPin.Write(bit8Mode);

            if ((_displayFunction & Lcd_8Bitmode) == Lcd_8Bitmode)
            {
                Write8Bits(value);
            }
            else
            {
                var b = (byte) ((value >> 4) & 0x0F);
                Write4Bits(b);
                b = (byte) (value & 0x0F);
                Write4Bits(b);
            }
        }

        private void Write(byte value)
        {
            Send(value, GpioPinValue.High);
        }

        private void Command(byte value)
        {
            Send(value, GpioPinValue.Low);
        }

        public async Task ClearAsync()
        {
            Command(LcdCleardisplay);
            await Task.Delay(2);

            for (var i = 0; i < _rows; i++)
                _buffer[i] = "";

            _currentrow = 0;

            await HomeAsync();
        }

        public async Task HomeAsync()
        {
            Command(LcdReturnhome);
            await Task.Delay(2);
        }

        public void Write(string text)
        {
            var data = Encoding.UTF8.GetBytes(text);

            foreach (var ch in data)
                Write(ch);
        }

        public void SetCursor(byte col, byte row)
        {
            var rowOffsets = new[] {0x00, 0x40, 0x14, 0x54};

            /*if (row >= _numlines)
            {
                row = _numlines - 1;    // we count rows starting w/0
            }
            */

            Command((byte) (LcdSetddramaddr | (col + rowOffsets[row])));
        }

        // Turn the display on/off (quickly)
        public void DisplayOff()
        {
            _displayControl &= ~LcdDisplayon;
            Command((byte) (LcdDisplayControl | _displayControl));
        }

        public void DisplayOn()
        {
            _displayControl |= LcdDisplayon;
            Command((byte) (LcdDisplayControl | _displayControl));
        }

        // Turns the underline cursor on/off
        public void NoCursor()
        {
            _displayControl &= ~LcdCursoron;
            Command((byte) (LcdDisplayControl | _displayControl));
        }

        public void Cursor()
        {
            _displayControl |= LcdCursoron;
            Command((byte) (LcdDisplayControl | _displayControl));
        }

        // Turn on and off the blinking cursor
        public void NoBlink()
        {
            _displayControl &= ~LcdBlinkon;
            Command((byte) (LcdDisplayControl | _displayControl));
        }

        public void Blink()
        {
            _displayControl |= LcdBlinkon;
            Command((byte) (LcdDisplayControl | _displayControl));
        }

        // These commands scroll the display without changing the RAM
        public void ScrollDisplayLeft()
        {
            Command(LcdCursorshift | LcdDisplaymove | LcdMoveleft);
        }

        public void ScrollDisplayRight()
        {
            Command(LcdCursorshift | LcdDisplaymove | LcdMoveright);
        }

        // This is for text that flows Left to Right
        public void LeftToRight()
        {
            _displayMode |= LcdEntryleft;
            Command((byte) (LcdEntrymodeset | _displayMode));
        }

        // This is for text that flows Right to Left
        public void RightToLeft()
        {
            _displayMode &= ~LcdEntryleft;
            Command((byte) (LcdEntrymodeset | _displayMode));
        }

        // This will 'right justify' text from the cursor
        public void Autoscroll()
        {
            _displayMode |= LcdEntryshiftincrement;
            Command((byte) (LcdEntrymodeset | _displayMode));
        }

        // This will 'left justify' text from the cursor
        public void NoAutoscroll()
        {
            _displayMode &= ~LcdEntryshiftincrement;
            Command((byte) (LcdEntrymodeset | _displayMode));
        }

        // Allows us to fill the first 8 CGRAM locations
        // with custom characters
        public void CreateChar(byte location, byte[] charmap)
        {
            location &= 0x7; // we only have 8 locations 0-7
            Command((byte) (LcdSetcgramaddr | (location << 3)));
            for (var i = 0; i < 8; i++)
                Write(charmap[i]);
        }

        public void WriteLine(string text)
        {
            if (_currentrow >= _rows)
            {
                //let's do shift
                for (var i = 1; i < _rows; i++)
                {
                    _buffer[i - 1] = _buffer[i];
                    SetCursor(0, (byte) (i - 1));
                    Write(_buffer[i - 1].Substring(0, _cols));
                }
                _currentrow = _rows - 1;
            }
            _buffer[_currentrow] = text + _cleanline;
            SetCursor(0, (byte) _currentrow);
            var cuts = _buffer[_currentrow].Substring(0, _cols);
            Write(cuts);
            _currentrow++;
        }
    }
}