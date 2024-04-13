using System;
using System.Collections.Generic;
using System.Text;

namespace Biodiversity.Util.Assetloading;
[AttributeUsage(AttributeTargets.Property)]
internal class LoadFromBundleAttribute(string bundleFile) : Attribute {
    public string BundleFile { get; private set; } = bundleFile;
}
