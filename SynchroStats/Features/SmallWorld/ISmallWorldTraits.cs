namespace SynchroStats.Features.SmallWorld;

public interface ISmallWorldTraits
{
    int Level { get; }
    int AttackPoints { get; }
    int DefensePoints { get; }
    string MonsterType { get; }
    string MonsterAttribute { get; }
}
