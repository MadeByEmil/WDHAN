using Markdig;
using Fluid;
using Newtonsoft.Json;
using System;
using System.IO;
using SharpScss;
using System.Text;
using System.Collections.Generic;
using Fluid.Values;
using Newtonsoft.Json.Linq;
using System.Globalization;
using LibGit2Sharp;
using Markdig.Parsers;
using Markdig.Extensions.AutoLinks;
using Markdig.SyntaxHighlighting;
using System.Net;
using System.IO.Compression;

namespace WDHAN
{
    class Program
    {
        public const string version = "1.1";
        static void Main(string[] args)
        {
            try
            {

                try
                {
                    if (!args[0].Equals("help", StringComparison.OrdinalIgnoreCase) && !args[1].Equals("--blank", StringComparison.OrdinalIgnoreCase))
                    {
                        try
                        {
                            Directory.SetCurrentDirectory(args[1]);
                        }
                        catch (DirectoryNotFoundException ex)
                        {
                            Console.WriteLine("The specified directory does not exist. " + args[1], ex);
                        }
                    }
                }
                catch (DirectoryNotFoundException e)
                {
                    Console.WriteLine("The specified directory does not exist. {0}", e);
                }

                if(args.Length == 0)
                {
                    printHelpMsg(args);
                }
                else if (args[0].Equals("new", StringComparison.OrdinalIgnoreCase))
                {
                    createSite(args);
                }
                else if (args[0].Equals("build", StringComparison.OrdinalIgnoreCase))
                {
                    buildSite(args, true);
                }
                else if (args[0].Equals("b", StringComparison.OrdinalIgnoreCase))
                {
                    buildSite(args, true);
                }
                else if (args[0].Equals("serve", StringComparison.OrdinalIgnoreCase))
                {
                    serveSite(args);
                }
                else if (args[0].Equals("s", StringComparison.OrdinalIgnoreCase))
                {
                    serveSite(args);
                }
                else if (args[0].Equals("clean", StringComparison.OrdinalIgnoreCase))
                {
                    cleanSite();
                }
                else if (args[0].Equals("help", StringComparison.OrdinalIgnoreCase))
                {
                    printHelpMsg(args);
                }
                else
                {
                    printHelpMsg(args);
                }
            }
            catch(IndexOutOfRangeException)
            {
                if(args.Length == 0)
                {
                    printHelpMsg(args);
                }
                else if (args[0].Equals("new", StringComparison.OrdinalIgnoreCase))
                {
                    createSite(args);
                }
                else if (args[0].Equals("build", StringComparison.OrdinalIgnoreCase))
                {
                    buildSite(args, true);
                }
                else if (args[0].Equals("b", StringComparison.OrdinalIgnoreCase))
                {
                    buildSite(args, true);
                }
                else if (args[0].Equals("serve", StringComparison.OrdinalIgnoreCase))
                {
                    serveSite(args);
                }
                else if (args[0].Equals("s", StringComparison.OrdinalIgnoreCase))
                {
                    serveSite(args);
                }
                else if (args[0].Equals("clean", StringComparison.OrdinalIgnoreCase))
                {
                    cleanSite();
                }
                else if (args[0].Equals("help", StringComparison.OrdinalIgnoreCase))
                {
                    printHelpMsg(args);
                }
                else
                {
                    printHelpMsg(args);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Environment.Exit(1);
            }
        }
        static void serveSite(string[] args)
        {
            Console.WriteLine("Serving WDHAN project … ");
            Console.WriteLine("[MESSAGE]: This feature will be implemented in version 1.5.");
            Environment.Exit(0);
        }
        static void cleanSite()
        {
            try
            {
                GlobalConfiguration siteConfig = GlobalConfiguration.getConfiguration();
                Console.WriteLine("Cleaning project directory, " + siteConfig.destination + " … ");
                System.IO.DirectoryInfo outputDir = new DirectoryInfo(siteConfig.destination);

                foreach (var file in Directory.GetFiles(siteConfig.destination, "*.*", SearchOption.AllDirectories))
                {
                    if(!siteConfig.keep_files.Contains(file))
                    {
                        Console.WriteLine("Deleting " + file.Substring(siteConfig.destination.Length + 1));
                        File.Delete(file);
                    }
                    
                }

                /*
                // May lead to deletion of files in keep_files
                foreach (DirectoryInfo dir in outputDir.EnumerateDirectories())
                {
                    Console.WriteLine("Deleting " + dir.Name);
                    dir.Delete(true); 
                }
                */

                Console.WriteLine("Cleaning temporary files, " + siteConfig.source + "/temp" + " … ");
                System.IO.DirectoryInfo tempDir = new DirectoryInfo(siteConfig.source + "/temp");

                foreach (var file in Directory.GetFiles(siteConfig.destination, "*.*", SearchOption.AllDirectories))
                {
                    Console.WriteLine("Deleting " + file.Substring(siteConfig.source.Length + "/temp".Length + 1));
                    File.Delete(file); 
                }
                    
                foreach (DirectoryInfo dir in tempDir.EnumerateDirectories())
                {
                    Console.WriteLine("Deleting " + dir.Name);
                    dir.Delete(true); 
                }
            }   
            catch(DirectoryNotFoundException)
            {
                // This is expected if either _site or temp are not found.
                /*
                Console.WriteLine("For developers:\n" + ex);
                Console.WriteLine("ERROR [DirectoryNotFoundException]: The project directory you're trying to clean cannot be found.");
                */
                Environment.Exit(1);
            }
            catch(System.Security.SecurityException ex)
            {
                Console.WriteLine("For developers:\n" + ex);
                Console.WriteLine("ERROR [System.Security.SecurityException]: You do not have write access to the project directory you're trying to clean.");
                Environment.Exit(1);
            }
            catch(ArgumentException ex)
            {
                Console.WriteLine("For developers:\n" + ex);
                Console.WriteLine("ERROR [ArgumentException]: The project directory you're trying to clean cannot be found.");
                Environment.Exit(1);
            }
            catch(UnauthorizedAccessException ex)
            {
                Console.WriteLine("For developers:\n" + ex);
                Console.WriteLine("ERROR [UnauthorizedAccessException]: You do not have write access to the project directory you're trying to clean.");
                Environment.Exit(1);
            }
            catch(IOException ex)
            {
                Console.WriteLine("For developers:\n" + ex);
                Console.WriteLine("ERROR [IOException]: An I/O error has occured. Ensure the project directory is not read-only, and no other programs are using files in the directory.");
                Environment.Exit(1);
            }
            catch(Exception ex)
            {
                Console.WriteLine("For developers:\n" + ex);
                Console.WriteLine("ERROR [Exception]: Cannot clean project files. Please ensure the directory you're trying to clean exists and can be accessed.");
                Environment.Exit(1);
            }
        }
        static void createSite(string[] args)
        {
            try 
            {
                // Create blank site scaffolding
                if (args[1].Equals("--blank", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        Console.WriteLine("Creating project files … ");

                        var defaultCollections = new List<string>() { "posts" };

                        Console.WriteLine("Creating /_plugins");
                        Directory.CreateDirectory("./_plugins");
                        Console.WriteLine("Creating /_includes");
                        Directory.CreateDirectory("./_includes");
                        Console.WriteLine("Creating /_layouts");
                        Directory.CreateDirectory("./_layouts");
                        Console.WriteLine("Creating /_sass");
                        Directory.CreateDirectory("./_sass");
                        Console.WriteLine("Creating _/posts");
                        Directory.CreateDirectory("./_posts");
                        Console.WriteLine("Creating /_drafts");
                        Directory.CreateDirectory("./_drafts");
                        Console.WriteLine("Creating /_data");
                        Directory.CreateDirectory("./_data");
                        Console.WriteLine("Creating _config.json");

                        GlobalConfiguration defaultConfig = new GlobalConfiguration
                        {
                            source = ".",
                            destination = @"./_site",
                            collections_dir = ".",
                            plugins_dir = "_plugins",
                            layouts_dir = "_layouts",
                            data_dir = "_data",
                            includes_dir = "_includes",
                            sass_dir = "_sass",
                            collections = defaultCollections,
                            include = new List<string> { ".htaccess" },
                            exclude = new List<string> { },
                            keep_files = new List<string> { ".git", ".svn" },
                            culture = "en-US",
                            markdown_ext = "markdown,mkdown,mkdn,mkd,md",
                            //show_drafts = false, //Intentionally blank, generates null field on output
                            whitelist = new List<string> { },
                            plugins = new List<string> { },
                            excerpt_separator = @"\n\n",
                            baseurl = "",
                            show_dir_listing = false,
                            permalink = "date",
                            url = "",
                            time = DateTime.UtcNow,
                            user = new Dictionary<string, object> { { "author", "WDHAN User" }, { "title", "WDHAN Project" } }
                        };
                        string defaultConfigSerialized = JsonConvert.SerializeObject(defaultConfig, Formatting.Indented);
                        using (FileStream fs = File.Create("./_config.json"))
                        {
                            fs.Write(Encoding.UTF8.GetBytes(defaultConfigSerialized), 0, Encoding.UTF8.GetBytes(defaultConfigSerialized).Length);
                        }

                        Console.WriteLine("Creating _posts/_config.json");
                        var postsVariables = new Dictionary<string, object>();
                        postsVariables.Add("output", true);
                        string postsConfigSerialized = JsonConvert.SerializeObject(postsVariables, Formatting.Indented);
                        using (FileStream fs = File.Create("./_posts" + "/_config.json"))
                        {
                            fs.Write(Encoding.UTF8.GetBytes(postsConfigSerialized), 0, Encoding.UTF8.GetBytes(postsConfigSerialized).Length);
                        }
                    }
                    catch (ArgumentNullException ex)
                    {
                        Console.WriteLine("For developers:\n" + ex);
                        Console.WriteLine("ERROR [ArgumentNullException]: Issue with generating default configuration file in WDHAN project.\nPlease report this at https://github.com/MadeByEmil/WDHAN/issues/new?assignees=limeschool&labels=bug&template=bug_report.md&title=%5BBUG%5D+-+");
                    }
                    catch (ArgumentException ex)
                    {
                        Console.WriteLine("For developers:\n" + ex);
                        Console.WriteLine("ERROR [ArgumentException]: Issue with generating default configuration file in WDHAN project.\nPlease report this at https://github.com/MadeByEmil/WDHAN/issues/new?assignees=limeschool&labels=bug&template=bug_report.md&title=%5BBUG%5D+-+");
                    }
                    catch (DirectoryNotFoundException ex)
                    {
                        Console.WriteLine("For developers:\n" + ex);
                        Console.WriteLine("ERROR [DirectoryNotFoundException]: The path to your WDHAN project is inaccessible. Verify it still exists.");
                    }
                    catch (UnauthorizedAccessException ex)
                    {
                        Console.WriteLine("For developers:\n" + ex);
                        Console.WriteLine("ERROR [UnauthorizedAccessException]: Access to WDHAN files is denied. Try changing file permissions, or run with higher privileges.");
                        Environment.Exit(1);
                    }
                    catch (PathTooLongException ex)
                    {
                        Console.WriteLine("For developers:\n" + ex);
                        Console.WriteLine("ERROR [PathTooLongException]: The path to your WDHAN project is too long for your file system to handle.");
                        Environment.Exit(1);
                    }
                    catch (IOException ex)
                    {
                        Console.WriteLine("For developers:\n" + ex);
                        Console.WriteLine("ERROR [IOException]: A problem has occured with writing data to your system. Verify your OS and data storage device are working correctly.");
                        Environment.Exit(1);
                    }
                    catch (NotSupportedException ex)
                    {
                        Console.WriteLine("For developers:\n" + ex);
                        Console.WriteLine("ERROR [NotSupportedException]: WDHAN cannot create your project's output directory. Verify your OS and data storage device are working correctly, and you have proper permissions.");
                        Environment.Exit(1);
                    }
                }
            }    
            catch (IndexOutOfRangeException)
            {
                // Create site with default theme
                Console.WriteLine("Are you sure you wish to overwrite all pre-existing data and create a new project? (y/n)");
                var yesOrNo = Console.ReadLine(); 
                if(yesOrNo.Trim().Equals("y", StringComparison.OrdinalIgnoreCase))
                {
                    using (var client = new WebClient())
                    {
                        client.DownloadFile("https://github.com/MadeByEmil/wdhan-basic/archive/master.zip", "./basic.zip");
                        ZipFile.ExtractToDirectory("./basic.zip", "./");
                        File.Delete("./basic.zip");
                        foreach(var directory in Directory.GetDirectories("./wdhan-basic-master", "*", SearchOption.AllDirectories))
                        {
                            Directory.CreateDirectory(directory.Replace("./wdhan-basic-master", "./"));
                        }
                        foreach(var file in Directory.GetFiles("./wdhan-basic-master", "*", SearchOption.AllDirectories))
                        {
                            File.Copy(file, file.Replace("./wdhan-basic-master", "./"), true);
                        }
                        Directory.Delete("./wdhan-basic-master", true);
                    }
                }
                else
                {
                    Environment.Exit(0);
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("For developers:\n" + ex);
                Console.WriteLine("ERROR [Exception]: Cannot create project files. Please ensure the directory you're trying to create is supported in your file system.");
                Environment.Exit(1);
            }
        }
        public static void buildSite(string[] args, Boolean firstTime)
        {
            if(firstTime)
            {
                Console.WriteLine("Building project files … ");
            }
            Data.generateDataIndex();
            GlobalConfiguration.includeTime();
            GlobalConfiguration siteConfig = GlobalConfiguration.getConfiguration();
            Directory.CreateDirectory(siteConfig.destination);
            Directory.CreateDirectory(siteConfig.source + "/temp");
            foreach(var file in Directory.GetFiles(siteConfig.source, "*.*", SearchOption.AllDirectories))
            {
                if(!Path.GetDirectoryName(file).StartsWith("./_"))
                {
                    //string fileDest = Path.GetDirectoryName(file) + "/" + siteConfig.destination + "/" + Path.GetFileName(file);


                    Boolean first = false;
                    Boolean second = false;
                    string fileContents = "";

                    try
                    {
                        if(File.ReadAllLines(file)[0].Equals("---", StringComparison.OrdinalIgnoreCase))
                        {
                            foreach(var line in File.ReadAllLines(file))
                            {
                                if(line.Equals("---", StringComparison.OrdinalIgnoreCase) && !first)
                                {
                                    first = true;
                                    continue;
                                }
                                if(line.Equals("---", StringComparison.OrdinalIgnoreCase) && first)
                                {
                                    second = true;
                                    continue;
                                }
                                if(first && second)
                                {
                                    fileContents += line;
                                }
                            }
                            if(first && second)
                            {
                                var page = new Page { path = file, content = parseDocument(file), frontmatter = Page.parseFrontMatter(file) };
                                string fileDest = Permalink.GetPermalink(page).parsePagePermalink(Page.getDefinedPage(page));
                                if(fileDest.Substring(0, 1).Equals("/", StringComparison.OrdinalIgnoreCase))
                                {
                                    fileDest = "./" + siteConfig.destination + fileDest;
                                }

                                if(Path.GetExtension(file).Equals(".scss", StringComparison.OrdinalIgnoreCase) || Path.GetExtension(file).Equals(".sass", StringComparison.OrdinalIgnoreCase))
                                {
                                    try
                                    {
                                        var result = Scss.ConvertToCss(Sass.getSassContents(file));
                                        fileDest = siteConfig.destination + "/" + Path.GetDirectoryName(file);
                                        Directory.CreateDirectory(fileDest);
                                        using (FileStream fs = File.Create(fileDest + "/" + Path.GetFileNameWithoutExtension(file) + ".css"))
                                        {
                                            fs.Write(Encoding.UTF8.GetBytes(result.Css), 0, Encoding.UTF8.GetBytes(result.Css).Length);
                                            if(!firstTime)
                                            {
                                                Console.WriteLine(file + " → " + fileDest);
                                            }
                                        }
                                    }
                                    catch(SharpScss.ScssException ex)
                                    {
                                        Console.WriteLine(ex.File);
                                        Console.WriteLine(ex);
                                    }
                                }
                                else if(GlobalConfiguration.isMarkdown(Path.GetExtension(file).Substring(1)))
                                {
                                    fileDest = siteConfig.destination + "/" + Path.GetDirectoryName(file);
                                    var result = parseDocument(file);
                                    var pageObject = Page.getDefinedPage(new Page { frontmatter = WDHANFile.parseFrontMatter(file), content = result, path = file });
                                    var builder = new MarkdownPipelineBuilder().UseAdvancedExtensions().UseSyntaxHighlighting();
                                    builder.BlockParsers.TryRemove<IndentedCodeBlockParser>();
                                    var pipeline = builder.Build();
                                    builder.Extensions.Remove(pipeline.Extensions.Find<AutoLinkExtension>());
                                    try
                                    {
                                        result = Markdown.ToHtml(result, pipeline);
                                        pageObject.content = result;
                                        using (FileStream fs = File.Create(fileDest + "/" + Path.GetFileNameWithoutExtension(file) + ".html"))
                                        {
                                            fs.Write(Encoding.UTF8.GetBytes(result), 0, Encoding.UTF8.GetBytes(result).Length);
                                            if(!firstTime)
                                            {
                                                Console.WriteLine(file + " → " + fileDest);
                                            }
                                        }
                                    }
                                    catch(ArgumentNullException ex)
                                    {
                                        Console.WriteLine("For developers:\n" + ex);
                                        result = "ERROR [ArgumentNullException]: Pagewrite failed. Page contents are either corrupted or blank.";
                                        Environment.Exit(1);
                                    }
                                }
                                else
                                {
                                    fileDest = siteConfig.destination + "/" + Path.GetDirectoryName(file);
                                    if(!Path.GetDirectoryName(fileDest).Equals("", StringComparison.OrdinalIgnoreCase))
                                    {
                                        Directory.CreateDirectory(fileDest);
                                    }
                                    using (FileStream fs = File.Create(fileDest + "/" + Path.GetFileName(file)))
                                    {
                                        fs.Write(Encoding.UTF8.GetBytes(parseDocument(file)), 0, Encoding.UTF8.GetBytes(parseDocument(file)).Length);
                                        if(!firstTime)
                                        {
                                            Console.WriteLine(file + " → " + fileDest);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                // Copy file over (if not excluded)
                                string fileDest = Path.GetDirectoryName(file) + "/" + siteConfig.destination + "/" + Path.GetFileName(file);
                                if(!Path.GetDirectoryName(file).Equals("", StringComparison.OrdinalIgnoreCase))
                                {
                                    if(!siteConfig.exclude.Contains(file) && !siteConfig.exclude.Contains(Path.GetDirectoryName(file)))
                                    {
                                        File.Copy(file, fileDest, true);
                                    }
                                }
                                else
                                {
                                    if(!siteConfig.exclude.Contains(file))
                                    {
                                        File.Copy(file, fileDest, true);
                                    }
                                }
                            }
                        }
                        else
                        {
                            try
                            {
                                // Copy file over (if included)
                                string fileDest = Path.GetDirectoryName(file) + "/" + siteConfig.destination;
                                if(siteConfig.include.Contains(file) || siteConfig.include.Contains(Path.GetDirectoryName(file)))
                                {
                                    File.Copy(file, fileDest, true);
                                    Console.WriteLine(file + " → " + fileDest);
                                }
                            }
                            catch(NullReferenceException)
                            {

                            }
                        }
                    }
                    catch(IndexOutOfRangeException)
                    {
                        // Copy file over (if not excluded)
                        Directory.CreateDirectory(Path.GetDirectoryName(file) + "/" + siteConfig.destination + "/" + Path.GetDirectoryName(file));
                        string fileDest = Path.GetDirectoryName(file) + "/" + siteConfig.destination + "/" + Path.GetFileName(file);
                        if(!Path.GetDirectoryName(file).Equals("", StringComparison.OrdinalIgnoreCase))
                        {
                            if(!siteConfig.exclude.Contains(file) && !siteConfig.exclude.Contains(Path.GetDirectoryName(file)))
                            {
                                File.Copy(file, fileDest, true);
                            }
                        }
                        else
                        {
                            if(!siteConfig.exclude.Contains(file))
                            {
                                File.Copy(file, fileDest, true);
                            }
                        }
                    }
                }
            }

            // Handle _layout, _include, _data, _collection
            buildCollection(args, firstTime);

            if(firstTime)
            {
                GlobalConfiguration.includeTags(); // The process to include tags requires two passes at the build process.
                buildSite(args, false);
            }
        }

        static void buildCollection(string[] args, Boolean firstTime)
        {
            var siteConfig = GlobalConfiguration.getConfiguration();

            if(File.Exists(siteConfig.source + "/index.html"))
            {
                var siteIndex = siteConfig.source + "/index.html";
                var index = parseDocument(siteIndex);

                var builder = new MarkdownPipelineBuilder().UseAdvancedExtensions();
                builder.BlockParsers.TryRemove<IndentedCodeBlockParser>();
                var pipeline = builder.Build();
                builder.Extensions.Remove(pipeline.Extensions.Find<AutoLinkExtension>());
                try
                {
                    index = Markdown.ToHtml(index, pipeline);
                    Directory.CreateDirectory(siteConfig.destination);
                    using (FileStream fs = File.Create(siteConfig.destination + "/index.html"))
                    {
                        fs.Write(Encoding.UTF8.GetBytes(index), 0, Encoding.UTF8.GetBytes(index).Length);
                    }
                }
                catch(ArgumentNullException ex)
                {
                    Console.WriteLine("For developers:\n" + ex);
                    index = "ERROR [ArgumentNullException]: Pagewrite failed. Page contents are either corrupted or blank.";
                    Environment.Exit(1);
                }
            }

            foreach(var collection in siteConfig.collections)
            {
                Post.generateEntries();
                Data.generateDataIndex();
                if(JObject.Parse(File.ReadAllText(siteConfig.source + "/_" + collection + "/_config.json"))["output"].ToString().Equals("true", StringComparison.OrdinalIgnoreCase))
                {
                    foreach(var post in Directory.GetFiles(siteConfig.collections_dir + "/_" + collection))
                    {
                        var result = parseDocument(post);
                        var postObject = Post.getDefinedPost(new Post { frontmatter = WDHANFile.parseFrontMatter(post), content = result, path = post }, collection);
                        var postPath = Path.GetDirectoryName(siteConfig.destination + "/" + Permalink.GetPermalink(postObject).parsePostPermalink(collection, postObject));
                        Directory.CreateDirectory(postPath);
                        if(GlobalConfiguration.isMarkdown(Path.GetExtension(post).Substring(1)))
                        {
                            var builder = new MarkdownPipelineBuilder().UseAdvancedExtensions().UseSyntaxHighlighting();
                            builder.BlockParsers.TryRemove<IndentedCodeBlockParser>();
                            var pipeline = builder.Build();
                            builder.Extensions.Remove(pipeline.Extensions.Find<AutoLinkExtension>());
                            try
                            {
                                result = Markdown.ToHtml(result, pipeline);
                                postObject.content = result;
                                using (FileStream fs = File.Create(postPath + "/" + postObject.frontmatter["title"].ToString() + ".html"))
                                {
                                    fs.Write(Encoding.UTF8.GetBytes(result), 0, Encoding.UTF8.GetBytes(result).Length);
                                    if(!firstTime)
                                    {
                                        Console.WriteLine(post + " → " + postPath);
                                    }
                                }
                            }
                            catch(ArgumentNullException ex)
                            {
                                Console.WriteLine("For developers:\n" + ex);
                                result = "ERROR [ArgumentNullException]: Pagewrite failed. Page contents are either corrupted or blank.";
                                Environment.Exit(1);
                            }
                        }
                        else
                        {
                            using (FileStream fs = File.Create(postPath + "/" + Path.GetFileName(post)))
                            {
                                fs.Write(Encoding.UTF8.GetBytes(result), 0, Encoding.UTF8.GetBytes(result).Length);
                                if(!firstTime)
                                {
                                    Console.WriteLine(post + " → " + postPath);
                                }
                            }
                        }
                    }
                }
            }
            GlobalConfiguration.includeTime();
        }
        public static string parseDocument(string filePath)
        {            
            var siteConfig = GlobalConfiguration.getConfiguration();
            //var fileContents = WDHANFile.getFileContents(filePath);
            var fileContents = WDHANFile.parseRaw(filePath);
            fileContents = Include.evalInclude(filePath); // Expand includes (must happen after layouts are retreived, as layouts can have includes)

            try
            {
                var layout = Layout.getLayoutContents(Page.parseFrontMatter(filePath)["layout"].ToString(), filePath); // Get layout
                fileContents = layout.Replace("{{ content }}", fileContents); // Incorporate page into layout
            }
            catch(NullReferenceException)
            {

            }

            // Expand layouts, then parse includes

            // When a property of a JObject value is accessed, try to look into its properties
            TemplateContext.GlobalMemberAccessStrategy.Register<JObject, object>((source, name) => source[name]);

            // Convert JToken to FluidValue
            FluidValue.SetTypeMapping<JObject>(o => new ObjectValue(o));
            FluidValue.SetTypeMapping<JValue>(o => FluidValue.Create(o.Value));
            

            var siteModel = JObject.Parse(File.ReadAllText("./_config.json"));
            var dataSet = JObject.Parse(File.ReadAllText(siteConfig.source + "/temp/_data.json"));
            var pageModel = WDHANFile.parseFrontMatter(filePath);
            
            try
            {
                if(FluidTemplate.TryParse(fileContents, out var template))
                {
                    var context = new TemplateContext();
                    context.CultureInfo = new CultureInfo(siteConfig.culture);

                    siteModel.Merge(dataSet, new JsonMergeSettings { MergeArrayHandling = MergeArrayHandling.Union });
                    context.SetValue("site", siteModel);
                    context.SetValue("page", pageModel);
                    context.SetValue("wdhan", JObject.Parse("{\"version\": " + Program.version + "}"));
                    foreach(var collection in siteConfig.collections)
                    {
                        if(File.Exists(siteConfig.source + "/temp/_" + collection + "/_entries.json"))
                        {
                            var collectionModel = JObject.Parse(File.ReadAllText(siteConfig.source + "/_" + collection + "/_config.json"));
                            collectionModel.Merge(JObject.Parse(File.ReadAllText(siteConfig.source + "/temp/_" + collection + "/_entries.json")), new JsonMergeSettings { MergeArrayHandling = MergeArrayHandling.Union });
                            context.SetValue(collection, collectionModel);
                        }
                    }
                    return template.Render(context);
                }
                else
                {
                    Console.WriteLine("ERROR: Could not parse Liquid context for file " + filePath + ".");
                    Console.WriteLine("TryParse error:\n" + fileContents);
                    return fileContents;
                }
            }
            catch(ArgumentNullException ex)
            {
                Console.WriteLine("File " + filePath + " has no Liquid context to parse.\n" + ex.ToString());
                return fileContents;
            }
        }

        static void printHelpMsg(string[] args)
        {
            try
            {
                if (args[1].Equals("new", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("Creates an empty WDHAN project in the current directory.");
                }
                else if (args[1].Equals("build", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("Outputs a publishable WDHAN project.");
                }
                else if (args[1].Equals("b", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("Outputs a publishable WDHAN project.");
                }
                else if (args[1].Equals("serve", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("Rebuilds the site anytime a change is detected and hosts it.");
                }
                else if (args[1].Equals("s", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("Rebuilds the site anytime a change is detected.");
                }
                else if (args[1].Equals("clean", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("Deletes all generated files.");
                }
                else if (args[1].Equals("help", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("Prints a message outlining WDHAN's commands. A subparameter may be specified, displaying a message outlining the usage of the given parameter (e.g. 'wdhan help serve')");
                }
                else
                {
                    Console.WriteLine("Please specify a parameter (e.g. 'wdhan help new,' 'wdhan help build,' 'wdhan help serve,' 'wdhan help clean')");
                }
            }
            catch (IndexOutOfRangeException)
            {
                Console.WriteLine(
                    "WDHAN supports the following commands:\n" +
                    "   wdhan new - Creates an empty WDHAN project in the current directory.\n" +
                    "   wdhan build - Outputs a publishable WDHAN project.\n" +
                    "   wdhan b - Same as above.\n" +
                    "   wdhan serve - Rebuilds the site anytime a change is detected.\n" +
                    "   wdhan s - Same as above.\n" +
                    "   wdhan clean - Deletes all generated files.\n" +
                    "   wdhan help - Shows this message.\n" +
                    "   wdhan help <string> - Displays a message outlining the usage of a given parameter (e.g. 'wdhan help serve')"
                    );
            }
        }
    }
}
