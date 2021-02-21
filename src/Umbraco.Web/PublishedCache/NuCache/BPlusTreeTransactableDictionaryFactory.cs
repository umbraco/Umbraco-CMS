using CSharpTest.Net.Collections;
using CSharpTest.Net.Serialization;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using Umbraco.Core.Configuration;
using Umbraco.Web.Install;

namespace Umbraco.Web.PublishedCache.NuCache
{
    public class BPlusTreeTransactableDictionaryFactory<TKey, TValue> : ITransactableDictionaryFactory<TKey,TValue>
    {
        private readonly IGlobalSettings _globalSettings;
        private readonly ITransactableDictionarySerializer<TValue> _serializer;
        private readonly ITransactableDictionarySerializer<TKey> _keySerializer;
        private string _folderName;

        public BPlusTreeTransactableDictionaryFactory(IGlobalSettings globalSettings,
            ITransactableDictionarySerializer<TValue> valueSerializer,
            ITransactableDictionarySerializer<TKey> keySerializer)
        {
            _globalSettings = globalSettings;
            _serializer = valueSerializer;
            _keySerializer = keySerializer;
            _folderName = "NuCache";
        }

        public ITransactableDictionary<TKey, TValue> Get(string name, IComparer<TKey> keyComparer = null, bool isReadOnly = false)
        {
            var localContentDbPath = GetDbPath(name);
            var localContentCacheFilesExist = File.Exists(localContentDbPath);
            var keySerializer = new TransactableDictionaryBPlusTreeSerializerAdapter<TKey>(_keySerializer);
            var valueSerializer = new TransactableDictionaryBPlusTreeSerializerAdapter<TValue>(_serializer);
            var bplusTree = GetTree(localContentDbPath, localContentCacheFilesExist, keySerializer,valueSerializer, keyComparer, isReadOnly);
            return new BPlusTreeTransactableDictionary<TKey, TValue>(bplusTree, localContentDbPath, localContentCacheFilesExist);
        }
        public void Drop(string name)
        {
            var localContentDbPath = GetDbPath(name);
            var localContentCacheFilesExist = File.Exists(localContentDbPath);
            var dictDoc = new BPlusTreeTransactableDictionary<TKey, TValue>(null, localContentDbPath, localContentCacheFilesExist);
            dictDoc.Drop();
        }
        protected virtual string GetDbPath(string name)
        {
            var contentPath = GetLocalFilesPath();
            var localContentDbPath = Path.Combine(contentPath, $"{name}.db");
            return localContentDbPath;
        }

        protected virtual string GetLocalFilesPath()
        {
            var path = Path.Combine(_globalSettings.LocalTempPath, _folderName);

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            return path;
        }

        /// <summary>
        /// Ensures that the ITransactableDictionaryFactory has the proper environment to run.
        /// </summary>
        /// <param name="errors">The errors, if any.</param>
        /// <returns>A value indicating whether the ITransactableDictionaryFactory has the proper environment to run.</returns>
        public virtual bool EnsureEnvironment(out IEnumerable<string> errors)
        {
            // must have app_data and be able to write files into it
            var ok = FilePermissionHelper.TryCreateDirectory(GetLocalFilesPath());
            errors = ok ? Enumerable.Empty<string>() : new[] { "NuCache local files." };
            return ok;
        }

        protected virtual BPlusTree<TKey, TValue> GetTree(string filepath, bool exists, ISerializer<TKey> keySerializer,
            ISerializer<TValue> valueSerializer,
            IComparer<TKey> keyComparer = null, bool isReadOnly = false)
        {
           
            var options = new BPlusTree<TKey, TValue>.OptionsV2(keySerializer, valueSerializer)
            {
                CreateFile = exists ? CreatePolicy.IfNeeded : CreatePolicy.Always,
                FileName = filepath,

                // read or write but do *not* keep in memory
                CachePolicy = CachePolicy.None,

                // default is 4096, min 2^9 = 512, max 2^16 = 64K
                FileBlockSize = GetBlockSize(),
                // other options?
                ReadOnly = isReadOnly

            };
            if(keyComparer != null)
            {
                options.KeyComparer = keyComparer;
            }

            var tree = new BPlusTree<TKey, TValue>(options);

            // anything?
            //btree.

            return tree;
        }

        protected virtual int GetBlockSize()
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
       
    }
}
