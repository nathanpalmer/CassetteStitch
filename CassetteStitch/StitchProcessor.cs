using System;
using Cassette;
using Cassette.BundleProcessing;
using Cassette.Configuration;

namespace CassetteStitch
{
    public class StitchProcessor : IBundleProcessor<Bundle>
    {
        public void Process(Bundle bundle, CassetteSettings settings)
        {
            foreach(var asset in bundle.Assets)
            {
                if (asset.SourceFile.FullPath.EndsWith(".js", StringComparison.OrdinalIgnoreCase))
                {
                    asset.AddAssetTransformer(new StitchTransformer());
                    //var result = asset.OpenStream();
                    //using (var input = new StreamReader(result))
                    //{
                    //    var x = input.ReadToEnd();
                    //}
                }
            }
        }
    }
}