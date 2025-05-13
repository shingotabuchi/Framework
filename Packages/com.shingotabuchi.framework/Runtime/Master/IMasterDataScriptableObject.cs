using System;
using System.Collections.Generic;

public interface IMasterDataScriptableObject
{
    Type Type { get; }
    IReadOnlyList<IMasterData> Data { get; }
}