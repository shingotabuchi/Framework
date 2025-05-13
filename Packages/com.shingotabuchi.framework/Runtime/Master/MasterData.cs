using System;

namespace Fwk.Master
{
    public class MasterData<T> : IMasterData
    {
        public int Id { get; set; }
        public Type Type => typeof(T);
    }
}