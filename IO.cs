using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Markdig;
using Markdig.Parsers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Scriban;
using YamlDotNet.Serialization;

namespace Pigmeat.Core
{
    class IO
    {
        static string release = typeof(Program).GetTypeInfo().Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
        public static JObject GetYamlObject(string YamlString)
        {
            return JObject.Parse(JsonConvert.SerializeObject(new Deserializer().Deserialize(new StringReader(YamlString)), Formatting.None));
        }
        public static string GetGlobal()
        {
            if(File.Exists("./_global.yml"))
            {
                return GetYamlObject(File.ReadAllText("./_global.yml")).ToString(Formatting.None);
            }
            else
            {
                return File.ReadAllText("./_global.json");
            }
        }
        public static JObject GetPigmeat()
        {
            return JObject.Parse(JsonConvert.SerializeObject(new { version = release, time = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") }));
        }
        public static void AppendEntry(string Collection, JObject Entry)
        {
            // Get Collection data, deserialize entries into List of JObjects
            JObject CollectionObject = JObject.Parse(File.ReadAllText("./_" + Collection + "/collection.json"));
            List<JObject> Entries = JsonConvert.DeserializeObject<List<JObject>>(CollectionObject["entries"].ToString(Formatting.Indented));
            Entries.Add(Entry); // Add current entry to List

            // Sort by date, then title
            Entries.Sort((y, x) => x["date"].ToString().CompareTo(y["date"].ToString()));
            Entries.Sort((y, x) => x["title"].ToString().CompareTo(y["title"].ToString()));

            var DeserializedCollection = JsonConvert.DeserializeObject<Dictionary<string, object>>(CollectionObject.ToString(Formatting.Indented));
            DeserializedCollection["entries"] = Entries.ToArray(); // Add List into JSON

            File.WriteAllText("./_" + Collection + "/collection.json", JsonConvert.SerializeObject(DeserializedCollection, Formatting.Indented));
        }
        public static void CleanCollections()
        {
            foreach(var directory in Directory.GetDirectories("./", "_*", SearchOption.TopDirectoryOnly))
            {
                string Collection = directory.Substring(3);
                // Get Collection data, deserialize entries into List of JObjects
                JObject CollectionObject = JObject.Parse(File.ReadAllText("./_" + Collection + "/collection.json"));
                object[] Entries = new object[0];

                var DeserializedCollection = JsonConvert.DeserializeObject<Dictionary<string, object>>(CollectionObject.ToString(Formatting.Indented));
                DeserializedCollection["entries"] = new int[] { }; // Add empty List into JSON

                File.WriteAllText("./_" + Collection + "/collection.json", JsonConvert.SerializeObject(DeserializedCollection, Formatting.Indented));
            }
        }
        public static JObject GetCollections()
        {
            Dictionary<string, JObject> Collections = new Dictionary<string, JObject>();
            foreach(var directory in Directory.GetDirectories("./", "_*", SearchOption.TopDirectoryOnly))
            {
                Collections.Add(directory.Substring(3), JObject.Parse(File.ReadAllText(directory + "/collection.json")));
            }
            return JObject.Parse(JsonConvert.SerializeObject(Collections));
        }

        // Render with Scriban & Markdig
        public static string RenderRaw(JObject PageObject)
        {
            string PageContents = PageObject["content"].ToString();

            // Get outside data
            JObject Global = JObject.Parse(GetGlobal());
            Global.Merge(JObject.Parse(GetCollections().ToString(Formatting.None)), new JsonMergeSettings { MergeArrayHandling = MergeArrayHandling.Union });
            JObject Pigmeat = GetPigmeat();

            // Parse for includes
            //PageContents = Include.Parse(PageContents);
            PageContents = Include.Parse(PageContents, PageObject);

            var template = Template.ParseLiquid(PageContents);
            PageContents = template.Render(new { page = PageObject, global = Global, pigmeat = Pigmeat }); // Render with Scriban

            // Turn Markdown into HTML
            var builder = new MarkdownPipelineBuilder().UseAdvancedExtensions();
            builder.BlockParsers.TryRemove<IndentedCodeBlockParser>();
            var pipeline = builder.Build();
            PageContents = Markdown.ToHtml(PageContents, pipeline);
            return PageContents;
        }
        /*
        public static string RenderRaw(string PageContents)
        {
            // Get outside data
            JObject Global = JObject.Parse(GetGlobal());
            Global.Merge(JObject.Parse(GetCollections().ToString(Formatting.None)), new JsonMergeSettings { MergeArrayHandling = MergeArrayHandling.Union });
            JObject Pigmeat = GetPigmeat();

            // Parse for includes
            PageContents = Include.Parse(PageContents);

            var template = Template.ParseLiquid(PageContents);
            PageContents = template.Render(new { global = Global, pigmeat = Pigmeat }); // Render with Scriban

            // Turn Markdown into HTML
            var builder = new MarkdownPipelineBuilder().UseAdvancedExtensions();
            builder.BlockParsers.TryRemove<IndentedCodeBlockParser>();
            var pipeline = builder.Build();
            PageContents = Markdown.ToHtml(PageContents, pipeline);
            return PageContents;
        }
        */
        // Take layout, place Markdig-parsed content in layout, evaluate includes, render with Scriban
        public static void RenderPage(JObject PageObject, string Collection)
        {
            string PageContents = PageObject["content"].ToString();
            
            // Get outside data
            JObject Global = JObject.Parse(GetGlobal());
            Global.Merge(JObject.Parse(GetCollections().ToString(Formatting.None)), new JsonMergeSettings { MergeArrayHandling = MergeArrayHandling.Union });
            JObject Pigmeat = GetPigmeat();

            // If a page has a layout, include it
            if(PageObject.ContainsKey("layout"))
            {
                PageContents = File.ReadAllText("./layouts/" + PageObject["layout"].ToString() + ".html").Replace("{{ content }}", PageContents);
            }

            // Parse for includes
            PageContents = Include.Parse(PageContents, PageObject);

            var template = Template.ParseLiquid(PageContents);
            PageContents = template.Render(new { page = PageObject, global = Global, pigmeat = Pigmeat }); // Render with Scriban

            // Turn Markdown into HTML
            var builder = new MarkdownPipelineBuilder().UseAdvancedExtensions();
            builder.BlockParsers.TryRemove<IndentedCodeBlockParser>();
            var pipeline = builder.Build();
            PageContents = Markdown.ToHtml(PageContents, pipeline);

            Directory.CreateDirectory(Path.GetDirectoryName("./output/" + PageObject["url"].ToString()));
            File.WriteAllText("./output/" + PageObject["url"].ToString(), PageContents); 
            IO.AppendEntry(Collection, PageObject);
        }
    }
}