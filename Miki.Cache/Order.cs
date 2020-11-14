namespace Miki.Cache
{
    /// <summary>
    /// The direction of the iterator to start.
    /// </summary>
    public enum Order
    {
        /// <summary>
        /// Moves from the lowest value, and moves up accordingly.
        /// </summary>
        Ascending,

        /// <summary>
        /// Moves from the higherst value, and moves down accordingly.
        /// </summary>
        Descending
    }
}