using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.System.Profile;
using Windows.UI.ViewManagement;

namespace UWPHelpers
{
    public enum DeviceType
    {
        Phone,
        Desktop,
        Tablet,
        IoT,
        SurfaceHub,
        Other
    }

    public static class DeviceTypeHelper
    {
        public static DeviceType GetDeviceType()
        {
            switch (AnalyticsInfo.VersionInfo.DeviceFamily)
            {
                case "Windows.Mobile":
                    return DeviceType.Phone;
                case "Windows.Desktop":
                    return UIViewSettings.GetForCurrentView().UserInteractionMode == UserInteractionMode.Mouse ? DeviceType.Desktop : DeviceType.Tablet;
                case "Windows.Universal":
                    return DeviceType.IoT;
                case "Windows.Team":
                    return DeviceType.SurfaceHub;
                default:
                    return DeviceType.Other;
            }
        }
    }
}
