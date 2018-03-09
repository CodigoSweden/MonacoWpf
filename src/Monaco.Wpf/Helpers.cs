﻿using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Monaco.Wpf
{
    public static class Helpers
    {
        public static void SetBrowserEmulation()
        {
            var fbe = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_BROWSER_EMULATION", true);
            var exeName = Assembly.GetEntryAssembly().GetName().Name + ".exe";
            fbe.SetValue(exeName, 11001, RegistryValueKind.DWord);
        } 
    }

    public enum EditorLanguage
    { 
        CSharp,
        Typescript,
        Javascript,
        Css,
        Html,
        Json
    }
}
