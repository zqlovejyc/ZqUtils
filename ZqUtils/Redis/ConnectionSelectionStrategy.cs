using StackExchange.Redis;

namespace ZqUtils.Redis
{
    /// <summary>
    /// The strategies for selecting the <see cref="IConnectionMultiplexer"/>
    /// /// </summary>
    public enum ConnectionSelectionStrategy
    {
        /// <summary>
        /// Every call will return the least loaded <see cref="IConnectionMultiplexer"/>.
        /// The load of every connection is defined by it's <see cref="ServerCounters.TotalOutstanding"/>.
        /// For more info refer to https://github.com/StackExchange/StackExchange.Redis/issues/512 .
        /// </summary>
        LeastLoaded = 0,

        /// <summary>
        /// Every call to will return the next connection in the pool in random.
        /// </summary>
        Random = 1
    }
}
