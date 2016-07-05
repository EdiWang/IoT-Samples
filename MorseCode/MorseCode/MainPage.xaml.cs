using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Gpio;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace MorseCode
{
    public sealed partial class MainPage : Page
    {
        public GpioController GpioController { get; set; }

        public GpioPin BuzzPin { get; set; }

        public GpioPin LedPin { get; set; }

        public Dictionary<char, string> Morse { get; } = new Dictionary<char, string>
        {
            {'A', ".-"},
            {'B', "-..."},
            {'C', "-.-."},
            {'D', "-.."},
            {'E', "."},
            {'F', "..-."},
            {'G', "--."},
            {'H', "...."},
            {'I', ".."},
            {'J', ".---"},
            {'K', "-.-"},
            {'L', ".-.."},
            {'M', "--"},
            {'N', "-."},
            {'O', "---"},
            {'P', ".--."},
            {'Q', "--.-"},
            {'R', ".-."},
            {'S', "..."},
            {'T', "-"},
            {'U', "..-"},
            {'V', "...-"},
            {'W', ".--"},
            {'X', "-..-"},
            {'Y', "-.--"},
            {'Z', "--.."},
            {'0', "-----"},
            {'1', ".----"},
            {'2', "..---"},
            {'3', "...--"},
            {'4', "....-"},
            {'5', "....."},
            {'6', "-...."},
            {'7', "--..."},
            {'8', "---.."},
            {'9', "----."},
        };

        public MainPage()
        {
            this.InitializeComponent();

            GpioController = GpioController.GetDefault();

            BuzzPin = GpioController.OpenPin(5);
            BuzzPin.SetDriveMode(GpioPinDriveMode.Output);
            BuzzPin.Write(GpioPinValue.High);

            LedPin = GpioController.OpenPin(6);
            LedPin.SetDriveMode(GpioPinDriveMode.Output);
            LedPin.Write(GpioPinValue.High);
        }

        private string TextToMorse(string text)
        {
            text = text.ToUpper();

            var sb = new StringBuilder();
            foreach (var c in text)
            {
                if (Morse.ContainsKey(c))
                {
                    sb.Append(Morse[c]);
                    sb.Append("/");
                }
            }
            return sb.ToString();
        }

        private async void BtnBeepMorse_OnClick(object sender, RoutedEventArgs e)
        {
            var text = TxtSource.Text.Trim();
            if (!string.IsNullOrEmpty(text))
            {
                var morse = TextToMorse(text);
                TxtMessage.Text += morse + Environment.NewLine;
                await MorseToBeep(morse);
            }
        }

        private async Task MorseToBeep(string morse)
        {
            foreach (var c in morse)
            {
                switch (c)
                {
                    case '.':
                        BuzzPin.Write(GpioPinValue.Low);
                        LedPin.Write(GpioPinValue.Low);
                        await Task.Delay(100);
                        BuzzPin.Write(GpioPinValue.High);
                        LedPin.Write(GpioPinValue.High);
                        break;
                    case '-':
                        BuzzPin.Write(GpioPinValue.Low);
                        LedPin.Write(GpioPinValue.Low);
                        await Task.Delay(300);
                        BuzzPin.Write(GpioPinValue.High);
                        LedPin.Write(GpioPinValue.High);
                        break;
                    case '/':
                        BuzzPin.Write(GpioPinValue.High);
                        LedPin.Write(GpioPinValue.High);
                        await Task.Delay(200);
                        break;
                    default:
                        break;
                }

                await Task.Delay(50);
            }
        }
    }
}
