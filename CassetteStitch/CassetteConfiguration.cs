using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Cassette;
using Cassette.BundleProcessing;
using Cassette.Configuration;
using Cassette.Scripts;
using Cassette.Stylesheets;
using Cassette.Utilities;

namespace CassetteStitch
{
    /// <summary>
    /// Configures the Cassette asset modules for the web application.
    /// </summary>
    public class CassetteConfiguration : ICassetteConfiguration
    {
        public void Configure(BundleCollection bundles, CassetteSettings settings)
        {
            // TODO: Configure your bundles here...
            // Please read http://getcassette.net/documentation/configuration

            // This default configuration treats each file as a separate 'bundle'.
            // In production the content will be minified, but the files are not combined.
            // So you probably want to tweak these defaults!
            bundles.AddPerIndividualFile<StylesheetBundle>("Content");
            //bundles.AddPerIndividualFile<ScriptBundle>("Scripts", null, x =>
            //    {
            //        //x.Processor = new CommonJSScriptPipeline();
            //    });
            bundles.Add<ScriptBundle>("Scripts/internal", null, x =>
                {
                    x.Processor = new CommonJSScriptPipeline();
                });


            settings.IsDebuggingEnabled = true;
            //settings.UrlModifier = new FileExtensionUrlModifier();

            // To combine files, try something like this instead:
            //   bundles.Add<StylesheetBundle>("Content");
            // In production mode, all of ~/Content will be combined into a single bundle.

            // If you want a bundle per folder, try this:
            //   bundles.AddPerSubDirectory<ScriptBundle>("Scripts");
            // Each immediate sub-directory of ~/Scripts will be combined into its own bundle.
            // This is useful when there are lots of scripts for different areas of the website.

            // *** TOP TIP: Delete all ".min.js" files now ***
            // Cassette minifies scripts for you. So those files are never used.
        }
    }

    public class CommonJSScriptPipeline : ScriptPipeline
    {
        public CommonJSScriptPipeline()
        {
            CompileCoffeeScript = false;
        }

        protected override IEnumerable<IBundleProcessor<ScriptBundle>> CreatePipeline(ScriptBundle bundle, CassetteSettings settings)
        {
            yield return new AssignScriptRenderer();
            ////if (bundle.IsFromCache) yield break;

            //yield return new ParseJavaScriptReferences();
            //if (CompileCoffeeScript)
            //{
            //    yield return new ParseCoffeeScriptReferences();
            //    yield return new CompileCoffeeScript(CoffeeScriptCompiler);
            //}
            yield return new StitchProcessor();
            //yield return new SortAssetsByDependency();
            //if (!settings.IsDebuggingEnabled)
            //{
            //    yield return new ConcatenateAssets();
            //    yield return new MinifyAssets(Minifier);
            //}
        }
    }

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

    public class StitchTransformer : IAssetTransformer
    {
        public Func<Stream> Transform(Func<Stream> openSourceStream, IAsset asset)
        {
            const string Identifier = "require";

            return delegate
                {
                    using (var input = new StreamReader(openSourceStream()))
                    {
                        var result = @"
(function(/*! Stitch !*/) {
  if (!this." + Identifier + @") {
    var modules = {}, cache = {}, require = function(name, root) {
      var module = cache[name], path = expand(root, name), fn;
      if (module) {
        return module;
      } else if (fn = modules[path] || modules[path = expand(path, './index')]) {
        module = {id: name, exports: {}};
        try {
          cache[name] = module.exports;
          fn(module.exports, function(name) {
            return require(name, dirname(path));
          }, module);
          return cache[name] = module.exports;
        } catch (err) {
          delete cache[name];
          throw err;
        }
      } else {
        throw 'module \'' + name + '\' not found';
      }
    }, expand = function(root, name) {
      var results = [], parts, part;
      if (/^\.\.?(\/|$)/.test(name)) {
        parts = [root, name].join('/').split('/');
      } else {
        parts = name.split('/');
      }
      for (var i = 0, length = parts.length; i < length; i++) {
        part = parts[i];
        if (part == '..') {
          results.pop();
        } else if (part != '.' && part != '') {
          results.push(part);
        }
      }
      return results.join('/');
    }, dirname = function(path) {
      return path.split('/').slice(0, -1).join('/');
    };
    this." + Identifier + @" = function(name) {
      return require(name, '');
    }
    this." + Identifier + @".define = function(bundle) {
      for (var key in bundle)
        modules[key] = bundle[key];
    };
  }
  return this." + Identifier + @".define;
}).call(this)({
" + input.ReadToEnd() + @"
});";

                        return AsStream(result);
                    }
                };
        }

        static Stream AsStream(string s)
        {
            var source = new MemoryStream();
            var writer = new StreamWriter(source);
            writer.Write(s);
            writer.Flush();
            source.Position = 0;
            return source;
        }
    }

    public class FileExtensionUrlModifier : IUrlModifier
    {
        public string Modify(string url)
        {
            var uri = new Uri("http://localhost/"+url);
            var name = uri.Segments[uri.Segments.Length - 1];
            var path = uri.LocalPath.Replace(name, "");
            var extension = name.LastIndexOf(".") >= 0 ? name.Substring(name.LastIndexOf(".")) : "";
                name = name.Replace(extension, "");
            var hash = uri.Query.Substring(1);
            var filename = string.Format("{1}-{0}{2}", name, hash, extension);
            var modified = string.Format("{0}{1}", path, filename);

            return modified;
        }
    }

    public class FileExtensionUrlGenerator : IUrlGenerator
    {
        public string CreateBundleUrl(Bundle bundle)
        {
            throw new NotImplementedException();
        }

        public string CreateAssetUrl(IAsset asset)
        {
            throw new NotImplementedException();
        }

        public string CreateRawFileUrl(string filename, string hash)
        {
            throw new NotImplementedException();
        }
    }
}