using System.Diagnostics;

namespace SynchroStats;

public sealed class ProjectHandler : IProjectHandler
{
    public void RunProjects(IEnumerable<IProject> projectsToRun, IHandAnalyzerOutputStream outputStream)
    {
        var collection = new List<IProject>(projectsToRun);

        outputStream.Write($"{nameof(ProjectHandler)}: running {collection.Count:N0} project(s).");

        int i = 1;
        foreach(var project in collection)
        {
            outputStream.Write($"{nameof(ProjectHandler)}: running project #{i:N0} ({project.ProjectName}).");
            var stopwatch = Stopwatch.StartNew();
            project.Run(outputStream);
            stopwatch.Stop();
            outputStream.Write($"{nameof(ProjectHandler)}: finished project #{i:N0} ({project.ProjectName}). {stopwatch.Elapsed.TotalMilliseconds:N3} ms.");
            i++;
        }

        outputStream.Write($"{nameof(ProjectHandler)}: finished running project(s).");
    }
}

