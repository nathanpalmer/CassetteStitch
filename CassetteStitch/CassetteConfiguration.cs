using System;
using System.Collections.Generic;
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
            bundles.Add<ScriptBundle>("Scripts/internal", b => b.Processor = new ScriptPipeline().InsertBefore<SortAssetsByDependency>(new StitchProcessor()));

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
}