namespace SynchroStats;

public interface IHandAnalyzerOutputStream
{
    void Write(string message);
}

public sealed class HandAnalyzerConsoleOutputStream : IHandAnalyzerOutputStream
{
    public void Write(string message)
    {
        Console.WriteLine(message);
    }
}

public sealed class HandAnalyzerEmptyOutputStream : IHandAnalyzerOutputStream
{
    public void Write(string message)
    {
        
    }
}
