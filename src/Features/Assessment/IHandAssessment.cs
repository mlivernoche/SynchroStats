using SynchroStats.Features.Combinations;
using System;
using System.Collections.Generic;
using System.Text;

namespace SynchroStats.Features.Assessment;

public interface IHandAssessment<TCardGroupName>
    where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
{
    bool Included { get; }
    HandCombination<TCardGroupName> Hand { get; }
}
