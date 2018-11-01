using System;
using System.Collections.Generic;

namespace Article3000
{
    internal interface ISubscriber<TData>
    {
        void Recieve(object obj,Article<TData> article);
        HashSet<string> Tags { get; }
    }
}
