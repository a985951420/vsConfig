
using System.Text;
using System.Collections.Generic;
using System;

internal class Program
{
    static HashSet<string> files = new HashSet<string>();
    static string[] suffix = new string[] { ".csproj" };

    private static void Main(string[] args)
    {
        var vsFile = @"";
        var arg = args;
        if (arg.Length <= 0)
        {
            Console.WriteLine("请传入VS项目 sln 文件夹路径！");
            return;
        }
        vsFile = args[0];
        if (!Directory.Exists(vsFile))
        {
            Console.WriteLine("不存在当前文件夹！");
            return;
        }
        vsFile = vsFile.Replace(@"\", "/");
        Console.WriteLine("正在生成配置文件！请稍后。。。");
        listDirectory(vsFile);
        var allConfig = string.Empty;
        var allTask = string.Empty;
        var list = files.ToList();
        allConfig = Launch(vsFile, list);
        allTask = Task(vsFile, list);
        CreateConfig(vsFile, "launch.json", allConfig);
        Console.WriteLine($"{vsFile}  launch.json 配置生成完成！");
        CreateConfig(vsFile, "tasks.json", allTask);
        Console.WriteLine($"{vsFile} task.json 配置生成完成！");
    }
    /// <summary>
    /// 创建配置
    /// </summary>
    /// <param name="baseUrl">基础地址</param>
    /// <param name="name">文件名称</param>
    /// <param name="config">配置信息</param>
    static void CreateConfig(string baseUrl, string name, string config)
    {
        string fileName = $"{baseUrl}\\{name}";
        FileInfo fi = new FileInfo(fileName);
        try
        {
            // Check if file already exists. If yes, delete it.     
            if (fi.Exists)
            {
                Console.WriteLine("已存在当前文件！");
                return;
            }

            // Create a new file     
            using (FileStream fs = fi.Create())
            {
                Byte[] txt = new UTF8Encoding(true).GetBytes(config);
                fs.Write(txt, 0, txt.Length);
            }
        }
        catch (Exception Ex)
        {
            Console.WriteLine(Ex.ToString());
        }
    }
    /// <summary>
    /// 启动配置
    /// </summary>
    /// <param name="name"></param>
    /// <param name="fileUrl"></param>
    /// <returns></returns>
    static string Launch(string baseUrl, List<string> fileUrl)
    {
        var config = string.Empty;
        foreach (var item in fileUrl)
        {
            var nameext = Path.GetFileName(item);
            var name = Path.GetFileNameWithoutExtension(item);
            var r = item.Replace(baseUrl, string.Empty);
            var allUrl = r.Replace(nameext, string.Empty);
            var pconfig = @"
                    {
                        // Use IntelliSense to find out which attributes exist for C# debugging
                        // Use hover for the description of the existing attributes
                        // For further information visit https://github.com/OmniSharp/omnisharp-vscode/blob/master/debugger-launchjson.md
                        ""name"": ""{0}"",
                        ""type"": ""coreclr"",
                        ""request"": ""launch"",
                        ""preLaunchTask"": ""{0}.build"",
                        // If you have changed target frameworks, make sure to update the program path.
                        ""program"": ""${workspaceFolder}{1}bin/Debug/netcoreapp3.1/{0}.dll"",
                        ""args"": [],
                        ""cwd"": ""${workspaceFolder}/{1}"",
                        ""stopAtEntry"": false,
                        // Enable launching a web browser when ASP.NET Core starts. For more information: https://aka.ms/VSCode-CS-LaunchJson-WebBrowser
                        ""serverReadyAction"": {
                            ""action"": ""openExternally"",
                            ""pattern"": ""\\\\bNow listening on:\\\\s+(https?://\\\\S+)""
                        },
                        ""env"": {
                            ""ASPNETCORE_ENVIRONMENT"": ""Development""
                        },
                        ""sourceFileMap"": {
                            ""/Views"": ""${workspaceFolder}/Views""
                        }
                    }
        ";
            pconfig = pconfig.Replace("{0}", name);
            pconfig = pconfig.Replace("{1}", allUrl);
            pconfig += ",";
            config += pconfig;
        }
        config = config.TrimEnd(',');
        var launchConfig = @"
        {
            // 使用 IntelliSense 了解相关属性。 
            // 悬停以查看现有属性的描述。
            // 欲了解更多信息，请访问: https://go.microsoft.com/fwlink/?linkid=830387
            ""version"": ""0.2.0"",
            ""configurations"": [
                {config}
                ,{
                    ""name"": "".NET Core Attach"",
                    ""type"": ""coreclr"",
                    ""request"": ""attach""
                }
            ]
        }";
        return launchConfig.Replace("{config}", config);
    }
    /// <summary>
    /// 任务
    /// </summary>
    /// <returns></returns>
    static string Task(string baseUrl, List<string> fileUrl)
    {
        var config = string.Empty;
        foreach (var item in fileUrl)
        {
            var nameext = Path.GetFileName(item);
            var name = Path.GetFileNameWithoutExtension(item);
            var r = item.Replace(baseUrl, string.Empty);
            var allUrl = r.Replace(nameext, string.Empty);
            var pconfig = (@"
            {
                ""label"": ""{0}.build"",
                ""command"": ""dotnet"",
                ""type"": ""process"",
                ""args"": [
                    ""build"",
                    ""${workspaceFolder}{1}{0}.csproj"",
                    ""/property:GenerateFullPaths=true"",
                    ""/consoleloggerparameters:NoSummary""
                ],
                ""problemMatcher"": ""$msCompile""
            },
            {
                ""label"": ""{0}.publish"",
                ""command"": ""dotnet"",
                ""type"": ""process"",
                ""args"": [
                    ""publish"",
                    ""${workspaceFolder}{1}{0}.csproj"",
                    ""/property:GenerateFullPaths=true"",
                    ""/consoleloggerparameters:NoSummary""
                ],
                ""problemMatcher"": ""$msCompile""
            },
            {
                ""label"": ""{0}.watch"",
                ""command"": ""dotnet"",
                ""type"": ""process"",
                ""args"": [
                    ""watch"",
                    ""run"",
                    ""--project"",
                    ""${workspaceFolder}{1}{0}.csproj""
                ],
                ""problemMatcher"": ""$msCompile""
            }
        ");
            pconfig = pconfig.Replace("{0}", name).Replace("{1}", allUrl);
            pconfig += ",";
            config += pconfig;
        }
        config = config.TrimEnd(',');
        var taskconfig = @"
            {
                ""version"": ""2.0.0"",
                ""tasks"": [
                    {config}
                ]
            }
        ";
        return taskconfig.Replace("{config}", config);
    }
    /// <summary>
    /// 列出path路径对应的文件夹中的子文件夹和文件
    /// 然后再递归列出子文件夹内的文件和文件夹
    /// </summary>
    /// <param name="path">需要列出内容的文件夹的路径</param>
    private static void listDirectory(string path)
    {
        DirectoryInfo theFolder = new DirectoryInfo(@path);
        //遍历文件
        foreach (FileInfo NextFile in theFolder.GetFiles())
        {
            var fullName = NextFile.FullName.ToLower();
            var lastName = Path.GetFileName(fullName);
            string strExt = System.IO.Path.GetExtension(lastName);
            if (suffix.Contains(strExt))
            {
                if (!files.Any(s => s == NextFile.FullName))
                    files.Add(NextFile.FullName.Replace("\\", "/"));
            }
        }

        //遍历文件夹
        foreach (DirectoryInfo NextFolder in theFolder.GetDirectories())
        {
            listDirectory(NextFolder.FullName);
        }
    }
}