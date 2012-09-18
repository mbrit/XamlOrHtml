using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace XamlOrHtml
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                // walk...
                string temp = null;
                var packages = WalkPackages(ref temp);

                // count...
                int msXaml = 0;
                int msHtml = 0;
                int msUnknown = 0;
                int nonMsXaml = 0;
                int nonMsHtml = 0;
                int nonMsUnknown = 0;
                foreach (var package in packages)
                {
                    if (package.IsMicrosoft)
                    {
                        if (package.Type == PackageType.Xaml)
                            msXaml++;
                        else if (package.Type == PackageType.Html)
                            msHtml++;
                        else if (package.Type == PackageType.Unknown)
                            msUnknown++;
                    }
                    else
                    {
                        if (package.Type == PackageType.Xaml)
                            nonMsXaml++;
                        else if (package.Type == PackageType.Html)
                            nonMsHtml++;
                        else if (package.Type == PackageType.Unknown)
                            nonMsUnknown++;
                    }
                }

                // render...
                StringBuilder builder = new StringBuilder();
                RenderResult(builder, "Microsoft", msXaml, msHtml, msUnknown);
                builder.Append("\r\n");
                RenderResult(builder, "Non-Microsoft", nonMsXaml, nonMsHtml, nonMsUnknown);
                MessageBox.Show(builder.ToString());

                // show...
                Process.Start(temp);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            finally
            {
                if (Debugger.IsAttached)
                    Console.ReadLine();
            }
        }

        private static void RenderResult(StringBuilder builder, string name, int xaml, int html, int unknown)
        {
            decimal total = (decimal)(xaml + html + unknown);
            decimal percentageXaml = 0M;
            decimal percentageHtml = 0M;
            decimal percentageUnknown = 0M;
            if(total > 0)
            {
                percentageXaml = (decimal)xaml / total;
                percentageHtml = (decimal)html / total;
                percentageUnknown = (decimal)unknown / total;
            }

            // ok...
            builder.Append(name);
            builder.Append(" --> ");
            builder.Append("XAML: ");
            builder.Append(xaml);
            builder.Append(" (");
            builder.Append((percentageXaml * 100).ToString("n0"));
            builder.Append("%), ");
            builder.Append("HTML: ");
            builder.Append(html);
            builder.Append(" (");
            builder.Append((percentageHtml * 100).ToString("n0"));
            builder.Append("%), ");
            builder.Append("Unknown: ");
            builder.Append(unknown);
            builder.Append(" (");
            builder.Append((percentageUnknown * 100).ToString("n0"));
            builder.Append("%)");
        }

        private static List<PackageInfo> WalkPackages(ref string temp)
        {
            Console.WriteLine("Walking packages...");

            // find...
            var packages = new List<PackageInfo>();
            using (var key = Registry.ClassesRoot.OpenSubKey(@"Local Settings\Software\Microsoft\Windows\CurrentVersion\AppModel\Repository\Packages"))
            {
                foreach (var packageName in key.GetSubKeyNames())
                {
                    Console.WriteLine("..." + packageName);

                    using (var packageKey = key.OpenSubKey(packageName))
                    {
                        var package = new PackageInfo(packageKey);
                        packages.Add(package);
                    }
                }
            }

            // sort...
            packages.Sort(new DisplayNameComparer());

            // csv...
            temp = Path.GetTempFileName() + ".csv";
            using(var writer = new StreamWriter(temp))
            {
                writer.WriteLine("PackageId,DisplayName,RootFolder,NumXaml,NumJs,FoundStartPage,Type");

                foreach (var package in packages)
                {
                    writer.WriteLine("\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4}\",\"{5}\",\"{6}\"", package.PackageId, package.DisplayName, 
                        package.PackageRootFolder, package.XamlFiles.Count, package.JsFiles.Count, package.FoundStartPage, package.Type);
                }
            }

            // return...
            return packages;
        }

        private class DisplayNameComparer : IComparer<PackageInfo>
        {
            public int Compare(PackageInfo x, PackageInfo y)
            {
                return string.Compare(x.DisplayName, y.DisplayName, true);
            }
        }
    }
}
