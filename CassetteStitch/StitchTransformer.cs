using System;
using System.IO;
using Cassette;

namespace CassetteStitch
{
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
}