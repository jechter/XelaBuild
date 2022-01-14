using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Build.Execution;
using XelaBuild;
using XelaBuild.Core;
using XelaBuild.Core.Helpers;
public class ProjectAnalyzer
{
    static public void Analyze(string projectPath)
    {
        var clock = Stopwatch.StartNew();

        var provider = ProjectsProvider.FromList(new[] { projectPath }, "/Users/jechter/unity-hg/BenchBuild/bench/.vs/LibRoot/xelabuild");

        Environment.SetEnvironmentVariable("XelaBuildCacheDir", provider.BuildFolder);

        var inputs = new List<string>();
        var outputs = new List<string>();

        using var builder = new Builder(provider);
        var group = builder.LoadProjectGroup(ConfigurationHelper.Release());

        Console.WriteLine($"loading took {clock.ElapsedMilliseconds}");
        clock.Restart();

        var result = builder.RunRootOnly(@group, "ResolveAssemblyReferences", "GetTargetPath");
        foreach (var r in result)
        {
            foreach (var t in r.Value.ResultsByTarget)
            {
                foreach (var i in t.Value.Items)
                    (t.Key == "GetTargetPath" ? outputs : inputs).Add(i.ItemSpec);
            }
            
            var instance = r.Key.ProjectInstance;
            foreach (var item in instance.Items)
            {
                switch (item.ItemType)
                {
                    case "Compile":
                    case "Analyzer":
                        inputs.Add(item.EvaluatedInclude);
                        break;
                }
            }
        }

        Console.WriteLine($"Build took {clock.ElapsedMilliseconds}");
        clock.Restart();

        foreach (var input in inputs)
            Console.WriteLine($"input {input}");
        
        foreach (var output in outputs)
            Console.WriteLine($"output {output}");
    }
}
