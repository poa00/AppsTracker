﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppsTracker.Data.XmlSettings
{
    public sealed class ScreenshotsViewSettings : XmlSettingsBase
    {
        [SettingsNode]
        public double SeparatorPosition { get; set; }
    }
}