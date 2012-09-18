using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace XamlOrHtml
{
    internal class PackageInfo
    {
        internal string PackageId { get; private set; }
        internal string DisplayName { get; private set; }
        internal string PackageRootFolder { get; private set; }
        internal bool FoundStartPage { get; private set; }

        internal List<string> XamlFiles { get; private set; }
        internal List<string> JsFiles { get; private set; }

        internal PackageInfo(RegistryKey key)
        {
            this.PackageId = Path.GetFileName(key.Name);
            this.DisplayName = (string)key.GetValue("DisplayName");
            this.PackageRootFolder = (string)key.GetValue("PackageRootFolder");

            // walk the files...
            this.XamlFiles = new List<string>();
            this.JsFiles = new List<string>();
            WalkFiles(new DirectoryInfo(this.PackageRootFolder));

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

        private void WalkFiles(DirectoryInfo folder)
        {
            foreach (var file in folder.GetFiles())
            {
                if (string.Compare(file.Extension, ".xaml", true) == 0)
                    this.XamlFiles.Add(file.FullName);
                else if (string.Compare(file.Extension, ".js", true) == 0)
                    this.JsFiles.Add(file.FullName);
            }

            foreach (var child in folder.GetDirectories())
                this.WalkFiles(child);
        }

        internal PackageType Type
        {
            get
            {
                if (this.FoundStartPage)
                    return PackageType.Html;

                if (this.XamlFiles.Any())
                    return PackageType.Xaml;
                else if (this.JsFiles.Any())
                    return PackageType.Html;
                else
                    return PackageType.Unknown;
            }
        }

        public bool IsMicrosoft
        {
            get
            {
                return this.PackageId.Contains("Microsoft");
            }
        }
    }
}
