using System;

namespace Fwk.Master
{
    public interface IMasterData
    {
        int Id { get; set; }
        Type Type { get; }
    }
}