namespace Miki.Cache
{
    /// <summary>
    /// A value object which has a single entry that is sortable by cache clients implementing this
    /// interface.
    /// </summary>
    public struct SortedEntry<T>
    {
        /// <summary>
        /// The value of this object.
        /// </summary>
        public T Value { get; set; }

        /// <summary>
        /// The value this will be scored on.
        /// </summary>
        public double Score { get; set; }
    }
}