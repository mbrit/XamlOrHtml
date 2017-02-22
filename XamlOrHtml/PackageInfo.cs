using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace XamlOrHtml
{
    internal class PackageInfo
    {
        private const string XamlExt = ".xaml";
        private const string XbfExt = ".xbf";
        private const string JsExt = ".js";

        internal string PackageId { get; private set; }
        internal string DisplayName { get; private set; }
        internal string PackageRootFolder { get; private set; }
        internal bool FoundStartPage { get; private set; }
        internal bool MarkedUp { get; private set; }

        internal List<string> XamlFiles { get; private set; }
        internal List<string> JsFiles { get; private set; }

        internal PackageInfo(RegistryKey key)
        {
            PackageId = Path.GetFileName(key.Name);
            DisplayName = (string)key.GetValue("DisplayName");
            PackageRootFolder = (string)key.GetValue("PackageRootFolder");

            // walk the files...
            XamlFiles = new List<string>();
            JsFiles = new List<string>();
            WalkFiles(new DirectoryInfo(PackageRootFolder));

            // probe for a start page...
            var appKey = key.OpenSubKey("Applications");
            if (appKey != null)
            {
                using (appKey)
                {
                    foreach(var subAppName in appKey.GetSubKeyNames())
                    {
                        using (var subAppKey = appKey.OpenSubKey(subAppName))
                        {
                            if (subAppKey != null)
                            {
                                var start = (string)subAppKey.GetValue("DefaultStartPage");
                                if (!(string.IsNullOrEmpty(start)))
                                {
                                    FoundStartPage = true;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }

        private void WalkFiles(DirectoryInfo folder)
        {
            try
            {
                foreach (var file in folder.GetFiles())
                {
                    if (String.Compare(file.Extension, XamlExt , StringComparison.OrdinalIgnoreCase) == 0 || String.Compare(file.Extension, XbfExt, StringComparison.OrdinalIgnoreCase) == 0)
                        XamlFiles.Add(file.FullName);
                    else if (String.Compare(file.Extension, JsExt, StringComparison.OrdinalIgnoreCase) == 0)
                        JsFiles.Add(file.FullName);
                    else if (String.CompareOrdinal(file.Name, "MarkedUp.winmd") == 0)
                        MarkedUp = true;
                }

                foreach (var child in folder.GetDirectories())
                    WalkFiles(child);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        internal PackageType Type
        {
            get
            {
                if (FoundStartPage)
                    return PackageType.Html;

                if (XamlFiles.Any())
                    return PackageType.Xaml;
                
                if (JsFiles.Any())
                    return PackageType.Html;
                
                return PackageType.DirectX;
            }
        }

        public bool IsMicrosoft
        {
            get
            {
                return PackageId.Contains("Microsoft");
            }
        }
    }
}
