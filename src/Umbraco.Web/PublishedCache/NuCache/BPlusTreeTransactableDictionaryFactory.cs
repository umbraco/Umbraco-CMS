using CSharpTest.Net.Collections;
using CSharpTest.Net.Serialization;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using Umbraco.Core.Configuration;
using Umbraco.Web.Install;
using Umbraco.Web.PublishedCache.NuCache.DataSource;

namespace Umbraco.Web.PublishedCache.NuCache
{
    public class BPlusTreeTransactableDictionaryFactory : ITransactableDictionaryFactory
    {
        private readonly IGlobalSettings _globalSettings;
        private readonly ISerializer<ContentNodeKit> _contentNodeKitSerializer;

        public BPlusTreeTransactableDictionaryFactory(IGlobalSettings globalSettings,ISerializer<ContentNodeKit> contentNodeKitSerializer = null)
        {
            _globalSettings = globalSettings;
            _contentNodeKitSerializer = contentNodeKitSerializer;
        }

        public ITransactableDictionary<int, ContentNodeKit> Get(ContentCacheEntityType entityType)
        {
            switch (entityType)
            {
                case ContentCacheEntityType.Document:
                    var localContentDbPath = GetContentDbPath();
                    var localContentCacheFilesExist = File.Exists(localContentDbPath);
                    return new BPlusTreeTransactableDictionary<int, ContentNodeKit>(GetTree(localContentDbPath, localContentCacheFilesExist, _contentNodeKitSerializer), localContentDbPath, localContentCacheFilesExist);
                case ContentCacheEntityType.Media:
                    var localMediaDbPath = GetMediaDbPath();
                     var localMediaCacheFilesExist = File.Exists(localMediaDbPath);
                    return new BPlusTreeTransactableDictionary<int, ContentNodeKit>(GetTree(localMediaDbPath, localMediaCacheFilesExist, _contentNodeKitSerializer), localMediaDbPath, localMediaCacheFilesExist);
                case ContentCacheEntityType.Member:
                    throw new ArgumentException("Unsupported Entity Type", nameof(entityType));
                default:
                    throw new ArgumentException("Unsupported Entity Type", nameof(entityType));
            }
        }

        private string GetContentDbPath()
        {
            var contentPath = GetLocalFilesPath();
            var localContentDbPath = Path.Combine(contentPath, "NuCache.Content.db");
            return localContentDbPath;
        }

        private string GetMediaDbPath()
        {
            var mediaPath = GetLocalFilesPath();
            var localMediaDbPath = Path.Combine(mediaPath, "NuCache.Media.db");
            return localMediaDbPath;
        }

        private string GetLocalFilesPath()
        {
            var path = Path.Combine(_globalSettings.LocalTempPath, "NuCache");

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            return path;
        }

        /// <summary>
        /// Ensures that the ITransactableDictionaryFactory has the proper environment to run.
        /// </summary>
        /// <param name="errors">The errors, if any.</param>
        /// <returns>A value indicating whether the ITransactableDictionaryFactory has the proper environment to run.</returns>
        public bool EnsureEnvironment(out IEnumerable<string> errors)
        {
            // must have app_data and be able to write files into it
            var ok = FilePermissionHelper.TryCreateDirectory(GetLocalFilesPath());
            errors = ok ? Enumerable.Empty<string>() : new[] { "NuCache local files." };
            return ok;
        }

        public virtual BPlusTree<int, ContentNodeKit> GetTree(string filepath, bool exists, ISerializer<ContentNodeKit> contentNodeKitSerializer = null)
        {
            var keySerializer = new PrimitiveSerializer();
            var valueSerializer = contentNodeKitSerializer;
            var options = new BPlusTree<int, ContentNodeKit>.OptionsV2(keySerializer, valueSerializer)
            {
                CreateFile = exists ? CreatePolicy.IfNeeded : CreatePolicy.Always,
                FileName = filepath,

                // read or write but do *not* keep in memory
                CachePolicy = CachePolicy.None,

                // default is 4096, min 2^9 = 512, max 2^16 = 64K
                FileBlockSize = GetBlockSize(),

                // other options?
            };

            var tree = new BPlusTree<int, ContentNodeKit>(options);

            // anything?
            //btree.

            return tree;
        }

        private static int GetBlockSize()
        {
            var blockSize = 4096;

            var appSetting = ConfigurationManager.AppSettings["Umbraco.Web.PublishedCache.NuCache.BTree.BlockSize"];
            if (appSetting == null)
                return blockSize;

            if (!int.TryParse(appSetting, out blockSize))
                throw new ConfigurationErrorsException($"Invalid block size value \"{appSetting}\": not a number.");

            var bit = 0;
            for (var i = blockSize; i != 1; i >>= 1)
                bit++;
            if (1 << bit != blockSize)
                throw new ConfigurationErrorsException($"Invalid block size value \"{blockSize}\": must be a power of two.");
            if (blockSize < 512 || blockSize > 65536)
                throw new ConfigurationErrorsException($"Invalid block size value \"{blockSize}\": must be >= 512 and <= 65536.");

            return blockSize;
        }
        public void Drop(ContentCacheEntityType entityType)
        {
            switch (entityType)
            {
                case ContentCacheEntityType.Document:
                    var localContentDbPath = GetContentDbPath();
                    var localContentCacheFilesExist = File.Exists(localContentDbPath);
                    var dictDoc = new BPlusTreeTransactableDictionary<int, ContentNodeKit>(null, localContentDbPath, localContentCacheFilesExist);
                    dictDoc.Drop();
                    break;
                case ContentCacheEntityType.Media:
                    var localMediaDbPath = GetMediaDbPath();
                    var localMediaCacheFilesExist = File.Exists(localMediaDbPath);
                    var dictMedia = new BPlusTreeTransactableDictionary<int, ContentNodeKit>(null, localMediaDbPath, localMediaCacheFilesExist);
                    dictMedia.Drop();
                    break;
                case ContentCacheEntityType.Member:
                    throw new ArgumentException("Unsupported Entity Type", nameof(entityType));
                default:
                    throw new ArgumentException("Unsupported Entity Type", nameof(entityType));
            }
        }
    }
}
