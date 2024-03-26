namespace SynchroStats;

public interface IProjectHandler
{
    void RunProjects(IEnumerable<IProject> projectsToRun, IHandAnalyzerOutputStream outputStream);
}

