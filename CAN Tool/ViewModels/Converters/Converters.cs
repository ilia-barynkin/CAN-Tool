﻿using OmniProtocol;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using RVC;
using System.Linq;
using System.ComponentModel;


namespace CAN_Tool.ViewModels.Converters
{

    public class BindingListToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || value.GetType() != typeof(BindingList<int>))
                return "0";
            StringBuilder sb = new StringBuilder("");
            
            foreach (var val in value as BindingList<int>)
                sb.Append($"{val}.");
            sb.Remove(sb.Length - 1, 1);
            return sb.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class DataToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || value.GetType() != typeof(byte[]))
                return "0";
            StringBuilder sb = new StringBuilder("");
            byte[] bytes = value as byte[];
            foreach (byte b in bytes)
                sb.Append($"{b:X02}");

            return sb.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                System.Convert.ToUInt64((string)value, 16);
                string tempString = (string)value + "0000000000000000";
                byte[] data = new byte[8];
                for (int i = 0; i < 8; i++)
                    data[i] = System.Convert.ToByte(tempString.Substring(i * 2, 2), 16);
                return data;
            }
            catch { }
            return new byte[8];
        }
    }

    public class StringToDecConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                string tempString = (string)value;
                string temp2String = tempString.Where(x => char.IsDigit(x)).ToString();
                return System.Convert.ToInt32((string)value, 10);
            }
            catch {
                return 0;
                  }
        }
    }

    public class HexStringToIntConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return $"{value:X}";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                string tempString = (string)value;
                var arr = tempString.Where(x => char.IsDigit(x) || char.ToLower(x) == 'a' || char.ToLower(x) == 'b' || char.ToLower(x) == 'c' || char.ToLower(x) == 'd' || char.ToLower(x) == 'e' || char.ToLower(x) == 'f').ToArray();
                string res = new string(arr);
                Int64 tmp = System.Convert.ToInt64(res, 16);
                if (tmp > Int32.MaxValue) { return Int32.MaxValue; }
                if (tmp< Int32.MinValue) { return Int32.MinValue; }

                return System.Convert.ToInt32(res, 16);
            }
            catch { return 0; }

        }
    }

    public class HexStringToUlongConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                return ((ulong)value).ToString("X");
            }
            catch
            {
                return "0";
            }

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                string tempString = (string)value;
                var arr = tempString.Where(x => char.IsDigit(x) || char.ToLower(x) >= 'a' && char.ToLower(x) <= 'f').ToArray();
                string res = new string(arr);
                if (res.Length>16)
                    res=res.Substring(0,16);
                UInt64 tmp = System.Convert.ToUInt64(res, 16);
                return tmp;
            }
            catch { return 0; }

        }
    }


    public class FuelPumpIndicatorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double temp = System.Convert.ToDouble(value);
            return (temp / 100).ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return double.Parse(value.ToString());
        }
    }

    public class BoolToVisibleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value == true)
                return Visibility.Visible;
            else
                return Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((Visibility)value == Visibility.Visible);
        }
    }

    public class IntToVisibleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && (int)value != 0)
                return Visibility.Visible;
            else
                return Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("This is one way converter!");
        }
    }

    public class BinarToVisibleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && (DeviceType)value == DeviceType.Binar)
                return Visibility.Visible;
            else
                return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("This is one way converter!");
        }
    }

    public class AcInverterToVisibleConveter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && (DeviceType)value == DeviceType.AcInverter)
                return Visibility.Visible;
            else
                return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("This is one way converter!");
        }
    }


    public class PressureSensorToVisible : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && (DeviceType)value == DeviceType.PressureSensor)
                return Visibility.Visible;
            else
                return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("This is one way converter!");
        }
    }

    public class GenericLoadTrippleToVisible : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && (DeviceType)value == DeviceType.GenericLoadTripple)
                return Visibility.Visible;
            else
                return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("This is one way converter!");
        }
    }

    public class BootloaderToVisible : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && (DeviceType)value == DeviceType.BootLoader)
                return Visibility.Visible;
            else
                return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("This is one way converter!");
        }
    }


    public class PlanarToVisibleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && (DeviceType)value == DeviceType.Planar)
                return Visibility.Visible;
            else
                return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("This is one way converter!");
        }
    }

    public class HeaterToVisibleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && (DeviceType)value == DeviceType.Binar || (DeviceType)value == DeviceType.Planar)
                return Visibility.Visible;
            else
                return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("This is one way converter!");
        }
    }

    public class HcuToVisibleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && (DeviceType)value == DeviceType.Hcu)
                return Visibility.Visible;
            else
                return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("This is one way converter!");
        }
    }

    public class StateToBorderBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && (bool)value == true)
                return Brushes.DarkGray;
            else
                return Brushes.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("This is one way converter!");
        }
    }

    public class BoolToOpacity : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && (bool)value == true)
                return 1;
            else
                return 0.7;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("This is one way converter!");
        }
    }

    public class FarenheitConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
                if (App.Settings.UseImperial)
                    if (value.GetType() == typeof(float))
                        return Math.Round((float)value * 1.8 + 32);
                    else
                        return (int)Math.Round((int)value * 1.8 + 32);
                else
                    return value;
            else
                return null;

        }


        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
                if (App.Settings.UseImperial)
                    return ((int)value - 32) / 1.8;
                else
                    return value;
            else
                return null;
        }
    }

    public class FarenheitRelativeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
                if (App.Settings.UseImperial)
                    if (value.GetType() == typeof(float))
                        return Math.Round((float)value * 1.8);
                    else
                        return (int)Math.Round((int)value * 1.8);
                else
                    return value;
            else
                return null;

        }


        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
                if (App.Settings.UseImperial)
                    return ((int)value) / 1.8;
                else
                    return value;
            else
                return null;
        }
    }

    public class DgnConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return RVC.RVC.DGNs[(int)value];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
                return ((KeyValuePair<int, DGN>)value).Key;
            else
                return null;
        }
    }

    public class TimeSpanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return  new TimeSpan(0, 0, 0,  0, (int)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((TimeSpan)value).Milliseconds;
        }
    }

    public class RegularCanToVisibleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((WorkMode_t)value==WorkMode_t.RegularCan)
                return Visibility.Visible;
            else
                return Visibility.Collapsed;
            
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("This is one way converter!");
        }
    }

    public class OmniToVisibleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((WorkMode_t)value == WorkMode_t.Omni)
                return Visibility.Visible;
            else
                return Visibility.Collapsed;

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("This is one way converter!");
        }
    }

    public class RvcToVisibleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((WorkMode_t)value == WorkMode_t.Rvc)
                return Visibility.Visible;
            else
                return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("This is one way converter!");
        }
    }

    public class ConnectedZoneTypeToVisible : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((zoneType_t)value != zoneType_t.Disconnected)
                return Visibility.Visible;
            else
                return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("This is one way converter!");
        }
    }

    public class DefrostingZoneTypeToVisible : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((zoneType_t)value == zoneType_t.Defrosting)
                return Visibility.Visible;
            else
                return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("This is one way converter!");
        }
    }

    public class FurnaceZoneTypeToVisible : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((zoneType_t)value == zoneType_t.Furnace)
                return Visibility.Visible;
            else
                return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("This is one way converter!");
        }
    }

    public class HeatingZoneTypeToVisible : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((zoneType_t)value == zoneType_t.Furnace || (zoneType_t)value == zoneType_t.Radiator)
                return Visibility.Visible;
            else
                return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("This is one way converter!");
        }
    }

    public class HeatingZoneTypeToBool : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((zoneType_t)value == zoneType_t.Furnace || (zoneType_t)value == zoneType_t.Radiator)
                return true;
            else
                return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("This is one way converter!");
        }
    }

    public class ConnectedZoneTypeToBool : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((zoneType_t)value != zoneType_t.Disconnected)
                return true;
            else
                return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("This is one way converter!");
        }
    }

    public class NullToVisibleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return Visibility.Collapsed;
            else
                return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("This is one way converter!");
        }
    }

    public class ManualEnableConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || value.GetType() != typeof(OmniZoneHandler))
                return false;
            OmniZoneHandler zone = (OmniZoneHandler)value;
            if (zone.Connected!=zoneType_t.Furnace)
                return false;
            return zone.ManualMode;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("This is one way converter!");
        }
    }

}
