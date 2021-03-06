﻿// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using Windows.Devices.Enumeration;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media.Imaging;
// NOTE: The following using statements are only needed in order to demonstrate many of the
// different device selectors available from Windows Runtime APIs. You will only need to include
// the namespace for the Windows Runtime API your actual scenario needs.

namespace Edi.RPi.TestHub.Presenters
{
    public struct ProtectionLevelSelectorInfo
    {
        public string DisplayName
        {
            get;
            set;
        }

        public DevicePairingProtectionLevel ProtectionLevel
        {
            get;
            set;
        }
    }

    public static class ProtectionSelectorChoices
    {
        public static List<ProtectionLevelSelectorInfo> Selectors
        {
            get
            {
                List<ProtectionLevelSelectorInfo> selectors = new List<ProtectionLevelSelectorInfo>
                {
                    new ProtectionLevelSelectorInfo
                    {
                        DisplayName = "Default",
                        ProtectionLevel = DevicePairingProtectionLevel.Default
                    },
                    new ProtectionLevelSelectorInfo
                    {
                        DisplayName = "None",
                        ProtectionLevel = DevicePairingProtectionLevel.None
                    },
                    new ProtectionLevelSelectorInfo
                    {
                        DisplayName = "Encryption",
                        ProtectionLevel = DevicePairingProtectionLevel.Encryption
                    },
                    new ProtectionLevelSelectorInfo
                    {
                        DisplayName = "Encryption and authentication",
                        ProtectionLevel =
                            DevicePairingProtectionLevel.EncryptionAndAuthentication
                    }
                };

                return selectors;
            }
        }
    }

    public class BluetoothDeviceInformationDisplay : INotifyPropertyChanged
    {
        private DeviceInformation deviceInfo;
        private static string pairingPairedStateString = GetResourceString("BluetoothDeviceStatePairedText");
        private static string pairingReadyToPairStateString = GetResourceString("BluetoothDeviceStateReadyToPairText");
        private static string pairingUnknownStateString = GetResourceString("BluetoothDeviceStateUnknownText");

        public BluetoothDeviceInformationDisplay(DeviceInformation deviceInfoIn)
        {
            deviceInfo = deviceInfoIn;
            UpdateGlyphBitmapImage();
        }

        public DeviceInformationKind Kind => deviceInfo.Kind;

        public string IdWithoutProtocolPrefix => deviceInfo.Id.Substring(deviceInfo.Id.IndexOf("#") + 1);

        public string Id => deviceInfo.Id;

        public string Name => deviceInfo.Name;

        public BitmapImage GlyphBitmapImage
        {
            get;
            private set;
        }

        public bool CanPair => deviceInfo.Pairing.CanPair;

        public bool IsPaired => deviceInfo.Pairing.IsPaired;

        public string DevicePairingStateText
        {
            get
            {
                if (!deviceInfo.Pairing.IsPaired && deviceInfo.Pairing.CanPair)
                {
                    return pairingReadyToPairStateString ;
                }
                else if (deviceInfo.Pairing.IsPaired)
                {
                    return pairingPairedStateString;
                }
                else
                {
                    return pairingUnknownStateString;
                }
            }
        }

        public Windows.UI.Xaml.Visibility PairButtonVisiblilty => (!deviceInfo.Pairing.IsPaired && deviceInfo.Pairing.CanPair) ? Windows.UI.Xaml.Visibility.Visible : Windows.UI.Xaml.Visibility.Collapsed;

        public Windows.UI.Xaml.Visibility UnpairButtonVisiblilty => deviceInfo.Pairing.IsPaired ? Windows.UI.Xaml.Visibility.Visible : Windows.UI.Xaml.Visibility.Collapsed;

        public IReadOnlyDictionary<string, object> Properties => deviceInfo.Properties;

        public DeviceInformation DeviceInformation
        {
            get
            {
                return deviceInfo;
            }

            private set
            {
                deviceInfo = value;
            }
        }

        public void Update(DeviceInformationUpdate deviceInfoUpdate)
        {
            deviceInfo.Update(deviceInfoUpdate);

            OnPropertyChanged("Kind");
            OnPropertyChanged("Id");
            OnPropertyChanged("Name");
            OnPropertyChanged("DeviceInformation");
            OnPropertyChanged("CanPair");
            OnPropertyChanged("IsPaired");
            OnPropertyChanged("DevicePairingStateText");
            OnPropertyChanged("PairButtonVisiblilty");
            OnPropertyChanged("UnpairButtonVisiblilty");
            UpdateGlyphBitmapImage();
        }

        private async void UpdateGlyphBitmapImage()
        {
            // Not available on Athens
            //DeviceThumbnail deviceThumbnail = await deviceInfo.GetGlyphThumbnailAsync();
            //BitmapImage glyphBitmapImage = new BitmapImage();
            //await glyphBitmapImage.SetSourceAsync(deviceThumbnail);
            //GlyphBitmapImage = glyphBitmapImage;
            //OnPropertyChanged("GlyphBitmapImage");
        }
        
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        /// <summary>
        /// Return the named resource string
        /// </summary>
        /// <param name="resourceName"></param>
        /// <returns>A string containing the requested resource string value</returns>
        internal static string GetResourceString(string resourceName)
        {
            string theResourceString = "##Failed to get resource string##";
            var resourceLoader = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();
            theResourceString = resourceLoader.GetString(resourceName);
            return theResourceString;
        }
    }

    public class GeneralPropertyValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            object property = null;

            if (value is IReadOnlyDictionary<string, object> &&
                parameter is string &&
                false == String.IsNullOrEmpty((string)parameter))
            {
                IReadOnlyDictionary<string, object> properties = value as IReadOnlyDictionary<string, object>;
                string propertyName = parameter as string;

                property = properties[propertyName];
            }

            return property;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
