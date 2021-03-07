namespace Umbraco.Cms.Core.Services
{
    /// <summary>
    /// Defines a result object for the <see cref="ICacheInstructionService.EnsureInitialized(bool, int, object)"/> operation.
    /// </summary>
    public class CacheInstructionServiceInitializationResult
    {
        private CacheInstructionServiceInitializationResult()
        {
        }

        public bool Initialized { get; private set; }

        public bool ColdBootRequired { get; private set; }

        public int MaxId { get; private set; }

        public int LastId { get; private set; }

        public static CacheInstructionServiceInitializationResult AsUninitialized() => new CacheInstructionServiceInitializationResult { Initialized = false };

        public static CacheInstructionServiceInitializationResult AsInitialized(bool coldBootRequired, int maxId, int lastId) =>
            new CacheInstructionServiceInitializationResult
            {
                Initialized = true,
                ColdBootRequired = coldBootRequired,
                MaxId = maxId,
                LastId = lastId,
            };
    }
}
