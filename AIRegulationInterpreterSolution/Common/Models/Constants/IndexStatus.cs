namespace Common.Models.Constants
{
    /// <summary>
    /// Status of document indexing in Vector Database
    /// </summary>
    public enum IndexStatus
    {
        /// <summary>
        /// Document uploaded but not yet indexed
        /// </summary>
        Pending = 0,
        
        /// <summary>
        /// Indexing in progress
        /// </summary>
        Indexing = 1,
        
        /// <summary>
        /// Successfully indexed in vector database
        /// </summary>
        Indexed = 2,
        
        /// <summary>
        /// Indexing failed (check IndexError field)
        /// </summary>
        Failed = 3
    }
}
