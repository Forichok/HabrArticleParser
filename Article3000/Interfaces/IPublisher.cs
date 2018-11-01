using System;

namespace Article3000
{
    interface IPublisher<TData>
    {
        event EventHandler<Article<TData>> Released;
    }
}
